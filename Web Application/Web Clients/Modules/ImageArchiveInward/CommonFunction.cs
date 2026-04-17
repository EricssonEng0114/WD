using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
//using System.Web.Security.AntiXss;
using UBPC.Web.Common;
using UBPCWeb.Model;

namespace UBPCWeb.Modules.ImageArchiveInward
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
                DataTable mainTransDt = GetAllImageArchiveInward(paramList, "L");//Listing data
                DataTable totalRecord = GetAllImageArchiveInward(paramList, "C");//Total Count
                int totalCount = 0;
                if(totalRecord.Rows.Count > 0)
                {
                    totalCount = Convert.ToInt32(totalRecord.Rows[0][0]);
                }

                // HttpContext.Current.Session["rejectedMainDT"] = mainTransDt;

                var query = (from DataRow row in mainTransDt.Rows
                             select new RejDecModel
                             {
                                 TempID = row["TEMP_ID"].ToString(),
                                 date = string.Format("{0:yyyyMMdd}", row["IMG_BusDate"]),
                                 BusDate = (DateTime)row["IMG_BusDate"],
                                 BatchNo = row["IMG_BatchNum"].ToString(),
                                 IssuingBankType = row["IMG_IssuingBankType"].ToString(),
                                 IssuingBank = row["IMG_IssuingBank"].ToString(),
                                 IssuingBranch = row["IMG_IssuingBank"].ToString() + row["IMG_IssuingBranch"].ToString(),
                                 CheckNo = row["IMG_CheckNo"].ToString(),
                                 CheckDigit = row["IMG_CheckDigit"].ToString(),
                                 TRCode = row["IMG_TRCode"].ToString(),
                                 AcctNo = row["IMG_AcctNo"].ToString(),
                                 Amount = String.Format("{0:N2}", Convert.ToDecimal(row["IMG_Amount"])),
                                 NCF = row["IMG_NCF"].ToString(),
                                 UIC = row["IMG_UIC"].ToString(),
                                 ReturnCount = row["IMG_ReturnCount"].ToString(),
                                 TransactionType = row["IMG_TransactionType"].ToString(),
                                 ImageFolder = row["IMG_ImageFolder"].ToString()

                             });

                
                var gridData = new jQueryDataTableGridModel()
                {
                    aaData = query.ToList(),
                    iTotalDisplayRecords = totalCount,
                    iTotalRecords = totalCount
                };

                return gridData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable GetAllImageArchiveInward(String[] filterList, string flag)
        {
            //Param List
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            #region initialize
            string stmt = string.Empty;
            string filterCriteria = string.Empty;
            string dateTime = string.Empty;
            //string dateTimeTo = string.Empty;
            string amount = string.Empty;
            string operate = string.Empty;
            string clientCode = string.Empty;
            string micrAccNum = string.Empty;
            string chequeNum = string.Empty;
            string presentingBSB = string.Empty;

            string currentPage = string.Empty;
            string valueToDisplay = string.Empty;
            string selectedOrder = string.Empty;
            string ascDescOrder = string.Empty;
            string otherOrder = string.Empty;
            #endregion

            try
            {
                #region Assign Value to Session
                HttpContext.Current.Session["ImgArcInwardDateTime"] = filterList[0].ToString();
                //HttpContext.Current.Session["ImgArcInwardDateTimeTo"] = filterList[1].ToString();
                HttpContext.Current.Session["ImgArcInwardAmount"] = filterList[1].ToString();
                HttpContext.Current.Session["ImgArcInwardOperate"] = filterList[2].ToString();
                HttpContext.Current.Session["ImgArcInwardClientCode"] = filterList[3].ToString();
                HttpContext.Current.Session["ImgArcInwardMicrAccNum"] = filterList[4].ToString();
                HttpContext.Current.Session["ImgArcInwardChequeNum"] = filterList[5].ToString();
                HttpContext.Current.Session["ImgArcInwardPresentingBSB"] = filterList[6].ToString();

                HttpContext.Current.Session["ImgArcInwardCurrentPage"] = filterList[7].ToString();
                HttpContext.Current.Session["ImgArcInwardValueToDisplay"] = filterList[8].ToString();
                HttpContext.Current.Session["ImgArcInwardSelectedOrder"] = filterList[9].ToString();
                HttpContext.Current.Session["ImgArcInwardAscDescOrder"] = filterList[10].ToString();
                #endregion

                switch (filterList[9].ToString())
                {
                    case "0":
                        selectedOrder = "ITM_BatchNum";
                        otherOrder = "ITM_BusDate,ITM_CheckNo";
                        //otherOrder = "ITM_BusDate,ITM_CheckNo,ITM_IssuingBank,ITM_AcctNo,ITM_TRCode,ITM_Amount";
                        break;
                    case "1":
                        selectedOrder = "ITM_CheckNo";
                        otherOrder = "ITM_BusDate,ITM_BatchNum";
                        //otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_IssuingBank,ITM_AcctNo,ITM_TRCode,ITM_Amount";
                        break;
                    case "2":
                        selectedOrder = "BankBranch";
                        otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_CheckNo";
                        //otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_CheckNo,ITM_AcctNo,ITM_TRCode,ITM_Amount";
                        break;
                    case "3":
                        selectedOrder = "ITM_AcctNo";
                        otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_CheckNo";
                        //otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_CheckNo,ITM_IssuingBank,ITM_TRCode,ITM_Amount";
                        break;
                    case "4":
                        selectedOrder = "ITM_TRCode";
                        otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_CheckNo";
                        //otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_CheckNo,ITM_IssuingBank,ITM_AcctNo,ITM_Amount";
                        break;
                    case "5":
                        selectedOrder = "ITM_Amount";
                        otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_CheckNo";
                        //otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_CheckNo,ITM_IssuingBank,ITM_AcctNo,ITM_TRCode";
                        break;
                }

                #region assign value
                dateTime = HttpContext.Current.Session["ImgArcInwardDateTime"].ToString();
                //dateTimeTo = HttpContext.Current.Session["ImgArcInwardDateTimeTo"].ToString();
                amount = HttpContext.Current.Session["ImgArcInwardAmount"].ToString();
                operate = HttpContext.Current.Session["ImgArcInwardOperate"].ToString();

                clientCode = HttpContext.Current.Session["ImgArcInwardClientCode"].ToString();
                micrAccNum = HttpContext.Current.Session["ImgArcInwardMicrAccNum"].ToString();
                chequeNum = HttpContext.Current.Session["ImgArcInwardChequeNum"].ToString();

                currentPage = HttpContext.Current.Session["ImgArcInwardCurrentPage"] == null ? "1": HttpContext.Current.Session["ImgArcInwardCurrentPage"].ToString();
                valueToDisplay = HttpContext.Current.Session["ImgArcInwardValueToDisplay"] == null ? "10" : HttpContext.Current.Session["ImgArcInwardValueToDisplay"].ToString();
                ascDescOrder = HttpContext.Current.Session["ImgArcInwardAscDescOrder"] == null ? "ASC" : HttpContext.Current.Session["ImgArcInwardAscDescOrder"].ToString();


                if (HttpContext.Current.Session["ImgArcInwardPresentingBSB"].ToString() == "undefined")
                {
                    presentingBSB = "";
                }
                else
                {
                    presentingBSB = HttpContext.Current.Session["ImgArcInwardPresentingBSB"].ToString();
                }


                DateTime busdate = DateTime.ParseExact(dateTime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');

                //DateTime busdateTo = DateTime.ParseExact(dateTimeTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //string sqlDateTimeTo = busdateTo.Year.ToString() + busdateTo.Month.ToString().PadLeft(2, '0') + busdateTo.Day.ToString().PadLeft(2, '0');

                DBConnectionInfo dbConn = new DBConnectionInfo();
                dbConn.webConnStr = webDBConnStr;

                //string selClientDBConnStr = dbConn.GetSiteConnectionString(true, clientCode.Trim(), dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim()));
                string clientSite = dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim());
                #endregion
                //generate the sql statement here
                stmt = Resource.sqlStmtGetImageArchiveInwardListing;

                IDbDataParameter[] param =
                    new[] {
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BusDate", ParameterDirection.Input, sqlDateTime),
                        //dbHelperObj.CreateParameter(DbType.String, 8, "@BusDateTo", ParameterDirection.Input, sqlDateTimeTo),
                        dbHelperObj.CreateParameter(DbType.String, 25, "@Amount", ParameterDirection.Input, amount),
                        dbHelperObj.CreateParameter(DbType.String, 25, "@Operate", ParameterDirection.Input, operate),

                        dbHelperObj.CreateParameter(DbType.String, 25, "@ClientCode", ParameterDirection.Input, clientCode),
                        dbHelperObj.CreateParameter(DbType.String, 25, "@ClientSite", ParameterDirection.Input, clientSite),
                        dbHelperObj.CreateParameter(DbType.String, 25, "@MicrAccNum", ParameterDirection.Input, micrAccNum),
                        dbHelperObj.CreateParameter(DbType.String, 25, "@ChequeNum", ParameterDirection.Input, chequeNum),
                        dbHelperObj.CreateParameter(DbType.String, 25, "@PresentingBSB", ParameterDirection.Input, presentingBSB),
                        dbHelperObj.CreateParameter(DbType.String, 5, "@Flag", ParameterDirection.Input, flag),

                        dbHelperObj.CreateParameter(DbType.String, 0, "@CurrentPageStr", ParameterDirection.Input, currentPage),
                        dbHelperObj.CreateParameter(DbType.String, 0, "@ValueToDisplayStr", ParameterDirection.Input, valueToDisplay),
                        dbHelperObj.CreateParameter(DbType.String, 50, "@SelectedOrder", ParameterDirection.Input, selectedOrder),
                        dbHelperObj.CreateParameter(DbType.String, 4, "@AscDescOrder", ParameterDirection.Input, ascDescOrder),
                        dbHelperObj.CreateParameter(DbType.String, 100, "@OthersOrder", ParameterDirection.Input, otherOrder),
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

        public static DataTable GetImageArchivePath(String[] filterList, string clientCode)
        {
            //Param List
            //1 - Busdate, 2- client, 3 - presenting branch, 4 - category, 5 - Batch No             
            //CR015-20 add in user id for multi level filtering
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                string filterCriteria = string.Empty;
                //string batchDir = filterList[0].ToString().Trim();
                string batchNo = filterList[0].ToString().Trim();
                string busdate = filterList[1].ToString().Trim();
                DBConnectionInfo dbConn = new DBConnectionInfo();
                dbConn.webConnStr = webDBConnStr;

                //string selClientDBConnStr = dbConn.GetSiteConnectionString(true, clientCode.Trim(), dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim()));
                string clientSite = dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim());

                //DateTime datetime = DateTime.ParseExact(busdate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //string sqlDateTime = datetime.Year.ToString() + datetime.Month.ToString().PadLeft(2, '0') + datetime.Day.ToString().PadLeft(2, '0');

                //generate the sql statement here
                stmt = Resource.sqlstmtGetImageArchivePath;

                IDbDataParameter[] param =
                    new[] {
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BusDate", ParameterDirection.Input, busdate),
                        dbHelperObj.CreateParameter(DbType.String, 25, "@ClientCode", ParameterDirection.Input, clientCode),
                        dbHelperObj.CreateParameter(DbType.String, 25, "@ArchiveType", ParameterDirection.Input, "INW"),
                        dbHelperObj.CreateParameter(DbType.String, 25, "@ClientSite", ParameterDirection.Input, clientSite),
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

        #region Get Image Archive Details(Inward)
        public static DataTable GetSelectedImgArchiveInward(string siteConnString)
        {
            //Param List
            //1 - Batch Dir, 2- Batch No, 3 - Trans NO, 4 - New Bundle ID , 5- Site name,6-Required Approval           
            //

            IDbConnection iConn = dbHelperObj.initConnection(siteConnString);
            string stmt = string.Empty;

            try
            {
                DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
                String curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString().Trim();
                String curCheckNo = HttpContext.Current.Session["s_CurSelectedCheckNoImgArc"].ToString().Trim();
                String curUIC = HttpContext.Current.Session["s_CurSelectedUICImgArc"].ToString().Trim();
                String clientCode = HttpContext.Current.Session["s_CurSelectedClientImgArc"].ToString().Trim();

                DateTime busdate = DateTime.ParseExact(curBusdate.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');

                DBConnectionInfo dbConn = new DBConnectionInfo();
                dbConn.webConnStr = webDBConnStr;

                //string selClientDBConnStr = dbConn.GetSiteConnectionString(true, clientCode.Trim(), dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim()));
                string clientSite = dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim());

                stmt = Resource.sqlStmtGetSelectedImageArchiveInward;
                IDbDataParameter[] param = new[] {
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BusDate", ParameterDirection.Input, sqlDateTime),
                        dbHelperObj.CreateParameter(DbType.String, 10, "@BatchNum", ParameterDirection.Input, curBatchNo),
                        dbHelperObj.CreateParameter(DbType.String, 10, "@CheckNum", ParameterDirection.Input, curCheckNo),
                        dbHelperObj.CreateParameter(DbType.String, 30, "@UIC", ParameterDirection.Input, curUIC),
                        dbHelperObj.CreateParameter(DbType.String, 10, "@ClientCode", ParameterDirection.Input, clientCode),
                        dbHelperObj.CreateParameter(DbType.String, 10, "@ClientSite", ParameterDirection.Input, clientSite),
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
        public static DataTable GetSelectedImgArchiveInward(String siteDBConn, String[] filterList, string clientCode)
        {
            //Param List
            //1 - Batch Dir, 2- Batch No, 3 - Trans NO, 4 - New Bundle ID , 5- Site name,6-Required Approval           
            //

            IDbConnection iConn = dbHelperObj.initConnection(siteDBConn);
            string stmt = string.Empty;

            try
            {
                string filterCriteria = string.Empty;
                string batchNo = filterList[0].ToString().Trim();
                string busdate = filterList[1].ToString().Trim();
                string checkNo = filterList[2].ToString().Trim();
                string uic = filterList[3].ToString().Trim();

                DBConnectionInfo dbConn = new DBConnectionInfo();
                dbConn.webConnStr = webDBConnStr;

                //string selClientDBConnStr = dbConn.GetSiteConnectionString(true, clientCode.Trim(), dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim()));
                string clientSite = dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim());
                //generate the sql statement here
                stmt = Resource.sqlStmtGetSelectedImageArchiveInward;

                IDbDataParameter[] param =
                    new[] {
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BusDate", ParameterDirection.Input, busdate),
                        dbHelperObj.CreateParameter(DbType.String, 10, "@BatchNum", ParameterDirection.Input, batchNo),
                        dbHelperObj.CreateParameter(DbType.String, 10, "@CheckNum", ParameterDirection.Input, checkNo),
                        dbHelperObj.CreateParameter(DbType.String, 30, "@UIC", ParameterDirection.Input, uic),
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ClientCode", ParameterDirection.Input, clientCode),
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ClientSite", ParameterDirection.Input, clientSite),
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
        #endregion

        public static DataTable GetArchivalReportConnectionInfo(string reportCode, string clientCode)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtGetPrintArchivalReportInfo;
                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 10, "@ReportCode", ParameterDirection.Input,reportCode),
                    dbHelperObj.CreateParameter(DbType.String, 10, "@ClientCode", ParameterDirection.Input,clientCode)
                };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);
                return dtDB;
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
    }

    public class RejDecModel
    {
        //TBL_REJECTITEMSTATUS
        public DateTime BusDate { get; set; }
        public string date {  get; set; }
        public string ChequeNo { get; set; }
        public string PresentingBranch { get; set; }
        public string AccountNumber { get; set; }
        public string TranCode { get; set; }//TC?
        public string DepositorAccount { get; set; }
        public string RunNo { get; set; }
        public string BatchNo { get; set; }
        public string IssuingBankType { get; set; }
        public string IssuingBank { get; set; }
        public string IssuingBranch { get; set; }
        public string CheckNo { get; set; }
        public string CheckDigit { get; set; }
        public string TRCode { get; set; }
        public string AcctNo { get; set; }
        public string Amount { get; set; }
        public string NCF { get; set; }
        public string UIC { get; set; }
        public string ReturnCount { get; set; }
        public string TransactionType { get; set; }
        public string ImageFolder { get; set; }
        public string TxnNo { get; set; }
        public string Din { get; set; }
        public string TempID { get; set; }
        public DateTime RejectTime { get; set; }
        //public string PresentingBranch { get; set; }
        public string BatchDirectory { get; set; }
        public string Site { get; set; }
        public string NewBundle { get; set; }
        //public string BatchNo { get; set; }
        public string TransNo { get; set; }
        public string RejCategory { get; set; }
        public string Reason { get; set; }
        public string ProcMode { get; set; }
        public bool CanBreakdown { get; set; }
        public bool CanTolerate { get; set; }
        public bool IsCutOff { get; set; }
        public string RequiredApproval { get; set; }
    }
}