using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using UBPC.Web.Common;
using UBPCWeb.Model;

namespace UBPCWeb.Modules.ActionedItemHistory
{
    public class CommonFunction
    {

        private static SQLDBHelper dbHelperObj = new SQLDBHelper();

        private static string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();

        public static jQueryDataTableGridModel GetListingPageData(String[] paramList)
        {
            //Param List
            //1 - Date Time, 2- severity, 3 - caller, 4 - client         
            try
            {
                DataTable mainTransDt = GetAllActionedRejTrans(paramList);

                var query = (from DataRow row in mainTransDt.Rows
                             select new ActRejDecModel
                             {
                                 TempID = row["TEMP_ID"].ToString(),
                                 BatchDirectory = row["ITM_BatchDirectory"].ToString(),
                                 BatchNo = row["REJ_BATCHNUM"].ToString(),
                                 TransNo = row["REJ_TRANSNUM"].ToString(),
                                 RejCategory = row["REJ_CATEGORY"].ToString(),
                                 Reason = row["REJ_REASON"].ToString(),
                                 PresentingBranch = row["ITM_FLD10"].ToString(),
                                 RejectTime = (DateTime)row["REJ_REJECTTIME"],
                                 Site = row["REJ_SITECODE"].ToString(),
                                 ProcMode = row["PROC_MODE"].ToString(),
                                 CanBreakdown = row["REJ_ALLOWBREAKDOWN"].ToString().Equals("1"),
                                 CanTolerate = row["REJ_ALLOWTOLERANCE"].ToString().Equals("1"),
                                 NewBundle = row["NEWBUNDLE"].ToString(),
                                 ActionStatus = row["REJ_DECISION"].ToString().Trim().Length > 0 ? (row["REJ_DECISION"].ToString().Trim().Equals("A") ? "ACCEPTED" : "REJECTED") : "AUTO REJECTED",
                                 RequiredApproval = row["REJ_REQUIRED_APPROVAL"].ToString()
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

        public static DataTable GetSelectedRejTrans(String siteDBConn, DateTime busdate, String[] filterList)
        {
            //Param List
            //1 - Batch Dir, 2- Batch No, 3 - Trans NO, 4 - New Bundle ID , 5- Site name           
            //

            IDbConnection iConn = dbHelperObj.initConnection(siteDBConn);
            string stmt = string.Empty;

            try
            {
                string filterCriteria = string.Empty;
                string batchDir = filterList[0].ToString().Trim();
                string batchNo = filterList[1].ToString().Trim();
                string transNo = filterList[2].ToString().Trim();
                string newBundleID = filterList[3].ToString().Trim();
                string siteName = filterList[4].ToString().Trim();

                //generate the sql statement here
                stmt = Resource.sqlStmtGetSelectedRejTransactionList;

                IDbDataParameter[] param =
                    new[] {
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@Busdate", ParameterDirection.Input, busdate),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNo", ParameterDirection.Input, batchNo),
                        dbHelperObj.CreateParameter(DbType.String, 10, "@TransNo", ParameterDirection.Input, transNo),
                        dbHelperObj.CreateParameter(DbType.String, 6, "@BundleId", ParameterDirection.Input,newBundleID),
                        dbHelperObj.CreateParameter(DbType.String, 3, "@Site", ParameterDirection.Input,siteName)
                    };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);

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

        public static DataTable GetAllActionedRejTrans(String[] filterList)
        {
            //Param List
            //1 - Busdate, 2- client, 3 - presenting branch, 4 - batch no, 5 - category            
            //CR015-20 add in user id for multi level filtering
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {

                string filterCriteria = string.Empty;
                string dateTime = filterList[0].ToString();
                string client = filterList[1].ToString().Equals("ALL") ? "" : filterList[1].ToString();
                string presentingBch = filterList[2].ToString().Equals("ALL") ? "" : filterList[2].ToString();
                string category = filterList[3].ToString().Equals("ALL") ? "" : filterList[3].ToString();
                string batchNo = filterList[4].ToString();

                string status = string.Empty;

                //If it's aft cut off time and it's active date, shows all, else shows only those "C" status
                //Get cut off time
                //DBConnectionInfo dbConn = new DBConnectionInfo();
                //dbConn.webConnStr = webDBConnStr;

                //string selClientDBConnStr = dbConn.GetSiteConnectionString(true, client.Trim(), dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), client.Trim()));
                string webDecCutOff = GetCutOffTime(client.Trim());/* Parameters.GetParamValue(selClientDBConnStr, Resources.Resource.cstrCutOffTime);*/

                TimeSpan cutOffTime;
                bool isCutOff = false;
                if (!TimeSpan.TryParse(webDecCutOff, out cutOffTime))
                {
                    // handle validation error
                    Logger.Write(false, LogCallerID.ActionedItemHistory, client, "View Actioned Item History", "Invalid Cut off time detected.", LogEventType.Error, HttpContext.Current.Session["s_UserID"].ToString());
                }
                else
                {
                    DateTime dateTimeNow = Convert.ToDateTime(DateTime.Now);
                    DateTime dateTimeCutOff = Convert.ToDateTime(webDecCutOff);
                    isCutOff = (dateTimeNow.TimeOfDay.Ticks > dateTimeCutOff.TimeOfDay.Ticks);
                }

                //Check if chosen date = active busdate
                DateTime curActiveBusdate = DateTime.Today;
                curActiveBusdate = Convert.ToDateTime(HttpContext.Current.Session["CurBusdate"]);
                DateTime busdate = Convert.ToDateTime(dateTime);

                if (curActiveBusdate.Date.Equals(busdate.Date))
                {
                    status = isCutOff ? "" : "C";
                }

                string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');

                //generate the sql statement here
                stmt = Resource.sqlStmtGetRejectedTransactionList;

                IDbDataParameter[] param =
                    new[] {
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BusDate", ParameterDirection.Input, sqlDateTime),
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ClientBank", ParameterDirection.Input, client),
                        dbHelperObj.CreateParameter(DbType.String, 20, "@PresentingBSB", ParameterDirection.Input, presentingBch),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNo", ParameterDirection.Input, batchNo.Trim()),

                        dbHelperObj.CreateParameter(DbType.String, 2, "@RejCategory", ParameterDirection.Input, category),
                        dbHelperObj.CreateParameter(DbType.Boolean, 0, "@ForUnisysOps", ParameterDirection.Input, Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString())),
                        dbHelperObj.CreateParameter(DbType.String, 1, "@Status", ParameterDirection.Input, status),
                        dbHelperObj.CreateParameter(DbType.String, 50, "@UserID", ParameterDirection.Input, HttpContext.Current.Session["s_UserID"].ToString())
                    };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);

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
        private static string GetCutOffTime(string client)
        {
            using (SqlConnection connection = new SqlConnection(webDBConnStr))
            {
                // Setup the SQL Command
                SqlCommand command = new SqlCommand(Resources.Resource.GetWebDecCutOFfTime, connection);
                command.CommandType = CommandType.StoredProcedure;
                // Add the search parameter key
                command.Parameters.Add("@ClientCode", SqlDbType.VarChar).Value = client;

                // Open the connection
                connection.Open();

                // Execute the Query 
                Object obj = null;
                obj = command.ExecuteScalar();

                if (obj != null && obj != DBNull.Value)
                    return obj.ToString();
            }

            return string.Empty;
        }
        public static DataTable GetPresentingBranchList(DateTime busdate, string clientCode)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                string status = "";
                stmt = Resource.sqlStmtGetBranchList;

                string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');

                #region get cut off
                //If it's aft cut off time and it's active date, shows all, else shows only those "C" status
                //Get cut off time
                //DBConnectionInfo dbConn = new DBConnectionInfo();
                //dbConn.webConnStr = webDBConnStr;
                //string selClientDBConnStr = dbConn.GetSiteConnectionString(true, clientCode.Trim(), dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim()));
                string webDecCutOff = GetCutOffTime(clientCode.Trim()); /*Parameters.GetParamValue(selClientDBConnStr, Resources.Resource.cstrCutOffTime);*/

                TimeSpan cutOffTime;
                bool isCutOff = false;
                if (!TimeSpan.TryParse(webDecCutOff, out cutOffTime))
                {
                    // handle validation error
                    Logger.Write(false, LogCallerID.ActionedItemHistory, clientCode, "View Actioned Item History", "Invalid Cut off time detected.", LogEventType.Error, HttpContext.Current.Session["s_UserID"].ToString());
                }
                else
                {
                    DateTime dateTimeNow = Convert.ToDateTime(DateTime.Now);
                    DateTime dateTimeCutOff = Convert.ToDateTime(webDecCutOff);
                    isCutOff = (dateTimeNow.TimeOfDay.Ticks > dateTimeCutOff.TimeOfDay.Ticks);
                }
                #endregion

                //Check if chosen date = active busdate
                DateTime curActiveBusdate = DateTime.Today;
                if (curActiveBusdate.Date.Equals(busdate.Date))
                {
                    status = isCutOff ? "" : "C";
                }


                IDbDataParameter[] param = new[] {
                    dbHelperObj.CreateParameter(DbType.String, 8, "@BusDate", ParameterDirection.Input, sqlDateTime),
                    dbHelperObj.CreateParameter(DbType.String, 4, "@ClientBank", ParameterDirection.Input, clientCode),
                    dbHelperObj.CreateParameter(DbType.String, 1, "@Status", ParameterDirection.Input,status),
                    dbHelperObj.CreateParameter(DbType.Boolean, 0, "@ForUnisysOps", ParameterDirection.Input, Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString()))

                };


                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);

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

        public static String GetDirectoryForIFSPath(int transportID, string siteCode)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.sqlStmtGetIISVirDirForIFSPath;
                IDbDataParameter[] param = new[] {
                        dbHelperObj.CreateParameter(DbType.Int32, 0, "@TransportID", ParameterDirection.Input, transportID),
                        dbHelperObj.CreateParameter(DbType.String, 3, "@Site", ParameterDirection.Input, siteCode)
                    };

                DataTable virDirDT = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);
                string VirDirForIFSPath = virDirDT.Rows.Count > 0 ? (virDirDT.Rows[0]["IFS_IISFullPath"].ToString().Trim()) : "";

                return VirDirForIFSPath;
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

        #region For PV Reject
        public static DataTable GetDetailItemInfo(string module, string siteConnString, string decision, string rejReason)
        {
            IDbConnection iConn = dbHelperObj.initConnection(siteConnString);
            string stmt = string.Empty;

            try
            {
                DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
                String curBatchDir = HttpContext.Current.Session["s_CurSelectedBatchDirRejDec"].ToString().Trim();
                String curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString().Trim();
                int curTransNo = Convert.ToInt32(HttpContext.Current.Session["s_CurSelectedTransNoRejDec"].ToString().Trim());
                String curPresentingBSB = HttpContext.Current.Session["s_CurSelectedPresentingRejDec"].ToString().Trim();

                stmt = Resource.sqlStmtGetActionItemDetail;

                IDbDataParameter[] param = new[] {
                        dbHelperObj.CreateParameter(DbType.String, 2, "@Module", ParameterDirection.Input, module),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchDir", ParameterDirection.Input, curBatchDir),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNum", ParameterDirection.Input, curBatchNo),
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@Busdate", ParameterDirection.Input, curBusdate),
                        dbHelperObj.CreateParameter(DbType.Int32, 0, "@TransNum", ParameterDirection.Input, curTransNo),
                        dbHelperObj.CreateParameter(DbType.String, 1, "@RejDecision", ParameterDirection.Input, decision),
                        dbHelperObj.CreateParameter(DbType.String, 50, "@OriRejReason", ParameterDirection.Input, rejReason),

                    };

                DataTable rejectedMainItemDT = dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);

                return rejectedMainItemDT;

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

        #endregion

        #region For DI Reject
        public static DataTable GetDIFormatDetail(int formatNo)
        {
            IDbConnection iConn = dbHelperObj.initConnection(HttpContext.Current.Session["selClientDBConnStr"].ToString());
            string stmt = string.Empty;

            try
            {
                DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
                String curBatchDir = HttpContext.Current.Session["s_CurSelectedBatchDirRejDec"].ToString().Trim();
                String curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString().Trim();
                int curTransNo = Convert.ToInt32(HttpContext.Current.Session["s_CurSelectedTransNoRejDec"].ToString().Trim());
                String curPresentingBSB = HttpContext.Current.Session["s_CurSelectedPresentingRejDec"].ToString().Trim();

                stmt = Resource.sqlStmtGetDIFormatDef;
                IDbDataParameter[] param = new[] {
                        dbHelperObj.CreateParameter(DbType.Int16, 0, "@FormatNo", ParameterDirection.Input, formatNo)
                    };

                DataTable DIFormatDT = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);

                return DIFormatDT;

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

        #endregion

        #region For RL Reject
        public static DataTable GetRLDetailItemInfo(string siteConnString)
        {
            IDbConnection iConn = dbHelperObj.initConnection(siteConnString);
            string stmt = string.Empty;

            try
            {
                DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
                String curBatchDir = HttpContext.Current.Session["s_CurSelectedBatchDirRejDec"].ToString().Trim();
                String curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString().Trim();
                int curTransNo = Convert.ToInt32(HttpContext.Current.Session["s_CurSelectedTransNoRejDec"].ToString().Trim());
                String curPresentingBSB = HttpContext.Current.Session["s_CurSelectedPresentingRejDec"].ToString().Trim();

                stmt = Resource.sqlStmrGetRLItemDetail;
                IDbDataParameter[] param = new[] {
                    dbHelperObj.CreateParameter(DbType.String, 3, "@Module", ParameterDirection.Input, "ACT"),
                    dbHelperObj.CreateParameter(DbType.String, 8, "@BatchDir", ParameterDirection.Input, curBatchDir),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNum", ParameterDirection.Input, curBatchNo),
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@Busdate", ParameterDirection.Input, curBusdate),
                        dbHelperObj.CreateParameter(DbType.Int32, 0, "@TransNum", ParameterDirection.Input, curTransNo)
                    };

                DataTable rejectedMainItemDT = dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);

                return rejectedMainItemDT;
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


        #endregion

    }

    public class ActRejDecModel
    {
        //TBL_REJECTITEMSTATUS
        public string TempID { get; set; }
        public DateTime RejectTime { get; set; }
        public string PresentingBranch { get; set; }
        public string BatchDirectory { get; set; }
        public string Site { get; set; }
        public string BatchNo { get; set; }
        public string TransNo { get; set; }
        public string RejCategory { get; set; }
        public string Reason { get; set; }
        public string ProcMode { get; set; }
        public bool CanBreakdown { get; set; }
        public bool CanTolerate { get; set; }
        public string ActionStatus { get; set; }
        public string NewBundle { get; set; }
        public string RequiredApproval { get; set; }

    }
}