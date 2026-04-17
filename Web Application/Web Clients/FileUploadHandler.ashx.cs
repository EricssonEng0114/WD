using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.SessionState;
using UBPC.Web.Common;
using System.Web.Script.Serialization;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Web.Configuration;

namespace UBPCWeb
{
    /// <summary>
    /// Summary description for FileUploadHandler
    /// </summary>
    public class FileUploadHandler : IHttpHandler, IRequiresSessionState
    {
        private static SQLDBHelper dbHelperObj = new SQLDBHelper();
        private static string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();

        public void ProcessRequest(HttpContext context)
        {
            LogEntry log = new LogEntry();
            String uploadToFileServer = HttpContext.Current.Session["s_UploadFileServerPath"] == null ? string.Empty
                                    : HttpContext.Current.Session["s_UploadFileServerPath"].ToString();

            //Retrieve client code if the upload is from 
            string clientCode = uploadToFileServer.Length > 0 ? 
                                    (HttpContext.Current.Session["CurRepoRptSelectedClient"] == null ? string.Empty :
                                    HttpContext.Current.Session["CurRepoRptSelectedClient"].ToString()) 
                                : string.Empty;
               
            try
            {
                HttpRuntimeSection section = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
                int maxRequestLength = section.MaxRequestLength;


                String userID = HttpContext.Current.Session["s_UserID"].ToString().Trim();

                //Using XMLHttpRequest to call to FileUploadHandler
                if (uploadToFileServer.Length > 0)
                {
                    if (!Directory.Exists(uploadToFileServer))
                    {
                        Directory.CreateDirectory(uploadToFileServer);
                    }

                    for (int i = 0, il = context.Request.Files.Count; i < il; i++)
                    {
                        HttpPostedFile postedFile = context.Request.Files[i];
                        int length = postedFile.ContentLength;
                        //Validation , if more than 20mb, not allowed
                        //maxRequestLength x 1024
                        int maxLength = maxRequestLength * 1024;
                        int fileSize = postedFile.ContentLength * 1024;
                        if (length > 0 && length <= maxLength)
                        {
                            string fileName = postedFile.FileName;
                            /*
                            var bytes = new byte[length];
                            context.Request.InputStream.Read(bytes, 0, length);
                            

                            //Upload file and change to .txt extension due to HTML5 limitation
                            //HTML5 <a Download> attribute will have download failed for unknown extension such as .rpt, change to .txt is widely accepted in all browser types.
                            var filestream = new FileStream(Path.Combine(uploadToFileServer, fileName + ".txt"), FileMode.Create, FileAccess.ReadWrite);
                            filestream.Write(bytes, 0, length);
                            filestream.Close();
                            */
                            postedFile.SaveAs(uploadToFileServer + "//" + fileName + ".txt");
                            //Uploaded File updated to Database
                            UploadReportFiles(fileName, uploadToFileServer);
                        }
                    }
                }
                //Using AJAX Method to call to this handler
                else
                {
                    if (context.Request.Files.Count > 0)
                    {
                        string uploadFile = string.Empty;
                        for (int i = 0, il = context.Request.Files.Count; i < il; i++)
                        {
                            HttpPostedFile postedFile = context.Request.Files[i];
                            if (postedFile.ContentLength > 0)
                            {
                                //set which folder to save the file
                                string folderPath = "C://Temp//" + userID;

                                if (!Directory.Exists(folderPath))
                                {
                                    Directory.CreateDirectory(folderPath);
                                }

                                //set filename
                                string fileName = Path.GetFileName(postedFile.FileName);

                                //save the file in folder
                                postedFile.SaveAs(folderPath + "//" + fileName);

                                if (!string.IsNullOrEmpty(uploadFile)) uploadFile += ",";

                                uploadFile += fileName;
                            }
                        }


                        string json = new JavaScriptSerializer().Serialize(
                                new
                                {
                                    name = uploadFile
                                }
                        );

                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.ContentType = "text/json";
                        context.Response.Write(json);
                    }
                }
            }
            catch (Exception ex)
            {
                log = new LogEntry();
                log.Caller = uploadToFileServer ==string.Empty? LogCallerID.BPOOutlookModule: LogCallerID.ReportRepository;
                log.Severity = LogEventType.Error;
                log.Message = "FileUploadHandler:" + ex.Message;
                log.ClientCode = clientCode;
                log.Exception = ex;
                log.Write();

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        public void UploadReportFiles(string reportFileName, string uploadedPath)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                String userID = HttpContext.Current.Session["s_UserID"].ToString().Trim();

                #region Compare current busdate v.s selected busdate
                DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["CurRepoRptSelectedBusdate"]);
                string CurClient = HttpContext.Current.Session["CurRepoRptSelectedClient"].ToString();

                DateTime curActiveBusdate =  Convert.ToDateTime(HttpContext.Current.Session["CurBusdate"]);

                DateTime busdate;// = Convert.ToDateTime(dateTime);
                string[] formats = { "dd/MM/yyyy", "MM/dd/yyyy", "M/dd/yyyy" };
                
                if (!DateTime.TryParseExact(curBusdate.ToShortDateString(), 
                    formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out busdate))
                {
                    //Set today date if datetime failed
                    busdate = DateTime.Today;
                }

                #endregion

                //Retreive DB name
                string strArchivalDBName = string.Empty;

                if (!busdate.ToString("yyyyMMdd").Equals(curActiveBusdate.ToString("yyyyMMdd")))
                {
                    string yearofBusdate = busdate.Year.ToString();
                    strArchivalDBName = "RPT_Archival_" + yearofBusdate.Trim();
                }

                stmt = "sp_AddDeleteUploadedRptFile";
                IDbDataParameter[] param = new[] { 
                        dbHelperObj.CreateParameter(DbType.String, 3, "@Action", ParameterDirection.Input, "ADD"),
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@BusDate", ParameterDirection.Input, curBusdate),
                        dbHelperObj.CreateParameter(DbType.String, 10, "@ClientCode", ParameterDirection.Input, CurClient),
                        dbHelperObj.CreateParameter(DbType.String, 50, "@ArchivalDBName", ParameterDirection.Input, strArchivalDBName),
                        dbHelperObj.CreateParameter(DbType.String, 100, "@ReportFileName", ParameterDirection.Input, reportFileName),
                        dbHelperObj.CreateParameter(DbType.String, 200, "@ReportFilePath", ParameterDirection.Input, uploadedPath),                        
                        dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input, userID) 
                    };

                //dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);

               int retRec = dbHelperObj.executeNonQuery(iConn, CommandType.StoredProcedure, stmt, param);

                //Log Uploaded Success
                Logger.Write(false, LogCallerID.ReportRepository, CurClient, "Report FileName =" + reportFileName, "Report File Uploaded.", LogEventType.Information, userID);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                iConn.Close();
            }
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}