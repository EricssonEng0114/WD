using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
//using System.Web.Security.AntiXss;
using UBPC.Web.Common;
using UBPCWeb.Model;

namespace UBPCWeb.Modules.ImageArchiveOutward
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
                DataTable mainTransDt = GetAllImageArchiveOutwardListing(paramList, "L");//Listing data
                DataTable totalRecord = GetAllImageArchiveOutwardListing(paramList, "C");//Total Count
                int totalCount = 0;
                if(totalRecord.Rows.Count > 0)
                {
                    totalCount = Convert.ToInt32(totalRecord.Rows[0][0]);
                    HttpContext.Current.Session["Outward_totalRecordCount"] = totalCount;
                }

                // HttpContext.Current.Session["rejectedMainDT"] = mainTransDt;
                var query = (from DataRow row in mainTransDt.Rows
                             select new RejDecModel
                             {
                                 //ChequeNo, AccountNo, Amount
                                 TempID = row["TEMP_ID"].ToString(),
                                 BatchDirectory = row["IMG_BatchDir"].ToString(),
                                 BatchNo = row["IMG_BatchNum"].ToString(),
                                 TransNo = row["IMG_TransNum"].ToString(),
                                 BundleID = row["IMG_BundleID"].ToString(),
                                 date = string.Format("{0:yyyyMMdd}", row["IMG_BusDate"]),
                                 BusDate = (DateTime)row["IMG_BusDate"],
                                 ChequeNo = row["IMG_ItemType"].ToString() == "S" ? "" : row["IMG_ChequeNum"].ToString(),
                                 PresentingBranch = row["IMG_BSBCode"].ToString(),
                                 ChequeBSB = row["IMG_ChqBSB"].ToString(),
                                 AccountNumber = row["IMG_ItemType"].ToString() == "S" ? "" : row["IMG_AccountNum"].ToString(),
                                 TransCode = row["IMG_ItemType"].ToString() == "S" ? "" : row["IMG_TransCode"].ToString(),
                                 Amount = String.Format("{0:N2}", Convert.ToDecimal(row["IMG_Amount"])),
                                 DepositorAccount = row["IMG_DepositorAccNum"].ToString(),
                                 RunNo = row["IMG_RunNum"].ToString(),
                                 TxnNo = row["IMG_TxnNum"].ToString(),
                                 Din = row["IMG_Din"].ToString(),
                                 ItemType = row["IMG_ItemType"].ToString(),
                                 TransSeqNum = row["IMG_TransSeqNum"].ToString(),
                                 Site = row["IMG_Site"].ToString()

                             }) ;

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


        public static DataTable GetAllImageArchiveOutwardListing(String[] filterList, string flag)
        {
            //Param List
            //1 - Busdate, 2- client, 3 - presenting branch, 4 - category, 5 - Batch No             
            //CR015-20 add in user id for multi level filtering
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                #region Initialize
                string filterCriteria = string.Empty;
                string dateTime = string.Empty;
                //string dateTimeTo = string.Empty;
                string amount = string.Empty;
                string depositorAcc = string.Empty;
                string operate = string.Empty;

                string micrAccNum = string.Empty;
                string micrBSB = string.Empty;
                string chequeNum = string.Empty;
                string itemType = string.Empty;
                string presentingBSB = string.Empty;
                string clientCode = string.Empty;

                string currentPage = string.Empty;
                string valueToDisplay = string.Empty;
                string selectedOrder = string.Empty;
                string ascDescOrder = string.Empty;
                string otherOrder = string.Empty;
                #endregion

                #region Assign Value To Session
                HttpContext.Current.Session["ImgArcOutwardDateTime"] = filterList[0].ToString();
                //HttpContext.Current.Session["ImgArcOutwardDateTimeTo"] = filterList[1].ToString();
                HttpContext.Current.Session["ImgArcOutwardAmount"] = filterList[1].ToString();
                HttpContext.Current.Session["ImgArcOutwardDepAcc"] = filterList[2].ToString();
                HttpContext.Current.Session["ImgArcOutwardOperate"] = filterList[3].ToString();
                HttpContext.Current.Session["ImgArcOutwardMicrAccNum"] = filterList[4].ToString();
                HttpContext.Current.Session["ImgArcOutwardMicrBSB"] = filterList[5].ToString();
                HttpContext.Current.Session["ImgArcOutwardChequeNum"] = filterList[6].ToString();
                HttpContext.Current.Session["ImgArcOutwardItemType"] = filterList[7].ToString();
                HttpContext.Current.Session["ImgArcOutwardPresentingBSB"] = filterList[8].ToString();
                HttpContext.Current.Session["ImgArcOutwardClientCode"] = filterList[9].ToString();


                HttpContext.Current.Session["ImgArcOutwardCurrentPage"] = filterList[10].ToString();
                HttpContext.Current.Session["ImgArcOutwardValueToDisplay"] = filterList[11].ToString();
                HttpContext.Current.Session["ImgArcOutwardSelectedOrder"] = filterList[12].ToString();
                HttpContext.Current.Session["ImgArcOutwardAscDescOrder"] = filterList[13].ToString();
                #endregion

                switch (filterList[12].ToString())
                {
                    case "0":
                        selectedOrder = "ITM_BusDate";
                        otherOrder = "ITM_BatchDirectory, ITM_BatchNum, ITM_TransNum,ITM_TransSeqNum";
                        //otherOrder = "ITM_BatchNum,ITM_ItemType,ITM_Fld5,ITM_Fld4,ITM_Fld3";
                        break;
                    case "1":
                        selectedOrder = "ITM_BatchNum";
                        otherOrder = "ITM_BusDate, ITM_BatchDirectory, ITM_TransNum,ITM_TransSeqNum";
                        //otherOrder = "ITM_BusDate,ITM_ItemType,ITM_Fld5,ITM_Fld4,ITM_Fld3";
                        break;
                    case "2":
                        selectedOrder = "ITM_ItemType";
                        otherOrder = "ITM_BusDate, ITM_BatchDirectory, ITM_BatchNum, ITM_TransNum,ITM_TransSeqNum";
                        //otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_Fld5,ITM_Fld4,ITM_Fld3";
                        break;
                    case "3":
                        selectedOrder = "ChequeNum";
                        otherOrder = "ITM_BusDate, ITM_BatchDirectory, ITM_BatchNum, ITM_TransNum,ITM_TransSeqNum";
                        //otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_ItemType,ITM_Fld4,ITM_Fld3";
                        break;
                    case "4":
                        selectedOrder = "ITM_Fld4";
                        otherOrder = "ITM_BusDate, ITM_BatchDirectory, ITM_BatchNum, ITM_TransNum,ITM_TransSeqNum";
                        //otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_ItemType,ITM_Fld5,ITM_Fld3";
                        break;
                    case "5":
                        selectedOrder = "AccountNum";//ITM_Fld3
                        otherOrder = "ITM_BusDate, ITM_BatchDirectory, ITM_BatchNum, ITM_TransNum,ITM_TransSeqNum";
                        //otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_ItemType,ITM_Fld5,ITM_Fld4";
                        break;
                    case "6":
                        selectedOrder = "TransCode";//ITM_Fld2
                        otherOrder = "ITM_BusDate, ITM_BatchDirectory, ITM_BatchNum, ITM_TransNum,ITM_TransSeqNum";
                        //otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_ItemType,ITM_Fld5,ITM_Fld4,ITM_Fld3";
                        break;
                    case "7":
                        selectedOrder = "ITM_Amount";
                        otherOrder = "ITM_BusDate, ITM_BatchDirectory, ITM_BatchNum, ITM_TransNum,ITM_TransSeqNum";
                        //otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_ItemType,ITM_Fld5,ITM_Fld4,ITM_Fld3";
                        break;
                    case "8":
                        selectedOrder = "ITM_Fld12";
                        otherOrder = "ITM_BusDate, ITM_BatchDirectory, ITM_BatchNum, ITM_TransNum,ITM_TransSeqNum";
                        //otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_ItemType,ITM_Fld5,ITM_Fld4,ITM_Fld3";
                        break;
                    case "9":
                        selectedOrder = "ITM_TransNum";
                        otherOrder = "ITM_BusDate, ITM_BatchDirectory, ITM_BatchNum,ITM_TransSeqNum";
                       // otherOrder = "ITM_BusDate,ITM_BatchNum,ITM_ItemType,ITM_Fld5,ITM_Fld4,ITM_Fld3";
                        break;
                }

                #region Assign Value
                dateTime = HttpContext.Current.Session["ImgArcOutwardDateTime"].ToString();
                //dateTimeTo = HttpContext.Current.Session["ImgArcOutwardDateTimeTo"].ToString();
                amount = HttpContext.Current.Session["ImgArcOutwardAmount"].ToString();
                depositorAcc = HttpContext.Current.Session["ImgArcOutwardDepAcc"].ToString();
                operate = HttpContext.Current.Session["ImgArcOutwardOperate"].ToString();

                micrAccNum = HttpContext.Current.Session["ImgArcOutwardMicrAccNum"].ToString();
                micrBSB = HttpContext.Current.Session["ImgArcOutwardMicrBSB"].ToString();
                chequeNum = HttpContext.Current.Session["ImgArcOutwardChequeNum"].ToString();
                itemType = HttpContext.Current.Session["ImgArcOutwardItemType"].ToString();
                presentingBSB = HttpContext.Current.Session["ImgArcOutwardPresentingBSB"].ToString();
                clientCode = HttpContext.Current.Session["ImgArcOutwardClientCode"].ToString();

                currentPage = HttpContext.Current.Session["ImgArcOutwardCurrentPage"] == null ? "1" : HttpContext.Current.Session["ImgArcOutwardCurrentPage"].ToString();
                valueToDisplay = HttpContext.Current.Session["ImgArcOutwardValueToDisplay"] == null ? "25" : HttpContext.Current.Session["ImgArcOutwardValueToDisplay"].ToString();
                ascDescOrder = HttpContext.Current.Session["ImgArcOutwardAscDescOrder"] == null ? "ASC" : HttpContext.Current.Session["ImgArcOutwardAscDescOrder"].ToString();

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
                stmt = Resource.sqlStmtGetImageArchiveOutwardListing;// "SELECT * from TBL_LOG WITH(NOLOCK) where TimeStamp>=@startDateTime AND TimeStamp <=@endDateTime";

                IDbDataParameter[] param =
                    new[] {
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BusDate", ParameterDirection.Input, sqlDateTime),
                        //dbHelperObj.CreateParameter(DbType.String, 8, "@BusDateTo", ParameterDirection.Input, sqlDateTimeTo),
                        dbHelperObj.CreateParameter(DbType.String, 20, "@DepositorAcc", ParameterDirection.Input, depositorAcc),
                        dbHelperObj.CreateParameter(DbType.String, 25, "@Amount", ParameterDirection.Input, amount),
                        dbHelperObj.CreateParameter(DbType.String, 25, "@Operate", ParameterDirection.Input, operate),
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ClientCode", ParameterDirection.Input, clientCode),
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ClientSite", ParameterDirection.Input, clientSite),

                        dbHelperObj.CreateParameter(DbType.String, 25, "@MicrAccNum", ParameterDirection.Input, micrAccNum),//ITM_fld3
                        dbHelperObj.CreateParameter(DbType.String, 25, "@MicrBSB", ParameterDirection.Input, micrBSB),//ITM_fld4
                        dbHelperObj.CreateParameter(DbType.String, 25, "@ChequeNum", ParameterDirection.Input, chequeNum),//ITM_fld5
                        dbHelperObj.CreateParameter(DbType.String, 3, "@ItemType", ParameterDirection.Input, itemType),//ITM_fld8
                        dbHelperObj.CreateParameter(DbType.String, 25, "@PresentingBSB", ParameterDirection.Input, presentingBSB),//ITM_fld9
                        dbHelperObj.CreateParameter(DbType.String, 5, "@Flag", ParameterDirection.Input, flag),

                        dbHelperObj.CreateParameter(DbType.String, 10, "@CurrentPageStr", ParameterDirection.Input, currentPage),
                        dbHelperObj.CreateParameter(DbType.String, 10, "@ValueToDisplayStr", ParameterDirection.Input, valueToDisplay),
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

        #region Get Image Archive Details(Outward)
        public static DataTable GetSelectedImgArchiveOutward(string siteConnString, bool isBatchHeader)
        {
            IDbConnection iConn = dbHelperObj.initConnection(siteConnString);
            string stmt = string.Empty;
            string pattern = @"^\d{8}$";//datetime format 20240611 yyyyMMdd
            DateTime parsedDate;
            try
            {
                if (Regex.IsMatch(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"].ToString(), pattern))
                {
                    if (DateTime.TryParseExact(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"].ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    {
                        HttpContext.Current.Session["s_CurSelectedBusdateRejDec"] = parsedDate;
                    }
                }

                DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
                String curBatchDir = HttpContext.Current.Session["s_CurSelectedBatchDirImgArc"].ToString().Trim();
                String curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString().Trim();
                int curTransNo = Convert.ToInt32(HttpContext.Current.Session["s_CurSelectedTransNoImgArc"].ToString().Trim());
                //String curPresentingBSB = HttpContext.Current.Session["s_CurSelectedPresentingBSBImgArc"].ToString().Trim();
                String curTransSeqNum = HttpContext.Current.Session["s_CurSelectedTransSeqNumImgArc"].ToString().Trim();
                //String curDin = HttpContext.Current.Session["s_CurSelectedItemTypeImgArc"].ToString() == "B"? "" : HttpContext.Current.Session["s_CurSelectedDinImgArc"].ToString().Trim();
                String curItemType = HttpContext.Current.Session["s_CurSelectedItemTypeImgArc"].ToString().Trim();

                DateTime busdate = DateTime.ParseExact(curBusdate.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');
                String clientCode = HttpContext.Current.Session["s_CurSelectedClientImgArc"].ToString().Trim();
                String clientSite = HttpContext.Current.Session["s_CurSelectedSiteImgArc"].ToString().Trim();


                stmt = Resource.sqlStmtGetSelectedImageArchiveOutward;
                IDbDataParameter[] param = new[] {
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchDir", ParameterDirection.Input, curBatchDir),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BatchNum", ParameterDirection.Input, curBatchNo),
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BusDate", ParameterDirection.Input, sqlDateTime),
                        dbHelperObj.CreateParameter(DbType.String, 20, "@TransNum", ParameterDirection.Input, curTransNo),
                        dbHelperObj.CreateParameter(DbType.String, 20, "@TransSeqNum", ParameterDirection.Input, curTransSeqNum),
                        dbHelperObj.CreateParameter(DbType.Boolean, 5, "@IsBatchHeader", ParameterDirection.Input, isBatchHeader),
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ClientCode", ParameterDirection.Input, clientCode),
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ClientSite", ParameterDirection.Input, clientSite),
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ItemType", ParameterDirection.Input, curItemType)
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

        public static DataTable GetImageArchivePath(String[] filterList, string clientCode)
        {
            //Param List         
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                string filterCriteria = string.Empty;
                string datetime = filterList[5].ToString().Trim();
                string clientSite = filterList[6].ToString().Trim();

                //DateTime busdate = DateTime.ParseExact(datetime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');

                //generate the sql statement here
                stmt = Resource.sqlstmtGetImageArchivePath;

                IDbDataParameter[] param =
                    new[] {
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BusDate", ParameterDirection.Input, datetime),
                        dbHelperObj.CreateParameter(DbType.String, 25, "@ClientCode", ParameterDirection.Input, clientCode),
                        dbHelperObj.CreateParameter(DbType.String, 25, "@ClientSite", ParameterDirection.Input, clientSite),
                        dbHelperObj.CreateParameter(DbType.String, 25, "@ArchiveType", ParameterDirection.Input, "OUT"),
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

        public static DataTable GetTransactionAllItems(string clientCode, string sitecode, string batchDir,
            string batchNo, string datetime, string transNo, string transSeqNum, string isCurrentItemOnly, string isRejected, string rejectCategory)
        {
            //IDbConnection iConn = dbHelperObj.initConnection(GetClientConnectionString(clientCode,sitecode));

            string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;


            //DateTime busdate = DateTime.ParseExact(datetime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            //string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');

            try
            {
                stmt = "sp_GetPrintInfoForArchivalTransOutward";

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String,8, "@BatchDir", ParameterDirection.Input,batchDir),
                    dbHelperObj.CreateParameter(DbType.String,8, "@BatchNum", ParameterDirection.Input,batchNo),
                    dbHelperObj.CreateParameter(DbType.String,8, "@Busdate", ParameterDirection.Input,datetime),
                    dbHelperObj.CreateParameter(DbType.Int32,0, "@TransNum", ParameterDirection.Input,Convert.ToInt32(transNo)),
                    dbHelperObj.CreateParameter(DbType.String,10, "@TransSeqNum", ParameterDirection.Input, transSeqNum),
                    dbHelperObj.CreateParameter(DbType.String,10, "@isCurrentItem", ParameterDirection.Input, isCurrentItemOnly),
                    dbHelperObj.CreateParameter(DbType.String,10, "@isPrintRejected", ParameterDirection.Input, isRejected),
                    dbHelperObj.CreateParameter(DbType.String,10, "@RejectCategory", ParameterDirection.Input, rejectCategory),
                    dbHelperObj.CreateParameter(DbType.String,10, "@ClientCode", ParameterDirection.Input,clientCode),
                    dbHelperObj.CreateParameter(DbType.String,10, "@ClientSite", ParameterDirection.Input,sitecode)
                };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);
                return dtDB;
            }
            catch (Exception ex) { throw ex; }
        }

        public static DataTable GetTransSeqNum()
        {
            //Param List         
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                DateTime curBusdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
                String curBatchDir = HttpContext.Current.Session["s_CurSelectedBatchDirImgArc"].ToString().Trim();
                String curBatchNo = HttpContext.Current.Session["s_CurSelectedBatchNoRejDec"].ToString().Trim();
                int curTransNo = Convert.ToInt32(HttpContext.Current.Session["s_CurSelectedTransNoImgArc"].ToString().Trim());

                DateTime busdate = DateTime.ParseExact(curBusdate.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');
                String clientCode = HttpContext.Current.Session["s_CurSelectedClientImgArc"].ToString().Trim();
                String clientSite = HttpContext.Current.Session["s_CurSelectedSiteImgArc"].ToString().Trim();

                //generate the sql statement here
                stmt = Resource.sqlStmtGetImageArchiveOutwardTransSeqNum;

                IDbDataParameter[] param =
                    new[] {
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BusDate", ParameterDirection.Input, sqlDateTime),
                        dbHelperObj.CreateParameter(DbType.String, 10, "@BatchNum", ParameterDirection.Input, curBatchNo),
                        dbHelperObj.CreateParameter(DbType.String, 10, "@BatchDir", ParameterDirection.Input, curBatchDir),
                        dbHelperObj.CreateParameter(DbType.String, 4, "@TransNum", ParameterDirection.Input, curTransNo),
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
        public string date { get; set; }
        public string ItemType { get; set; }
        public string TransSeqNum { get; set; }
        public string ChequeNo { get; set; }
        public string PresentingBranch { get; set; }
        public string ChequeBSB { get; set; }
        public string AccountNumber { get; set; }
        public string TransCode { get; set; }
        public string Amount { get; set; }
        public string DepositorAccount { get; set; }
        public string RunNo { get; set; }
        public string BatchNo { get; set; }
        public string TxnNo { get; set; }
        public string Din { get; set; }
        public string TempID { get; set; }
        public DateTime RejectTime { get; set; }
        public string BatchDirectory { get; set; }
        public string Site { get; set; }
        public string BundleID { get; set; }
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