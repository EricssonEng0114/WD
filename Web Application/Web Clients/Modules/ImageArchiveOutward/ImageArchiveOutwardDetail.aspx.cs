using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using UBPC.Web.Common;
using UBPCWeb.Modules.Reports;
//using static System.Net.Mime.MediaTypeNames;

namespace UBPCWeb.Modules.ImageArchiveOutward
{
    public partial class ImageArchiveOutwardDetail : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private string clientList = string.Empty;
        private bool isPageValid = false;
        private bool isForUnisys = false;
        protected int displayBatchHeader = 0;

        public DateTime curBusdate = DateTime.Now;
        public String curBatchDir = string.Empty;
        public String curBatchNo = string.Empty;
        public String curPresentingBSB = string.Empty;
        public String curSelectedClient = string.Empty;
        public String curWsID = string.Empty;
        public String curBundleID = string.Empty;
        public String curSite = string.Empty;
        public String curRepresented = string.Empty;
        public String curUIC = string.Empty;
        public String curProcMode = string.Empty;
        public String curItemType = string.Empty;
        public String curDin = string.Empty;
        public String curTransportID = string.Empty;
        public string curTransSeqNum = string.Empty;
        public String currentTransNum = string.Empty;

        public bool curAllowBreakdown = false;
        public bool curAllowTolerance = false;

        public string curYearMonth = string.Empty;
        public string curTableName = string.Empty;
        public string curArchivePath = string.Empty;
        public string curIISVirtualPath = string.Empty;
        public string curType = string.Empty;
        public string fileDirectory = string.Empty;

        public String imageExist = string.Empty;
        public String curPaymentMode = string.Empty;
        public int curTransNo = 0;
        public int curTotalStubCount = 0;
        public decimal curTotalStubAmt = 0;
        public int curTotalChqCount = 0;
        public decimal curTotalChqAmt = 0;

        protected string ReportCode { get; set; }
        protected string ReportClient {  get; set; }

        private List<int> sequence;
        private int currentIndex;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Session["ImageArchiveOutwardVirtualStubImage"] = null;
                HttpContext.Current.Session["Logcaller_Inward_Outward"] = "ImageArchiveOutward";
                AssignSessionValue();

                try
                {
                    fileDirectory = curIISVirtualPath + "\\IFS_" + curTransportID + "\\";
                }
                catch(Exception ex)
                {
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.ImageArchiveOutward;
                    log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                    log.UserName = userID;
                    log.Severity = LogEventType.Error;
                    log.Message = ex.Message;
                    log.Exception = ex;
                    log.Write();
                }


                if (curRepresented == "True")
                {
                    string virtualDirectory = "~/" + curIISVirtualPath + "\\IFS_" + curTransportID + "\\";
                    string physicalPath = HttpContext.Current.Server.MapPath(virtualDirectory);
                    string outwardImagePath = physicalPath + curBatchDir;

                    ProcessImage(curUIC, outwardImagePath, Session["s_CurSelectedClientImgArc"].ToString(), curUIC);
                }

                if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
                {
                    PageValidatorResult validatorResult;
                    validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "ImageArchiveOutward");
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
                    curSite = Session["s_CurSelectedSiteImgArc"].ToString();

