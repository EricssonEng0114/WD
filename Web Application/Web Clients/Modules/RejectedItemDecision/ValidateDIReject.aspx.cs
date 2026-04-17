using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using UBPC.Web.Common;


namespace UBPCWeb.Modules.RejectedItemDecision
{
    public partial class ValidateDIReject : System.Web.UI.Page
    {
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
        public String curPresentingBSB = string.Empty;
        public String curSelectedClient = string.Empty;
        public String curSite = string.Empty;
        public bool curAllowBreakdown = false;
        public bool curAllowTolerance = false;

        public String curPaymentMode = string.Empty;
        public int curTransNo = 0;
        public int curTotalStubCount = 0;
        public decimal curTotalStubAmt = 0;
        public int curTotalChqCount = 0;
        public decimal curTotalChqAmt = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    Session["acceptEnabled"] = false;
                }
                AssignSessionValue();

                if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
                {
                    PageValidatorResult validatorResult;
                    validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "RejectedItemDecision");
                    isPageValid = validatorResult.Valid;

                    if (!isPageValid)
                    {
                        Session["s_GeneralMsg"] = validatorResult.ReturnMessage;
                        Response.Redirect("/Home", false);
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

                    curSelectedClient = Session["s_CurSelectedClientRejDec"].ToString();
                    curSite = Session["s_CurSelectedSiteRejDec"].ToString();

                    //Run First Time
                    if (!IsPostBack)
                    {
                        InitialiseSessionValue();
                        PerformTransaction();//Process transaction for first time
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
                log.Caller = LogCallerID.RejectedItemDecision;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/WebDecision", false);
                Context.ApplicationInstance.CompleteRequest();
            }

        }

        #region Helper Function

