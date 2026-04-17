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

namespace UBPCWeb.Modules.ActionedItemHistory
{
    public partial class ViewRLHistoryMultipleMode : System.Web.UI.Page
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
        public String curActionBy = string.Empty;
        public String curActionDateTime = string.Empty;
        public String curRemark = string.Empty;
        public String curDecision = string.Empty;

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
                AssignSessionValue();

                if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
                {
                    PageValidatorResult validatorResult;
                    validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "ActionedItemHistory");
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
                        LoadItemDataTableInfo();//Process transaction for first time
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

                 Response.Redirect("/History", false);
                 Context.ApplicationInstance.CompleteRequest();
             }
        }

        protected void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Update current table value first
            string curAcctNoOrBSB = string.Empty;
            int curIndex = Convert.ToInt16(Session["curSelListBoxItemIdx"]);
            string curIDValue = LstBSelectItem.Items[curIndex].Value;

            //Move to new selected item
            //Assign new selected index
            Session["curSelListBoxItemIdx"] = LstBSelectItem.SelectedIndex;
            RefreshItemPage();
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
                    //Get Action decision, Action by, Action Date Time, Actioned Remark from Main Data Table
                    curDecision = Session["s_CurSelectedRejDecision"].ToString().Trim();
                    curActionBy = Session["s_CurSelectedActionby"].ToString().Trim();
                    if (!string.IsNullOrEmpty(Session["s_CurSelectedActionDateTime"].ToString().Trim()))
                    {
                        curActionDateTime = Convert.ToDateTime(Session["s_CurSelectedActionDateTime"].ToString().Trim()).ToShortDateString();
                    }
                    curRemark = Session["s_CurSelectedRemark"].ToString().Trim();

                    //Initialise Common Item Datatable for first time
                    DataTable dtMainDetail = CommonFunction.GetRLDetailItemInfo(Session["selClientDBConnStr"].ToString());
                    Session["rejectedMainItemDT"] = dtMainDetail;

                    if (((DataTable)Session["rejectedMainItemDT"]).Rows.Count > 0)
                    {
                        //update reject reason title
                        lblRejReason.Text = "REJECT REASON: " + Session["s_CurSelectedRejectReason"].ToString();
                        
                        lblDecision.Text = curDecision.Equals("A") ? "ACCEPTED" : "REJECTED";
                        lblDecision.ForeColor = curDecision.Equals("A") ? System.Drawing.Color.Blue : System.Drawing.Color.Red;
                        lblActionBy.Text = curActionBy;
                        lblActionDateTime.Text = string.IsNullOrEmpty(curActionDateTime)?"No Action": Convert.ToDateTime(curActionDateTime).ToShortDateString();
                        //Edited AK on 2020-09-04 : CR015-20:Multi Level Approval Result
                        lblFinalActionBy.Text = curActionBy;
                        lblFinalActionDateTime.Text = String.IsNullOrEmpty(curActionDateTime) ? "No Action" : Convert.ToDateTime(curActionDateTime).ToShortDateString();

                        txtARemark.InnerText = curRemark;
                        //Populate Item to Listbox
                        LoadListBox();

                        //Get Virtual Directory for IFS Path
                        int curTransportID = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Rows[0]["ITM_TransportID"].ToString().PadLeft(2, '0'));

                        Session["VirDirForIFSPath"] = CommonFunction.GetDirectoryForIFSPath(curTransportID, curSite);

                        Session["curTransItemsCount"] = ((DataTable)Session["rejectedMainItemDT"]).Rows.Count;
                        Session["topItemPtr"] = 0;
                        //Show Data on screen
                        RefreshItemPage();

                        //CR015-20 display multi level approval result 
                        if (Session["s_RequiredApproval"].Equals("True"))
                        {
                            SingleActionReview.Visible = false;
                            string FirstReviewer = Session["s_1stReviewer"].ToString().Trim();
                            string FirstDecision = Session["s_1stDecision"].ToString().Trim();
                            string FirstActionDateTime = string.Empty;
                            if (!string.IsNullOrEmpty(Session["s_1stActionedDateTime"].ToString().Trim()))
                            {
                                FirstActionDateTime = Convert.ToDateTime(Session["s_1stActionedDateTime"].ToString().Trim()).ToShortDateString();
                            }
                            string SecondReviewer = Session["s_2ndReviewer"].ToString().Trim();
                            string SecondDecision = Session["s_2ndDecision"].ToString().Trim();
                            string SecondActionDateTime = string.Empty;
                            if (!string.IsNullOrEmpty(Session["s_2ndActionedDateTime"].ToString().Trim()))
                            {
                                SecondActionDateTime = Convert.ToDateTime(Session["s_2ndActionedDateTime"].ToString().Trim()).ToShortDateString();
                            }

                            lbl1stReviewBy.Text = String.IsNullOrEmpty(FirstReviewer) ? string.Empty : FirstReviewer + "(" + (FirstDecision.Equals("A") ? "Accepted" : "Rejected") + ")";
                            lbl1stActionDateTime.Text = string.IsNullOrEmpty(FirstActionDateTime) ? "No Action" : Convert.ToDateTime(FirstActionDateTime).ToShortDateString();

                            lbl2ndReviewBy.Text = String.IsNullOrEmpty(SecondReviewer) ? string.Empty : SecondReviewer + "(" + (SecondDecision.Equals("A") ? "Accepted" : "Rejected") + ")";
                            lbl2ndActionDateTime.Text = string.IsNullOrEmpty(SecondActionDateTime) ? "No Action" : Convert.ToDateTime(SecondActionDateTime).ToShortDateString();
                        }
                        else
                        {
                            FinalActionReview.Visible = false;
                            FirstReview.Visible = false;
                            SecondReview.Visible = false;
                        }
                    }
                    else
                    {
                        //Redirect to index page
                        Logger.Write(false, LogCallerID.ActionedItemHistory, curSelectedClient, "View Actioned Item History", "Unable to retrieve item details", LogEventType.Error, userID);
                        Response.Redirect("/History", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }
                else
                {
                    //Redirect to index page
                    Logger.Write(false, LogCallerID.ActionedItemHistory, curSelectedClient, "View Actioned Item History", "Invalid session value for View RL Actioned History Multiple Mode", LogEventType.Error, userID);
                    Response.Redirect("/History", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void RefreshItemPage()
        {
            Session["topItemSelected"] = true;
            DisplayItemsInfo();
        }

        private void DisplayItemsInfo()
        {
            try
            {
                if (Convert.ToInt32(Session["curTransItemsCount"]) > 0)
                {
                    //by default select the first stub item 
                    DisplaySelItemFieldsAndImage(true);

                    //Load top image here
                    //Call to ImageHandlerTop.ashx
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadTopImage", "LoadTopImage();", true);
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
                Session["curItemPtr"] = LstBSelectItem.SelectedIndex;

                DataRow itemRow = ((DataTable)Session["rejectedMainItemDT"]).Rows[Convert.ToInt16(Session["curItemPtr"])];

                //Store Current Image info to Session to Load selected image
                if (isTopItem)
                {
                    //current item point = top item
                    //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
                    Session["CurSelectedTopImage"] = Session["VirDirForIFSPath"].ToString().Trim() + "|" + itemRow["ITM_ImageFileName"].ToString() + "|1|0|"
                                                     + itemRow["ITM_Fr_ImgOffset"].ToString() + "|" + itemRow["ITM_Fr_ImgSize"].ToString()
                                                     + "|" + itemRow["ITM_Rr_ImgOffset"].ToString() + "|" + itemRow["ITM_Rr_ImgSize"].ToString()
                                                     + "|" + itemRow["ITM_TransSeqNum"].ToString();
                }

                //Display info fields on selected item 
                //calculate total stub n cheque count & amount
                curTotalStubAmt = Convert.ToDecimal(((DataTable)Session["rejectedMainItemDT"]).Compute("SUM(ITM_Amount)", "ITM_ItemType='S'"));
                curTotalStubCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='S'"));
                lblTotalStubCount.Text = "Total Stub Count: " + curTotalStubCount.ToString();
                txtTotalStubAmt.Text = curTotalStubAmt.Equals(0) ? "0.00" : String.Format("{0:N}", curTotalStubAmt);

                curTotalChqAmt = Convert.ToDecimal(((DataTable)Session["rejectedMainItemDT"]).Compute("SUM(ITM_Amount)", "ITM_ItemType='C'"));
                curTotalChqCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='C'"));
                lblTotalChequeCount.Text = "Total Cheque Count: " + curTotalChqCount.ToString();
                txtTotalChequeAmt.Text = curTotalChqAmt.Equals(0) ? "0.00" : String.Format("{0:N}", curTotalChqAmt);

                //Transaction Info Tab
                lblBusdate.Text = curBusdate.ToShortDateString();
                lblTransNo.Text = curTransNo.ToString();
                lblBatchDir.Text = curBatchDir;
                lblBatchNo.Text = curBatchNo;

                //Transaction Info Tab
                lblAllowBD.Text = curAllowBreakdown ? "Allow Breakdown:Yes" : "Allow Breakdown:No";
                lblAllowTol.Text = curAllowTolerance ? ", Allow Tolerance:Yes" : ", Allow Tolerance:No";

                //Selected Item Info Tab
                bool selItemIsStub = itemRow["ITM_ItemType"].ToString().Trim().Equals("S");
                bool selItemIsCheq = !selItemIsStub;
                //lblSelType.Text = selItemIsStub ? "STUB" : "CHEQUE";

                if (selItemIsStub)
                {
                    //Check if it's BPC then show "BSB"
                    lblTopItem1.Text = Convert.ToBoolean(itemRow["BST_IsBPC"].ToString().Trim()).Equals(true) ?
                                      "Presenting BSB:" : "Depositor Account No.:";
                    txtTopItem1.Text = itemRow["ITM_FLD12"].ToString().Trim();

                }
                else
                {
                    lblTopItem1.Text = "Cheque BSB:";
                    txtTopItem1.Text = itemRow["ITM_FLD4"].ToString().Trim();
                }

                txtTopItemAmount.Text = (Convert.ToDecimal(itemRow["ITM_Amount"].ToString()).Equals(0))
                    ? "0.00" : String.Format("{0:N}", Convert.ToDecimal(itemRow["ITM_Amount"].ToString()));

                //Display reject reason 
                lblItemRejReason.Text = itemRow["ITM_RejectReason"] == null ? "" :
                    itemRow["ITM_ItemType"].ToString().Equals("S") ? "" :
                    string.IsNullOrEmpty(itemRow["ITM_RejectReason"].ToString().Trim())? "":
                    "* " + itemRow["ITM_RejectReason"].ToString().Trim();

                //Display reject reason 
                lblItemRejReason.Text = itemRow["ITM_RejectReason"] == null ? "" :
                    string.IsNullOrEmpty(itemRow["ITM_RejectReason"].ToString().Trim()) ? "" :
                    "* " + itemRow["ITM_RejectReason"].ToString().Trim();

                if (string.IsNullOrEmpty(lblItemRejReason.Text))
                {
                    lblItemRejReason.Font.Size = 3;
                }
                else
                {
                    lblItemRejReason.Font.Size = 12;
                }
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
            Session["curSelectedStub"] = 0;
            Session["curSelListBoxItemIdx"] = 0;
            Session["VirDirForIFSPath"] = "";
            Session["rejectedMainItemDT"] = new DataTable();
        }

        private void ClearSessionValue()
        {
            Session["topItemSelected"] = null;
            Session["curTransItemsCount"] = null;
            Session["curItemPtr"] = null;
            Session["topItemPtr"] = null;
            Session["curSelectedStub"] = null;
            Session["curSelListBoxItemIdx"] = null;
            Session["VirDirForIFSPath"] = null;
            Session["stubDT"] = null;
            Session["chequeDT"] = null;
        }

        private void AssignSessionValue()
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

        private bool ValidateSessionValue()
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

        private void LoadListBox() 
        {
            //get default index
            int curSelectedIndex = this.LstBSelectItem.SelectedIndex.Equals(-1) ? 0 : this.LstBSelectItem.SelectedIndex;

            if (LstBSelectItem.Items.Count > 0) LstBSelectItem.Items.Clear();

            //Populate Item to Listbox
            this.LstBSelectItem.DataSource = ((DataTable)Session["rejectedMainItemDT"]);
            this.LstBSelectItem.DataTextField = "ListBoxTitle";
            this.LstBSelectItem.DataValueField = "TMPID";
            this.LstBSelectItem.DataBind();
            this.LstBSelectItem.SelectedIndex = curSelectedIndex;
            if (this.LstBSelectItem.Items.Count > 0)
            {
                this.LstBSelectItem.Items[curSelectedIndex].Selected = true;
            }
        }     

        #endregion

        #region Event Handling

        protected void btnBackToListing_Click(object sender, EventArgs e)
        {
            ClearSessionValue();
            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadSpinner", "loadSpinner();", true);
            Response.Redirect("/History", false);
            Context.ApplicationInstance.CompleteRequest();
        }

      
        #endregion

        #region Web Method
        [WebMethod]
        public static string getImagePath(string id)
        {
            DataRow itemRow = ((DataTable)HttpContext.Current.Session["rejectedMainItemDT"]).Rows[Convert.ToInt16(HttpContext.Current.Session["curItemPtr"])];
            string curSelTopImg = HttpContext.Current.Session["CurSelectedTopImage"].ToString();

            //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
            string[] strArr = curSelTopImg.Split('|');
            string virDir = strArr[0].ToString();
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

            //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
            HttpContext.Current.Session["CurSelectedTopImage"] = updSessionValue;
         
            //Random ID is needed, upon "updatePanel" ajax calling it will not trigger postback, by using 
            //Query string will "postback" therefore able to call to handler to reload image
            string randomID = System.Guid.NewGuid().ToString().Replace("-", "");

            return "/ImageHandlerTop.ashx?id=" + randomID;
        }

        #endregion
    }
}
