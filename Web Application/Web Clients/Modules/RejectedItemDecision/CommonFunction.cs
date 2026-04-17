using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
//using System.Web.Security.AntiXss;
using UBPC.Web.Common;
using UBPCWeb.Model;

namespace UBPCWeb.Modules.RejectedItemDecision
{
    public class CommonFunction
    {

        private static SQLDBHelper dbHelperObj = new SQLDBHelper();

        private static string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();

        public static jQueryDataTableGridModel GetListingPageData(String[] paramList)
        {
            //Param List
            //1 - Date Time, 2- severity, 3 - caller, 4 - client , 5  - batch No        
            try
            {
                DataTable mainTransDt = GetAllRejTrans(paramList);

               // HttpContext.Current.Session["rejectedMainDT"] = mainTransDt;

                var query = (from DataRow row in mainTransDt.Rows
                             select new RejDecModel
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
                                 IsCutOff = HttpContext.Current.Session["s_IsCutOffTime"] == null ? false: Convert.ToBoolean(HttpContext.Current.Session["s_IsCutOffTime"]),
                                 RequiredApproval = row["REJ_REQUIRED_APPROVAL"].ToString(),
                                 TotalChequeAmount = String.Format("{0:N2}", Convert.ToDecimal(row["TOTAL_CHEQUE_AMOUNT"]))
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
        
        public static DataTable GetSelectedRejTrans(String siteDBConn, DateTime busdate,String[] filterList)
        {
            //Param List
            //1 - Batch Dir, 2- Batch No, 3 - Trans NO, 4 - New Bundle ID , 5- Site name,6-Required Approval           
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
        
        public static DataTable GetAllRejTrans(String[] filterList)
        {
            //Param List
            //1 - Busdate, 2- client, 3 - presenting branch, 4 - category, 5 - Batch No             
            //CR015-20 add in user id for multi level filtering
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                string filterCriteria = string.Empty;
                string dateTime = filterList[0].ToString();
                string client = filterList[1].ToString().Equals("ALL") ? "" : filterList[1].ToString();
                string presentingBch = filterList[2].ToString();
                string category = filterList[3].ToString().Equals("ALL") ? "" : filterList[3].ToString();
                string batchNo = filterList[4].ToString();

                DateTime busdate = Convert.ToDateTime(dateTime);

                string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2,'0') + busdate.Day.ToString().PadLeft(2,'0');
                               
                //generate the sql statement here
                stmt = Resource.sqlStmtGetRejectedTransactionList;// "SELECT * from TBL_LOG WITH(NOLOCK) where TimeStamp>=@startDateTime AND TimeStamp <=@endDateTime";
                                
                IDbDataParameter[] param =
                    new[] { 
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BusDate", ParameterDirection.Input, sqlDateTime),
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ClientBank", ParameterDirection.Input, client),
                        dbHelperObj.CreateParameter(DbType.String, 20, "@PresentingBSB", ParameterDirection.Input, presentingBch.Trim()),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNo", ParameterDirection.Input, batchNo.Trim()),                        
                        dbHelperObj.CreateParameter(DbType.String, 2, "@RejCategory", ParameterDirection.Input, category),
                        dbHelperObj.CreateParameter(DbType.String, 1, "@Status", ParameterDirection.Input, "W"),
                        dbHelperObj.CreateParameter(DbType.Boolean, 0, "@ForUnisysOps", ParameterDirection.Input, Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString())),
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
        
        public static bool IsTransReset()
        {
            IDbConnection iConn = dbHelperObj.initConnection(HttpContext.Current.Session["selClientDBConnStr"].ToString());
            string stmt = string.Empty;
            bool isBeingReset = false;
            
            try
            {
                DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
                String curBatchDir = HttpContext.Current.Session["s_CurSelectedBatchDirRejDec"].ToString().Trim();
                String curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString().Trim();
                int curTransNo = Convert.ToInt32(HttpContext.Current.Session["s_CurSelectedTransNoRejDec"].ToString().Trim());
                String curNewBundleID = HttpContext.Current.Session["s_CurSelectedNewBundleRejDec"].ToString().Trim();

                stmt = Resource.sqlStmtGetResetItem;
                IDbDataParameter[] param = new[] { 
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@Busdate", ParameterDirection.Input, curBusdate),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNum", ParameterDirection.Input, curBatchNo),
                        dbHelperObj.CreateParameter(DbType.Int16, 0, "@TransNum", ParameterDirection.Input, curTransNo),
                        dbHelperObj.CreateParameter(DbType.String, 6, "@NewBundleID", ParameterDirection.Input, curNewBundleID) 
                    };

                SqlDataReader readerStatus = dbHelperObj.executeReader(iConn, CommandType.Text, stmt, param);
                if (readerStatus.Read()) 
                {
                    isBeingReset = readerStatus[0] != null ? Convert.ToInt16(readerStatus[0].ToString()) > 0 : false;
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
                string ifsPath = virDirDT.Rows.Count > 0 ? (virDirDT.Rows[0]["IFS_IISFullPath"].ToString().Trim()) : "";

                return ifsPath;
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

        public static bool checkCompletedTrans(string operatorID, DateTime Busdate, string batchNo, int RLTransNo)
        {
            bool match = false;
            IDbConnection iConn = dbHelperObj.initConnection(HttpContext.Current.Session["selClientDBConnStr"].ToString());
            string stmt = string.Empty;
            
            try
            {
                String curNewBundleID = HttpContext.Current.Session["s_CurSelectedNewBundleRejDec"].ToString().Trim();

                stmt = Resource.sqlStmtCheckCompletedInUseTrans;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String,8, "@OperID", ParameterDirection.Input,operatorID.Trim()),
                    dbHelperObj.CreateParameter(DbType.DateTime, 0, "@CurBusDate", ParameterDirection.Input,Busdate),
                    dbHelperObj.CreateParameter(DbType.String,8, "@BatchNum", ParameterDirection.Input,batchNo.Trim()),                    
                    dbHelperObj.CreateParameter(DbType.Int32,0, "@TransNo", ParameterDirection.Input,RLTransNo),
                    dbHelperObj.CreateParameter(DbType.String, 6, "@NewBundleID", ParameterDirection.Input, curNewBundleID) 
                };

                int recChanged = (int)dbHelperObj.executeScalar(iConn, CommandType.Text, stmt, param);

                if (recChanged > 0)
                    match = true;

                return match;
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
    
        public static int UpdateTransactionStatus(string clientCode, string status, string operatorID, DateTime Busdate, string batchNo, int RLTransNo)
        {
            int recAffected = 0;

            IDbConnection iConn = dbHelperObj.initConnection(HttpContext.Current.Session["selClientDBConnStr"].ToString());
            string stmt = string.Empty;


            try
            {
                stmt = Resource.sqlStmtUpdTransactionStatus;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 1, "@status", ParameterDirection.Input,status),
                    dbHelperObj.CreateParameter(DbType.String,8, "@OperID", ParameterDirection.Input,operatorID.Trim()),
                    dbHelperObj.CreateParameter(DbType.DateTime, 0, "@CurBusDate", ParameterDirection.Input,Busdate),
                    dbHelperObj.CreateParameter(DbType.String,8, "@BatchNum", ParameterDirection.Input,batchNo),                    
                    dbHelperObj.CreateParameter(DbType.Int32,0, "@TransNo", ParameterDirection.Input,RLTransNo),
                    dbHelperObj.CreateParameter(DbType.String,8, "@BundleID", ParameterDirection.Input, HttpContext.Current.Session["s_CurSelectedNewBundleRejDec"].ToString())
                };

                recAffected = dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);

                if (recAffected > 0)
                {
                    //Log the Batch Status Update Details
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.RejectedItemDecision;
                    log.ClientCode = clientCode;
                    log.UserName = operatorID;
                    log.Severity = LogEventType.Information;
                    log.Message = string.Format(Resource.msgTransStatusChanged, status);
                    //Business Date: {0}; Batch No: {1}; RL Trans No: {2}
                    log.Data = String.Format(Resource.msgTransMaintData, Busdate.ToShortDateString(), batchNo,RLTransNo.ToString());
                    log.Write();
                }
                else
                {
                    string msg = string.Format("The transaction {0} is being used by another user. Please try again later", RLTransNo.ToString());
                    //Log the Item Rej Details
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.RejectedItemDecision;
                    log.ClientCode = clientCode;
                    log.UserName = operatorID;
                    log.Severity = LogEventType.Information;
                    log.Message = msg;
                    //Business Date: {0}; Batch No: {1}; RL Trans No: {2}
                    log.Data = String.Format(Resource.msgTransMaintData, Busdate.ToShortDateString(), batchNo, RLTransNo.ToString());
                    log.Write();

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

        public static int ResetTransactionStatus(string clientCode, string status, string operatorID, DateTime Busdate, string batchNo, int RLTransNo)
        {
            int recAffected = 0;

            IDbConnection iConn = dbHelperObj.initConnection(HttpContext.Current.Session["selClientDBConnStr"].ToString());
            string stmt = string.Empty;


            try
            {
                stmt = Resource.sqlStmtResetTransactionStatus;
                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String,8, "@OperID", ParameterDirection.Input,operatorID.Trim()),
                    dbHelperObj.CreateParameter(DbType.DateTime, 0, "@CurBusDate", ParameterDirection.Input,Busdate),
                    dbHelperObj.CreateParameter(DbType.String,8, "@BatchNum", ParameterDirection.Input,batchNo),                    
                    dbHelperObj.CreateParameter(DbType.Int32,0, "@TransNo", ParameterDirection.Input,RLTransNo),
                    dbHelperObj.CreateParameter(DbType.String,6, "@BundleID", ParameterDirection.Input, HttpContext.Current.Session["s_CurSelectedNewBundleRejDec"].ToString())
                };

                recAffected = dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);

                if (recAffected > 0)
                {
                    //Log the Batch Status Update Details
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.RejectedItemDecision;
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
                    string msg = string.Format("Back to listing failed at reset transaction {0}. Please try again later", RLTransNo.ToString());
                    //Log the Item Rej Details
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.RejectedItemDecision;
                    log.ClientCode = clientCode;
                    log.UserName = operatorID;
                    log.Severity = LogEventType.Information;
                    log.Message = msg;
                    //Business Date: {0}; Batch No: {1}; RL Trans No: {2}
                    log.Data = String.Format(Resource.msgTransMaintData, Busdate.ToShortDateString(), batchNo, RLTransNo.ToString());
                    log.Write();

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
      
        public static int CompleteTransactionStatus(string clientCode, string operatorID, DateTime Busdate, string batchNo, int RLTransNo, string rejDec, string rejRemark)
        {
            int recAffected = 0;

            IDbConnection iConn = dbHelperObj.initConnection(HttpContext.Current.Session["selClientDBConnStr"].ToString());
            string stmt = string.Empty;
            
            try
            {
                string newBundleID = HttpContext.Current.Session["s_CurSelectedNewBundleRejDec"] == null ? "" : HttpContext.Current.Session["s_CurSelectedNewBundleRejDec"].ToString();
                stmt = Resource.sqlStmtCompleteTranStatus;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String,8, "@OperID", ParameterDirection.Input,operatorID.Trim()),
                    dbHelperObj.CreateParameter(DbType.DateTime, 0, "@CurBusDate", ParameterDirection.Input,Busdate),
                    dbHelperObj.CreateParameter(DbType.String,8, "@BatchNum", ParameterDirection.Input,batchNo),                    
                    dbHelperObj.CreateParameter(DbType.Int32,0, "@TransNo", ParameterDirection.Input,RLTransNo),
                    dbHelperObj.CreateParameter(DbType.String,1, "@RejDecision", ParameterDirection.Input,rejDec),    
                    dbHelperObj.CreateParameter(DbType.String,0, "@Remark", ParameterDirection.Input,rejRemark),
                    dbHelperObj.CreateParameter(DbType.String,6, "@NewBundle", ParameterDirection.Input,newBundleID)                       
                };

                recAffected = dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);

                if (recAffected > 0)
                {
                    //Log the Batch Status Update Details
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.RejectedItemDecision;
                    log.ClientCode = clientCode;
                    log.UserName = operatorID;
                    log.Severity = LogEventType.Information;
                    log.Message = string.Format(Resource.msgTransStatusChanged, "C");
                    //Business Date: {0}; Batch No: {1}; RL Trans No: {2}
                    log.Data = String.Format(Resource.msgTransMaintData, Busdate.ToShortDateString(), batchNo, RLTransNo.ToString());
                    log.Write();
                }
                else
                {
                    string msg = string.Format("Update transaction {0} completed failed. Please try again later", RLTransNo.ToString());
                    //Log the Item Rej Details
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.RejectedItemDecision;
                    log.ClientCode = clientCode;
                    log.UserName = operatorID;
                    log.Severity = LogEventType.Information;
                    log.Message = msg;
                    //Business Date: {0}; Batch No: {1}; RL Trans No: {2}
                    log.Data = String.Format(Resource.msgTransMaintData, Busdate.ToShortDateString(), batchNo, RLTransNo.ToString());
                    log.Write();

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

        //Accept or Reject Transaction
        public static void UpdatedAcceptedItems(string siteConnString, string rejRemark, string DIPaymentType, string module)
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

                String userID = HttpContext.Current.Session["s_UserID"].ToString().Trim();

                stmt = Resource.sqlStmtUpdAcceptedTrans;
                IDbDataParameter[] param = new[] { 
                        dbHelperObj.CreateParameter(DbType.String, 3, "@Module", ParameterDirection.Input, module),
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@BusDate", ParameterDirection.Input, curBusdate),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchDir", ParameterDirection.Input, curBatchDir),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNum", ParameterDirection.Input, curBatchNo),
                        dbHelperObj.CreateParameter(DbType.Int32, 0, "@TransID", ParameterDirection.Input, curTransNo),
                        dbHelperObj.CreateParameter(DbType.String, 50, "@OperID", ParameterDirection.Input, userID),
                        dbHelperObj.CreateParameter(DbType.String, 1, "@RejDecision", ParameterDirection.Input, "A"),
                        dbHelperObj.CreateParameter(DbType.String, 0, "@RejRemark", ParameterDirection.Input, rejRemark),
                        dbHelperObj.CreateParameter(DbType.String, 2, "@DIPaymentType", ParameterDirection.Input, DIPaymentType)
                    };

                    dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);

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

        //CR015-20 Multi level review decision update
        public static void UpdateMultiLevelDecision(string siteConnString, string rejRemark, string decision, string level,bool routeToFinal)
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

                String userID = HttpContext.Current.Session["s_UserID"].ToString().Trim();

                stmt = Resource.sqlStmtUpdMultiLevelDecision;
                IDbDataParameter[] param = new[] {
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@BusDate", ParameterDirection.Input, curBusdate),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchDir", ParameterDirection.Input, curBatchDir),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNum", ParameterDirection.Input, curBatchNo),
                        dbHelperObj.CreateParameter(DbType.Int32, 0, "@TransID", ParameterDirection.Input, curTransNo),
                        dbHelperObj.CreateParameter(DbType.String, 50, "@OperID", ParameterDirection.Input, userID),
                        dbHelperObj.CreateParameter(DbType.String, 1, "@RejDecision", ParameterDirection.Input, decision),
                        dbHelperObj.CreateParameter(DbType.Boolean, 1, "@RouteToFinal", ParameterDirection.Input, routeToFinal),
                        dbHelperObj.CreateParameter(DbType.String, 1, "@DecisionLevel", ParameterDirection.Input, level),
                        dbHelperObj.CreateParameter(DbType.String, 0, "@RejRemark", ParameterDirection.Input, rejRemark),
                    };

                dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);

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
        public static DataTable GetDetailItemInfo(string module, string siteConnString) 
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

                stmt = Resource.sqlStmtGetPVItemDetail;
                IDbDataParameter[] param = new[] { 
                        dbHelperObj.CreateParameter(DbType.String, 2, "@Module", ParameterDirection.Input, module),
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
        public static Decimal SetRLAmount(string amountText) 
        {
            decimal RLAmt = 0;
            string rlAmtInText = amountText.Replace(".", "00");

            return (Convert.ToDecimal(rlAmtInText) / 100);
        }

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
                    dbHelperObj.CreateParameter(DbType.String, 3, "@Module", ParameterDirection.Input, "REJ"),
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

        public static String GetOtherStubAccountNo(string siteConnString, string selDinNo)
        {
            IDbConnection iConn = dbHelperObj.initConnection(siteConnString);
            string stmt = string.Empty;

            try
            {
                DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
                String curBatchDir = HttpContext.Current.Session["s_CurSelectedBatchDirRejDec"].ToString().Trim();
                String curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString().Trim();

                stmt = Resource.sqlStmtGetOtherStubAcctNo;
                IDbDataParameter[] param = new[] { 
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@Busdate", ParameterDirection.Input, curBusdate),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchDir", ParameterDirection.Input, curBatchDir),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNum", ParameterDirection.Input, curBatchNo),
                        dbHelperObj.CreateParameter(DbType.Int64, 0, "@Din", ParameterDirection.Input, Convert.ToInt64(selDinNo.ToString().Trim()))
                    };

                SqlDataReader readerStatus = dbHelperObj.executeReader(iConn, CommandType.Text, stmt, param);
                string othAcctNo = string.Empty;
                if (readerStatus.Read())
                {
                    othAcctNo = readerStatus[0] == null ? string.Empty: readerStatus[0].ToString().Trim();
                }

                return othAcctNo;
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

        //InsertStubItems
        public static void InsertStubItems(DataRow dr)
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

                String userID = HttpContext.Current.Session["s_UserID"].ToString().Trim();


                stmt = Resource.sqlStmtInsNewStubItem;
                IDbDataParameter[] param = new[] { 
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@Busdate", ParameterDirection.Input, curBusdate),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchDir", ParameterDirection.Input, curBatchDir),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNum", ParameterDirection.Input, curBatchNo),
                         dbHelperObj.CreateParameter(DbType.String, 3, "@WsID", ParameterDirection.Input, dr["ITM_WsID"].ToString().Trim()),
                        dbHelperObj.CreateParameter(DbType.Int32, 0, "@TransNum", ParameterDirection.Input, curTransNo),                        
                        dbHelperObj.CreateParameter(DbType.Int64, 0, "@DinNo", ParameterDirection.Input,Convert.ToInt64(dr["ITM_DIN"].ToString().Trim())),
                        dbHelperObj.CreateParameter(DbType.Decimal, 0, "@Amount", ParameterDirection.Input, dr["ITM_AMOUNT"].ToString().Trim()),                        
                        dbHelperObj.CreateParameter(DbType.String, 20, "@DepAcctNo", ParameterDirection.Input, dr["ITM_FLD12"].ToString().Trim()),
                        dbHelperObj.CreateParameter(DbType.Int16, 0, "@TranSeqNo", ParameterDirection.Input, dr["ITM_TransSeqNum"].ToString().Trim())

                    };

                dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);

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

        public static void UpdatedAmendedItems(DataRow dr)
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

                String userID = HttpContext.Current.Session["s_UserID"].ToString().Trim();

                stmt = Resource.sqlStmtUpdRLAmendedItem;
                IDbDataParameter[] param = new[] { 
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@BusDate", ParameterDirection.Input, curBusdate),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchDir", ParameterDirection.Input, curBatchDir),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNum", ParameterDirection.Input, curBatchNo),
                        dbHelperObj.CreateParameter(DbType.Int32, 0, "@TransID", ParameterDirection.Input, curTransNo),                        
                        dbHelperObj.CreateParameter(DbType.Int64, 0, "@DinNo", ParameterDirection.Input,Convert.ToInt64(dr["ITM_DIN"].ToString().Trim())),
                        dbHelperObj.CreateParameter(DbType.Decimal, 0, "@UpdAmount", ParameterDirection.Input, dr["ITM_AMOUNT"].ToString().Trim()),
                        dbHelperObj.CreateParameter(DbType.String, 20, "@UpdBSB", ParameterDirection.Input, dr["ITM_FLD4"].ToString().Trim()),
                        dbHelperObj.CreateParameter(DbType.String, 20, "@UpdAcctNo", ParameterDirection.Input, dr["ITM_FLD12"].ToString().Trim()),
                        dbHelperObj.CreateParameter(DbType.Int16, 0, "@TranSeqNo", ParameterDirection.Input, dr["ITM_TransSeqNum"].ToString().Trim())

                    };

                dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);

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

