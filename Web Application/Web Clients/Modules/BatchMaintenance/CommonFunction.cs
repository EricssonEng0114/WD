using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using UBPC.Web.Common;
using UBPCWeb.Model;


namespace UBPCWeb.Modules.BatchMaintenance
{
    public class CommonFunction
    {          
       // public static string selClientDBConnStr = string.Empty;
        private static SQLDBHelper dbHelperObj = new SQLDBHelper();

        private static string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();

        public static jQueryDataTableGridModel GetListingPageData(String[] paramList)
        {
            //Param List
            //1 - Date Time, 2- severity, 3 - caller, 4 - client         
            try
            {
                DataTable mainTransDt = GetAllRejTrans(paramList);

                var query = (from DataRow row in mainTransDt.Rows
                             select new BatchMaintModel
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
                                 InUsedBy =  row["REJ_ACTIONEDBY"] == null? "": row["REJ_ACTIONEDBY"].ToString().Trim()

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


        public static DataTable GetAllRejTrans(String[] filterList)
        {
            //Param List
            //1 - Busdate, 2- client, 3 - presenting branch, 4 - category , 5 - batch no      
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

                DateTime busdate = Convert.ToDateTime(dateTime);

                string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');

                //generate the sql statement here
                stmt = Resource.sqlStmtGetRejectedTransactionList;// "SELECT * from TBL_LOG WITH(NOLOCK) where TimeStamp>=@startDateTime AND TimeStamp <=@endDateTime";

                IDbDataParameter[] param =
                    new[] { 
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BusDate", ParameterDirection.Input, sqlDateTime),
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ClientBank", ParameterDirection.Input, client),
                        dbHelperObj.CreateParameter(DbType.String, 20, "@PresentingBSB", ParameterDirection.Input, presentingBch.Trim()),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNo", ParameterDirection.Input, batchNo.Trim()),   
                        dbHelperObj.CreateParameter(DbType.String, 2, "@RejCategory", ParameterDirection.Input, category),
                        dbHelperObj.CreateParameter(DbType.String, 1, "@Status", ParameterDirection.Input, "I"),
                        dbHelperObj.CreateParameter(DbType.Boolean, 0, "@ForUnisysOps", ParameterDirection.Input, Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString())),
                        dbHelperObj.CreateParameter(DbType.String, 0, "@UserID", ParameterDirection.Input, HttpContext.Current.Session["s_UserID"].ToString())
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
     

        public static DataTable GetPresentingBranchList(string clientCode)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;
            string mainSite = new DBConnectionInfo().GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim());

            try
            {
                stmt = Resource.sqlStmtGetBranchList;
                IDbDataParameter[] param = new[] { 
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ClientBank", ParameterDirection.Input, clientCode),
                        dbHelperObj.CreateParameter(DbType.String, 3, "@Site", ParameterDirection.Input,mainSite),
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

        public static bool IsTransReset()
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;
            bool isBeingReset = false;

            try
            {
                DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
                String curBatchDir = HttpContext.Current.Session["s_CurSelectedBatchDirRejDec"].ToString().Trim();
                String curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString().Trim();
                int curTransNo = Convert.ToInt32(HttpContext.Current.Session["s_CurSelectedTransNoRejDec"].ToString().Trim());


                stmt = Resource.sqlStmtGetResetItem;
                IDbDataParameter[] param = new[] { 
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@Busdate", ParameterDirection.Input, curBusdate),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNum", ParameterDirection.Input, curBatchNo),
                        dbHelperObj.CreateParameter(DbType.Int16, 0, "@TransNum", ParameterDirection.Input, curTransNo)
                    };

                SqlDataReader readerStatus = dbHelperObj.executeReader(iConn, CommandType.Text, stmt, param);
                if (readerStatus.Read())
                {
                    isBeingReset = readerStatus[0] != null ? Convert.ToBoolean(readerStatus[0].ToString().Trim()) : false;
                }

                return isBeingReset;
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
             
        public static int ResetTransactionStatus(string clienctDBConnStr, string clientCode, string status, string operatorID, DateTime Busdate, string batchNo, int RLTransNo, string bundleID)
        {
            int recAffected = 0;

            IDbConnection iConn = dbHelperObj.initConnection(clienctDBConnStr);
            string stmt = string.Empty;


            try
            {
                stmt = Resource.sqlStmtResetTransactionStatus;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String,8, "@OperID", ParameterDirection.Input,operatorID),
                    dbHelperObj.CreateParameter(DbType.DateTime, 0, "@CurBusDate", ParameterDirection.Input,Busdate),
                    dbHelperObj.CreateParameter(DbType.String,8, "@BatchNum", ParameterDirection.Input,batchNo),                    
                    dbHelperObj.CreateParameter(DbType.Int32,0, "@TransNo", ParameterDirection.Input,RLTransNo),
                    dbHelperObj.CreateParameter(DbType.String,6, "@BundleID", ParameterDirection.Input,bundleID)
                };

                recAffected = dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);

                if (recAffected > 0)
                {
                    //Log the Batch Status Update Details
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.BatchMaintenance;
                    log.ClientCode = clientCode;
                    log.UserName = operatorID;
                    log.Severity = LogEventType.Information;
                    log.Message = string.Format(Resource.msgTransStatusChanged, status);
                    //Business Date: {0}; Batch No: {1}; RL Trans No: {2}
                    log.Data = String.Format(Resource.msgTransMaintData, Busdate.ToShortDateString(), batchNo, RLTransNo.ToString());
                    log.Write();
                }
                else
                {
                    return 0;
                }

                return recAffected;
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

        public static int ResetTransactionUponLogout(string clientConn, string operatorID, string clientCode)
        {
            int recAffected = 0;

            IDbConnection iConn = dbHelperObj.initConnection(clientConn);
            string stmt = string.Empty;
            
            try
            {
                //DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
                //String curBatchDir = HttpContext.Current.Session["s_CurSelectedBatchDirRejDec"].ToString().Trim();
                //String curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString().Trim();
                //String curTranNo = HttpContext.Current.Session["s_CurSelectedTransNoRejDec"].ToString().Trim();
                //String clientCode = HttpContext.Current.Session["s_CurSelectedClientRejDec"].ToString().Trim();

                stmt = Resource.stmtResetTransUponLogout;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String,8, "@OperID", ParameterDirection.Input,operatorID)
                };

                recAffected = dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);

                if (recAffected > 0)
                {
                    //Log the Batch Status Update Details
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.BatchMaintenance;
                    log.ClientCode = clientCode;
                    log.UserName = operatorID;
                    log.Severity = LogEventType.Information;
                    log.Message = "Timeout: Transaction status reset back to W";// string.Format(Resource.msgTransStatusChanged, "W");
                    //Business Date: {0}; Batch No: {1}; RL Trans No: {2}
                   // log.Data = String.Format(Resource.msgTransMaintData, curBusdate.ToShortDateString(), curBatchNo, curTranNo);
                    log.Write();
                }
                else
                {
                    return 0;
                }

                return recAffected;
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
        
        public static string RetrieveDBLatestStatus(String clientDBConn,String clientCode, DateTime busdate, string batchNo, int transNo, string newBundleID)
        {
            IDbConnection iConn = dbHelperObj.initConnection(clientDBConn);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtGetREJLatestStatus;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.DateTime, 0, "@Busdate", ParameterDirection.Input,busdate),
                    dbHelperObj.CreateParameter(DbType.String,8, "@BatchNum", ParameterDirection.Input,batchNo),                    
                    dbHelperObj.CreateParameter(DbType.Int32,0, "@TransNo", ParameterDirection.Input,transNo),
                    dbHelperObj.CreateParameter(DbType.String,6, "@BundleID", ParameterDirection.Input,newBundleID)

                };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);
                string latestStatus = string.Empty;
                if (dtDB.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(dtDB.Rows[0][0].ToString()))
                    {
                        latestStatus = dtDB.Rows[0][0].ToString();
                    }
                }

                return latestStatus;
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

    public class BatchMaintModel
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
        public string InUsedBy { get; set; }
        public string NewBundle { get; set; }
    }
}