        private void PerformTransaction()
        {
            try
            {
                #region Check if Transaction in use or completed or locked
                //check if current status is 'C' or 'I' status, then refersh listing page
                bool isCompletedOrLocked = false;
                isCompletedOrLocked = CommonFunction.checkCompletedTrans(userID, curBusdate, curBatchNo, curTransNo);

                if (isCompletedOrLocked)
                {
                    //Redirect back to listing page
                    //Prompt "Unable to proceed as selected transaction is completed or being in used by others.";   
                    Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Validate Rejected Item Decision", "Unable to proceed as selected transaction is completed or being in used by others", LogEventType.Error, userID);
                    Response.Redirect("/WebDecision", false);
                    Context.ApplicationInstance.CompleteRequest();
                }

                //Update status for this operator as 'I'
                bool isUpdateStatusSuccess = false;
                isUpdateStatusSuccess = !CommonFunction.UpdateTransactionStatus(curSelectedClient, "I", userID, curBusdate, curBatchNo, curTransNo).Equals(0);

                if (!isUpdateStatusSuccess)
                {
                    //Redirect back to listing page
                    //Prompt "Unable to proceed as selected transaction is completed or being in used by others.";
                    Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Validate Rejected Item Decision", "Unable to proceed as selected transaction is completed or being in used by others", LogEventType.Error, userID);
                    Response.Redirect("/WebDecision", false);
                    Context.ApplicationInstance.CompleteRequest();
                }

                #endregion

                //Show item detail info
                LoadItemDataTableInfo();
            }
            catch (Exception ex) 
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.RejectedItemDecision;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = "Error at Perform Transaction:"+ ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/WebDecision", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        private void LoadItemDataTableInfo()
        {
            try
            {
                //Validate session value 
                if (ValidateSessionValue())
                {
                    //Populate item info to web control, default top image item
                    //Initialise Common Item Datatable for first time
                    DataTable dtDetail = CommonFunction.GetDetailItemInfo("DI", Session["selClientDBConnStr"].ToString());
                   Session["rejectedMainItemDT"] = dtDetail;

                    //assign stub to cheque image value if virtual stub existed
                    RemapItemDataTable();

                    if (((DataTable)Session["rejectedMainItemDT"]).Rows.Count > 0)
                    {
                        //Update Reject Reason Title
                        lblRejReason.Text = "REJECT REASON: " + Session["s_CurSelectedRejectReason"].ToString();
                        
                        //Get Virtual Directory for IFS Path
                        int curTransportID = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Rows[0]["ITM_TransportID"].ToString().PadLeft(2, '0'));

                        Session["VirDirForIFSPath"] = CommonFunction.GetDirectoryForIFSPath(curTransportID, curSite);

                        Session["curTransItemsCount"] = ((DataTable)Session["rejectedMainItemDT"]).Rows.Count;
                        //Default Top Image as selected
                        btnSelectTop.Enabled = false;
                        btnSelectTop.Text = "Selected";
                        //PE-WD-23-001 Obsoleted Version of jQuery in Use
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "setBackgroun", "setTopImgBg();", true);
                        //CR015-20 load remark from previous reviewer
                        if (!Session["s_ReviewNo"].ToString().Equals("1"))
                        {
                            txtARemark.InnerText = Session["s_RejectedRemark"].ToString();
                        }
                        Session["topItemPtr"] = 0;
                        //Show Data on screen
                        RefreshItemPage();
                    }
                    else
                    {
                        //Redirect to index page
                        Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Validate Rejected Item Decision", "Unable to retrieve item details", LogEventType.Error, userID);
                        Response.Redirect("/WebDecision", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }
                else
                {
                    //Redirect to index page
                    Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Validate Rejected Item Decision", "Invalid session value for DI reject re.", LogEventType.Error, userID);
                    Response.Redirect("/WebDecision", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.RejectedItemDecision;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/WebDecision", false);
                Context.ApplicationInstance.CompleteRequest();

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
                    txtTopItemAmount.Text = String.Format("{0:N}", Convert.ToDecimal(itemTop["ITM_Amount"].ToString()));

                    //by default select the top field 
                    DisplaySelItemFieldsAndImage(Convert.ToBoolean(Session["topItemSelected"]));

                    //Load top image here
                    //Call to ImageHandlerTop.ashx
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadTopImage", "LoadTopImage();", true);
                    
                    //if this is a System-Generated stub, flip the image
                    if (Convert.ToBoolean(itemTop["SystemGen"]))
                    {
                        // flipImage("{0}", "{1}", "{2}");
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "flipTopImage", String.Format("flipImage('{0}', '{1}', '{2}');", "DI", "TOP", "#DIImgViewerTop"), true);

                    }

                    //if there is a bottom Item display it's image and amount in bottom Pannel
                    //otherwise hide bottom Pannel
                    if (Convert.ToInt16(Session["topItemPtr"]) + 1 < Convert.ToInt16(Session["curTransItemsCount"]))
                    {
                        //Show Bottom Panel
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "hidebottom", String.Format("showhideBottom('{0}');", "1"), true);

                        DataRow itemBottom = ((DataTable)Session["rejectedMainItemDT"]).Rows[Convert.ToInt16(Session["topItemPtr"]) + 1];
                        txtBottomItemAmount.Text = String.Format("{0:N}", Convert.ToDecimal(itemBottom["ITM_Amount"].ToString()));

                        //Load Bottom Image
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadBottomImage", "LoadBottomImage();", true);

                        ////if this is a System-Generated stub, flip the image
                        if (Convert.ToBoolean(itemBottom["SystemGen"]))
                        {
                            // flipImage("{0}", "{1}", "{2}");
                            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "flipBottomImage", String.Format("flipImage('{0}', '{1}', '{2}');", "DI", "BOT", "#DIImgViewerBottom"), true);
                        }
                    }
                    else
                    {
                        //Hide bottom part
                        //hide div bottom panel
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "hidebottom", String.Format("showhideBottom('{0}');", "0"), true);
                    }
                }
            }
            catch (Exception ex) 
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.RejectedItemDecision;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message =  ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/WebDecision", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        private void DisplaySelItemFieldsAndImage(bool isTopItem)
        {
            try
            {
                if (CommonFunction.IsTransReset())
                {
                    Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Tranaction Reset", "Unable to proceed as selected transaction is already reset", LogEventType.Error, userID);
                    Response.Redirect("/WebDecision", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    Session["curItemPtr"] = Convert.ToInt16(Session["topItemPtr"]) + ((isTopItem) ? 0 : 1);

                    DataRow itemRow = ((DataTable)Session["rejectedMainItemDT"]).Rows[Convert.ToInt16(Session["curItemPtr"])];

                    #region Load Image
                    //Store Current Image info to Session to Load selected image
                    if (isTopItem)
                    {
                        //current item point = top item
                        //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
                        Session["CurSelectedTopImage"] = Session["VirDirForIFSPath"].ToString().Trim() + "|" + itemRow["ITM_ImageFileName"].ToString() + "|1|0|"
                                                         + itemRow["ITM_Fr_ImgOffset"].ToString() + "|" + itemRow["ITM_Fr_ImgSize"].ToString()
                                                         + "|" + itemRow["ITM_Rr_ImgOffset"].ToString() + "|" + itemRow["ITM_Rr_ImgSize"].ToString()
                                                         + "|" + itemRow["ITM_TransSeqNum"].ToString();

                        //Check if bottom item available
                        if (Convert.ToInt16(Session["curItemPtr"]) + 1 < Convert.ToInt16(Session["curTransItemsCount"]))
                        {
                            DataRow bottomItmRow = ((DataTable)Session["rejectedMainItemDT"]).Rows[Convert.ToInt16(Session["curItemPtr"]) + 1];
                            //cutrent item pointer + 1 = bottom item info
                            Session["CurSelectedBottomImage"] = Session["VirDirForIFSPath"].ToString().Trim() + "|" + bottomItmRow["ITM_ImageFileName"].ToString() + "|1|0|"
                                                                 + bottomItmRow["ITM_Fr_ImgOffset"].ToString() + "|" + bottomItmRow["ITM_Fr_ImgSize"].ToString()
                                                                 + "|" + bottomItmRow["ITM_Rr_ImgOffset"].ToString() + "|" + bottomItmRow["ITM_Rr_ImgSize"].ToString()
                                                                 + "|" + bottomItmRow["ITM_TransSeqNum"].ToString();
                        }
                        else
                        {
                            Session["CurSelectedBottomImage"] = "";
                        }

                    }
                    else
                    {
                        //cutrent item pointer - 1 = top item info
                        DataRow topItmRow = ((DataTable)Session["rejectedMainItemDT"]).Rows[Convert.ToInt16(Session["curItemPtr"]) - 1];
                        Session["CurSelectedTopImage"] = Session["VirDirForIFSPath"].ToString().Trim() + "|" + topItmRow["ITM_ImageFileName"].ToString() + "|1|0|"
                                                        + topItmRow["ITM_Fr_ImgOffset"].ToString() + "|" + topItmRow["ITM_Fr_ImgSize"].ToString()
                                                        + "|" + topItmRow["ITM_Rr_ImgOffset"].ToString() + "|" + topItmRow["ITM_Rr_ImgSize"].ToString()
                                                        + "|" + topItmRow["ITM_TransSeqNum"].ToString();

                        //current item point = bottom item
                        Session["CurSelectedBottomImage"] = Session["VirDirForIFSPath"].ToString().Trim() + "|" + itemRow["ITM_ImageFileName"].ToString() + "|1|0|"
                                                         + itemRow["ITM_Fr_ImgOffset"].ToString() + "|" + itemRow["ITM_Fr_ImgSize"].ToString()
                                                         + "|" + itemRow["ITM_Rr_ImgOffset"].ToString() + "|" + itemRow["ITM_Rr_ImgSize"].ToString()
                                                         + "|" + itemRow["ITM_TransSeqNum"].ToString();
                    }
                    #endregion

                    #region DI Format Information
                    //Hide all DI Fields 
                    HideAllDIFields(false);

                    //Display Relevant DI Format Information 
                    if (itemRow["ITM_ItemType"].ToString().Equals("S"))
                    {
                        // ITM_DI_Format
                        if (!string.IsNullOrEmpty(itemRow["ITM_DI_Format"].ToString().Trim()))
                        {
                            //Get FI Format
                            DataTable DIFormatDT = CommonFunction.GetDIFormatDetail(Convert.ToInt16(itemRow["ITM_DI_Format"].ToString().Trim()));

                            if (DIFormatDT.Rows.Count > 0)
                            {
                                foreach (DataRow dr in DIFormatDT.Rows) 
                                {
                                    string diRefNo = dr["DIF_DIRefNo"].ToString().Trim();
                                    string diLabel = dr["DIF_Label"].ToString().Trim();
                                    string diValRuleNo = dr["DIF_ValRuleNo"].ToString().Trim();
                                    string diDesc = dr["DID_Desc"].ToString().Trim();

                                    AssignDIReference(itemRow["ITM_DI_Format"].ToString().Trim(),diRefNo, diDesc, diLabel, itemRow);
                                }
                            }
                            else
                            {
                                txtNA.Visible = true;
                                HideAllDIFields(true);
                            }
                        }
                        else
                        {
                            txtNA.Visible = true;
                            HideAllDIFields(true);
                        }
                    }
                    else
                    {
                        txtNA.Visible = true;
                        HideAllDIFields(true);
                    }
                    #endregion

                    //Display info fields on selected item 
                    //calculate total stub n cheque count & amount
                    curTotalStubAmt = Convert.ToDecimal(((DataTable)Session["rejectedMainItemDT"]).Compute("SUM(ITM_Amount)", "ITM_ItemType='S'"));
                    curTotalStubCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='S'"));
                    lblTotalStubCount.Text = "Total Stub Count: " + curTotalStubCount.ToString();
                    txtTotalStubAmt.Text = String.Format("{0:N}", curTotalStubAmt);

                    curTotalChqAmt = Convert.ToDecimal(((DataTable)Session["rejectedMainItemDT"]).Compute("SUM(ITM_Amount)", "ITM_ItemType='C'"));
                    curTotalChqCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='C'"));
                    lblTotalChequeCount.Text = "Total Cheque Count: " + curTotalChqCount.ToString();
                    txtTotalChequeAmt.Text = String.Format("{0:N}", curTotalChqAmt);

                    //Transaction Info Tab
                    lblAllowBD.Text = curAllowBreakdown ? "Allow Breakdown:Yes" : "Allow Breakdown:No";
                    lblAllowTol.Text = curAllowTolerance ? ", Allow Tolerance:Yes" : ", Allow Tolerance:No";

                    lblBusdate.Text = curBusdate.ToShortDateString();
                    lblTransNo.Text = curTransNo.ToString();
                    lblBatchDir.Text = curBatchDir;
                    lblBatchNo.Text = curBatchNo;

                    //Selected Item Info Tab
                    bool selItemIsStub = itemRow["ITM_ItemType"].ToString().Trim().Equals("S");
                    bool selItemIsCheq = !selItemIsStub;
                    lblSelType.Text = selItemIsStub ? "STUB" : "CHEQUE";
                    lblSeqNo.Text = itemRow["ITM_TransSeqNum"].ToString();
                    lblDIN.Text = itemRow["ITM_DIN"].ToString();
                    txtSelAcctNo.Text = itemRow["ITM_Fld12"].ToString().Trim();

                    if (selItemIsStub)
                    {
                        Session["curSelectedStub"] = Session["curItemPtr"];
                    }

                    //Selected Account Info
                    //if this item does not have account details display InfoNotAvail 
                    //else display the details
                    bool noAif = itemRow.IsNull("AIF_AcctNum");
                    if (!noAif)
                    {
                        txtA1stName.InnerText = itemRow["firstName"].ToString();
                        txtA2ndAcctName.InnerText = itemRow["secondName"].ToString();
                        txtA3rdAcctName.InnerText = itemRow["thirdName"].ToString();
                        txtA4thAcctName.InnerText = itemRow["fourthName"].ToString();
                    }
                    else
                    {
                        txtA1stName.InnerText = "NO ACCOUNT INFORMATION";
                        txtA2ndAcctName.InnerText = "NO ACCOUNT INFORMATION";
                        txtA3rdAcctName.InnerText = "NO ACCOUNT INFORMATION";
                        txtA4thAcctName.InnerText = "NO ACCOUNT INFORMATION";
                    }
                    
                    switch (itemRow["ITM_ProcMode"].ToString().Trim())
                    {
                        case "S":
                            curPaymentMode = "SINGLE";
                            break;
                        case "M":
                        case "N":
                            curPaymentMode = "MULTIPLE";
                            break;
                        case "C":
                            curPaymentMode = "CHEQUES ONLY WITH";
                            break;
                    }

                    //Enabled and disable next and prev
                    if (Convert.ToInt16(Session["curTransItemsCount"]) > 2)
                    {
                        btnNextPage.Enabled = (Convert.ToInt16(Session["topItemPtr"]) + 2 < Convert.ToInt16(Session["curTransItemsCount"]));
                        btnPrevPage.Enabled = (Convert.ToInt16(Session["topItemPtr"]) - 2 >= 0);

                        btnPrevPage.CssClass = !btnPrevPage.Enabled ? "btn btn-info disabled" : "btn btn-info";
                        btnNextPage.CssClass = !btnNextPage.Enabled ? "btn btn-info disabled" : "btn btn-info";
                    }
                    else
                    {
                        btnNextPage.Visible = (Convert.ToInt16(Session["topItemPtr"]) + 2 < Convert.ToInt16(Session["curTransItemsCount"]));
                        btnPrevPage.Visible = (Convert.ToInt16(Session["topItemPtr"]) - 2 >= 0);
                    }

                    if (itemRow["ITM_ProcMode"].ToString().Trim().Equals("M") || itemRow["ITM_ProcMode"].ToString().Trim().Equals("N"))
                    {
                        Session["acceptEnabled"] = Session["acceptEnabled"] ?? false;

                        if (Convert.ToBoolean(Session["acceptEnabled"]) == true || !btnNextPage.Visible || (btnNextPage.Visible && !btnNextPage.Enabled))
                        {
                            Session["acceptEnabled"] = true;
                            btnAccept.Enabled = true;
                            btnAccept.CssClass = "btn btn-success";
                        }

                        else
                        {
                            Session["acceptEnabled"] = false;
                            btnAccept.Enabled = false;
                            btnAccept.CssClass = "btn btn-info disabled";
                        }
                    }                    
                }
            }
            catch (Exception ex) 
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.RejectedItemDecision;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/WebDecision", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        private void AssignDIReference(string diFormat, string diRefNo, string diDesc, string diLabel,DataRow itemRow) 
        {
            switch (diRefNo)
            {
                case "1":
                    PnlRef1.Visible = true;
                    lblRef1.Text = diDesc + "(" + diLabel + "):";
                    //LOAN Account will display from Payment Type column, else display reference 1
                    txtRef1.Text = diFormat.Equals("1") 
                        ? string.IsNullOrEmpty(itemRow["ITM_DI_PaymentType"].ToString()) ? "LP" : itemRow["ITM_DI_PaymentType"].ToString() 
                        : itemRow["ITM_Ref1"].ToString();
                    break;
                case "2":
                    PnlRef2.Visible = true;
                    lblRef2.Text = diDesc + "(" + diLabel + "):";
                    txtRef2.Text = itemRow["ITM_Ref2"].ToString();
                    break;
                case "3":
                    PnlRef3.Visible = true;
                    lblRef3.Text = diDesc + "(" + diLabel + "):";
                    txtRef3.Text = itemRow["ITM_Ref3"].ToString();
                    break;
                case "4":
                    PnlRef4.Visible = true;
                    lblRef4.Text = diDesc + "(" + diLabel + "):";
                    txtRef4.Text = itemRow["ITM_Ref4"].ToString();
                    break;
                case "5":
                    PnlRef5.Visible = true;
                    lblRef5.Text = diDesc + "(" + diLabel + "):";
                    txtRef5.Text = itemRow["ITM_Ref5"].ToString();
                    break;
                case "6":
                    PnlRef6.Visible = true;
                    lblRef6.Text = diDesc + "(" + diLabel + "):";
                    txtRef6.Text = itemRow["ITM_Ref6"].ToString();
                    break;
                case "7":
                    PnlRef7.Visible = true;
                    lblRef7.Text = diDesc + "(" + diLabel + "):";
                    txtRef7.Text = itemRow["ITM_Ref7"].ToString();
                    break;
                case "8":
                    PnlRef8.Visible = true;
                    lblRef8.Text = diDesc + "(" + diLabel + "):";
                    txtRef8.Text = itemRow["ITM_Ref8"].ToString();
                    break;
                case "9":
                    PnlRef9.Visible = true;
                    lblRef9.Text = diDesc + "(" + diLabel + "):";
                    txtRef9.Text = itemRow["ITM_Ref9"].ToString();
                    break;
                case "10":
                    PnlRef10.Visible = true;
                    lblRef10.Text = diDesc + "(" + diLabel + "):";
                    txtRef10.Text = itemRow["ITM_Ref10"].ToString();
                    break;
            }
        }

        private void HideAllDIFields(bool hideNA) 
        {
            txtNA.Visible = hideNA;
            PnlRef1.Visible = false;
            PnlRef2.Visible = false;
            PnlRef3.Visible = false;
            PnlRef4.Visible = false;
            PnlRef5.Visible = false;
            PnlRef6.Visible = false;
            PnlRef7.Visible = false;
            PnlRef8.Visible = false;
            PnlRef9.Visible = false;
            PnlRef10.Visible = false;
        }

        private void RefreshItemPage()
        {
            Session["topItemSelected"] = true;
            DisplayTwoItemsInfo();
            Session["curItemPtr"] = Convert.ToInt16(Session["topItemPtr"]) + ((Convert.ToBoolean(Session["topItemSelected"])) ? 0 : 1);
        }

        private int RemapItemDataTable()
        {
            try
            {
                int rowCount = 0;
                short chq_TransportID = 0;
                string chq_ImageFileName = string.Empty;
                long chq_Fr_ImgOffset = 0;
                long chq_Fr_ImgSize = 0;
                long chq_Rr_ImgOffset = 0;
                long chq_Rr_ImgSize = 0;
                long chq_JPEGFr_ImgOffset = 0;
                long chq_JPEGFr_ImgSize = 0;
                long chq_JPEGRr_ImgOffset = 0;
                long chq_JPEGRr_ImgSize = 0;


                Boolean gotChqImgDetails = false;

                rowCount = ((DataTable)Session["rejectedMainItemDT"]).Rows.Count;
                if (rowCount > 0)
                {
                    //cycle through all items to count the number of good(acct verifiable) stubs
                    //while at it, get a copy of the first cheques's rear image
                    DataTable mainItemDT = ((DataTable)Session["rejectedMainItemDT"]);

                    for (int i = 0; i < rowCount; i++)
                    {
                        //capture the top chq imgdetails
                        if (!gotChqImgDetails && mainItemDT.Rows[i]["ITM_ItemType"].Equals("C"))
                        {
                            gotChqImgDetails = true;
                            chq_TransportID = Convert.ToInt16(mainItemDT.Rows[i]["ITM_TransportID"]);
                            chq_ImageFileName = mainItemDT.Rows[i]["ITM_ImageFileName"].ToString();
                            chq_Fr_ImgOffset = Convert.ToInt64(mainItemDT.Rows[i]["ITM_Fr_ImgOffset"].ToString());
                            chq_Fr_ImgSize = Convert.ToInt64(mainItemDT.Rows[i]["ITM_Fr_ImgSize"].ToString());
                            chq_Rr_ImgOffset = Convert.ToInt64(mainItemDT.Rows[i]["ITM_Rr_ImgOffset"].ToString());
                            chq_Rr_ImgSize = Convert.ToInt64(mainItemDT.Rows[i]["ITM_Rr_ImgSize"].ToString());
                            chq_JPEGFr_ImgOffset = Convert.ToInt64(mainItemDT.Rows[i]["ITM_JPEGFr_ImgOffset"].ToString());
                            chq_JPEGFr_ImgSize = Convert.ToInt64(mainItemDT.Rows[i]["ITM_JPEGFr_ImgSize"].ToString());
                            chq_JPEGRr_ImgOffset = Convert.ToInt64(mainItemDT.Rows[i]["ITM_JPEGRr_ImgOffset"].ToString());
                            chq_JPEGRr_ImgSize = Convert.ToInt64(mainItemDT.Rows[i]["ITM_JPEGRr_ImgSize"].ToString());
                        }
                    }

                    if (gotChqImgDetails)
                    {
                        // cycle through it again and for surrogate stubs (system inserted stubs)
                        // make the rear chq image the stub front image
                        for (int i = 0; i < rowCount; i++)
                        {
                            //initialise sysGen to false
                            ((DataTable)Session["rejectedMainItemDT"]).Rows[i]["SystemGen"] = false;
                            if (((DataTable)Session["rejectedMainItemDT"]).Rows[i]["ITM_ItemType"].ToString().Equals("S") &&
                                (((DataTable)Session["rejectedMainItemDT"]).Rows[i]["ITM_ImageFileName"].ToString().Trim().Length.Equals(0))) //surrogate stubs have  blank ImageFileName
                            {
                                ((DataTable)Session["rejectedMainItemDT"]).Rows[i]["SystemGen"] = true;
                                ((DataTable)Session["rejectedMainItemDT"]).Rows[i]["ITM_TransportID"] = chq_TransportID;
                                ((DataTable)Session["rejectedMainItemDT"]).Rows[i]["ITM_ImageFileName"] = chq_ImageFileName;
                                ((DataTable)Session["rejectedMainItemDT"]).Rows[i]["ITM_Fr_ImgOffset"] = chq_Fr_ImgOffset;
                                ((DataTable)Session["rejectedMainItemDT"]).Rows[i]["ITM_Fr_ImgSize"] = chq_Fr_ImgSize;
                                ((DataTable)Session["rejectedMainItemDT"]).Rows[i]["ITM_Rr_ImgOffset"] = chq_Rr_ImgOffset;
                                ((DataTable)Session["rejectedMainItemDT"]).Rows[i]["ITM_Rr_ImgSize"] = chq_Rr_ImgSize;
                                ((DataTable)Session["rejectedMainItemDT"]).Rows[i]["ITM_JPEGFr_ImgOffset"] = chq_JPEGFr_ImgOffset;
                                ((DataTable)Session["rejectedMainItemDT"]).Rows[i]["ITM_JPEGFr_ImgSize"] = chq_JPEGFr_ImgSize;
                                ((DataTable)Session["rejectedMainItemDT"]).Rows[i]["ITM_JPEGRr_ImgOffset"] = chq_JPEGRr_ImgOffset;
                                ((DataTable)Session["rejectedMainItemDT"]).Rows[i]["ITM_JPEGRr_ImgSize"] = chq_JPEGRr_ImgSize;
                            }
                        }
                    }
                }

                return (rowCount);
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
                if (string.IsNullOrEmpty(Session["s_CurSelectedClientRejDec"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedSiteRejDec"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedBusdateRejDec"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedBatchDirRejDec"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedBatchNoRejDec"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedTransNoRejDec"].ToString())
                    || string.IsNullOrEmpty(Session["s_CurSelectedPresentingRejDec"].ToString()))
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
                curBatchDir = Session["s_CurSelectedBatchDirRejDec"].ToString();
                curBatchNo = Session["s_CurSelectedBatchNoRejDec"].ToString();
                curTransNo = Convert.ToInt32(Session["s_CurSelectedTransNoRejDec"].ToString());
                curPresentingBSB = Session["s_CurSelectedPresentingRejDec"].ToString();

                curSelectedClient = Session["s_CurSelectedClientRejDec"].ToString();
                curSite = Session["s_CurSelectedSiteRejDec"].ToString();

                curAllowBreakdown = Convert.ToBoolean(Session["s_CurSelectedAllowBreakdown"]);
                curAllowTolerance = Convert.ToBoolean(Session["s_CurSelectedAllowTolerance"]);
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

        #endregion

        #region Web Method by Ajax calling

        [WebMethod]
        public static string getImagePath(string id)
        {
            int topItemPtr = Convert.ToInt16(HttpContext.Current.Session["topItemPtr"]);
            DataTable detailDT = ((DataTable)HttpContext.Current.Session["rejectedMainItemDT"]);

            DataRow itemRow = id.Contains("TOP") ? detailDT.Rows[topItemPtr]:detailDT.Rows[topItemPtr + 1];

            string curSelTopImg = id.Contains("TOP") ? HttpContext.Current.Session["CurSelectedTopImage"].ToString() :
                HttpContext.Current.Session["CurSelectedBottomImage"].ToString();

            //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
            string[] strArr = curSelTopImg.Split('|');
            string virDir = strArr[0].ToString().Trim();
            string imgFileName = strArr[1].ToString();
            string isFront = strArr[2].ToString();
            string isJpeg = strArr[3].ToString();

            string frontOffset = strArr[4].ToString();
            string frontSize = strArr[5].ToString();
            string rearOffset = strArr[6].ToString(); ;
            string rearSize = strArr[7].ToString(); ;
            string updIsFront = isFront;
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
            }

            string updSessionValue = virDir + "|" + imgFileName + "|" + updIsFront + "|" + updIsJpeg + "|"
                    + (string.IsNullOrEmpty(frontOffset) ? "0" : frontOffset) + "|" + (string.IsNullOrEmpty(frontSize) ? "0" : frontSize)
                    + "|" + (string.IsNullOrEmpty(rearOffset) ? "0" : rearOffset) + "|" + (string.IsNullOrEmpty(rearSize) ? "0" : rearSize)
                    + "|" + itemRow["ITM_TransSeqNum"].ToString();

            if (id.Contains("TOP"))
            {
                //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
                HttpContext.Current.Session["CurSelectedTopImage"] = updSessionValue;
            }
            else
            {
                //bottom
                HttpContext.Current.Session["CurSelectedBottomImage"] = updSessionValue;
            }

            //Random ID is needed, upon "updatePanel" ajax calling it will not trigger postback, by using 
            //Query string will "postback" therefore able to call to handler to reload image
            string randomID = System.Guid.NewGuid().ToString().Replace("-", "");

            if (id.Contains("TOP"))
            {
                //get current jpeg or tiff then switch
                return "/ImageHandlerTop.ashx?id=" + randomID;
            }
            else 
            {
                return "/ImageHandlerBottom.ashx?id=" + randomID;
            }
        }

        #endregion

        #region Event Handling

        protected void btnBackToListing_Click(object sender, EventArgs e)
        {
            try
            {
                //Update status for this operator as 'I'
                bool isResetStatusSuccess = false;
                isResetStatusSuccess = !CommonFunction.ResetTransactionStatus(curSelectedClient, "W", userID, curBusdate, curBatchNo, curTransNo).Equals(0);
                //Clear all session value upon back to listing
                ClearSessionValue();            
            }
            catch (Exception ex) 
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.RejectedItemDecision;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadSpinner", "loadSpinner();", true);
            Response.Redirect("/WebDecision", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnSelectTop_Click(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "setBackgroun", "setTopBg();", true);

            btnSelectTop.Enabled = false;
            btnSelectTop.Text = "Selected";

            btnSelectBottom.Enabled = true;
            btnSelectBottom.Text = "<i class='fa fa fa-sign-in'></i> Select";

            Session["topItemSelected"] = true;
            DisplayTwoItemsInfo();
            Session["curItemPtr"] = Convert.ToInt16(Session["topItemPtr"]) + ((Convert.ToBoolean(Session["topItemSelected"])) ? 0 : 1);
        }

        protected void btnSelectBottom_Click(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "setBackgrounColor", "setBottomBg();", true);

            btnSelectBottom.Enabled = false;
            btnSelectBottom.Text = "Selected";

            btnSelectTop.Enabled = true;
            btnSelectTop.Text = "<i class='fa fa fa-sign-in'></i> Select";

            Session["topItemSelected"] = false;
            DisplayTwoItemsInfo();
            Session["curItemPtr"] = Convert.ToInt16(Session["topItemPtr"]) + ((Convert.ToBoolean(Session["topItemSelected"])) ? 0 : 1);
        }

        protected void btnNextPage_Click(object sender, EventArgs e)
        {
            try
            {
                if (CommonFunction.IsTransReset())
                {
                    Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Tranaction Reset", "Unable to proceed as selected transaction is already reset", LogEventType.Error, userID);
                    Response.Redirect("/WebDecision", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {

                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "setBackgroun", "setTopBg();", true);

                    btnSelectTop.Enabled = false;
                    btnSelectTop.Text = "Selected";

                    btnSelectBottom.Enabled = true;
                    btnSelectBottom.Text = "<i class='fa fa fa-sign-in'></i> Select";

                    //if top is already pointing to the last item  or second to last item, 
                    //reset the view to the first item of the transaction
                    //else view the next item(s) which starts two items down
                    if (Convert.ToInt16(Session["topItemPtr"]) + 2 >= Convert.ToInt16(Session["curTransItemsCount"]))
                        Session["topItemPtr"] = 0;
                    else
                        Session["topItemPtr"] = Convert.ToInt16(Session["topItemPtr"]) + 2;

                    RefreshItemPage();
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.RejectedItemDecision;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = "Click Next Error:" + ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/WebDecision", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void btnPrevPage_Click(object sender, EventArgs e)
        {
            try
            {
                if (CommonFunction.IsTransReset())
                {
                    Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Tranaction Reset", "Unable to proceed as selected transaction is already reset", LogEventType.Error, userID);
                    Response.Redirect("/WebDecision", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "setBackgroun", "setTopBg();", true);

                    btnSelectTop.Enabled = false;
                    btnSelectTop.Text = "Selected";

                    btnSelectBottom.Enabled = true;
                    btnSelectBottom.Text = "<i class='fa fa fa-sign-in'></i> Select";

                    Session["topItemPtr"] = Convert.ToInt16(Session["topItemPtr"]) - 2;
                    RefreshItemPage();
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.RejectedItemDecision;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = "Click Prev Error:" + ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/WebDecision", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }     

        protected void btnAccept_Click(object sender, EventArgs e)
        {
            try
            {
                if (CommonFunction.IsTransReset())
                {
                    Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Tranaction Reset", "Unable to proceed as selected transaction is already reset", LogEventType.Error, userID);
                    Response.Redirect("/WebDecision", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    //CR015-20 check this transation required multi level approval
                    bool UpdFinal = ProceedToFinalDecision("A");

                    if (UpdFinal)
                    {
                        //Update TBL_REJECTITEMSTATUS
                        string remark = txtARemark.InnerText.ToString().Trim().Length > 0 ? txtARemark.InnerText.ToString() : "";

                        //Update TBL_ITEM -> ITM_REJECTED = 0 , ITM_REJECTREASON = ''
                        //INSERT TBL_FCMSTATUS
                        CommonFunction.UpdatedAcceptedItems(Session["selClientDBConnStr"].ToString(), remark, "", "DI");

                        string msg = string.Format(Resource.msgAcceptedItem, curTransNo.ToString());

                        LogEntry log = new LogEntry();
                        log.Caller = LogCallerID.RejectedItemDecision;
                        log.ClientCode = curSelectedClient;
                        log.UserName = userID;
                        log.Severity = LogEventType.Information;
                        log.Message = msg;
                        //BusDate={0}|BatchNo={1}|BatchDir={2}|TransNo={3}
                        log.Data = string.Format(Resource.strLogTransactionDetails, curBusdate.ToShortDateString(), curBatchNo, curBatchDir, curTransNo.ToString());
                        log.Write();
                    }

                    Response.Redirect("/WebDecision", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.RejectedItemDecision;
                log.ClientCode = curSelectedClient;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = "Click Accept Error:" + ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/WebDecision", false);
                Context.ApplicationInstance.CompleteRequest();

            }
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            try
            {
                if (CommonFunction.IsTransReset())
                {
                    Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Tranaction Reset", "Unable to proceed as selected transaction is already reset", LogEventType.Error, userID);
                    Response.Redirect("/WebDecision", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    //CR015-20 check this transation required multi level approval
                    bool UpdFinal = ProceedToFinalDecision("R");

                    if (UpdFinal)
                    {
                        string remark = txtARemark.InnerText.ToString().Trim().Length > 0 ? txtARemark.InnerText.ToString() : "";

                        //Update TBL_REJECTITEMSTATUS
                        bool isCompleteStatusSuccess = false;
                        isCompleteStatusSuccess = !CommonFunction.CompleteTransactionStatus(curSelectedClient, userID, curBusdate, curBatchNo, curTransNo, "R", remark).Equals(0);

                        string msg = string.Format(Resource.msgItemReject, curTransNo.ToString());

                        LogEntry log = new LogEntry();
                        log.Caller = LogCallerID.RejectedItemDecision;
                        log.ClientCode = curSelectedClient;
                        log.UserName = userID;
                        log.Severity = LogEventType.Information;
                        log.Message = msg;
                        //BusDate={0}|BatchNo={1}|BatchDir={2}|TransNo={3}
                        log.Data = string.Format(Resource.strLogTransactionDetails, curBusdate.ToShortDateString(), curBatchNo, curBatchDir, curTransNo.ToString());
                        log.Write();
                    }

                    Response.Redirect("/WebDecision", false);
                    Context.ApplicationInstance.CompleteRequest();
                }

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.RejectedItemDecision;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = "Click Reject Error:" + ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/WebDecision", false);
                Context.ApplicationInstance.CompleteRequest();

            }
        }

        //CR015-20 check for multi level approval
        public bool ProceedToFinalDecision(string decision)
        {
            bool updFinal = false;
            bool routeToFinal = false;
            string remark = txtARemark.InnerText.ToString().Trim().Length > 0 ? txtARemark.InnerText.ToString() : "";
            bool isForUnisys = Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(Session["s_UserForUnisys"].ToString());

            try
            {
                if (Session["s_RequiredApproval"].ToString().Equals("True") && !isForUnisys)
                {
                    if (!Session["s_ReviewNo"].ToString().Equals("3"))
                    {
                        if (!Session["s_ReviewNo"].ToString().Equals("1"))
                        {
                            routeToFinal = !Session["s_1stDecision"].ToString().Equals(decision) ? true : false;
                            updFinal = Session["s_1stDecision"].ToString().Equals(decision) ? true : false;
                        }

                        //update decision for multi level review
                        CommonFunction.UpdateMultiLevelDecision(Session["selClientDBConnStr"].ToString(), remark, decision, Session["s_ReviewNo"].ToString(), routeToFinal);

                        string msg = string.Format(Resource.msgItemDecisionLevel, curTransNo.ToString(), Session["s_ReviewNo"].ToString(), "A");
                        LogEntry log = new LogEntry();
                        log.Caller = LogCallerID.RejectedItemDecision;
                        log.ClientCode = curSelectedClient;
                        log.UserName = userID;
                        log.Severity = LogEventType.Information;
                        log.Message = msg;
                        //BusDate={0}|BatchNo={1}|BatchDir={2}|TransNo={3}
                        log.Data = string.Format(Resource.strLogTransactionDetails, curBusdate.ToShortDateString(), curBatchNo, curBatchDir, curTransNo.ToString());
                        log.Write();
                    }
                    else
                        updFinal = true;
                }
                else
                {
                    updFinal = true;
                }
            }
            catch(Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.RejectedItemDecision;
                log.ClientCode = curSelectedClient;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

            return updFinal;
        }


        #endregion

    }
}