        public static int UpdateRejectedRLItem(string clientCode, string operatorID)
        {
            int recAffected = 0;

            IDbConnection iConn = dbHelperObj.initConnection(HttpContext.Current.Session["selClientDBConnStr"].ToString());
            string stmt = string.Empty;
            DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
            String curBatchDir = HttpContext.Current.Session["s_CurSelectedBatchDirRejDec"].ToString().Trim();
            String curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString().Trim();
            int curTransNo = Convert.ToInt32(HttpContext.Current.Session["s_CurSelectedTransNoRejDec"].ToString().Trim());


            try
            {
                stmt = Resource.sqlStmtUpdRejectRLItems;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.DateTime, 0, "@Busdate", ParameterDirection.Input,curBusdate),
                    dbHelperObj.CreateParameter(DbType.String,8, "@BatchDir", ParameterDirection.Input,curBatchDir),  
                    dbHelperObj.CreateParameter(DbType.String,8, "@BatchNo", ParameterDirection.Input,curBatchNo),                    
                    dbHelperObj.CreateParameter(DbType.Int32,0, "@TransNo", ParameterDirection.Input,curTransNo)
                };

                recAffected = dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);

                if (recAffected > 0)
                {
                }
                else
                {
                    string msg = string.Format("Reject Transaction Failed. Please try again later", curTransNo.ToString());
                    //Log the Item Rej Details
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.RejectedItemDecision;
                    log.ClientCode = clientCode;
                    log.UserName = operatorID;
                    log.Severity = LogEventType.Information;
                    log.Message = msg;
                    //Business Date: {0}; Batch No: {1}; RL Trans No: {2}
                    log.Data = String.Format(Resource.msgTransMaintData, curBusdate.ToShortDateString(), curBatchNo, curTransNo.ToString());
                    log.Write();

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

        public static void UpdatedAcceptedItemsRLOnly(string siteConnString, string rejRemark, bool isMultipleStub, bool isBPC)
        {
            IDbConnection iConn = dbHelperObj.initConnection(siteConnString);
            string stmt = string.Empty;

            try
            {
                DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
                String curBatchDir = HttpContext.Current.Session["s_CurSelectedBatchDirRejDec"].ToString().Trim();
                String curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString().Trim();
                String curWsid = HttpContext.Current.Session["s_CurSelectedWsIDRejDec"].ToString().Trim();

                int curTransNo = Convert.ToInt32(HttpContext.Current.Session["s_CurSelectedTransNoRejDec"].ToString().Trim());
                String curPresentingBSB = HttpContext.Current.Session["s_CurSelectedPresentingRejDec"].ToString().Trim();

                String userID = HttpContext.Current.Session["s_UserID"].ToString().Trim();

                stmt = Resource.sqlStmtUpdAcceptedTransRLOnly;
                IDbDataParameter[] param = new[] { 
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@BusinessDate", ParameterDirection.Input, curBusdate),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchDirectory", ParameterDirection.Input, curBatchDir),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNumber", ParameterDirection.Input, curBatchNo),
                        dbHelperObj.CreateParameter(DbType.Int32, 0, "@TransNumber", ParameterDirection.Input, curTransNo),
                        dbHelperObj.CreateParameter(DbType.Boolean, 0, "@MultipleStub", ParameterDirection.Input, (isMultipleStub?0:1)),                        
                        dbHelperObj.CreateParameter(DbType.String, 50, "@OperID", ParameterDirection.Input, userID),
                        dbHelperObj.CreateParameter(DbType.String, 1, "@RejDecision", ParameterDirection.Input, "A"),
                        dbHelperObj.CreateParameter(DbType.String, 0, "@RejRemark", ParameterDirection.Input, rejRemark),
                        dbHelperObj.CreateParameter(DbType.Boolean, 0, "@forPosting", ParameterDirection.Input, isBPC? 1:0),
                        dbHelperObj.CreateParameter(DbType.String, 3, "@WsID", ParameterDirection.Input, curWsid)
                };

                dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);

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

        public static void UpdateReviewerResult(string siteConnString)
        {
            IDbConnection iConn = dbHelperObj.initConnection(siteConnString);
            string stmt = string.Empty;

            try
            {
                DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
                String curBatchDir = HttpContext.Current.Session["s_CurSelectedBatchDirRejDec"].ToString().Trim();
                String curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString().Trim();
                String curWsid = HttpContext.Current.Session["s_CurSelectedWsIDRejDec"].ToString().Trim();

                int curTransNo = Convert.ToInt32(HttpContext.Current.Session["s_CurSelectedTransNoRejDec"].ToString().Trim());

                String userID = HttpContext.Current.Session["s_UserID"].ToString().Trim();

                stmt = Resource.sqlStmtUpdAcceptedTransRLOnly;
                IDbDataParameter[] param = new[] {
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@BusinessDate", ParameterDirection.Input, curBusdate),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchDirectory", ParameterDirection.Input, curBatchDir),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNumber", ParameterDirection.Input, curBatchNo),
                        dbHelperObj.CreateParameter(DbType.Int32, 0, "@TransNumber", ParameterDirection.Input, curTransNo),
                        dbHelperObj.CreateParameter(DbType.String, 50, "@OperID", ParameterDirection.Input, userID)
                };

                dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);

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

        public static Int64 GetMaxDinNoPerBatch()
        {
            IDbConnection iConn = dbHelperObj.initConnection(HttpContext.Current.Session["selClientDBConnStr"].ToString());
            string stmt = string.Empty;

            DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
            String curBatchDir = HttpContext.Current.Session["s_CurSelectedBatchDirRejDec"].ToString().Trim();
            String curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString().Trim();

            try
            {
                stmt = Resource.sqlStmtGetMaxDinNo;
                IDbDataParameter[] param = new[] { 
                        dbHelperObj.CreateParameter(DbType.DateTime, 0, "@Busdate", ParameterDirection.Input, curBusdate),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchDir", ParameterDirection.Input, curBatchDir),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNum", ParameterDirection.Input, curBatchNo)
                    };

                DataTable maxDinDT = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);
                Int64 maxDinNoPerBatch = maxDinDT.Rows.Count > 0 ? Convert.ToInt64((maxDinDT.Rows[0][0].ToString().Trim())) : 0;

                return maxDinNoPerBatch;
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

    public class RejDecModel
    {
        //TBL_REJECTITEMSTATUS
        public string TempID { get; set; }
        public DateTime RejectTime { get; set; }
        public string PresentingBranch { get; set; }
        public string BatchDirectory { get; set; }
        public string Site { get; set; }
        public string NewBundle { get; set; }
        public string BatchNo { get; set; }
        public string TransNo { get; set; }
        public string RejCategory { get; set; }
        public string Reason { get; set; }
        public string ProcMode { get; set; }
        public bool CanBreakdown { get; set; }
        public bool CanTolerate { get; set; }
        public bool IsCutOff { get; set; }
        public string RequiredApproval { get; set; }
        public string TotalChequeAmount { get; set; }
    }
}