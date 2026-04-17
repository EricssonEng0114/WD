using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UBPCWeb.Model;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.VisualBasic;
using UBPC.Web.Common;
using UBPC.Web.Collections;

namespace UBPCWeb.Modules.ReportRepository
{
    public class CommonFunction
    {
        private static WebClientCollection clients = null;
        private static RLWorksourceCollection worksources = null;
        private static SQLDBHelper dbHelperObj = new SQLDBHelper();
        private static string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();

        public static jQueryDataTableGridModel GetListingPageData(String[] paramList)
        {
            try
            {
                DateTime busdate;
               // string[] formats = { "dd/MM/yyyy", "MM/dd/yyyy", "M/dd/yyyy" };
                /*
                if (!DateTime.TryParseExact(HttpContext.Current.Session["CurRepoRptSelectedBusdate"].ToString(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out busdate))
                {
                    //Set today date if datetime failed
                    busdate = DateTime.Today;
                }*/
                string repoDt = HttpContext.Current.Session["CurRepoRptSelectedBusdate"] == null?DateTime.Now.ToString("dd/MM/yyyy"):HttpContext.Current.Session["CurRepoRptSelectedBusdate"].ToString();
                List<string> RepoDt = repoDt.Split('/').ToList();
                busdate = new DateTime(int.Parse(RepoDt.Last()), int.Parse(RepoDt.First().PadLeft(2, '0')), int.Parse(RepoDt[1].PadLeft(2, '0')));
                //yyyymmdd
                //string busdateFormat = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');
                string busdateFormat = busdate.ToString("yyyyMMdd");
                string folderName = busdate.ToString("yyyy") + "_" + busdate.ToString("MM");
                DataTable dtDB = GetAllReport(paramList);

                var query = (from DataRow row in dtDB.Rows
                             select new ReportRepoModel
                             {
                                 TempID = row["TMPID"].ToString(),
                                 ReportFileName = row["REPO_ReportFileName"].ToString(),
                                 ReportClientCode = row["REPO_ClientCode"].ToString(),
                                 ReportFileUploadedAt = row["REPO_FileUploadedAt"].ToString(),
                                 ReportFileUploadedBy = row["REPO_FileUploadedBy"].ToString(),
                                 ReportFileUploadedPath = row["REPO_FileUploadedPath"].ToString(),
                                 ReportFileUploadedFullPath = "FileUploadPath\\REPO_" + folderName+"\\" + busdateFormat 
                                                              + "\\" + row["REPO_ClientCode"].ToString().Trim()+ "\\" + row["REPO_ReportFileName"].ToString().Trim()+ ".txt",
                                 ReportFileAllowDelete = Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"]).Equals(true) ? "1":"0"
                             });

                var gridData = new jQueryDataTableGridModel()
                {
                    aaData = query.ToList(),
                    iTotalDisplayRecords = query.Count(),
                    iTotalRecords = query.Count()
                };

                return gridData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable GetAllReport(String[] filterList)
        {
            //Param List
            //1 - Date Time, 2- client  
                        
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                string filterCriteria = string.Empty;
                string dateTime = filterList[0].ToString();
                string client = filterList[1].ToString();

                DateTime curActiveBusdate = String.IsNullOrEmpty(HttpContext.Current.Session["CurBusdate"].ToString())
                    ? DateTime.Now : Convert.ToDateTime(HttpContext.Current.Session["CurBusdate"]);
                
                DateTime busdate;// = Convert.ToDateTime(dateTime);
                /*
                string[] formats = { "dd/MM/yyyy", "MM/dd/yyyy", "M/dd/yyyy" };

                if (!DateTime.TryParseExact(dateTime, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out busdate))
                {
                    //Set today date if datetime failed
                    busdate = DateTime.Today;                    
                }
                */

                HttpContext.Current.Session["CurRepoRptSelectedBusdate"] = dateTime;
                string repoDt = HttpContext.Current.Session["CurRepoRptSelectedBusdate"] == null ? DateTime.Now.ToString("dd/MM/yyyy") : HttpContext.Current.Session["CurRepoRptSelectedBusdate"].ToString();
                List<string> RepoDt = repoDt.Split('/').ToList();
                busdate = new DateTime(int.Parse(RepoDt.Last()), int.Parse(RepoDt.First().PadLeft(2,'0')), int.Parse(RepoDt[1].PadLeft(2,'0')));

                //Retreive DB name
                string strArchivalDBName = string.Empty;

                if (!busdate.ToString("yyyyMMdd").Equals(curActiveBusdate.ToString("yyyyMMdd")))
                {
                    string yearofBusdate = busdate.Year.ToString();
                    strArchivalDBName = "RPT_Archival_" + yearofBusdate.Trim();
                }
                
                //generate the sql statement here
                stmt = Resource.stmtGetAllReport;        

                IDbDataParameter[] param =
                 new[] { 
                        dbHelperObj.CreateParameter(DbType.Date, 0, "@BusDate", ParameterDirection.Input, busdate),
                        dbHelperObj.CreateParameter(DbType.String, 10, "@ClientCode", ParameterDirection.Input, client),
                        dbHelperObj.CreateParameter(DbType.String, 50, "@ArchivalDBName", ParameterDirection.Input, strArchivalDBName)
                    };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);


                return dtDB;
            }
            catch (Exception ex)
            {
                //Exceptin due to Database or Table not existed, return empty table instead
                DataTable dtEmpty = new DataTable();

                DataColumn emptyCol = dtEmpty.Columns.Add("CustID", typeof(Int32));
                emptyCol.AllowDBNull = false;
                emptyCol.Unique = true;

                dtEmpty.Columns.Add("REPO_ReportFileName", typeof(String));
                dtEmpty.Columns.Add("REPO_ClientCode", typeof(String));
                dtEmpty.Columns.Add("REPO_FileUploadedAt", typeof(String));
                dtEmpty.Columns.Add("REPO_FileUploadedBy", typeof(String));

                return dtEmpty;
            }
            finally
            {
                iConn.Close();
            }
        }

       
        public static String BuildConnectionString(string clientCode)
        {
            IDbConnection iConn = dbHelperObj.initConnection(ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString());
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtGetClientConnectionString;
                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 10, "@ClientCode", ParameterDirection.Input,clientCode)
                };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);
                return dtDB.Rows.Count > 0 ? dtDB.Rows[0][0].ToString() : null;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                iConn.Close();
            }
        }
        
        public static DataTable GetClientInfoList(bool isForUnisys)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = isForUnisys ? Resource.stmtGetClientInfoList : Resource.stmtGetClientInfoListExcUnisys;
                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt);

                return dtDB;

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

    }

    public class ReportRepoModel
    {
        //TBL_REPO_REPORT
        public string TempID { get; set; }
        public string ReportFileName { get; set; }
        public string ReportClientCode { get; set; }
        public string ReportFileUploadedAt { get; set; }
        public string ReportFileUploadedPath { get; set; }
        public string ReportFileUploadedBy { get; set; }
        public string ReportFileUploadedFullPath { get; set; }
        public string ReportFileAllowDelete { get; set; }


    }
}