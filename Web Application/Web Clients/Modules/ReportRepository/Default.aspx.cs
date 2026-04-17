using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UBPC.Encryption;
using UBPC.Web.Common;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.VisualBasic;
using System.Web.Services;
using System.Web.Script.Services;
using UBPCWeb.Model;
using System.Web.Script.Serialization;
using System.IO;
using System.Net;
using System.Web.Configuration;
using System.IO.Compression;
using System.Security.AccessControl;

namespace UBPCWeb.Modules.ReportRepository
{
    public partial class Default : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private bool isPageValid = false;
        private bool isForUnisys = false;
        private string clientList = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString().Trim();
            userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString().Trim();
            isForUnisys = Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(Session["s_UserForUnisys"].ToString());
            clientList = Session["s_UserClients"] == null ? string.Empty : Session["s_UserClients"].ToString().Trim();

            if (Session["RetainDeleteSelection"] == null)
                Session["RetainDeleteSelection"] = "0";

            if (Session["HideReport"] == null)
                Session["HideReport"] = "0";

            if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
            {
                PageValidatorResult validatorResult;

                //For password chnge, validate password by encryption class
                validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "ReportRepository");
                isPageValid = validatorResult.Valid;

                if (!isPageValid)
                {
                    Session["s_GeneralMsg"] = validatorResult.ReturnMessage;
                    Response.Redirect("/Home", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    Session["s_GeneralMsg"] = string.Empty;
                    isPageValid = true;

                    HttpRuntimeSection section = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
                    Session["s_MaxRequestLength"] = (section.MaxRequestLength * 1024);


                }
            }
            else
            {
                //invalid user - back to login page
                Session["s_UserID"] = "";
                Response.Redirect("/Login", false);
                Context.ApplicationInstance.CompleteRequest();
            }

            if (isPageValid & !Page.IsPostBack)
            {
                LoadControl();
            }

