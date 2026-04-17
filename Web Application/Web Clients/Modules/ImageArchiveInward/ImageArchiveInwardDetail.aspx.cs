using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using UBPC.Web.Common;
using System.EnterpriseServices.CompensatingResourceManager;
using System.Text.RegularExpressions;

namespace UBPCWeb.Modules.ImageArchiveInward
{
    public partial class ImageArchiveInwardDetail : System.Web.UI.Page
    {
        #region initialize
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private string clientList = string.Empty;
        private bool isPageValid = false;
        private bool isForUnisys = false;

        public DateTime curBusdate = DateTime.Now;
        public String curBatchDir = string.Empty;
        public String curBatchNo = string.Empty;
        public String curChequeBSB = string.Empty;
        public String curSelectedClient = string.Empty;
        public String curSite = string.Empty;

        public string curYearMonth = string.Empty;
        public string curTableName = string.Empty;
        public string curArchivePath = string.Empty;
        public string curIISVirtualPath = string.Empty;
        public string curType = string.Empty;

        public String curIssuingBankType = string.Empty;
        public String curIssuingBank = string.Empty;
        public String curIssuingBranch = string.Empty;
        public String curCheckNo = string.Empty;
        public String curCheckDigit = string.Empty;
        public String curTRCode = string.Empty;
        public String curAcctNo = string.Empty;
        public String curAmount = string.Empty;
        public String curNCF = string.Empty;
        public String curUIC = string.Empty;
        public String curReturnCount = string.Empty;
        public String curTransactionType = string.Empty;
        public String curImageFolder = string.Empty;

        public bool curAllowBreakdown = false;
        public bool curAllowTolerance = false;

        public String curPaymentMode = string.Empty;
        public int curTransNo = 0;
        public int curTotalStubCount = 0;
        public decimal curTotalStubAmt = 0;
        public int curTotalChqCount = 0;
        public decimal curTotalChqAmt = 0;
        List<string> listOfImgFile;

        protected string ReportCode { get; set; }
        protected string ReportClient { get; set; }
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                HttpContext.Current.Session["Logcaller_Inward_Outward"] = "ImageArchiveInward";
                AssignSessionValue();
                //string fileDirectory = curIISVirtualPath + "\\" + curBusdate.ToString("yyyyMMdd") + "\\" + curTransactionType + "\\" + curImageFolder + "\\archives";

                string virtualDirectory = "~/"+ curIISVirtualPath;

                // Retrieve the physical path of the directory
                string physicalPath = HttpContext.Current.Server.MapPath(virtualDirectory);

                string inwardImagePath = physicalPath + "\\" + curBusdate.ToString("yyyyMMdd") + "\\" + curTransactionType + "\\" + curImageFolder + "\\archives";


                // string path = HttpContext.Current.Server.MapPath(fileDirectory);
                // string inwardImagePath = path.Replace("\\ImageArchiveInwardDetail", "");

                ProcessImage(curUIC, inwardImagePath, Session["s_CurSelectedClientImgArc"].ToString(), curUIC);

                if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
                {
                    PageValidatorResult validatorResult;
                    validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "ImageArchiveInward");
                    isPageValid = validatorResult.Valid;

                    if (!isPageValid)
                    {
                        Session["s_GeneralMsg"] = validatorResult.ReturnMessage;
                        Response.Redirect("Home", false);
                    }
                    else
                    {
                        Session["s_GeneralMsg"] = string.Empty;
                        isPageValid = true;
                    }
                }
                else
                {
                    //invalid user - back to login page
                    Session["s_UserID"] = "";
                    Response.Redirect("/Login", false);
                    Context.ApplicationInstance.CompleteRequest();
                }

                if (isPageValid)
                {
                    //Build connection string for site and set curDataTable
                    DBConnectionInfo dbConn = new DBConnectionInfo();
                    dbConn.webConnStr = webDBConnStr;
                    Session["selClientDBConnStr"] = dbConn.GetSiteConnectionString(true, curSelectedClient, curSite);

                    curSelectedClient = Session["s_CurSelectedClientImgArc"].ToString();
                    curSite = Session["s_CurSelectedSiteRejDec"].ToString();

                    //Run First Time
                    if (!IsPostBack)
                    {
                        Session["isInward"] = true;
                        InitialiseSessionValue();

                        LoadItemDataTableInfo();
                    }
                    else
                    {
                        //Set Valid navigation
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "setValidNav", "setValidNavigation();", true);
                    }
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveInward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/ImageArchiveInward", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }


        #region Helper Function

        private void LoadItemDataTableInfo()
        {
            try
            {
                //Validate session value 
                if (ValidateSessionValue())
                {
                    //Populate item info to web control, default top image item
                    //Initialise Common Item Datatable for first time
                    DataTable dtDetail = CommonFunction.GetSelectedImgArchiveInward(webDBConnStr);
                    Session["rejectedMainItemDT"] = dtDetail;

                    if (((DataTable)Session["rejectedMainItemDT"]).Rows.Count > 0)
                    {

                        Session["curTransItemsCount"] = ((DataTable)Session["rejectedMainItemDT"]).Rows.Count;

                        Session["topItemPtr"] = 0;
                        //Show Data on screen
                        RefreshItemPage();
                    }
                    else
                    {
                        //Redirect to index page
                        Logger.Write(false, LogCallerID.ImageArchiveInward, curSelectedClient, "Validate Image Archive Inward", "Unable to retrieve item details", LogEventType.Error, userID);
                        Response.Redirect("/ImageArchiveInward", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }
                else
                {
                    //Redirect to index page
                    Logger.Write(false, LogCallerID.ImageArchiveInward, curSelectedClient, "Validate Image Archive Inward", "Invalid session value for Image Archive Inward.", LogEventType.Error, userID);
                    Response.Redirect("/ImageArchiveInward", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DisplayTwoItemsInfo()
        {
            try
            {
                if (Convert.ToInt16(Session["curTransItemsCount"]) > 0)
                {
                    //Display Top Image Item
                    DataRow itemTop = ((DataTable)Session["rejectedMainItemDT"]).Rows[Convert.ToInt16(Session["topItemPtr"])];

                    //by default select the top field 
                    DisplaySelItemFieldsAndImage(Convert.ToBoolean(Session["topItemSelected"]));

                    //Load top image here
                    //Call to ImageHandlerInward.ashx
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadLeftImage", "LoadLeftImage();", true);
                    //ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "flipTopImage", String.Format("flipImage('{0}', '{1}', '{2}');", "IAI", "BOT", "#PVImgViewerRight"), true);
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadRightImage", "LoadRightImage();", true);

                    //if there is a bottom Item display it's image and amount in bottom Pannel
                    //otherwise hide bottom Pannel
                    if ((Convert.ToInt16(Session["topItemPtr"])) + 1 < Convert.ToInt16(Session["curTransItemsCount"]))
                    {
                        //Show Bottom Panel
                        //ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "hidebottom", String.Format("showhideBottom('{0}');", "1"), true);

                        DataRow itemBottom = ((DataTable)Session["rejectedMainItemDT"]).Rows[(Convert.ToInt16(Session["topItemPtr"])) + 1];

                        //Load Bottom Image
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadRightImage", "LoadRightImage();", true);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DisplaySelItemFieldsAndImage(bool isTopItem)
        {
            try
            {

                Session["curItemPtr"] = (Convert.ToInt16(Session["topItemPtr"])) + ((isTopItem) ? 0 : 1);

                DataRow itemRow = ((DataTable)Session["rejectedMainItemDT"]).Rows[Convert.ToInt16(Session["curItemPtr"])];

                //Store Current Image info to Session to Load selected image
                if (isTopItem)
                {
                    //current item point = top item
                    //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
                    Session["CurSelectedTopImage"] = "C:\\Temp\\" + curBusdate + "|" + curUIC + "|1|0";
                    Session["CurSelectedBottomImage"] = "C:\\Temp\\" + curBusdate + "|" + curUIC + "|0|0";
                }

                lblBatchNum.Text = curBatchNo.ToString();
                lblAmount.Text = curAmount.ToString();
                lblTranCode.Text = curTRCode.ToString();
                lblChequeAccNum.Text = curAcctNo.ToString();
                lblChequeBSB.Text = curChequeBSB.ToString();
                lblChequeSerial.Text = curCheckNo.ToString();
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveInward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/ImageArchiveInward", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        private void RefreshItemPage()
        {
            try
            {
                Session["topItemSelected"] = true;
                DisplayTwoItemsInfo();
                Session["curItemPtr"] = (Convert.ToInt16(Session["topItemPtr"])) + ((Convert.ToBoolean(Session["topItemSelected"])) ? 0 : 1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool ValidateSessionValue()
        {
            try
            {
                if (string.IsNullOrEmpty(Session["s_CurSelectedBusdateRejDec"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedBatchNoRejDec"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedIssuingBankImgArc"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedIssuingBranchImgArc"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedCheckNoImgArc"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedCheckDigitImgArc"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedTRCodeImgArc"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedAcctNoImgArc"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedAmountImgArc"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedNCFImgArc"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedUICImgArc"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedTransactionTypeImgArc"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedImageFolderImgArc"].ToString())
                    )
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void AssignSessionValue()
        {
            try
            {
                userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString().Trim();
                userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString().Trim();
                isForUnisys = Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(Session["s_UserForUnisys"].ToString());
                clientList = Session["s_UserClients"] == null ? string.Empty : Session["s_UserClients"].ToString().Trim();

                curSelectedClient = Session["s_CurSelectedClientImgArc"].ToString();
                //curSite = Session["s_CurSelectedSiteRejDec"].ToString();

                curBusdate = Convert.ToDateTime(Session["s_CurSelectedBusdateRejDec"]);
                curBatchNo = Session["s_CurSelectedBatchNoRejDec"].ToString();
                curIssuingBankType = Session["s_CurSelectedIssuingBankTypeImgArc"].ToString();
                curIssuingBank = Session["s_CurSelectedIssuingBankImgArc"].ToString();
                curIssuingBranch = Session["s_CurSelectedIssuingBranchImgArc"].ToString();
                curCheckNo = Session["s_CurSelectedCheckNoImgArc"].ToString();
                curCheckDigit = Session["s_CurSelectedCheckDigitImgArc"].ToString();
                curTRCode = Session["s_CurSelectedTRCodeImgArc"].ToString();
                curAcctNo = Session["s_CurSelectedAcctNoImgArc"].ToString();
                string stringAmount = Session["s_CurSelectedAmountImgArc"].ToString();
                decimal decimalAmount = decimal.Parse(stringAmount);
                curAmount = String.Format("{0:N2}", decimalAmount);
                curNCF = Session["s_CurSelectedNCFImgArc"].ToString();
                curUIC = Session["s_CurSelectedUICImgArc"].ToString();
                curReturnCount = Session["s_CurSelectedReturnCountImgArc"].ToString();
                curTransactionType = Session["s_CurSelectedTransactionTypeImgArc"].ToString();
                curImageFolder = Session["s_CurSelectedImageFolderImgArc"].ToString();
                curChequeBSB = curIssuingBank.ToString() + curIssuingBranch.ToString();

                curYearMonth = Session["s_CurSelectedYearMonthImgArc"].ToString();
                curTableName = Session["s_CurSelectedTableNameImgArc"].ToString();
                curArchivePath = Session["s_CurSelectedArchivePathImgArc"].ToString();
                curIISVirtualPath = Session["s_CurSelectedIISVirtualPathImgArc"].ToString();
                curType = Session["s_CurSelectedTypeImgArc"].ToString();


                lblBatchNum.Text = curBatchNo.ToString();
                lblAmount.Text = curAmount.ToString();
                lblTranCode.Text = curTRCode.ToString();
                lblChequeAccNum.Text = curAcctNo.ToString();
                lblChequeBSB.Text = curChequeBSB.ToString();
                lblChequeSerial.Text = curCheckNo.ToString();
                lblCheckDigit.Text = curCheckDigit.ToString();
                lblNCFTag.Text = curNCF.ToString();
                if (string.IsNullOrEmpty(curReturnCount.TrimStart('0').ToString()))
                {
                    lblReturnCount.Text = "0";
                }
                else
                {
                    lblReturnCount.Text = curReturnCount.TrimStart('0').ToString();
                }
                lblTransactionType.Text = curTransactionType.ToString();


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void InitialiseSessionValue()
        {
            Session["topItemSelected"] = false;
            Session["curTransItemsCount"] = 0;
            Session["curItemPtr"] = 0;
            Session["topItemPtr"] = 0;
            Session["bottomItemPtr"] = 0;
            Session["curSelectedStub"] = 0;

            Session["VirDirForIFSPath"] = "";
            Session["rejectedMainItemDT"] = new DataTable();

        }

        private void ClearSessionValue()
        {
            Session["topItemSelected"] = null;
            Session["curTransItemsCount"] = null;
            Session["curItemPtr"] = null;
            Session["topItemPtr"] = null;
            Session["bottomItemPtr"] = null;
            Session["curSelectedStub"] = null;

            Session["VirDirForIFSPath"] = null;
            Session["rejectedMainItemDT"] = null;
        }

        private void ProcessImage(string filename, string fileDirectory, string bankCode, string UIC)
        {


            DBConnectionInfo dbConn = new DBConnectionInfo();
            dbConn.webConnStr = webDBConnStr;
            Session["selClientDBConnStr"] = dbConn.GetSiteConnectionString(true, Session["s_CurSelectedClientImgArc"].ToString(), curSite);
            string outputPath = @"C:\Temp";
            string fullName = filename + ".IMG";

            try
            {
                if (!string.IsNullOrEmpty(outputPath))
                {
                    try
                    {
                        if (File.Exists(Path.Combine(fileDirectory, fullName)))
                        {
                            //listOfImgFile.Add(Path.Combine(fileDirectory, fullName));
                            string path = Path.Combine(fileDirectory, fullName);
                            System.Drawing.Image tifImg = System.Drawing.Image.FromFile(path);
                            Guid objguid = tifImg.FrameDimensionsList[0];
                            FrameDimension fd = new FrameDimension(objguid);
                            int page = tifImg.GetFrameCount(fd);
                            int index = 0;
                            string outputFileName = string.Empty;
                            string directory = "C:\\Temp\\" + ((DateTime)curBusdate).ToString("yyyyMMdd");
                            if (!Directory.Exists(directory))
                                Directory.CreateDirectory(directory);

                            Session.Remove("ImageArchiveInwardFrontJPEG");
                            Session.Remove("ImageArchiveInwardFrontImg");
                            Session.Remove("ImageArchiveInwardRearImg");

                            for (index = 0; index < page; index++)
                            {
                                tifImg.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Page, index);
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    if (index == 0)
                                    {
                                        tifImg.Save(ms, ImageFormat.Jpeg);
                                        outputFileName = UIC + ".jpg";
                                        File.WriteAllBytes(directory + "\\" + outputFileName, ms.ToArray());
                                        Session["ImageArchiveInwardFrontJPEG"] = ms.ToArray();
                                        //outputFileName = bankCode + "_" + UIC + ".jpg";
                                    }
                                    else
                                    {
                                        if (index == 1)
                                        {
                                            tifImg.Save(ms, ImageFormat.Tiff);
                                            outputFileName = UIC + ".FIM";
                                            File.WriteAllBytes(directory + "\\" + outputFileName, ms.ToArray());
                                            Session["ImageArchiveInwardFrontImg"] = ms.ToArray();
                                        }
                                        else
                                        {
                                            tifImg.Save(ms, ImageFormat.Tiff);
                                            outputFileName = UIC + ".RIM";
                                            File.WriteAllBytes(directory + "\\" + outputFileName, ms.ToArray());
                                            Session["ImageArchiveInwardRearImg"] = ms.ToArray();
                                        }
                                    }

                                    ms.Dispose();
                                }

                            }

                            tifImg.Dispose();

                            string sqlDateTime = ((DateTime)curBusdate).ToString("yyyyMMdd");
                            string ImgPath = sqlDateTime + "\\" + curUIC;
                            ChequeViewerCtrl chqImgObj = new ChequeViewerCtrl();
                            //string IFSPath, bool isFront, bool isJPEG, String frontImgPath, String rearImgPath, String frontJpegImgPath

                            if (Directory.Exists(directory))
                            {
                                Directory.Delete(directory, true);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogEntry log = new LogEntry();
                        log.Caller = LogCallerID.ImageArchiveInward;
                        log.Severity = LogEventType.Error;
                        log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                        log.Alert = true;
                        log.Message = ex.Message;
                        log.Write();

                    }
                }

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveInward;
                log.Severity = LogEventType.Error;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.Alert = true;
                log.Message = ex.Message;
                log.Write();
            }
        }

        
        #endregion

        #region Web Method by Ajax calling

        [WebMethod]
        public static string getImagePath(string id)
        {
            int topItemPtr = Convert.ToInt16(HttpContext.Current.Session["topItemPtr"]);
            DataTable detailDT = ((DataTable)HttpContext.Current.Session["rejectedMainItemDT"]);

            DataRow itemRow = id.Contains("TOP") ? detailDT.Rows[topItemPtr] : detailDT.Rows[topItemPtr];

            DateTime busdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);
            string uic = HttpContext.Current.Session["s_CurSelectedUICImgArc"].ToString();



            string curSelTopImg = id.Contains("TOP") ? HttpContext.Current.Session["CurSelectedTopImage"].ToString() :
                HttpContext.Current.Session["CurSelectedBottomImage"].ToString();



            //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
            string[] strArr = curSelTopImg.Split('|');
            string virDir = "C:\\Temp\\" + ((DateTime)busdate).ToString("yyyyMMdd");
            string imgFileName = uic;
            string isFront = strArr[2].ToString();
            string isJpeg = strArr[3].ToString();

            string updIsFront = isFront;
            string updIsRear = isFront;
            string updIsJpeg = isJpeg;

            if (id.Contains("Toggle"))
            {
                if (isJpeg.Equals("1"))
                {
                    //Switch to tiff
                    updIsJpeg = "0";
                }
                else
                {
                    //Switch to jpeg
                    updIsJpeg = "1";
                }
            }

            if (id.Contains("Flip"))
            {
                updIsFront = isFront.Equals("1") ? "0" : "1";
                updIsRear = isFront.Equals("0") ? "1" : "0";
            }


            string updSessionValue = virDir + "|" + imgFileName + "|" + updIsFront + "|" + updIsJpeg;
            string updSessionValueRear = virDir + "|" + imgFileName + "|" + updIsRear + "|" + updIsJpeg;

            if (id.Contains("TOP"))
            {
                //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
                HttpContext.Current.Session["CurSelectedTopImage"] = updSessionValue;
            }
            else
            {
                //bottom
                HttpContext.Current.Session["CurSelectedBottomImage"] = updSessionValueRear;
            }

            //Random ID is needed, upon "updatePanel" ajax calling it will not trigger postback, by using 
            //Query string will "postback" therefore able to call to handler to reload image
            string randomID = System.Guid.NewGuid().ToString().Replace("-", "");

            HttpContext.Current.Session["Logcaller_Inward_Outward"] = "ImageArchiveInward";
            if (id.Contains("TOP"))
            {
                //get current jpeg or tiff then switch
                return "/ImageHandlerInward.ashx?id=" + randomID;
            }
            else
            {
                return "/ImageHandlerInwardRear.ashx?id=" + randomID;
            }
        }

        [WebMethod]
        public static string getReportUrl()
        {
            string clientcode = HttpContext.Current.Session["CurArchivalSelectedRptClient"].ToString();
            if (clientcode.Equals("CIMB"))
            {
                clientcode = "CIMB";
            }
            else
            {
                clientcode = "ALL";
            }
            DataTable reportInfo = CommonFunction.GetArchivalReportConnectionInfo("PAR01", clientcode);
            DataRow itemRow = reportInfo.Rows[0];
            string reportcode = HttpContext.Current.Session["CurArchivalSelectedRptCode"].ToString();
            string sitecode = HttpContext.Current.Session["CurArchivalSelectedSiteCode"].ToString();
            //Assign Report Session
            string busdate = HttpContext.Current.Session["CurArchivalSelectedBusdate"].ToString();
            string checkno = HttpContext.Current.Session["CurArchivalSelectedCheckNo"].ToString();
            string uic = HttpContext.Current.Session["CurArchivalSelectedUIC"].ToString(); 


            string url = string.Empty;
            url = "/Modules/ImageArchiveInward/PrintArchivalReportViewer.aspx?v=" + DateTime.Now.Ticks;


            string fullUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + url;
            return fullUrl;
        }
        #endregion

        #region Event Handling

        protected void btnBackToListing_Click(object sender, EventArgs e)
        {
            try
            {
                ClearSessionValue();
                Session["IsClearSearchCriteria_Inward"] = "False";
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveInward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadSpinner", "loadSpinner();", true);
            Response.Redirect("/ImageArchiveInward", false);
            Context.ApplicationInstance.CompleteRequest();
        }
        protected void btnViewBatchHeader_Click(object sender, EventArgs e)
        {
            try
            {
                ClearSessionValue();
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveInward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadSpinner", "loadSpinner();", true);
            Response.Redirect("/ImageArchiveInward", false);
            Context.ApplicationInstance.CompleteRequest();
        }
        protected void btnPrevItem_Click(object sender, EventArgs e)
        {
            try
            {
                ClearSessionValue();
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveInward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadSpinner", "loadSpinner();", true);
            Response.Redirect("/ImageArchiveInward", false);
            Context.ApplicationInstance.CompleteRequest();
        }
        protected void btnNextItem_Click(object sender, EventArgs e)
        {
            try
            {
                ClearSessionValue();
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveInward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadSpinner", "loadSpinner();", true);
            Response.Redirect("/ImageArchiveInward", false);
            Context.ApplicationInstance.CompleteRequest();
        }
        protected void btnPrintTransaction_Click(object sender, EventArgs e)
        {
            try
            {
                DBConnectionInfo dbConn = new DBConnectionInfo();
                dbConn.webConnStr = webDBConnStr;

                //string selClientDBConnStr = dbConn.GetSiteConnectionString(true, clientCode.Trim(), dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim()));
                string clientSite = dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), curSelectedClient);
                Session["s_CurSelectedSiteRejDec"] = clientSite.ToString();

                string clientcode = curSelectedClient;
                if (clientcode.Equals("CIMB"))
                {
                    clientcode = "CIMB";
                }
                else
                {
                    clientcode = "ALL";
                }
                DataTable reportInfo = CommonFunction.GetArchivalReportConnectionInfo("PAR01", clientcode);
                DataRow itemRow = reportInfo.Rows[0];
                Session["CurArchivalSelectedRptClient"] = Session["s_CurSelectedClientImgArc"];
                Session["CurArchivalSelectedRptCode"] = itemRow["RPT_ReportCode"];
                Session["CurArchivalSelectedSiteCode"] = Session["s_CurSelectedSiteRejDec"];
                //Assign Report Session
                Session["CurArchivalSelectedBusdate"] = Session["s_CurSelectedBusdateRejDec"];
                Session["CurArchivalSelectedBatchNo"] = Session["s_CurSelectedBatchNoRejDec"];
                Session["CurArchivalSelectedCheckNo"] = Session["s_CurSelectedCheckNoImgArc"];
                Session["CurArchivalSelectedUIC"] = Session["s_CurSelectedUICImgArc"];

                ReportCode = Session["CurArchivalSelectedRptCode"].ToString(); // Replace with actual retrieval logic
                ReportClient = Session["CurArchivalSelectedRptClient"].ToString(); // Replace with actual retrieval logic

                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadCR", "openCR();", true);
                //if (isImageExist())
                //{
                //    if (curRepresented == "False")
                //    {
                //        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadLeftImage", "LoadLeftImage();", true);
                //        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadRightImage", "LoadRightImage();", true);
                //    }
                //    else
                //    {
                //        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadLeftImage2", "LoadLeftImage2();", true);
                //        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadRightImage2", "LoadRightImage2();", true);
                //    }
                //}
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveInward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }
        }
        #endregion
    }
}