                    //Run First Time
                    if (!IsPostBack)
                    {
                        HttpContext.Current.Session["currentTransNum"] = currentTransNum;//HttpContext.Current.Session["s_CurSelectedTransNoImgArc"];

                        //Assign Transaction Sequence
                        DataTable dtTransSeqNum = CommonFunction.GetTransSeqNum();
                        List<int> sequenceList = GetSequenceList(dtTransSeqNum);
                        List<string> itemTypeList = GetItemTypeList(dtTransSeqNum);
                        HttpContext.Current.Session["CurrentTransSeqNumList"] = sequenceList;
                        HttpContext.Current.Session["CurrentItemTypeList"] = itemTypeList;

                        if (Session["CurrentTransSeqNumList"].ToString() != null && HttpContext.Current.Session["CurrentItemTypeList"].ToString() != null)
                        {
                            List<int> sequence = (List<int>)HttpContext.Current.Session["CurrentTransSeqNumList"];
                            int currentSequence = Convert.ToInt32(curTransSeqNum);//get the current transSeqNum
                            int currentSequenceIndex = GetCurrent(sequence, currentSequence);


                            if (!itemTypeList.Contains("B"))//transaction doesn't include BH then disable button
                            {
                                disableViewBHButton();
                            }
                            else
                            {
                                enableViewBHButton();
                            }

                            if (currentSequenceIndex == 0)
                            {
                                disablePrevButton();
                                disableViewBHButton();
                            }
                            else if (currentSequenceIndex == sequence.Count - 1)
                            {
                                disableNextButton();
                            }
                            

                        }

                        Session["isInward"] = false;
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
                log.Caller = LogCallerID.ImageArchiveOutward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/ImageArchiveOutward", false);
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
                    DataTable dtDetail = CommonFunction.GetSelectedImgArchiveOutward(webDBConnStr.ToString(), false);
                    Session["imageArchiveOutwardDT"] = dtDetail;

                    if (((DataTable)Session["imageArchiveOutwardDT"]).Rows.Count > 0)
                    {
                        //Update Reject Reason Title
                        //lblRejReason.Text = "REJECT REASON: " + Session["s_CurSelectedRejectReason"].ToString();

                        //Get Virtual Directory for IFS Path
                        int curTransportID = Convert.ToInt16(((DataTable)Session["imageArchiveOutwardDT"]).Rows[0]["ITM_TransportID"].ToString().PadLeft(2, '0'));

                        Session["VirDirForIFSPath"] = CommonFunction.GetDirectoryForIFSPath(curTransportID, curSite);

                        Session["curTransItemsCount"] = ((DataTable)Session["imageArchiveOutwardDT"]).Rows.Count;
                        //Default Top Image as selected
                        //btnSelectTop.Enabled = false;
                        //btnSelectTop.Text = "Selected";

                        //ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "setBackgroun", "setTopImgBg();", true);
                        Session["topItemPtr"] = 0;
                        //Show Data on screen
                        RefreshItemPage();
                    }
                    else
                    {
                        //Redirect to index page
                        Logger.Write(false, LogCallerID.ImageArchiveOutward, curSelectedClient, "Validate Image Archive Outward", "Unable to retrieve item details", LogEventType.Error, userID);
                        Response.Redirect("/ImageArchiveOutward", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }
                else
                {
                    //Redirect to index page
                    Logger.Write(false, LogCallerID.ImageArchiveOutward, curSelectedClient, "Validate Image Archive Outward", "Invalid session value for Image Archive Outward.", LogEventType.Error, userID);
                    Response.Redirect("/ImageArchiveOutward", false);
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
                ContentPlaceHolder MainContent = (ContentPlaceHolder)this.Master.FindControl("MainAdminContent");
                if (Convert.ToInt16(Session["curTransItemsCount"]) > 0)
                {
                    //Display Top Image Item
                    DataRow itemTop = ((DataTable)Session["imageArchiveOutwardDT"]).Rows[Convert.ToInt16(Session["topItemPtr"])];

                    //by default select the top field 
                    DisplaySelItemFieldsAndImage(Convert.ToBoolean(Session["topItemSelected"]));

                    //Load top image here
                    //Call to ImageHandlerLeft.ashx

                    //if (isImageExist() ||
                    //    (HttpContext.Current.Session["s_CurSelectedProcModeImgArc"].ToString() == "C"
                    //&& HttpContext.Current.Session["s_CurSelectedItemTypeImgArc"].ToString() == "S"))
                    //{
                        if (curRepresented == "False")
                        {
                            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadLeftImage", "LoadLeftImage();", true);
                            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadRightImage", "LoadRightImage();", true);
                        }
                        else
                        {
                            if (Session["s_CurSelectedItemTypeImgArc"].ToString() == "S")
                            {
                                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadLeftImage", "LoadLeftImage();", true);
                                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadRightImage", "LoadRightImage();", true);
                            }
                            else
                            {
                                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadLeftImage2", "LoadLeftImage2();", true);
                                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadRightImage2", "LoadRightImage2();", true);
                            }
                        }
                    //}

                    //if there is a bottom Item display it's image and amount in bottom Pannel
                    //otherwise hide bottom Pannel
                    if ((Convert.ToInt16(Session["topItemPtr"]))  < Convert.ToInt16(Session["curTransItemsCount"]))
                    //if ((Convert.ToInt16(Session["topItemPtr"]))  < Convert.ToInt16(Session["curTransItemsCount"]) 
                    //    && ((DataTable)Session["rejectedMainItemDTBot"]).Rows.Count > 0)
                    {
                        DataRow itemBottom = ((DataTable)Session["imageArchiveOutwardDT"]).Rows[(Convert.ToInt16(Session["topItemPtr"]))];

                        //Load Bottom Image
                        if (Session["OutwardRearImgOffSet"].ToString() != "0" && Session["OutwardRearImgSize"].ToString() != "0")
                        {
                            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadRightImage", "LoadRightImage();", true);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveOutward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/ImageArchiveOutward", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        private void DisplaySelItemFieldsAndImage(bool isTopItem)
        {
            try
            {


                Session["curItemPtr"] = (Convert.ToInt16(Session["topItemPtr"])) + ((isTopItem) ? 0 : 1);
                DataRow itemRow = ((DataTable)Session["imageArchiveOutwardDT"]).Rows[Convert.ToInt16(Session["curItemPtr"])];
                Session["OutwardFrontImgOffSet"] = itemRow["ITM_Fr_ImgOffset"].ToString();
                Session["OutwardFrontImgSize"] = itemRow["ITM_Fr_ImgSize"].ToString();
                Session["OutwardRearImgOffSet"] = itemRow["ITM_Rr_ImgOffset"].ToString();
                Session["OutwardRearImgSize"] = itemRow["ITM_Rr_ImgSize"].ToString();

                bool showVirtualStubCondition = (Session["s_CurSelectedProcModeImgArc"].ToString() == "C" && Session["s_CurSelectedItemTypeImgArc"].ToString() == "S")
                    || (curRepresented == "True" && Session["s_CurSelectedItemTypeImgArc"].ToString() == "S");

                if (showVirtualStubCondition)
                {
                    getVirtualStubImage();
                }

                //Store Current Image info to Session to Load selected image
                if (isTopItem)
                {
                    //current item point = top item
                    //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
                    if (curRepresented == "False")
                    {
                        if (Session["s_CurSelectedProcModeImgArc"].ToString() == "C" && Session["s_CurSelectedItemTypeImgArc"].ToString() == "S")
                        {
                            Session["CurSelectedTopImage"] = "C:\\Temp\\" + curBusdate + "|" + curUIC + "|1|0";
                        }
                        else
                        {
                            //ImageConverter.RetrieveJPEGImageByteByOffset
                            Session["CurSelectedTopImage"] = fileDirectory.ToString().Trim() + "|" + itemRow["ITM_ImageFileName"].ToString() + "|1|0|"
                                                             + itemRow["ITM_Fr_ImgOffset"].ToString() + "|" + itemRow["ITM_Fr_ImgSize"].ToString()
                                                             + "|" + itemRow["ITM_Rr_ImgOffset"].ToString() + "|" + itemRow["ITM_Rr_ImgSize"].ToString() + "|0";
                        }
                    }
                    else
                    {
                        Session["CurSelectedTopImage"] = "C:\\Temp\\" + curBusdate + "|" + curUIC + "|1|0";
                    }

                    ////Check if bottom item available
                    if (Convert.ToInt16(Session["curItemPtr"]) < Convert.ToInt16(Session["curTransItemsCount"]))
                    {
                        if (Session["s_CurSelectedProcModeImgArc"].ToString() == "C" && Session["s_CurSelectedItemTypeImgArc"].ToString() == "S")
                        {
                            Session["CurSelectedBottomImage"] = "C:\\Temp\\" + curBusdate + "|" + curUIC + "|0|0";
                        }
                        else
                        {
                            Session["CurSelectedBottomImage"] = fileDirectory.ToString().Trim() + "|" + itemRow["ITM_ImageFileName"].ToString() + "|0|0|"
                                     + itemRow["ITM_Fr_ImgOffset"].ToString() + "|" + itemRow["ITM_Fr_ImgSize"].ToString()
                                     + "|" + itemRow["ITM_Rr_ImgOffset"].ToString() + "|" + itemRow["ITM_Rr_ImgSize"].ToString() + "|0";
                        }
                    }
                    else
                    {
                        Session["CurSelectedBottomImage"] = "C:\\Temp\\" + curBusdate + "|" + curUIC + "|0|0";
                    }

                }
                else
                {
                    //cutrent item pointer - 1 = top item info
                    DataRow topItmRow = ((DataTable)Session["imageArchiveOutwardDT"]).Rows[(Convert.ToInt16(Session["curItemPtr"])) - 1];
                    if (curRepresented == "False")
                    {
                        if (Session["s_CurSelectedProcModeImgArc"].ToString() == "C" && Session["s_CurSelectedItemTypeImgArc"].ToString() == "S")
                        {
                            Session["CurSelectedTopImage"] = "C:\\Temp\\" + curBusdate + "|" + curUIC + "|1|0";
                        }
                        else
                        {
                            Session["CurSelectedTopImage"] = fileDirectory.ToString().Trim() + "|" + itemRow["ITM_ImageFileName"].ToString() + "|1|0|"
                                                             + itemRow["ITM_Fr_ImgOffset"].ToString() + "|" + itemRow["ITM_Fr_ImgSize"].ToString()
                                                             + "|" + itemRow["ITM_Rr_ImgOffset"].ToString() + "|" + itemRow["ITM_Rr_ImgSize"].ToString() + "|0";
                        }
                    }
                    else
                    {
                        Session["CurSelectedTopImage"] = "C:\\Temp\\" + curBusdate + "|" + curUIC + "|1|0";
                    }

                    //current item point = bottom item
                    if (((DataTable)Session["imageArchiveOutwardDT"]).Rows.Count > 0
                        && curRepresented == "False")
                    {
                        if (Session["s_CurSelectedProcModeImgArc"].ToString() == "C" && Session["s_CurSelectedItemTypeImgArc"].ToString() == "S")
                        {
                            Session["CurSelectedBottomImage"] = "C:\\Temp\\" + curBusdate + "|" + curUIC + "|0|0";
                        }
                        else
                        {
                            Session["CurSelectedBottomImage"] = fileDirectory.ToString().Trim() + "|" + itemRow["ITM_ImageFileName"].ToString() + "|0|0|"
                                                         + itemRow["ITM_Fr_ImgOffset"].ToString() + "|" + itemRow["ITM_Fr_ImgSize"].ToString()
                                                         + "|" + itemRow["ITM_Rr_ImgOffset"].ToString() + "|" + itemRow["ITM_Rr_ImgSize"].ToString() + "|0";
                        }
                    }
                    else
                    {
                        Session["CurSelectedBottomImage"] = "C:\\Temp\\" + curBusdate + "|" + curUIC + "|0|0";
                    }

                }


                //lblBusDate.Text = curBusdate.ToShortDateString();
                Session["transNum"] = curTransNo.ToString();
                Session["batchNum"] = curBatchNo.ToString();
                Session["refNum"] = itemRow["ITM_RefNUm"].ToString();
                Session["ref1"] = itemRow["ITM_Ref1"].ToString();
                Session["ref2"] = itemRow["ITM_Ref2"].ToString();
                Session["ref3"] = itemRow["ITM_Ref3"].ToString();
                Session["ref4"] = itemRow["ITM_Ref4"].ToString();
                Session["depositorACNum"] = itemRow["ITM_Fld12"].ToString();
                Session["amount"] = itemRow["ITM_Amount"].ToString();
                Session["tranCode"] = itemRow["ITM_Fld2"].ToString();
                Session["chequeAccNum"] = itemRow["ITM_Fld3"].ToString();
                Session["chequeBSB"] = itemRow["ITM_Fld4"].ToString();
                Session["chequeSerial"] = itemRow["ITM_Fld5"].ToString();
                Session["din"] = itemRow["ITM_DIN"].ToString();
                Session["presentingBSB"] = itemRow["ITM_Fld10"].ToString();
                Session["worksource"] = itemRow["ITM_WsID"].ToString();
                Session["bundleNum"] = itemRow["ITM_BundleID"].ToString();
                Session["processingMode"] = itemRow["ITM_ProcMode"].ToString();
                Session["primaryName"] = itemRow["PrimaryName"].ToString();
                Session["1stSecondaryName"] = itemRow["SecondaryName1"].ToString();
                Session["2ndSecondaryName"] = itemRow["SecondaryName2"].ToString();
                Session["3rdSecondaryName"] = itemRow["SecondaryName3"].ToString();

                lblBusDate.Text = Convert.ToDateTime(itemRow["ITM_BusDate"].ToString()).ToString("dd/MM/yyyy");
                lblTransNum.Text = itemRow["ITM_TransNum"].ToString();
                //lblBatchDir.Text = curBatchDir;
                lblBatchNum.Text = curBatchNo;
                lblRefNum.Text = itemRow["ITM_RefNum"].ToString();
                lblPERunNumber.Text = itemRow["ITM_PERunNum"].ToString();
                lblRunNum.Text = itemRow["ITM_RunNum"].ToString();

                #region Product Type
                DataTable dtTransSeqNum = CommonFunction.GetTransSeqNum();
                List<int> lockBoxList = GetIsLockBoxList(dtTransSeqNum);
                List<int> sequenceList = GetSequenceList(dtTransSeqNum);
                int currentSequence = Convert.ToInt32(Session["s_CurSelectedTransSeqNumImgArc"]);//get the current transSeqNum
                int currentSequenceIndex = GetCurrent(sequenceList, currentSequence);//get current transSeqNum index first

                if (lockBoxList[currentSequenceIndex].ToString() == "1")
                {
                    lblProductType.Text = "Lockbox";
                }
                else if (itemRow["BST_IsBPC"].ToString() == "True")
                {
                    lblProductType.Text = "BPC";
                }
                else if (itemRow["ITM_WsID"].ToString() == "021")
                {
                    lblProductType.Text = "Normal";
                }
                else if (itemRow["ITM_WsID"].ToString() == "022")
                {
                    lblProductType.Text = "MOPO";
                }
                else if (itemRow["ITM_WsID"].ToString() == "023")
                {
                    lblProductType.Text = "NCI";
                }
                else if (itemRow["ITM_WsID"].ToString() == "024")
                {
                    lblProductType.Text = "Foreign Cheque";
                }
                else
                {
                    lblProductType.Text = "N/A";
                }
                #endregion

                lblRef1.Text = itemRow["ITM_Ref1"].ToString();
                lblRef2.Text = itemRow["ITM_Ref2"].ToString();
                lblRef3.Text = itemRow["ITM_Ref3"].ToString();
                lblRef4.Text = itemRow["ITM_Ref4"].ToString();
                lblBoxNo.Text = itemRow["BH_BarCodeBoxNo"].ToString();
                lblDepositorACNum.Text = itemRow["ITM_Fld12"].ToString();
                string stringAmount = itemRow["ITM_Amount"].ToString();
                decimal decimalAmount = decimal.Parse(stringAmount);
                lblAmount.Text = itemRow["ITM_ItemType"].ToString() == "B" ? "" : String.Format("{0:N2}", decimalAmount);
                lblTranCode.Text = (itemRow["ITM_ItemType"].ToString() == "S") ? "" : itemRow["ITM_Fld2"].ToString();
                if (itemRow["ITM_ItemType"].ToString() == "S")
                {
                    lblChequeAccNum.Text = (itemRow["ITM_ItemType"].ToString() == "S") ? "" : itemRow["ITM_Fld3"].ToString();
                }
                else if (itemRow["ITM_ItemType"].ToString() == "B")
                {
                    lblChequeAccNum.Text = "";
                }
                else
                {
                    lblChequeAccNum.Text = itemRow["ITM_Fld3"].ToString();
                }
                
                lblChequeBSB.Text = (itemRow["ITM_ItemType"].ToString() == "S") ? "" : itemRow["ITM_Fld4"].ToString();
                lblChequeSerial.Text = (itemRow["ITM_ItemType"].ToString() == "S") ? "" : itemRow["ITM_Fld5"].ToString();
                lblDin.Text = itemRow["ITM_DIN"].ToString();

                Session["s_CurSelectedItemTypeImgArc"] = itemRow["ITM_ItemType"].ToString();

                lblPresentingBSB.Text = (itemRow["ITM_ItemType"].ToString() == "B") ? itemRow["ITM_Fld1"].ToString() : itemRow["ITM_Fld10"].ToString();
                lblWorksource.Text = itemRow["ITM_WsID"].ToString();
                lblBundleNum.Text = itemRow["ITM_BundleID"].ToString();
                lblProcessingMode.Text = itemRow["ITM_ProcMode"].ToString();
                lblPrimaryName.Text = itemRow["PrimaryName"].ToString();
                lbl1stSecondaryName.Text = itemRow["SecondaryName1"].ToString();
                lbl2ndSecondaryName.Text = itemRow["SecondaryName2"].ToString();
                lbl3rdSecondaryName.Text = itemRow["SecondaryName3"].ToString();
                lblNCFTag.Text = itemRow["ITM_NCF"].ToString() == "" ? "" : lblNCFTag.Text = itemRow["ITM_NCF"].ToString();

                if (itemRow["ITM_Returned"].ToString() == "True")
                {
                    itemReturned.Visible = true;
                    lblReturnStatus.Text = "Yes";
                    lblReturnedReason.Text = itemRow["ITM_ReturnedReason"].ToString().Trim();
                    lblReturnedDate.Text = Convert.ToDateTime(itemRow["ITM_ReturnedDate"]).ToString("dd/MM/yyyy").Trim();
                }
                else
                {
                    lblReturnStatus.Text = "No";
                    itemReturned.Visible = false;
                }


                if (itemRow["ITM_Rejected"].ToString() == "True")
                {
                    if (itemRow["ITM_ItemType"].ToString() == "B")
                    {
                        title.Style["visibility"] = "hidden";
                        lblRejectedReason.Text = "";
                    }
                    else
                    {
                        title.Style["visibility"] = "visible";
                        lblRejectedReason.Text = itemRow["ITM_RejectReason"].ToString();
                        lblItemStatus.ForeColor = Color.Black;
                        lblItemStatus.Text = "Item Processing Status: " +
                            "<span style='background-color:red; padding:5px 30px; " +
                            "border-radius:5px;width:26%; display:inline-block;text-align:center;'>" + "REJECTED" + "</span>";
                    }
                }
                else
                {
                    if (itemRow["ITM_ItemType"].ToString() == "B")
                    {
                        title.Style["visibility"] = "hidden";
                        lblRejectedReason.Text = "";
                    }
                    else
                    {
                        title.Style["visibility"] = "visible";
                        lblRejectedReason.Text = "";
                        lblItemStatus.ForeColor = Color.Black;
                        lblItemStatus.Text = "Item Processing Status: " +
                            "<span style='background-color:limegreen; padding:5px 30px; " +
                            "border-radius:5px;width:26%; display:inline-block;text-align:center;'>" + "OK" + "</span>";
                    }
                }

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveOutward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/ImageArchiveOutward", false);
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
                    || string.IsNullOrEmpty(Session["s_CurSelectedBatchDirImgArc"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedBatchNoRejDec"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedTransNoImgArc"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedSiteImgArc"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedClientImgArc"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedWsIDImgArc"].ToString())
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

                curBusdate = Convert.ToDateTime(Session["s_CurSelectedBusdateRejDec"]);
                curBatchDir = Session["s_CurSelectedBatchDirImgArc"].ToString();
                curBatchNo = Session["s_CurSelectedBatchNoRejDec"].ToString();
                currentTransNum = Session["s_CurSelectedTransNoImgArc"].ToString() == "0" ? Session["CurrentTransNum"].ToString() : Session["s_CurSelectedTransNoImgArc"].ToString();
                curTransNo = Convert.ToInt32(Session["s_CurSelectedTransNoImgArc"].ToString());
                curPresentingBSB = Session["s_CurSelectedPresentingBSBImgArc"].ToString();
                curWsID = Session["s_CurSelectedWsIDImgArc"].ToString();
                curBundleID = Session["s_CurSelectedNewBundleImgArc"].ToString();

                curYearMonth = Session["s_CurSelectedYearMonthImgArc"].ToString();
                curTableName = Session["s_CurSelectedTableNameImgArc"].ToString();
                curArchivePath = Session["s_CurSelectedArchivePathImgArc"].ToString();
                curIISVirtualPath = Session["s_CurSelectedIISVirtualPathImgArc"].ToString() + "\\" + curBusdate.ToString("yyyyMMdd");
                curType = Session["s_CurSelectedTypeImgArc"].ToString();

                curSelectedClient = Session["s_CurSelectedClientImgArc"].ToString();
                curSite = Session["s_CurSelectedSiteImgArc"].ToString();

                curUIC = Session["s_CurSelectedUICImgArc"].ToString();

                curRepresented = Session["s_CurSelectedRepresentedImgArc"].ToString();
                curProcMode = Session["s_CurSelectedProcModeImgArc"].ToString();
                curItemType = Session["s_CurSelectedItemTypeImgArc"].ToString();
                curDin = Session["s_CurSelectedDinImgArc"].ToString();

                curTransportID = Session["s_CurSelectedTransportIDImgArc"].ToString().PadLeft(2, '0');
                curTransSeqNum = Session["s_CurSelectedTransSeqNumImgArc"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region Transaction Sequence Number
        private static List<int> GetSequenceList(DataTable dataTable)
        {
            List<int> sequenceList = new List<int>();
            int seqNum;
            foreach (DataRow row in dataTable.Rows)
            {
                if (int.TryParse(row["TransSeqNum"].ToString(), out seqNum))
                {
                    sequenceList.Add(seqNum);
                }
            }

            return sequenceList;
        }

        private static List<string> GetItemTypeList(DataTable dataTable)
        {
            List<string> itemTypeList = new List<string>();

            foreach (DataRow row in dataTable.Rows)
            {
                string itemType = row["ItemType"].ToString();
                itemTypeList.Add(itemType);
            }

            return itemTypeList;
        }

        private static List<int> GetIsLockBoxList(DataTable dataTable)
        {
            List<int> isLockBox = new List<int>();
            int lockbox;
            foreach (DataRow row in dataTable.Rows)
            {
                if (int.TryParse(row["isLockBox"].ToString(), out lockbox))
                {
                    isLockBox.Add(lockbox);
                }
            }

            return isLockBox;
        }

        public static int GetIndex(List<int> sequence, int number)
        {
            if (sequence == null)
            {
                throw new ArgumentException("Sequence cannot be null.");
            }

            return sequence.IndexOf(number);
        }

        public int GetCurrent(List<int> sequenceList, int currentSequenceValue)
        {
            return sequenceList.IndexOf(currentSequenceValue);
        }

        public int GetCurrentItemType(List<string> itemTypeSequenceList, string currentItemTypeValue)
        {
            return itemTypeSequenceList.IndexOf(currentItemTypeValue);
        }

        public static int GetNextIndex(List<int> sequence, ref int currentIndex)
        {
            if (sequence == null || sequence.Count == 0)
            {
                throw new ArgumentException("Sequence cannot be null or empty.");
            }

            if (currentIndex < sequence.Count - 1)
            {
                currentIndex++;
            }

            return currentIndex;
        }

        public static int GetNextItemTypeIndex(List<string> sequence, ref int currentIndex)
        {
            if (sequence == null || sequence.Count == 0)
            {
                throw new ArgumentException("Sequence cannot be null or empty.");
            }

            if (currentIndex >= 0)
            {
                currentIndex++;
            }

            return currentIndex;
        }

        public static int GetPreviousIndex(List<int> sequence, ref int currentIndex)
        {
            if (sequence == null || sequence.Count == 0)
            {
                throw new ArgumentException("Sequence cannot be null or empty.");
            }

            if (currentIndex > 0)
            {
                currentIndex--;
            }

            //return sequence[currentIndex];
            return currentIndex;
        }
        public static int GetPreviousItemTypeIndex(List<string> sequence, ref int currentIndex)
        {
            if (sequence == null || sequence.Count == 0)
            {
                throw new ArgumentException("Sequence cannot be null or empty.");
            }

            if (currentIndex > 0)
            {
                currentIndex--;
            }

            return currentIndex;
        }
        #endregion


        private void InitialiseSessionValue()
        {
            Session["topItemSelected"] = false;
            Session["curTransItemsCount"] = 0;
            Session["curItemPtr"] = 0;
            Session["topItemPtr"] = 0;
            Session["bottomItemPtr"] = 0;
            Session["curSelectedStub"] = 0;

            Session["VirDirForIFSPath"] = "";
            Session["imageArchiveOutwardDT"] = new DataTable();

        }

        private void ClearSessionValue()
        {
            Session["CurSelectedTopImage"] = null;
            Session["CurSelectedBottomImage"] = null;
            Session["topItemSelected"] = null;
            Session["curTransItemsCount"] = null;
            Session["curItemPtr"] = null;
            Session["topItemPtr"] = null;
            Session["bottomItemPtr"] = null;
            Session["curSelectedStub"] = null;

            Session["VirDirForIFSPath"] = null;
            Session["imageArchiveOutwardDT"] = null;
            Session["Base64ImageTopFrontOutward"] = null;
            Session["Base64ImageTopRearOutward"] = null;
            Session["Base64ImageBatchHeader"] = null;

            Session["currentTransNum"] = "";

            Session["OutwardFrontImgOffSet"] = null;
            Session["OutwardFrontImgSize"] = null;
            Session["OutwardRearImgOffSet"] = null;
            Session["OutwardRearImgSize"] = null;

            Session["ImageArchiveInwardFrontImg"] = null;
            Session["ImageArchiveInwardRearImg"] = null;
            Session["ImageArchiveOutwardVirtualStubImage"] = null;
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
                                        tifImg.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                        outputFileName = UIC + ".jpg";
                                        File.WriteAllBytes(directory + "\\" + outputFileName, ms.ToArray());
                                        Session["ImageArchiveInwardFrontJPEG"] = ms.ToArray();
                                    }
                                    else
                                    {
                                        if (index == 1)
                                        {
                                            tifImg.Save(ms, System.Drawing.Imaging.ImageFormat.Tiff);
                                            outputFileName = UIC + ".FIM";
                                            File.WriteAllBytes(directory + "\\" + outputFileName, ms.ToArray());
                                            Session["ImageArchiveInwardFrontImg"] = ms.ToArray();
                                        }
                                        else
                                        {
                                            tifImg.Save(ms, System.Drawing.Imaging.ImageFormat.Tiff);
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
                        log.Caller = LogCallerID.ImageArchiveOutward;
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
                log.Caller = LogCallerID.ImageArchiveOutward;
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
            bool isRepresented = (HttpContext.Current.Session["s_CurSelectedRepresentedImgArc"].ToString() == "True");
            bool isCurSelectedProcModeC = HttpContext.Current.Session["s_CurSelectedProcModeImgArc"].ToString() == "C";
            bool isCurSelectedItemTypeS = HttpContext.Current.Session["s_CurSelectedItemTypeImgArc"].ToString() == "S";

            int topItemPtr = Convert.ToInt16(HttpContext.Current.Session["topItemPtr"]);
            DataTable detailDT;
            if (id.Contains("TOP"))
            {
                detailDT  = ((DataTable)HttpContext.Current.Session["imageArchiveOutwardDT"]);
            }
            else
            {
                //detailDT = ((DataTable)HttpContext.Current.Session["rejectedMainItemDTBot"]);
                detailDT = ((DataTable)HttpContext.Current.Session["imageArchiveOutwardDT"]);
            }

            DataRow itemRow = id.Contains("TOP") ? detailDT.Rows[topItemPtr] : detailDT.Rows[topItemPtr];

            string curSelTopImg = id.Contains("TOP") ? HttpContext.Current.Session["CurSelectedTopImage"].ToString() :
                HttpContext.Current.Session["CurSelectedBottomImage"].ToString();
            string uic = HttpContext.Current.Session["s_CurSelectedUICImgArc"].ToString();
            DateTime busdate = Convert.ToDateTime(HttpContext.Current.Session["s_CurSelectedBusdateRejDec"]);

            //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
            string[] strArr = curSelTopImg.Split('|');
            string isFront = strArr[2].ToString();
            string isJpeg = strArr[3].ToString();

            string virDir = string.Empty;
            string imgFileName = string.Empty;

            string frontOffset = string.Empty;
            string frontSize = string.Empty;
            string rearOffset = string.Empty;
            string rearSize = string.Empty;


            if ((isCurSelectedProcModeC && isCurSelectedItemTypeS) || isRepresented)
            {
                virDir = "C:\\Temp\\" + ((DateTime)busdate).ToString("yyyyMMdd");
                imgFileName = uic;
            }
            else if (!isRepresented)
            {
                virDir = strArr[0].ToString();
                imgFileName = strArr[1].ToString();

                frontOffset = strArr[4].ToString();
                frontSize = strArr[5].ToString();
                rearOffset = strArr[6].ToString();
                rearSize = strArr[7].ToString();

            }


            string updIsFront = isFront;
            string updIsRear = isFront;
            string updIsJpeg = isJpeg;

            if (id.Contains("Toggle"))
            {
                if (isJpeg.Equals("1"))
                {
                    //Switch to tiff
                    frontOffset = itemRow["ITM_Fr_ImgOffset"].ToString();
                    frontSize = itemRow["ITM_Fr_ImgSize"].ToString();
                    rearOffset = itemRow["ITM_Rr_ImgOffset"].ToString();
                    rearSize = itemRow["ITM_Rr_ImgSize"].ToString();
                    updIsJpeg = "0";
                }
                else
                {
                    //Switch to jpeg
                    frontOffset = itemRow["ITM_JPEGFr_ImgOffset"].ToString();
                    frontSize = itemRow["ITM_JPEGFr_ImgSize"].ToString();
                    rearOffset = itemRow["ITM_JPEGRr_ImgOffset"].ToString();
                    rearSize = itemRow["ITM_JPEGRr_ImgSize"].ToString();
                    updIsJpeg = "1";
                }
            }

            if (id.Contains("Flip"))
            {
                updIsFront = isFront.Equals("1") ? "0" : "1";
                updIsRear = isFront.Equals("0") ? "1" : "0";
            }
            string updSessionValue = string.Empty;
            string updSessionValueRear = string.Empty;


            if ((isCurSelectedProcModeC && isCurSelectedItemTypeS) || isRepresented)
            {
                imgFileName = uic;
                updSessionValue = virDir + "|" + imgFileName + "|" + updIsFront + "|" + updIsJpeg;
                updSessionValueRear = virDir + "|" + imgFileName + "|" + updIsFront + "|" + updIsJpeg;
            }
            else if (!isRepresented)
            {
                updSessionValue = virDir + "|" + imgFileName + "|" + updIsFront + "|" + updIsJpeg + "|"
                    + (string.IsNullOrEmpty(frontOffset) ? "0" : frontOffset) + "|" + (string.IsNullOrEmpty(frontSize) ? "0" : frontSize)
                    + "|" + (string.IsNullOrEmpty(rearOffset) ? "0" : rearOffset) + "|" + (string.IsNullOrEmpty(rearSize) ? "0" : rearSize) + "|0";
                updSessionValueRear = virDir + "|" + imgFileName + "|" + updIsRear + "|" + updIsJpeg + "|"
                    + (string.IsNullOrEmpty(frontOffset) ? "0" : frontOffset) + "|" + (string.IsNullOrEmpty(frontSize) ? "0" : frontSize)
                    + "|" + (string.IsNullOrEmpty(rearOffset) ? "0" : rearOffset) + "|" + (string.IsNullOrEmpty(rearSize) ? "0" : rearSize) + "|0";
            }


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
            if (HttpContext.Current.Session["OutwardFrontImgOffSet"].ToString() != "0" && HttpContext.Current.Session["OutwardFrontImgSize"].ToString() != "0"
                && HttpContext.Current.Session["OutwardRearImgOffSet"].ToString() != "0" && HttpContext.Current.Session["OutwardRearImgSize"].ToString() != "0"
                || (isRepresented && !isCurSelectedItemTypeS))
            {
                if (id.Contains("TOP"))
                {
                    //get current jpeg or tiff then switch

                    if (HttpContext.Current.Session["s_CurSelectedRepresentedImgArc"].ToString() == "False")
                    {
                        return "/ImageHandlerLeft.ashx?id=" + randomID;
                    }
                    else
                    {
                        HttpContext.Current.Session["Logcaller_Inward_Outward"] = "ImageArchiveOutward";
                        return "/ImageHandlerInward.ashx?id=" + randomID;
                    }
                }
                else
                {
                    if (HttpContext.Current.Session["s_CurSelectedRepresentedImgArc"].ToString() == "False")
                    {
                        return "/ImageHandlerRight.ashx?id=" + randomID;
                    }
                    else
                    {
                        HttpContext.Current.Session["Logcaller_Inward_Outward"] = "ImageArchiveOutward";
                        return "/ImageHandlerInwardRear.ashx?id=" + randomID;
                    }
                }
            }
            else
            {
                return null;
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
            string batchdir = HttpContext.Current.Session["CurArchivalSelectedBatchDir"].ToString();
            string batchnum = HttpContext.Current.Session["CurArchivalSelectedBatchNo"].ToString();
            string transnum = HttpContext.Current.Session["CurArchivalSelectedTransNo"].ToString();
            string isCurrentItemOnly = HttpContext.Current.Session["CurArchivalSelectedItemOnly"].ToString();


            string url = string.Empty;
            if (isCurrentItemOnly == "true")
            {
                string din = HttpContext.Current.Session["CurArchivalSelectedDin"].ToString();
                url = "/Modules/ImageArchiveOutward/PrintArchivalReportViewer.aspx?v=" + DateTime.Now.Ticks;
            }
            else
            {
                HttpContext.Current.Session["CurArchivalSelectedDin"] = "";
                url = "/Modules/ImageArchiveOutward/PrintArchivalReportViewer.aspx?v=" + DateTime.Now.Ticks;
            }
            
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
                Session["IsClearSearchCriteria_Outward"] = "False";
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveOutward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadSpinner", "loadSpinner();", true);
            Response.Redirect("/ImageArchiveOutward", false);
            Context.ApplicationInstance.CompleteRequest();
        }
        protected void btnViewBatchHeader_Click(object sender, EventArgs e)
        {
            try
            {
                HttpContext.Current.Session["s_CurSelectedTransNoImgArc"] = "0";
                HttpContext.Current.Session["s_CurSelectedTransSeqNumImgArc"] = "0";
                Session["s_CurSelectedItemTypeImgArc"] = "B";
                LoadItemDataTableInfo();
                enableNextButton();
                disablePrevButton();
                disableViewBHButton();
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveOutward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }
        }

        protected void btnPrevItem_Click(object sender, EventArgs e)
        {
            Session["s_CurSelectedTransNoImgArc"] = Session["currentTransNum"].ToString();
            try
            {
                DataTable dtTransSeqNum = CommonFunction.GetTransSeqNum();
                List<int> sequenceList = GetSequenceList(dtTransSeqNum);
                List<string> itemTypeList = GetItemTypeList(dtTransSeqNum);
                HttpContext.Current.Session["CurrentTransSeqNumList"] = sequenceList;
                HttpContext.Current.Session["CurrentItemTypeList"] = itemTypeList;

                int currentSequence = Convert.ToInt32(curTransSeqNum);//get the current transSeqNum
                string currentItemType = curItemType;
                int currentSequenceIndex = GetCurrent(sequenceList, currentSequence);//get current transSeqNum index first
                int currentItemTypeSequenceIndex = currentSequenceIndex;
                //int currentItemTypeSequenceIndex = GetCurrentItemType(itemTypeList, curItemType);//get current ItemType index first
                //int currentItemTypeSequenceIndex2 = currentItemTypeSequenceIndex == currentSequenceIndex ? currentItemTypeSequenceIndex : currentSequenceIndex;

                int previousIndex = GetPreviousIndex(sequenceList, ref currentSequenceIndex);//then only get previousIndex
                int previousItemTypeIndex = GetPreviousItemTypeIndex(itemTypeList, ref currentItemTypeSequenceIndex);//then only get nextItemTypeIndex

                string newSelectedTransSeqNum = sequenceList[previousIndex].ToString();
                string newSelectedItemType = itemTypeList[previousItemTypeIndex].ToString();
                Session["ImgArcCurrentIndex"] = previousIndex;
                Session["s_CurSelectedTransSeqNumImgArc"] = newSelectedTransSeqNum;
                Session["s_CurSelectedItemTypeImgArc"] = newSelectedItemType;

                if (previousIndex == 0)
                {
                    if (itemTypeList.Contains("B"))
                    {
                        Session["s_CurSelectedTransNoImgArc"] = "0";
                    }
                    else
                    {
                        Session["s_CurSelectedTransNoImgArc"] = Session["currentTransNum"];
                    }
                    disablePrevButton();
                    disableViewBHButton();
                }
                else
                {
                    Session["s_CurSelectedTransNoImgArc"] = Session["currentTransNum"];
                    enablePrevButton();
                    displayViewBHButton();
                }
                enableNextButton();

                LoadItemDataTableInfo();
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveOutward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }
        }
        protected void btnNextItem_Click(object sender, EventArgs e)
        {
            Session["s_CurSelectedTransNoImgArc"] = Session["currentTransNum"].ToString();
            try
            {
                DataTable dtTransSeqNum = CommonFunction.GetTransSeqNum();
                List<int> sequenceList = GetSequenceList(dtTransSeqNum);
                List<string> itemTypeList = GetItemTypeList(dtTransSeqNum);
                HttpContext.Current.Session["CurrentTransSeqNumList"] = sequenceList;
                HttpContext.Current.Session["CurrentItemTypeList"] = itemTypeList;

                int currentSequence = Convert.ToInt32(curTransSeqNum);//get the current transSeqNum
                string currentItemType = curItemType;
                int currentSequenceIndex = GetCurrent(sequenceList, currentSequence);//get current transSeqNum index first
                int currentItemTypeSequenceIndex = currentSequenceIndex;
                //int currentItemTypeSequenceIndex = GetCurrentItemType(itemTypeList, curItemType);//get current ItemType index first
                //int currentItemTypeSequenceIndex2 = currentItemTypeSequenceIndex == currentSequenceIndex ? currentItemTypeSequenceIndex : currentSequenceIndex;

                int nextIndex = GetNextIndex(sequenceList, ref currentSequenceIndex);//then only get nextIndex
                int nextItemTypeIndex = GetNextItemTypeIndex(itemTypeList, ref currentItemTypeSequenceIndex);//then only get nextItemTypeIndex

                string newSelectedTransSeqNum = sequenceList[nextIndex].ToString();
                string newSelectedItemType = itemTypeList[nextItemTypeIndex].ToString();
                Session["ImgArcCurrentIndex"] = nextIndex;
                Session["s_CurSelectedTransSeqNumImgArc"] = newSelectedTransSeqNum;
                Session["s_CurSelectedItemTypeImgArc"] = newSelectedItemType;

                if (nextIndex == sequenceList.Count - 1)
                {
                    disableNextButton();
                }
                else if (nextIndex < sequenceList.Count - 1)
                {
                    enableNextButton();
                }
                enablePrevButton();
                displayViewBHButton();

                LoadItemDataTableInfo();

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveOutward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

        }

        protected void btnPrintTransaction_Click(object sender, EventArgs e)
        {
            try
            {
                string clientcode = Session["s_CurSelectedClientImgArc"].ToString().Trim();
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
                Session["CurArchivalSelectedSiteCode"] = Session["s_CurSelectedSiteImgArc"];
                //Assign Report Session
                Session["CurArchivalSelectedBusdate"] = Session["s_CurSelectedBusdateRejDec"];
                Session["CurArchivalSelectedBatchDir"] = Session["s_CurSelectedBatchDirImgArc"];
                Session["CurArchivalSelectedBatchNo"] = Session["s_CurSelectedBatchNoRejDec"];
                Session["CurArchivalSelectedTransNo"] = Session["currentTransNum"];
                Session["CurArchivalSelectedItemOnly"] = false;
                Session["CurArchivalIsRejected"] = false;
                Session["CurArchivalIsMultiple"] = "False";
                Session["CurArchivalSelectedTransSeqNum"] = "";//Session["s_CurSelectedTransSeqNumImgArc"];

                ReportCode = Session["CurArchivalSelectedRptCode"].ToString(); // Replace with actual retrieval logic
                ReportClient = Session["CurArchivalSelectedRptClient"].ToString(); // Replace with actual retrieval logic

                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadCR", "openCR();", true);
                //if (isImageExist() ||
                //        (HttpContext.Current.Session["s_CurSelectedProcModeImgArc"].ToString() == "C"
                //    && HttpContext.Current.Session["s_CurSelectedItemTypeImgArc"].ToString() == "S"))
                //{
                    if (curRepresented == "False")
                    {
                        if(HttpContext.Current.Session["s_CurSelectedProcModeImgArc"].ToString() == "C"
                            && HttpContext.Current.Session["s_CurSelectedItemTypeImgArc"].ToString() == "S")
                        {
                            getVirtualStubImage();
                        }
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadLeftImage", "LoadLeftImage();", true);
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadRightImage", "LoadRightImage();", true);
                    }
                    else
                    {
                        if(Session["s_CurSelectedItemTypeImgArc"].ToString() == "S")
                        {
                            getVirtualStubImage();
                            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadLeftImage", "LoadLeftImage();", true);
                            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadRightImage", "LoadRightImage();", true);
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadLeftImage2", "LoadLeftImage2();", true);
                            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadRightImage2", "LoadRightImage2();", true);
                        }
                    //}
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveOutward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }
        }

        protected void btnPrintItemOnly_Click(object sender, EventArgs e)
        {
            try
            {
                string clientcode = Session["s_CurSelectedClientImgArc"].ToString().Trim();
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
                Session["CurArchivalSelectedSiteCode"] = Session["s_CurSelectedSiteImgArc"];
                //Assign Report Session
                Session["CurArchivalSelectedBusdate"] = Session["s_CurSelectedBusdateRejDec"];
                Session["CurArchivalSelectedBatchDir"] = Session["s_CurSelectedBatchDirImgArc"];
                Session["CurArchivalSelectedBatchNo"] = Session["s_CurSelectedBatchNoRejDec"];
                Session["CurArchivalSelectedTransNo"] = Session["s_CurSelectedTransNoImgArc"];
                Session["CurArchivalSelectedItemOnly"] = true;
                Session["CurArchivalIsRejected"] = false;
                Session["CurArchivalIsMultiple"] = "False";
                Session["CurArchivalSelectedDin"] = Session["s_CurSelectedDinImgArc"];
                Session["CurArchivalSelectedTransSeqNum"] = Session["s_CurSelectedTransSeqNumImgArc"];

                ReportCode = Session["CurArchivalSelectedRptCode"].ToString(); // Replace with actual retrieval logic
                ReportClient = Session["CurArchivalSelectedRptClient"].ToString(); // Replace with actual retrieval logic

                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadCR", "openCR();", true);
                //if (isImageExist() ||
                //        (HttpContext.Current.Session["s_CurSelectedProcModeImgArc"].ToString() == "C"
                //    && HttpContext.Current.Session["s_CurSelectedItemTypeImgArc"].ToString() == "S"))
                //{
                    if (curRepresented == "False")
                    {
                        if (HttpContext.Current.Session["s_CurSelectedProcModeImgArc"].ToString() == "C"
                            && HttpContext.Current.Session["s_CurSelectedItemTypeImgArc"].ToString() == "S")
                        {
                            getVirtualStubImage();
                        }
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadLeftImage", "LoadLeftImage();", true);
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadRightImage", "LoadRightImage();", true);
                    }
                    else
                    {
                        if (Session["s_CurSelectedItemTypeImgArc"].ToString() == "S")
                        {
                            getVirtualStubImage();
                            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadLeftImage", "LoadLeftImage();", true);
                            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadRightImage", "LoadRightImage();", true);
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadLeftImage2", "LoadLeftImage2();", true);
                            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadRightImage2", "LoadRightImage2();", true);
                        }
                    //}
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveOutward;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }
        }

        private void enableNextButton()
        {
            btnNextItem.CssClass = btnNextItem.CssClass.Replace(" btn[disabled]", " btn-success").Trim();
            btnNextItem.Enabled = true;
        }
        
        private void disableNextButton()
        {
            btnNextItem.Enabled = false;
            btnNextItem.CssClass = btnNextItem.CssClass.Replace(" btn-success", " btn[disabled]").Trim();
        }

        private void enablePrevButton()
        {
            btnPrevItem.Enabled = true;
            btnPrevItem.CssClass = btnPrevItem.CssClass.Replace(" btn[disabled]", " btn-success").Trim();
        }

        private void disablePrevButton()
        {
            btnPrevItem.Enabled = false;
            btnPrevItem.CssClass = btnPrevItem.CssClass.Replace(" btn-success", " btn[disabled]").Trim();
        }
        private void enableViewBHButton()
        {
            btnViewBatchHeader.Enabled = true;
            btnViewBatchHeader.CssClass = btnPrevItem.CssClass.Replace(" btn[disabled]", " btn-success").Trim();
        }

        private void disableViewBHButton()
        {
            btnViewBatchHeader.Enabled = false;
            btnViewBatchHeader.CssClass = btnViewBatchHeader.CssClass.Replace(" btn-success", " btn[disabled]").Trim();
        }

        private void displayViewBHButton()
        {
            Session["s_CurSelectedTransNoImgArc"] = Session["currentTransNum"];
            DataTable dtTransSeqNum = CommonFunction.GetTransSeqNum();
            List<int> sequenceList = GetSequenceList(dtTransSeqNum);
            List<string> itemTypeList = GetItemTypeList(dtTransSeqNum);
            HttpContext.Current.Session["CurrentTransSeqNumList"] = sequenceList;
            HttpContext.Current.Session["CurrentItemTypeList"] = itemTypeList;
            if (Session["CurrentTransSeqNumList"].ToString() != null && HttpContext.Current.Session["CurrentItemTypeList"].ToString() != null)
            {
                if (!itemTypeList.Contains("B"))//transaction doesn't include BH then disable button
                {
                    disableViewBHButton();
                }
                else
                {
                    enableViewBHButton();
                }
            }
        }

        private bool isImageExist()
        {
            if (Session["OutwardFrontImgOffSet"].ToString() != "0" && Session["OutwardFrontImgSize"].ToString() != "0"
                        && Session["OutwardRearImgOffSet"].ToString() != "0" && Session["OutwardRearImgSize"].ToString() != "0")
            {
                imageExist = "True";
                return true;
            }
            else if (Session["ImageArchiveInwardFrontImg"] != null && Session["ImageArchiveInwardRearImg"] != null)
            {
                imageExist = "True";
                return true;
            }
            else if (curRepresented == "True")
            {
                imageExist = "True";
                return true;
            }
            else
            {
                imageExist = "False";
                return false;
            }
        }

        public void getVirtualStubImage()
        {
            string virtualDirectory = "/Content/images/";
            string virtualStubImagePath = HttpContext.Current.Server.MapPath(virtualDirectory);
            string fullPath = virtualStubImagePath + "VirtualStubImage.jpg";
            using (FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                // Create a memory stream to hold the image data
                using (System.Drawing.Image image = System.Drawing.Image.FromStream(fileStream, true, true))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        // Save the image to the memory stream in JPEG format
                        image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                        // Return the byte array from the memory stream
                        Session["ImageArchiveOutwardVirtualStubImage"] = memoryStream.ToArray();
                    }
                }
            }
        }


        #endregion
    }

}