            if (Page.IsPostBack)
            {
                #region Delete File Action
                string tagOperation = Request.Form["tag"] == null ? string.Empty : Request.Form["tag"];
                if (tagOperation.Equals("DEL"))
                {
                    //Perform Update
                    //  string announID = Request.Form["tableID"] == null ? string.Empty : Request.Form["tableID"];
                    string deletedFileName = Session["MainDTParam"].ToString();

                    if (!string.IsNullOrEmpty(deletedFileName))
                    {
                        DeleteReportFile(deletedFileName);

                        string uploadToFileServer = Session["s_UploadFileServerPath"] == null ? string.Empty
                                                   :Session["s_UploadFileServerPath"].ToString();

                        //Uploaded file will have extension of ".txt", e.g: dailybilling.rpt -> dailybilling.rpt.txt
                        string fullPathFile = Path.Combine(uploadToFileServer, deletedFileName + ".txt");

                        //Delete physical files?
                        if (System.IO.File.Exists(fullPathFile))
                        {
                            System.IO.File.Delete(fullPathFile);
                        }

                        Logger.Write(false, LogCallerID.ReportRepository, ddlClient.SelectedValue.ToString(), "FileName =" + deletedFileName, "Report File Deleted.", LogEventType.Information, userID);

                    }
                    else
                    {
                        //Invalid ID retrieve for delete operation
                        Logger.Write(false, LogCallerID.ReportRepository, ddlClient.SelectedValue.ToString(), "Delete Uploaded Reports", "Invalid Report File Name retrieved for delete operation", LogEventType.Error, userID);
                    }

                    //Retain current busdate and current client code selection
                    Session["RetainDeleteSelection"] = 1;

                    Response.Redirect("/ReportRepository", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                #endregion
                
                /*
                if (hfUploadFlag.Value.Equals("1")) 
                {
                    //Refresh listing after upload success
                    btnSearch_Click(sender, e);
                }
                */
                //btnSearch_Click(sender, e);
            }
        }

        private void LoadControl()
        {
            try
            {
                //Update files type allow to FileUpload component
                Session["s_FileTypes"] = Parameters.GetParamValue(webDBConnStr, "FileTypeAllowed");
                Session["s_FileCount"] = Parameters.GetParamValue(webDBConnStr, "FileCountAllowed");                

                string fileTypes = Session["s_FileTypes"].ToString();
                uploadFile.Attributes["accept"] = fileTypes;
                                
                //Load Client Drop Down
                string[] cltArr = clientList.Split(',');
                List<ListItem> cltitems = new List<ListItem>();
                foreach (string clientCode in cltArr)
                {
                    cltitems.Add(new ListItem(clientCode, clientCode));
                }

                this.ddlClient.DataSource = from i in cltitems select new ListItem() { Text = i.Text, Value = i.Value };
                this.ddlClient.DataTextField = "Text";
                this.ddlClient.DataValueField = "Value";
                this.ddlClient.DataBind();
                
                if (this.ddlClient.Items.Count > 0)
                {
                    this.ddlClient.Items[0].Selected = true;
                }

                //Build connection string for site and set curDataTable
                DBConnectionInfo dbConn = new DBConnectionInfo();
                dbConn.webConnStr = webDBConnStr;
               // Session["CurBusdate"] = DateTime.Now;
                DateTime curActiveBusdate =Session["CurBusdate"] == null ? DateTime.Now : Convert.ToDateTime(Session["CurBusdate"]);
                txtDateTime.Text = curActiveBusdate.ToShortDateString();

                //Initialise selected busdate
                if (Session["CurActionSelectedBusdate"] == null || Session["CurActionSelectedBusdate"] == "")
                {
                    Session["CurActionSelectedBusdate"] = curActiveBusdate.ToShortDateString();
                }

                if (HttpContext.Current.Session["RetainDeleteSelection"].ToString() =="1")
                {
                    if (HttpContext.Current.Session["CurRepoRptSelectedClient"] == null)
                    {
                        HttpContext.Current.Session["CurRepoRptSelectedClient"] = ddlClient.SelectedValue.ToString();
                    }
                    else 
                    {
                        ddlClient.SelectedValue = HttpContext.Current.Session["CurRepoRptSelectedClient"].ToString();
                    }

                    if (HttpContext.Current.Session["CurRepoRptSelectedBusdate"] == null)
                    {
                        HttpContext.Current.Session["CurRepoRptSelectedBusdate"] = curActiveBusdate.ToShortDateString();
                    }
                    else 
                    {
                        txtDateTime.Text = Convert.ToDateTime(HttpContext.Current.Session["CurRepoRptSelectedBusdate"]).ToShortDateString();
                    }
                }
                else 
                {
                    HttpContext.Current.Session["CurRepoRptSelectedClient"] = ddlClient.SelectedValue.ToString();
                    HttpContext.Current.Session["CurRepoRptSelectedBusdate"] = curActiveBusdate.ToShortDateString();
                }

                if (Convert.ToBoolean(Session["s_UserForUnisys"]).Equals(true))
                {
                    lblInvalidDataSelected.Visible = false;
                }
                
                UpdateActiveBusdate();
                //Set Directory Path Format for Uploaded Files
                SetDirPathForUploadedFile();

                HttpContext.Current.Session["RetainDeleteSelection"] = "0";//Reset deletion back to 0
                //Initialise Grid
                //busdate, client, branch, category
                /*
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initRepoReportDataTable('{0}','{1}');", txtDateTime.Text, ddlClient.SelectedValue.ToString()), true);

                ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);
                 * */
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ReportRepository;
                log.ClientCode = ddlClient.SelectedValue == null ? "" : ddlClient.SelectedValue.ToString();
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }
        }

        private void DeleteReportFile(string reportFileName)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string CurClient =  HttpContext.Current.Session["CurRepoRptSelectedClient"] == null? ""
                            : HttpContext.Current.Session["CurRepoRptSelectedClient"].ToString();


            try
            {
                string stmt = string.Empty;
                stmt = Resource.stmtSQLDeleteUploadedReport;

                #region Compare current busdate v.s selected busdate
                DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["CurRepoRptSelectedBusdate"]);

                DateTime curActiveBusdate = Convert.ToDateTime(HttpContext.Current.Session["CurBusdate"]);

                DateTime busdate;
                /*
                string[] formats = { "dd/MM/yyyy", "MM/dd/yyyy", "M/dd/yyyy" };

                if (!DateTime.TryParseExact(curBusdate.ToShortDateString(),
                    formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out busdate))
                {
                    //Set today date if datetime failed
                    busdate = DateTime.Today;
                }
                */
                string repoDt = HttpContext.Current.Session["CurRepoRptSelectedBusdate"] == null ? DateTime.Now.ToString("dd/MM/yyyy") : HttpContext.Current.Session["CurRepoRptSelectedBusdate"].ToString();
                List<string> RepoDt = repoDt.Split('/').ToList();
                busdate = new DateTime(int.Parse(RepoDt.Last()), int.Parse(RepoDt.First().PadLeft(2, '0')), int.Parse(RepoDt[1].PadLeft(2, '0')));

                #endregion

                //Retreive DB name
                string strArchivalDBName = string.Empty;

                if (!busdate.ToString("yyyyMMdd").Equals(curActiveBusdate.ToString("yyyyMMdd")))
                {
                    string yearofBusdate = busdate.Year.ToString();
                    strArchivalDBName = "RPT_Archival_" + yearofBusdate.Trim();
                }

                IDbDataParameter[] param = new[] { 
                        dbHelperObj.CreateParameter(DbType.String, 3, "@Action", ParameterDirection.Input, "DEL"),
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@BusDate", ParameterDirection.Input, curBusdate),
                        dbHelperObj.CreateParameter(DbType.String, 10, "@ClientCode", ParameterDirection.Input, CurClient),
                        dbHelperObj.CreateParameter(DbType.String, 50, "@ArchivalDBName", ParameterDirection.Input, strArchivalDBName),
                        dbHelperObj.CreateParameter(DbType.String, 100, "@ReportFileName", ParameterDirection.Input, reportFileName),
                        dbHelperObj.CreateParameter(DbType.String, 200, "@ReportFilePath", ParameterDirection.Input, ""),                        
                        dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input, userID) 
                    };


                dbHelperObj.executeNonQuery(iConn, CommandType.StoredProcedure, stmt, param);

                //Log record who deleting
                Logger.Write(false, LogCallerID.ReportRepository, CurClient, "File Name:" + reportFileName, "Deleting Report File", LogEventType.Information, userID);

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ReportRepository;
                log.ClientCode = CurClient;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                throw ex;
            }
            finally
            {
                iConn.Close();
            }
        }
        
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string dateTime = string.Empty;
            string client = string.Empty;
            
            dateTime = txtDateTime.Text.Trim();
            client = ddlClient.SelectedValue.ToString().Trim();
            //Shinyi:Used in Fileuploadhandler to upload file and update to database the uploaded file
            Session["CurRepoRptSelectedBusdate"] = dateTime;
            Session["CurRepoRptSelectedClient"] = client;
                 
            
            if (string.IsNullOrEmpty(dateTime.Trim()))
            {
                Logger.Write(false, LogCallerID.Reports, client, "Validate Search", "Date is Empty", LogEventType.Error, userID);
            }
            else
            {
                //For Client User: Compare selected date v.s current busdate
                if (!Session["s_CurClientMaxDownloadDays"].ToString().Equals("0"))
                {
                    //Check client max download days
                    int maxDownloadDay = Convert.ToInt32(Session["s_CurClientMaxDownloadDays"].ToString());

                    DateTime curActiveBusdate = Convert.ToDateTime(HttpContext.Current.Session["CurBusdate"]);
                    DateTime curSelectedBusdate = Convert.ToDateTime(HttpContext.Current.Session["CurRepoRptSelectedBusdate"]);
                    
                    if (Convert.ToBoolean(Session["s_UserForUnisys"]).Equals(true))
                    {
                        lblInvalidDataSelected.Visible = false;
                        Session["HideReport"] = "0";
                    }
                    else
                    {
                        bool validDate = IsValidHistoricalDate(curActiveBusdate, curSelectedBusdate, maxDownloadDay);
                        lblInvalidDataSelected.Visible = !validDate;
                        client = lblInvalidDataSelected.Visible ? string.Empty : client;//Do not show record if invalid date selected

                        if (lblInvalidDataSelected.Visible)
                        {
                            lblInvalidDataSelected.Text = "Please select a date up to " + maxDownloadDay.ToString() + " day(s) of history date";
                            Session["HideReport"] = "1";
                        }
                        else 
                        {
                            Session["HideReport"] = "0";
                        }
                      
                    }

                }

                //Set Directory Path for Uploaded File - Reset in case differnet client selected
                SetDirPathForUploadedFile();


                ////date, severity, caller,client
                //ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                //    string.Format("initRepoReportDataTable('{0}','{1}');", dateTime, client), true);

                ////date, severity, caller,client
                //ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);
                 
            }

        }

        private void UpdateActiveBusdate()
        {
            try
            {
                DBConnectionInfo db = new DBConnectionInfo();
                db.webConnStr = webDBConnStr;
                string mainUVRPSConnection = db.GetSiteConnectionString(true, ddlClient.SelectedValue.ToString(), db.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), ddlClient.SelectedValue.ToString().Trim()));

                DateTime curActiveBusdate = DateTime.Today;
                if (BusinessDate.GetActive(mainUVRPSConnection) != null)
                {
                    curActiveBusdate = Convert.ToDateTime(BusinessDate.GetActive(mainUVRPSConnection));
                }

               

                if (Session["RetainDeleteSelection"] == null)
                    Session["RetainDeleteSelection"] = 0;

                Session["CurRepoRptSelectedBusdate"] = Session["RetainDeleteSelection"].ToString().Equals("1")? Session["CurRepoRptSelectedBusdate"]: curActiveBusdate.ToShortDateString();
                Session["CurActionSelectedBusdate"] = curActiveBusdate.ToShortDateString();
                Session["CurRepoRptSelectedClient"] = ddlClient.SelectedValue.ToString();

                Session["CurBusdate"] = curActiveBusdate.ToShortDateString();

                txtDateTime.Text = Session["CurRepoRptSelectedBusdate"].ToString();// curActiveBusdate.ToShortDateString();

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Reports;
                log.ClientCode = ddlClient.SelectedValue == null ? "" : ddlClient.SelectedValue.ToString();
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

        }

        private void SetDirPathForUploadedFile()
        {
            //Retrieve Virtual Directories Folder name from Param
            //Get par value 2
            string paramFileUploadVirDir = Parameters.GetParamValue(webDBConnStr, "ReportRootSrc", false);//FileUploadPath

            string strRptPath = string.Empty;

            DateTime busdate;
            //string[] formats = { "MM/dd/yyyy", "M/dd/yyyy", "dd/MM/yyyy" };         

            //if (!DateTime.TryParseExact(txtDateTime.Text, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out busdate))
            //{
            //    //Set today date if datetime failed
            //    busdate = DateTime.Today;
            //}

            string repoDt = txtDateTime.Text == null ? DateTime.Now.ToString("dd/MM/yyyy") : txtDateTime.Text;
            List<string> RepoDt = repoDt.Split('/').ToList();
            busdate = new DateTime(int.Parse(RepoDt.Last()), int.Parse(RepoDt.First().PadLeft(2, '0')), int.Parse(RepoDt[1].PadLeft(2, '0')));


            //yyyymmdd
            string busdateFormat = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');

            //FileUploadPath/REPO_2022_12/20230902/CIMB
            string completePath = Path.Combine(paramFileUploadVirDir + "//" + "REPO_" + busdateFormat.Substring(0, 4) + "_" + busdateFormat.Substring(4, 2)
                                                                     + "//" + busdateFormat + "//" + ddlClient.SelectedValue.ToString().Trim());
            strRptPath = Server.MapPath("/" + completePath);
                        
            Session["s_UploadFileServerPath"] = strRptPath;
        }

        private bool IsValidHistoricalDate(DateTime curentBusdate, DateTime selectedDate, int maxDaysAllowed)
        {
            DateTime currentDate = curentBusdate;
            int businessDays = 0;
            for (DateTime date = selectedDate; date < currentDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    businessDays++;
                }
            }
            return businessDays <= maxDaysAllowed;
        }
        
        [WebMethod]
        public static string getReportUrl(string rptCode, string cltCode, string wsID, string busdate)
        {
            string url = string.Empty;
            url = "/CRViewerReportForm/ReportViewer.aspx?ReportCode=" + rptCode + "&ReportClt=" + cltCode + "&ReportDate=" + busdate + "&WrkSourceID=" + wsID;

           string fullUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + url;
            return fullUrl;
        }
               
        protected void ddlClient_SelectedIndexChanged1(object sender, EventArgs e)
        {
            //Set Valid navigation
            UpdateActiveBusdate();
            SetDirPathForUploadedFile();
            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "setValidNav", "setValidNavigation();", true);
        }

        [WebMethod]
        public static string assignSessionDT(string selID)
        {
            //Get 1 row of DataTable record and assign to session
            HttpContext.Current.Session["MainDTParam"] = selID.Trim();
            return "1";
        }

        [WebMethod]
        public static string DownloadFiles(List<string> files)
        {
            // Create a unique zip file for this download
           // string zipPath = Path.GetTempFileName() + ".zip";//
            string zipPath = GetUniqueFilename() + ".zip";

            using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (string file in files)
                {
                    string fullfilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/"+file);// HttpContext.Current.Server.MapPath(file);
                    string fileName = Path.GetFileNameWithoutExtension(fullfilePath); // filename: abc.doc.txt, get file name without extension = abc.doc
                    ZipArchiveEntry ent = zip.CreateEntryFromFile(fullfilePath, fileName);

                   

                    //File.SetAttributes(Path.Combine(zipPath, fileName), FileAttributes.Normal);
                }
            }


            //File.SetAttributes(zipPath, FileAttributes.Normal);


            //FileUploadPath/ZIP_YYYYMM/YYYYMMDD

            string yearMonthStr = DateTime.Now.ToString("yyyyMM");
            string dateStr = DateTime.Now.ToString("yyyyMMdd");

            // Create the directory path
            string directoryPath = "~/FileUploadPath/ZIP_" + yearMonthStr + "/" + dateStr;
            string mappedDirectoryPath = System.Web.Hosting.HostingEnvironment.MapPath(directoryPath);

            // Create the directory if it doesn't exist
            if (!Directory.Exists(mappedDirectoryPath))
            {
                Directory.CreateDirectory(mappedDirectoryPath);
            }

            // Move the zip file to the newly created directory
            string downloadPath = directoryPath + "/" + Path.GetFileName(zipPath);
            string mappedDownloadPath = System.Web.Hosting.HostingEnvironment.MapPath(downloadPath);
            File.Move(zipPath, mappedDownloadPath);

            // Set write permissions for the .zip file
            File.SetAttributes(mappedDownloadPath, FileAttributes.Normal);


            // Set Access Control list permissions for the .zip file - ensure IIS IUSRS can download
            FileSecurity fileSecurity = File.GetAccessControl(mappedDownloadPath);
            fileSecurity.AddAccessRule(new FileSystemAccessRule("IIS_IUSRS", FileSystemRights.FullControl, AccessControlType.Allow));
            File.SetAccessControl(mappedDownloadPath, fileSecurity);

          
             // Set the Content-Disposition header for attachment
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "application/zip";
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=\"{Path.GetFileName(zipPath)}\"");

            // Return the URL of the file
            string url = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute(downloadPath);
            return url;

        }


        private static string GetUniqueFilename()
        {
            string datePrefix = DateTime.Now.ToString("yyyyMMddHHmmssfff"); // Includes the time down to milliseconds
            string uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 6); // Get only the first 6 characters of the GUID
            return Path.Combine(Path.GetTempPath(), datePrefix + "_" + uniqueSuffix + "_Rpt");
        }


    }
}