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
    public partial class ValidateRLReject : System.Web.UI.Page, IPostBackEventHandler
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
                string rejReason = Session["s_CurSelectedRejectReason"] == null ? "" : Session["s_CurSelectedRejectReason"].ToString().Trim();
                isForUnisys = Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(Session["s_UserForUnisys"].ToString());

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

        public void RaisePostBackEvent(string Arg)
        {
            txtTopItemAmount_TextChanged(txtTopItemAmount, null);
        }

        #region Helper Function
        private void PerformTransaction()
        {
            try
            {
                Session["curSelListBoxItemIdx"] = 0;

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

                //Show item detail info
                LoadItemDataTableInfo();
            }
            catch (Exception ex)
            {
                throw ex;
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
                    DataTable dtDetails = CommonFunction.GetRLDetailItemInfo(Session["selClientDBConnStr"].ToString());
                    Session["rejectedMainItemDT"] = dtDetails;

                    if (((DataTable)Session["rejectedMainItemDT"]).Rows.Count > 0)
                    {
                        //Assign Max Din No existed per batch
                        Session["maxDinNoPerBatch"] = CommonFunction.GetMaxDinNoPerBatch();


                        //Load Stub and Cheque Datatable
                        if (!((DataTable)Session["rejectedMainItemDT"]).Rows.Count.Equals(1))
                        {
                            txtTopItem1.Enabled = true;
                            txtTopItemAmount.Enabled = true;

                            Session["stubDT"] = ((DataTable)Session["rejectedMainItemDT"]).Select("ITM_ItemType = 'S'").CopyToDataTable();
                        }
                        else
                        {
                            txtTopItem1.Enabled = false;
                            txtTopItemAmount.Enabled = false;

                            Session["stubDT"] = ((DataTable)Session["rejectedMainItemDT"]).Clone();
                        }

                        Session["chequeDT"] = ((DataTable)Session["rejectedMainItemDT"]).Select("ITM_ItemType = 'C'").CopyToDataTable();



                        //Populate Item to Listbox
                        LoadListBox("");

                        //Get Virtual Directory for IFS Path - get first cheque item transport ID for cheque only mode, Stub wont have
                        int curTransportID = Convert.ToInt32(((DataTable)Session["chequeDT"]).Rows[0]["ITM_TransportID"].ToString().PadLeft(2, '0'));

                        Session["VirDirForIFSPath"] = CommonFunction.GetDirectoryForIFSPath(curTransportID, curSite);

                        Session["curTransItemsCount"] = ((DataTable)Session["rejectedMainItemDT"]).Rows.Count;
                        Session["topItemPtr"] = 0;
                        //Show Data on screen
                        RefreshItemPage();

                        ////set visibility of add and delete stub button
                        //
                        //BOONCHONG PE-WD-24-002
                        if (isForUnisys)
                        {
                            bool isBPC = Convert.ToBoolean(Session["isBatchBPC"]);
                            if (isBPC)
                            {
                                //update BPC TRANS wording if it's BPC Reject Reason
                                lblRejReason.Text = "BPC TRANS REJECT REASON: " + Session["s_CurSelectedRejectReason"].ToString();
                                
                                btnAddStub.Visible =((DataTable)Session["stubDT"]).Rows.Count == 0? true: false; //BPC Reject from RL do not have stub so need to allow Ops to add
                                btnDelStub.Visible = ((DataTable)Session["stubDT"]).Rows.Count == 0 ? true : false; ;
                                btnAccept.Visible = true;
                            }
                            else
                            {
                                //update reject reason title
                                lblRejReason.Text = "REJECT REASON: " + Session["s_CurSelectedRejectReason"].ToString();

                                btnAddStub.Visible = curAllowBreakdown;
                                btnDelStub.Visible = curAllowBreakdown;
                                btnAccept.Visible = curAllowBreakdown;
                            }
                        }
                        else
                        {
                            //update reject reason title
                            lblRejReason.Text = "REJECT REASON: " + Session["s_CurSelectedRejectReason"].ToString();

                            btnAddStub.Visible = ((DataTable)Session["stubDT"]).Rows.Count == 0;
                            btnDelStub.Visible = ((DataTable)Session["stubDT"]).Rows.Count == 0;
                            btnAccept.Visible = true;
                        }

                        //CR015-20 disable button for final review
                        //CR015-20 load remark from previous reviewer
                        if (!Session["s_ReviewNo"].ToString().Equals("1"))
                        {
                            txtARemark.InnerText = Session["s_RejectedRemark"].ToString();

                            if (Session["s_ReviewNo"].ToString().Equals("3"))
                            {
                                btnAddStub.Visible = false;
                                btnDelStub.Visible = false;
                                txtTopItem1.Attributes.Add("readonly", "readonly");
                                txtTopItemAmount.Attributes.Add("readonly", "readonly");
                                txtBottomBSB.Attributes.Add("readonly", "readonly");
                                txtBottomAmount.Attributes.Add("readonly", "readonly");
                            }
                        }
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
                    Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Validate Rejected Item Decision", "Invalid session value for RL reject.", LogEventType.Error, userID);
                    Response.Redirect("/WebDecision", false);
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
            try
            {
                Session["topItemSelected"] = true;
                DisplayItemsInfo();
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                    //Top Always show rear of cheque
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadTopImage", "LoadTopImage();", true);
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadBottomImage", "LoadBottomImage();", true);
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
                //if transaction being reset, back to listing page
                if (CommonFunction.IsTransReset())
                {
                    Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Tranaction Reset", "Unable to proceed as selected transaction is already reset", LogEventType.Error, userID);
                    Response.Redirect("/WebDecision", false);
                    ClientScript.RegisterStartupScript(this.GetType(), "PromptMsg",
                        string.Format("displaymsg('{0}','{1}');", "Rejected Item Decision", "Unable to proceed as selected transaction is already reset."), true);

                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {

                    Session["curItemPtr"] = LstBSelectItem.SelectedIndex;

                    DataRow sitemRow = null;
                    int rowIdx = Convert.ToInt16(Session["curItemPtr"]);
                    if (((DataTable)Session["stubDT"]).Rows.Count > 0)
                        sitemRow = ((DataTable)Session["stubDT"]).Rows[rowIdx];

                    DataRow citemRow = ((DataTable)Session["chequeDT"]).Rows[0];//cheque only mode only will have 1 cheque item

                    //set stub info enable or disable
                    txtTopItem1.Enabled = sitemRow != null;
                    txtTopItemAmount.Enabled = sitemRow != null;

                    //Store Current Image info to Session to Load selected image
                    //current top item = back of cheque
                    //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
                    Session["CurSelectedTopImage"] = Session["VirDirForIFSPath"].ToString().Trim() + "|" + citemRow["ITM_ImageFileName"].ToString() + "|0|0|"
                                                     + citemRow["ITM_Fr_ImgOffset"].ToString() + "|" + citemRow["ITM_Fr_ImgSize"].ToString()
                                                     + "|" + citemRow["ITM_Rr_ImgOffset"].ToString() + "|" + citemRow["ITM_Rr_ImgSize"].ToString()
                                                     + "|" + citemRow["ITM_TransSeqNum"].ToString();

                    //current bottom item = front of cheque
                    //virtual dir|imageFileName|isFront|isJpeg|frontOffset|frontSize|rearOffset|rearSize
                    Session["CurSelectedBottomImage"] = Session["VirDirForIFSPath"].ToString().Trim() + "|" + citemRow["ITM_ImageFileName"].ToString() + "|1|0|"
                                                     + citemRow["ITM_Fr_ImgOffset"].ToString() + "|" + citemRow["ITM_Fr_ImgSize"].ToString()
                                                     + "|" + citemRow["ITM_Rr_ImgOffset"].ToString() + "|" + citemRow["ITM_Rr_ImgSize"].ToString()
                                                     + "|" + citemRow["ITM_TransSeqNum"].ToString();

                    //Display info fields on selected item 
                    //calculate total stub n cheque count & amount
                    curTotalStubAmt = ((DataTable)Session["stubDT"]).Rows.Count > 0 ? Convert.ToDecimal(((DataTable)Session["stubDT"]).Compute("SUM(ITM_Amount)", string.Empty)) : 0;
                    curTotalStubCount = ((DataTable)Session["stubDT"]).Rows.Count > 0 ? Convert.ToInt32(((DataTable)Session["stubDT"]).Compute("COUNT(ITM_Amount)", string.Empty)) : 0;
                    lblTotalStubCount.Text = "Total Stub Count: " + curTotalStubCount.ToString();
                    txtTotalStubAmt.Text = curTotalStubAmt.Equals(0) ? "0.00" : String.Format("{0:N}", curTotalStubAmt);

                    curTotalChqAmt = Convert.ToDecimal(((DataTable)Session["chequeDT"]).Compute("SUM(ITM_Amount)", "ITM_ItemType='C'"));
                    curTotalChqCount = Convert.ToInt32(((DataTable)Session["chequeDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='C'"));
                    lblTotalChequeCount.Text = "Total Cheque Count: " + curTotalChqCount.ToString();
                    txtTotalChequeAmt.Text = curTotalChqAmt.Equals(0) ? "0.00" : String.Format("{0:N}", curTotalChqAmt);

                    //Transaction Info Tab
                    lblAllowBD.Text = curAllowBreakdown ? "Allow Breakdown:Yes" : "Allow Breakdown:No";
                    lblAllowTol.Text = curAllowTolerance ? ", Allow Tolerance:Yes" : ", Allow Tolerance:No";

                    lblBusdate.Text = curBusdate.ToShortDateString();
                    lblTransNo.Text = curTransNo.ToString();
                    lblBatchDir.Text = curBatchDir;
                    lblBatchNo.Text = curBatchNo;

                    Session["curSelectedStub"] = Session["curItemPtr"];

                    //Check if it's BPC then show "BSB"
                    Session["isBatchBPC"] = Convert.ToBoolean(citemRow["BST_IsBPC"].ToString().Trim());

                    lblTopItem1.Text = Convert.ToBoolean(citemRow["BST_IsBPC"].ToString().Trim()).Equals(true) && Session["s_CurSelectedClientRejDec"].ToString().Equals("OCBC") ?
                                    "Presenting BSB:" : "Depositor Account No.:";


                    //Added by BOONCHONG PE-WD-24-002 
                    //Edited Shinyi: Show Yellow Highlight only for Unisys Ops
                    bool isUnisysUser = Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(Session["s_UserForUnisys"].ToString());
                    if (Convert.ToBoolean(Session["isBatchBPC"]) && isUnisysUser)
                    {
                        lblRejReason.Style["color"] = "red";
                        lblRejReason.Style["font-weight"] = "bold";
                        lblRejReason.Style["background-color"] = "yellow";

                        txtTopItem1.Text = citemRow["ITM_FLD10"].ToString();//IF BPC, grab Presenting BSB from Cheque's FLD10 as no conversion involved.
                        txtTopItem1.ReadOnly = true;
                    }
                    else 
                    {
                        txtTopItem1.Text = sitemRow == null ? "" : sitemRow["ITM_FLD12"].ToString().Trim();
                        txtTopItem1.ReadOnly = false;
                    }

                    //txtTopItem1.MaxLength = Convert.ToBoolean(citemRow["BST_IsBPC"].ToString().Trim()).Equals(true) ? 7 : 16;

                    txtTopItemAmount.Text = sitemRow == null ? "0.00" :
                                        Convert.ToDecimal(sitemRow["ITM_AMOUNT"].ToString()).Equals(0) ? "0.00"
                                        : String.Format("{0:N}", Convert.ToDecimal(sitemRow["ITM_AMOUNT"].ToString().Trim()));
                    Session["curSelectedStub"] = Session["curItemPtr"];

                    //Display stub Reject Reason
                    if (sitemRow == null)
                    {
                        lblRejReasonForStub.Text = string.Empty;
                    }
                    else
                    {
                        lblRejReasonForStub.Text = sitemRow["ITM_RejectReason"] == null ? "" :
                            string.IsNullOrEmpty(sitemRow["ITM_RejectReason"].ToString().Trim()) ? "" :
                            "* " + sitemRow["ITM_RejectReason"].ToString().Trim();
                    }

                    if (string.IsNullOrEmpty(lblRejReasonForStub.Text))
                    {
                        lblRejReasonForStub.Font.Size = 3;
                    }
                    else
                    {
                        lblRejReasonForStub.Font.Size = 12;
                    }

                    lblBottom.Text = "Cheque BSB:";
                    txtBottomBSB.Text = citemRow["ITM_FLD4"].ToString().Trim();
                    txtBottomAmount.Text = citemRow == null ? "0.00" :
                                        Convert.ToDecimal(citemRow["ITM_AMOUNT"].ToString()).Equals(0) ? "0.00"
                                        : String.Format("{0:N}", Convert.ToDecimal(citemRow["ITM_AMOUNT"].ToString().Trim()));

                    //Added by Shinyi PE-WD-24-002 - if no amount & bsb then allow amendment
                    if (Convert.ToBoolean(Session["isBatchBPC"]) && isUnisysUser)
                    {
                        //Enable the textbox for amendment 
                        //Amount Textbox will be enabled for amendment to balance amount to accept BPC Transaction
                        //BSB Field will only be enabled if no value
                        txtBottomBSB.ReadOnly = string.IsNullOrEmpty(txtBottomBSB.Text.Trim()) ? false : true;
                        txtTopItem1.ReadOnly = string.IsNullOrEmpty(txtTopItem1.Text.Trim()) ? false : true;
                    }

                    //txtBottomBSB.MaxLength = 7;

                    //Display cheque reject reason 
                    lblItemRejReason.Text = citemRow["ITM_RejectReason"] == null ? "" :
                        string.IsNullOrEmpty(citemRow["ITM_RejectReason"].ToString().Trim()) ? "" :
                        "* " + citemRow["ITM_RejectReason"].ToString().Trim();

                    if (string.IsNullOrEmpty(lblItemRejReason.Text))
                    {
                        lblItemRejReason.Font.Size = 3;
                    }
                    else
                    {
                        lblItemRejReason.Font.Size = 12;
                    }

                    //Display info fields on selected item
                    //Added Shinyi - store Stub Ori Dep Acct No and Cheq Ori BSB - CIMB need for validation upon accepting trans
                    Session["CurOriStubDepAcctNo"] = citemRow["ITM_OriDepositor"] == null ? string.Empty : citemRow["ITM_OriDepositor"].ToString();
                    Session["CurOriChequeBSB"] = citemRow["ITM_OriChqBSB"] == null ? string.Empty : citemRow["ITM_OriChqBSB"].ToString();
                    //Shinyi - store Batch bsb - RHB need for Validate GL Account Number
                    Session["CurOriBathchBSB"] = sitemRow == null ? citemRow["ITM_FLD10"].ToString() : sitemRow["ITM_FLD10"].ToString();

                    if (Session["CurOtherStubDepAcct"] == null)
                    {
                        Session["CurOtherStubDepAcct"] = string.Empty;
                    }

                    //Shinyi - store other transaction stub dep account - CIMB does not allow cross product in a batch for different transactions
                    Session["CurOtherStubDepAcct"] = string.IsNullOrEmpty(Session["CurOtherStubDepAcct"].ToString()) ?
                        CommonFunction.GetOtherStubAccountNo(Session["selClientDBConnStr"].ToString(),
                        (sitemRow == null ? "0" : sitemRow["ITM_DIN"].ToString().Trim()))
                        : Session["CurOtherStubDepAcct"].ToString().Trim();
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
            Session["stubDT"] = new DataTable();
            Session["chequeDT"] = new DataTable();
            Session["CurOtherStubDepAcct"] = "";
            Session["CurOriBathchBSB"] = "";
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
            Session["CurOtherStubDepAcct"] = "";
            Session["CurOriBathchBSB"] = "";
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

        private void LoadListBox(string tmpID)
        {
            try
            {
                int curSelectedIndex = 0;
                if (string.IsNullOrEmpty(tmpID))
                {
                    //get default index
                    curSelectedIndex = this.LstBSelectItem.SelectedIndex.Equals(-1) ? 0 : this.LstBSelectItem.SelectedIndex;
                }
                else
                {
                    //with value , find index : This refer to newly added stub
                    string filterRow = String.Format("TMPID='{0}'", tmpID);

                    DataRow[] result = ((DataTable)Session["stubDT"]).Select(filterRow);
                    if (result.Length > 0)
                    {
                        curSelectedIndex = ((DataTable)Session["stubDT"]).Rows.IndexOf(result[0]);
                    }
                }

                if (LstBSelectItem.Items.Count > 0) LstBSelectItem.Items.Clear();

                //Populate Item to Listbox
                if (((DataTable)Session["stubDT"]).Rows.Count > 0)
                {
                    this.LstBSelectItem.DataSource = ((DataTable)Session["stubDT"]);
                    this.LstBSelectItem.DataTextField = "ListBoxTitle";
                    this.LstBSelectItem.DataValueField = "TMPID";
                    this.LstBSelectItem.DataBind();
                    this.LstBSelectItem.SelectedIndex = curSelectedIndex;
                    if (this.LstBSelectItem.Items.Count > 0)
                    {
                        this.LstBSelectItem.Items[curSelectedIndex].Selected = true;
                    }
                }

                Session["curSelListBoxItemIdx"] = curSelectedIndex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool ValidateAccept(out string oerrMsg, string bsbOrAccntNo, decimal amount, bool isStub)
        {
            try
            {
                bool isValidInput = false;
                oerrMsg = string.Empty;

                if (isStub)
                {
                    if (string.IsNullOrEmpty(bsbOrAccntNo.Trim()) || bsbOrAccntNo.Trim().Equals("0"))
                    {
                        txtTopItem1.Focus();
                        isValidInput = false;
                        oerrMsg = lblTopItem1.Text.Contains("BSB") ? "Please enter valid BSB" : "Please enter valid depositor account no.";

                        if (amount.Equals(0))
                        {
                            oerrMsg = lblTopItem1.Text.Contains("BSB") ? "Please enter valid BSB and amount must be greater than zero"
                                : "Please enter valid Depositor Account No. and amount must be greater than zero";
                        }
                    }
                    else if ((!(string.IsNullOrEmpty(bsbOrAccntNo.Trim()) || bsbOrAccntNo.Trim().Equals("0"))) && amount.Equals(0))
                    {
                        txtTopItemAmount.Focus();
                        isValidInput = false;
                        oerrMsg = "Please enter valid amount greater than zero.";
                    }
                    else
                    {
                        isValidInput = true;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(bsbOrAccntNo.Trim()) || bsbOrAccntNo.Trim().Equals("0"))
                    {
                        txtBottomBSB.Focus();
                        isValidInput = false;
                        oerrMsg = "Please enter valid cheque BSB.";

                        if (amount.Equals(0))
                        {
                            oerrMsg = "Please enter valid cheque BSB and amount must be greater than zero";
                        }
                    }
                    else if (!(string.IsNullOrEmpty(bsbOrAccntNo.Trim()) || bsbOrAccntNo.Trim().Equals("0")) && amount.Equals(0))
                    {
                        txtBottomAmount.Focus();
                        isValidInput = false;
                        oerrMsg = "Please enter valid amount greater than zero.";
                    }
                    else
                    {
                        isValidInput = true;
                    }
                }

                //Check if contain other stub account no, if empty, get from other stub in existing transaction
                if (string.IsNullOrEmpty(Session["CurOtherStubDepAcct"].ToString().Trim()))
                {
                    //check if current transaction contain only 1 stub or multiple stub
                    if (((DataTable)Session["stubDT"]).Rows.Count > 1)
                    {
                        //if current selected index = 0, get the next stub row, else get the first stub row
                        int othIndex = Convert.ToInt16(Session["curSelListBoxItemIdx"]) == 0 ? 1 : 0;
                        DataRow othRow = ((DataTable)Session["stubDT"]).Rows[othIndex];
                        Session["CurOtherStubDepAcct"] = othRow["ITM_Fld12"].ToString().Trim();
                    }
                    else
                    {
                        Session["CurOtherStubDepAcct"] = string.Empty;
                    }
                }

                //stub depositor account require cdv checking
                RejDecValidatorResult rejDecValidator;
                rejDecValidator = RejDecValidation.ValidateAcceptedTransaction(Session["selClientDBConnStr"].ToString(), bsbOrAccntNo, isStub, Convert.ToBoolean(Session["isBatchBPC"]), Session["CurOriChequeBSB"].ToString().Trim(), Session["CurOriStubDepAcctNo"].ToString().Trim(), Session["CurOriBathchBSB"].ToString().Trim(), Session["CurOtherStubDepAcct"].ToString().Trim());
                isValidInput = rejDecValidator.Valid;
                oerrMsg = rejDecValidator.ReturnMessage;

                if (Session["s_CurSelectedClientRejDec"].ToString().Equals("OCBC"))
                {
                    if (isValidInput && isStub && !Convert.ToBoolean(Session["isBatchBPC"]))
                    {
                        //CDV Checking                    
                        rejDecValidator = RejDecValidation.ValidateCDV(Session["selClientDBConnStr"].ToString(), bsbOrAccntNo);
                        isValidInput = rejDecValidator.Valid;
                        oerrMsg = rejDecValidator.ReturnMessage;
                    }

                    ////Bsb validation for BPC's Stub & Cheque Items
                    //if ((isValidInput && isStub && Convert.ToBoolean(Session["isBatchBPC"])) || (isValidInput && !isStub))
                    //{
                    //    //BSB Checking
                    //    //get main site connection string
                    //    DBConnectionInfo dbConn = new DBConnectionInfo();
                    //    dbConn.webConnStr = webDBConnStr;
                    //    string mainSiteConnStr = dbConn.GetSiteConnectionString(true, Session["s_CurSelectedClientRejDec"].ToString(), dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), Session["s_CurSelectedClientRejDec"].ToString()));

                    //    rejDecValidator = RejDecValidation.ValidateBSB(mainSiteConnStr, bsbOrAccntNo);
                    //    isValidInput = rejDecValidator.Valid;
                    //    oerrMsg = rejDecValidator.ReturnMessage;
                    //}
                }

                return isValidInput;
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

                oerrMsg = "Error Validating Accept.";
                return false;
            }
        }

        private void RefreshListBoxAndCalTotalAmt()
        {
            try
            {
                if (Session["stubDT"] != null)
                {
                    if (((DataTable)Session["stubDT"]).Rows.Count > 0)
                    {

                        int curIndex = Convert.ToInt16(Session["curSelListBoxItemIdx"]);
                        DataRow rowBefore = ((DataTable)Session["stubDT"]).Rows[curIndex];

                        //Check if current stub count  = 1 only
                        //Update the stub first
                        #region Update Stub
                        string curRowTmpID = rowBefore["TMPID"].ToString().Trim();
                        string filterRow = String.Format("TMPID='{0}'", curRowTmpID);
                        DataRow mainRowToUpd = ((DataTable)Session["rejectedMainItemDT"]).Select(filterRow).FirstOrDefault();

                        decimal amount = 0;
                        amount = string.IsNullOrEmpty(txtTopItemAmount.Text.Trim()) ? 0 : Convert.ToDecimal(txtTopItemAmount.Text.Trim());

                        rowBefore["RowUpdated"] = 1;
                        rowBefore["ITM_Fld12"] = txtTopItem1.Text.Trim();
                        rowBefore["ITM_Amount"] = amount;

                        string listBoxTitle = string.Empty;
                        if (!rowBefore["ListBoxTitle"].ToString().Trim().Contains("*"))
                        {
                            listBoxTitle = rowBefore["ITM_ItemType"].ToString().Trim() + ": RM " + String.Format("{0:n}", amount);

                        }
                        else
                        {
                            listBoxTitle = rowBefore["ITM_ItemType"].ToString().Trim() + ": RM " + String.Format("{0:n}", amount) + "*";

                        }

                        rowBefore["ListBoxTitle"] = listBoxTitle;

                        if (mainRowToUpd != null)
                        {
                            mainRowToUpd["RowUpdated"] = 1;
                            mainRowToUpd["ITM_Fld12"] = txtTopItem1.Text.Trim();
                            mainRowToUpd["ITM_Amount"] = amount;
                        }

                        LoadListBox("");
                        #endregion

                        #region Update Cheque
                        decimal camount = 0;
                        camount = string.IsNullOrEmpty(txtBottomAmount.Text.Trim()) ? 0 : Convert.ToDecimal(txtBottomAmount.Text.Trim());


                        DataRow mainCRowToUpd = ((DataTable)Session["rejectedMainItemDT"]).Select("ITM_ItemType = 'C'").FirstOrDefault();
                        mainCRowToUpd["RowUpdated"] = 1;
                        mainCRowToUpd["ITM_Fld4"] = txtBottomBSB.Text.Trim();
                        mainCRowToUpd["ITM_Amount"] = camount;

                        DataRow cRowToUpd = ((DataTable)Session["chequeDT"]).Rows[0];
                        cRowToUpd["RowUpdated"] = 1;
                        cRowToUpd["ITM_Fld4"] = txtBottomBSB.Text.Trim();
                        cRowToUpd["ITM_Amount"] = camount;
                        #endregion

                        //calculate total stub n cheque count & amount
                        curTotalStubAmt = ((DataTable)Session["stubDT"]).Rows.Count > 0 ? Convert.ToDecimal(((DataTable)Session["stubDT"]).Compute("SUM(ITM_Amount)", string.Empty)) : 0;
                        curTotalStubCount = ((DataTable)Session["stubDT"]).Rows.Count > 0 ? Convert.ToInt32(((DataTable)Session["stubDT"]).Compute("COUNT(ITM_Amount)", string.Empty)) : 0;
                        lblTotalStubCount.Text = "Total Stub Count: " + curTotalStubCount.ToString();
                        txtTotalStubAmt.Text = curTotalStubAmt.Equals(0) ? "0.00" : String.Format("{0:N}", curTotalStubAmt);

                        curTotalChqAmt = Convert.ToDecimal(((DataTable)Session["rejectedMainItemDT"]).Compute("SUM(ITM_Amount)", "ITM_ItemType='C'"));
                        curTotalChqCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='C'"));
                        lblTotalChequeCount.Text = "Total Cheque Count: " + curTotalChqCount.ToString();
                        txtTotalChequeAmt.Text = curTotalChqAmt.Equals(0) ? "0.00" : String.Format("{0:N}", curTotalChqAmt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool UpdateCurrentSelectedItem(out string errMsg)
        {
            try
            {
                bool isValidInput = false;
                errMsg = string.Empty;

                //Update current table value first
                string curAcctNo = string.Empty;
                decimal curAmount = 0;

                int curIndex = Convert.ToInt16(Session["curSelListBoxItemIdx"]);
                DataRow row = ((DataTable)Session["stubDT"]).Rows[curIndex];

                string curRowTmpID = row["TMPID"].ToString().Trim();
                string filterRow = String.Format("TMPID='{0}'", curRowTmpID);
                DataRow mainRowToUpd = ((DataTable)Session["rejectedMainItemDT"]).Select(filterRow).FirstOrDefault();

                bool isRowChange = false;
                decimal stubAmount = string.IsNullOrEmpty(txtTopItemAmount.Text.Trim()) ? 0 : Convert.ToDecimal(txtTopItemAmount.Text.Trim());

                if (row != null)
                {
                    bool isValidate = true;
                    isValidate = ValidateAccept(out errMsg, txtTopItem1.Text.Trim(), stubAmount, true);
                    if (isValidate)
                    {
                        isRowChange = Convert.ToBoolean(row["RowUpdated"]);

                        curAcctNo = row["ITM_FLD12"].ToString().Trim();
                        curAmount = Convert.ToDecimal(row["ITM_Amount"].ToString().Trim());

                        if (!isRowChange)
                        {
                            if ((!curAcctNo.Equals(txtTopItem1.Text.Trim())) || !(curAmount.Equals(Convert.ToDecimal(txtTopItemAmount.Text.Trim()))))
                            {
                                isRowChange = true;
                            }
                        }

                        isValidInput = true;

                    }
                    else
                    {
                        isValidInput = false;
                    }

                    if (isValidInput)
                    {
                        if (isRowChange)
                        {
                            row["RowUpdated"] = 1;
                            row["ITM_Fld12"] = txtTopItem1.Text.Trim();
                            row["ITM_Amount"] = Convert.ToDecimal(txtTopItemAmount.Text.Trim());


                            string listBoxTitle = string.Empty;
                            if (!row["ListBoxTitle"].ToString().Trim().Contains("*"))
                            {
                                listBoxTitle = row["ITM_ItemType"].ToString().Trim() + ": " + "RM " + String.Format("{0:n}", Convert.ToDecimal(txtTopItemAmount.Text.Trim()));
                            }
                            else
                            {
                                listBoxTitle = row["ITM_ItemType"].ToString().Trim() + ": " + "RM " + String.Format("{0:n}", Convert.ToDecimal(txtTopItemAmount.Text.Trim())) + "*";
                            }

                            row["ListBoxTitle"] = listBoxTitle;

                            if (mainRowToUpd != null)
                            {
                                mainRowToUpd["RowUpdated"] = 1;
                                mainRowToUpd["ITM_Fld12"] = txtTopItem1.Text.Trim();
                                mainRowToUpd["ITM_Amount"] = Convert.ToDecimal(txtTopItemAmount.Text.Trim());
                            }

                            LoadListBox("");
                        }
                        else
                        {
                            if (!row["ITM_StubCreatedbyWeb"].Equals(1) && !row["RowUpdated"].ToString().Trim().Equals("1"))
                            {
                                row["RowUpdated"] = 0;
                            }
                            else
                            {
                                row["RowUpdated"] = 1;
                            }
                        }
                    }
                }
                else
                {
                    isValidInput = false;
                }

                return isValidInput;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        protected void btnBackToListing_Click(object sender, EventArgs e)
        {
            try
            {
                ///Reset back 
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

        protected void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string errMsg = string.Empty;
                bool isValidChanged = UpdateCurrentSelectedItem(out errMsg);
                int curIndex = Convert.ToInt16(Session["curSelListBoxItemIdx"]);
                isValidChanged = true;//avoid prompt message when navigating through differnet stub
                if (isValidChanged)
                {
                    //Move to new selected item
                    //Assign new selected index
                    Session["curSelListBoxItemIdx"] = LstBSelectItem.SelectedIndex;
                    RefreshItemPage();
                }
                else
                {
                    LstBSelectItem.SelectedIndex = curIndex;
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "promptAlert", string.Format("alertBox('{0}');", errMsg), true);
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

        protected void btnAddStub_Click(object sender, ImageClickEventArgs e)
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
                    string errMsg = "";
                    decimal stubAmount = string.IsNullOrEmpty(txtTopItemAmount.Text.Trim()) ? 0 : Convert.ToDecimal(txtTopItemAmount.Text.Trim());

                    bool isValidate = true;
                    //First time if no stub found - no validation needed
                    //isValidate = CommonFunction.stubDT.Rows.Count > 0? ValidateAccept(out errMsg, txtTopItem1.Text.Trim(), stubAmount, true):true;
                    isValidate = ((DataTable)Session["stubDT"]).Rows.Count > 0 ? UpdateCurrentSelectedItem(out errMsg) : true;
                    if (isValidate)
                    {
                        //Add stub only
                        //default - insert 1 stub into stub datatable
                        //if gt virtual stub - copy virtual stub info 
                        //if no virtual stub - copt cheque info

                        //Check if current stub count  = 1 only
                        //Update the stub first
                        if (((DataTable)Session["stubDT"]).Rows.Count.Equals(1))
                        {
                            //Refresh Listbox and amount
                            RefreshListBoxAndCalTotalAmt();
                        }

                        DataRow row = ((DataTable)Session["stubDT"]).Rows.Count > 0 ?
                            ((DataTable)Session["stubDT"]).Rows[0] : ((DataTable)Session["chequeDT"]).Rows[0];

                        Int64 maxDinNo = 0;
                        Int64 curUpdDTDinNo = Convert.ToInt64(((DataTable)Session["rejectedMainItemDT"]).Compute("max(ITM_DIN)", string.Empty));
                        maxDinNo = (curUpdDTDinNo > Convert.ToInt32(Session["maxDinNoPerBatch"])) ? curUpdDTDinNo : Convert.ToInt32(Session["maxDinNoPerBatch"]);

                        Int64 newDinNo = maxDinNo + 1;

                        DataRow newRow = ((DataTable)Session["stubDT"]).NewRow();
                        DataRow newRowToMain = ((DataTable)Session["rejectedMainItemDT"]).NewRow();

                        string tmpIDValue = newDinNo.ToString() + row["TMPID"].ToString();
                        string amount = Convert.ToDecimal(row["ITM_AMOUNT"].ToString().Trim()).Equals(0) ? "0.00" : String.Format("{0:N}", Convert.ToDecimal(row["ITM_AMOUNT"].ToString().Trim()));

                        #region assign new row value
                        newRow["TMPID"] = tmpIDValue;
                        newRow["ListBoxTitle"] = "S" + ": RM 0.00";
                        newRow["ITM_BusDate"] = row["ITM_BusDate"];
                        newRow["ITM_BatchDirectory"] = row["ITM_BatchDirectory"].ToString();
                        newRow["ITM_BatchNum"] = row["ITM_BatchNum"].ToString();
                        newRow["ITM_DIN"] = newDinNo;
                        newRow["ITM_WsID"] = row["ITM_WsID"].ToString();
                        newRow["ITM_ProcMode"] = row["ITM_ProcMode"].ToString();
                        newRow["ITM_RunNum"] = row["ITM_RunNum"];
                        newRow["ITM_PERunNum"] = row["ITM_PERunNum"];
                        newRow["ITM_TransNum"] = row["ITM_TransNum"];
                        newRow["ITM_TransSeqNum"] = row["ITM_TransSeqNum"];
                        newRow["ITM_ItemType"] = 'S';
                        newRow["ITM_TransportID"] = row["ITM_TransportID"].ToString();
                        newRow["ITM_OneToMany"] = 1;
                        newRow["ITM_Fld1"] = string.Empty;
                        newRow["ITM_Fld2"] = string.Empty;
                        newRow["ITM_Fld3"] = string.Empty;
                        newRow["ITM_Fld4"] = string.Empty;
                        newRow["ITM_Fld5"] = string.Empty;
                        newRow["ITM_Fld6"] = string.Empty;
                        newRow["ITM_Fld7"] = string.Empty;
                        newRow["ITM_Fld8"] = string.Empty;
                        //Edited Shinyi - Add Fld10 to sp_ValidateAcceptedTransactionForWeb
                        newRow["ITM_Fld9"] = row["ITM_Fld9"] == null ? string.Empty : row["ITM_Fld9"].ToString().Trim();
                        newRow["ITM_Fld10"] = row["ITM_Fld10"] == null ? string.Empty : row["ITM_Fld10"].ToString().Trim();

                        newRow["ITM_Fld11"] = string.Empty;
                        newRow["ITM_Fld12"] = string.Empty;
                        newRow["ITM_BundleID"] = row["ITM_BundleID"].ToString();
                        newRow["ITM_Amount"] = 0;
                        newRow["ITM_OrigAmount"] = 0;
                        newRow["ITM_DI_Format"] = 0;
                        newRow["ITM_ImageFileName"] = string.Empty;
                        newRow["ITM_Fr_ImgOffset"] = 0;
                        newRow["ITM_Fr_ImgSize"] = 0;
                        newRow["ITM_Rr_ImgOffset"] = 0;
                        newRow["ITM_Rr_ImgSize"] = 0;
                        newRow["ITM_JPEGFr_ImgOffset"] = 0;
                        newRow["ITM_JPEGFr_ImgSize"] = 0;
                        newRow["ITM_JPEGRr_ImgOffset"] = 0;
                        newRow["ITM_JPEGRr_ImgSize"] = 0;
                        newRow["ITM_Rejected"] = 0;
                        newRow["ITM_RejectReason"] = string.Empty;
                        newRow["ITM_RejectOper"] = string.Empty;
                        newRow["ITM_RejectPVMode"] = string.Empty;
                        newRow["ITM_PVRejectNoPrint"] = false;
                        newRow["ITM_Ref1"] = string.Empty;
                        newRow["ITM_FLAG"] = row["ITM_FLAG"].ToString();
                        newRow["Returned"] = row["Returned"].ToString();
                        newRow["SystemGen"] = "1";
                        newRow["ITM_OriDepositor"] = string.Empty;
                        newRow["RowUpdated"] = "1";
                        newRow["ITM_StubCreatedbyWeb"] = "1";
                        newRow["BST_IsBPC"] = row["BST_IsBPC"];
                        #endregion

                        #region assign new row value to  main datatable

                        newRowToMain["TMPID"] = tmpIDValue;
                        newRowToMain["ListBoxTitle"] = "S" + ": RM 0.00";
                        newRowToMain["ITM_BusDate"] = row["ITM_BusDate"];
                        newRowToMain["ITM_BatchDirectory"] = row["ITM_BatchDirectory"].ToString();
                        newRowToMain["ITM_BatchNum"] = row["ITM_BatchNum"].ToString();
                        newRowToMain["ITM_DIN"] = newDinNo;
                        newRowToMain["ITM_WsID"] = row["ITM_WsID"].ToString();
                        newRowToMain["ITM_ProcMode"] = row["ITM_ProcMode"].ToString();
                        newRowToMain["ITM_RunNum"] = row["ITM_RunNum"];
                        newRowToMain["ITM_PERunNum"] = row["ITM_PERunNum"];
                        newRowToMain["ITM_TransNum"] = row["ITM_TransNum"];
                        newRowToMain["ITM_TransSeqNum"] = row["ITM_TransSeqNum"];
                        newRowToMain["ITM_ItemType"] = 'S';
                        newRowToMain["ITM_TransportID"] = row["ITM_TransportID"].ToString();
                        newRowToMain["ITM_OneToMany"] = 1;
                        newRowToMain["ITM_Fld1"] = string.Empty;
                        newRowToMain["ITM_Fld2"] = string.Empty;
                        newRowToMain["ITM_Fld3"] = string.Empty;
                        newRowToMain["ITM_Fld4"] = string.Empty;
                        newRowToMain["ITM_Fld5"] = string.Empty;
                        newRowToMain["ITM_Fld6"] = string.Empty;
                        newRowToMain["ITM_Fld7"] = string.Empty;
                        newRowToMain["ITM_Fld8"] = string.Empty;
                        newRowToMain["ITM_Fld9"] = row["ITM_Fld9"] == null ? string.Empty : row["ITM_Fld9"].ToString().Trim();
                        newRowToMain["ITM_Fld10"] = row["ITM_Fld10"] == null ? string.Empty : row["ITM_Fld10"].ToString().Trim();
                        newRowToMain["ITM_Fld11"] = string.Empty;
                        newRowToMain["ITM_Fld12"] = string.Empty;
                        newRowToMain["ITM_BundleID"] = row["ITM_BundleID"].ToString();
                        newRowToMain["ITM_Amount"] = 0;
                        newRowToMain["ITM_OrigAmount"] = 0;
                        newRowToMain["ITM_DI_Format"] = 0;
                        newRowToMain["ITM_ImageFileName"] = string.Empty;
                        newRowToMain["ITM_Fr_ImgOffset"] = 0;
                        newRowToMain["ITM_Fr_ImgSize"] = 0;
                        newRowToMain["ITM_Rr_ImgOffset"] = 0;
                        newRowToMain["ITM_Rr_ImgSize"] = 0;
                        newRowToMain["ITM_JPEGFr_ImgOffset"] = 0;
                        newRowToMain["ITM_JPEGFr_ImgSize"] = 0;
                        newRowToMain["ITM_JPEGRr_ImgOffset"] = 0;
                        newRowToMain["ITM_JPEGRr_ImgSize"] = 0;
                        newRowToMain["ITM_Rejected"] = 0;
                        newRowToMain["ITM_RejectReason"] = string.Empty;
                        newRowToMain["ITM_RejectOper"] = string.Empty;
                        newRowToMain["ITM_RejectPVMode"] = string.Empty;
                        newRowToMain["ITM_PVRejectNoPrint"] = false;
                        newRowToMain["ITM_Ref1"] = string.Empty;
                        newRowToMain["ITM_FLAG"] = row["ITM_FLAG"].ToString();
                        newRowToMain["Returned"] = row["Returned"].ToString();
                        newRowToMain["SystemGen"] = "1";
                        newRowToMain["ITM_OriDepositor"] = string.Empty;
                        newRowToMain["RowUpdated"] = "1";
                        newRowToMain["ITM_StubCreatedbyWeb"] = "1";
                        newRowToMain["BST_IsBPC"] = row["BST_IsBPC"];
                        #endregion

                        ((DataTable)Session["stubDT"]).Rows.Add(newRow);
                        ((DataTable)Session["rejectedMainItemDT"]).Rows.Add(newRowToMain);
                        ((DataTable)Session["stubDT"]).AcceptChanges();
                        ((DataTable)Session["rejectedMainItemDT"]).AcceptChanges();

                        txtTopItem1.Focus();

                        LoadListBox(tmpIDValue);

                        RefreshItemPage();
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "unlaodspinner", "unloadSpin();", true);
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "unlaodspinner", "unloadSpin();", true);
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "promptAlert", string.Format("alertBox('{0}');", errMsg), true);
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

        protected void btnDelStub_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                if (((DataTable)Session["stubDT"]).Rows.Count > 0)
                {

                    if (CommonFunction.IsTransReset())
                    {
                        Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Tranaction Reset", "Unable to proceed as selected transaction is already reset", LogEventType.Error, userID);
                        Response.Redirect("/WebDecision", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {

                        //if only 1 stub left, not allow to delete
                        if (((DataTable)Session["rejectedMainItemDT"]).Select("ITM_ItemType = 'S'").Count() <= 1)
                        {
                            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "promptAlert", string.Format("alertBox('{0}');", "At least 1 stub to be remained."), true);
                        }
                        else
                        {
                            string errMsg = "";

                            bool isValidate = UpdateCurrentSelectedItem(out errMsg);
                            if (isValidate)
                            {
                                //get current index
                                int curIndex = LstBSelectItem.SelectedIndex;
                                string curIDValue = LstBSelectItem.Items[curIndex].Value;
                                string filterRow = String.Format("TMPID='{0}'", curIDValue);
                                DataRow row = ((DataTable)Session["stubDT"]).Select(filterRow).FirstOrDefault();
                                DataRow mainRow = ((DataTable)Session["rejectedMainItemDT"]).Select(filterRow).FirstOrDefault();

                                //Only stub created by web can be deleted, others cant
                                if (row["ITM_StubCreatedbyWeb"].ToString().Equals("1"))
                                {
                                    ((DataTable)Session["stubDT"]).Rows.Remove(row);
                                    ((DataTable)Session["rejectedMainItemDT"]).Rows.Remove(mainRow);
                                    ((DataTable)Session["rejectedMainItemDT"]).AcceptChanges();
                                }

                                this.LstBSelectItem.SelectedIndex = 0;
                                Session["curSelListBoxItemIdx"] = 0;
                                LoadListBox("");

                                RefreshItemPage();
                            }
                            else
                            {
                                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "unlaodspinner", "unloadSpin();", true);
                                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "promptAlert", string.Format("alertBox('{0}');", errMsg), true);

                            }
                        }
                    }
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "promptAlert", string.Format("alertBox('{0}');", "No stub to delete. Please proceed to add 1 stub"), true);
                }

                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "unlaodspinner", "unloadSpin();", true);
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


        protected void TextBox1_Click(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;

            decimal oriDecNo = 0;
            if (Decimal.TryParse(txt.Text, out oriDecNo))
            {
                txt.Text = Convert.ToDecimal(txt.Text).ToString().Replace(".", "00");
            }
            else
            {
                txt.Text = "0";
            }
        }


        protected void txtTopItemAmount_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (string.IsNullOrEmpty(tb.Text.Trim()))
            {
                tb.Text = "";
                tb.Focus();
            }
            else
            {
                Decimal amount = 0;
                if (Decimal.TryParse(tb.Text, out amount))
                {
                    tb.Text = String.Format("{0:N}", amount);
                }
                else
                {
                    tb.Text = "";
                    tb.Focus();
                }
            }

            RefreshListBoxAndCalTotalAmt();
        }

        protected void txtTopItem1_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (string.IsNullOrEmpty(txt.Text.Trim()))
            {
                if (txt.ID.Equals("txtTopItem1"))
                {
                    txtTopItem1.Text = "";
                    txtTopItem1.Focus();
                }

                if (txt.ID.Equals("txtBottomBSB"))
                {
                    txtBottomBSB.Text = "";
                    txtBottomBSB.Focus();
                }
            }
            else
            {
                if (!UBPC.Web.Common.CommonFunction.isNumberOnly(txt.Text.Trim()))
                {
                    txt.Text = "";
                    txt.Focus();
                }
            }

            RefreshListBoxAndCalTotalAmt();

            //Set Valid navigation
            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "setValidNav", "setValidNavigation();", true);

        }

        protected void btnAccept_Click(object sender, EventArgs e)
        {
            string errMsg;
            decimal stubAmount = string.IsNullOrEmpty(txtTopItemAmount.Text.Trim()) ? 0 : Convert.ToDecimal(txtTopItemAmount.Text.Trim());
            decimal chqAmount = string.IsNullOrEmpty(txtBottomAmount.Text.Trim()) ? 0 : Convert.ToDecimal(txtBottomAmount.Text.Trim());
            bool isValidAccept = false;
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

                    //validate if got stub
                    if (((DataTable)Session["rejectedMainItemDT"]).Select("ITM_ItemType = 'S'").Count() <= 0)
                    {
                        isValidAccept = false;
                        errMsg = "At least 1 stub to be added.";
                    }
                    else
                    {

                        //validate stub
                        if (ValidateAccept(out errMsg, txtTopItem1.Text.Trim(), stubAmount, true))
                        {
                            //validate cheque
                            if (ValidateAccept(out errMsg, txtBottomBSB.Text.Trim(), chqAmount, false))
                            {
                                //Proceed to accept
                                //validate total stub amount and total cheque amount
                                if ((txtTotalChequeAmt.Text.ToString().Trim().Equals(txtTotalStubAmt.Text.ToString().Trim())) && Convert.ToDecimal(txtTotalChequeAmt.Text.ToString().Trim()) > 0)
                                {
                                    foreach (DataRow dr in ((DataTable)Session["rejectedMainItemDT"]).Rows)
                                    {
                                        decimal updAmt = string.IsNullOrEmpty(dr["ITM_Amount"].ToString().Trim()) ? 0 : Convert.ToDecimal(dr["ITM_Amount"].ToString().Trim());
                                        string acctNoOrBSB = dr["ITM_ItemType"].ToString().Trim().Equals("S") ? dr["ITM_Fld12"].ToString().Trim() : dr["ITM_Fld4"].ToString().Trim();

                                        decimal amount = string.IsNullOrEmpty(dr["ITM_Amount"].ToString().Trim()) ? 0 : Convert.ToDecimal(dr["ITM_Amount"].ToString().Trim());

                                        if (ValidateAccept(out errMsg, acctNoOrBSB, updAmt, dr["ITM_ItemType"].ToString().Trim().Equals("S")))
                                        {
                                            if (amount <= 0)
                                            {
                                                isValidAccept = false;
                                                errMsg = "Amount for each Stub and Cheque must be greater than zero. Please amend accordingly.";
                                                break;
                                            }
                                            else
                                            {
                                                isValidAccept = true;
                                            }
                                        }
                                        else
                                        {
                                            isValidAccept = false;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    //Amount not tallied
                                    isValidAccept = false;
                                    errMsg = "Total Stub Amount must be tallied with Total Cheque Amount and Amount must be greater than zero. Please amend accordingly.";
                                }
                            }
                            else
                            {
                                isValidAccept = false;
                            }
                        }
                        else
                        {
                            isValidAccept = false;
                        }
                    }

                    if (!isValidAccept)
                    {
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "promptAlert", string.Format("alertBox('{0}');", errMsg), true);
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "confirmAcceptbox", "confirmAccept();", true);

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
                log.Message = "Click Accpet Error:" + ex.Message;
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
                //Refresh Listbox and amount
                if (CommonFunction.IsTransReset())
                {
                    Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Tranaction Reset", "Unable to proceed as selected transaction is already reset", LogEventType.Error, userID);
                    Response.Redirect("/WebDecision", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    //CR015-20 check this transation required multi level approval
                    //CR015-20 check this transation required multi level approval
                    int countAmend = 0;
                    bool isForUnisys = Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(Session["s_UserForUnisys"].ToString());
                    bool bMultiApproval = Convert.ToBoolean(Session["s_ClientRequiredApproval"].ToString());
                    decimal minAmt = string.IsNullOrEmpty(Session["s_App_MinAmt"].ToString()) ? 0 : Convert.ToDecimal(Session["s_App_MinAmt"].ToString());
                    if (!isForUnisys)
                    {
                        //check whether the cheque amount is over min amount for multi level approval
                        if (!Session["s_RequiredApproval"].ToString().Equals("True") && bMultiApproval)
                        {
                            foreach (DataRow dr in ((DataTable)Session["rejectedMainItemDT"]).Rows)
                            {
                                decimal updAmt = string.IsNullOrEmpty(dr["ITM_Amount"].ToString().Trim()) ? 0 : Convert.ToDecimal(dr["ITM_Amount"].ToString().Trim());

                                if (dr["RowUpdated"].ToString().Trim().Equals("1"))
                                {
                                    countAmend++;
                                    CommonFunction.UpdatedAmendedItems(dr);
                                }

                            }

                            foreach (DataRow dr in ((DataTable)Session["rejectedMainItemDT"]).Rows)
                            {
                                if (!dr["ITM_ItemType"].ToString().Trim().Equals("S"))
                                {
                                    decimal updAmt = string.IsNullOrEmpty(dr["ITM_Amount"].ToString().Trim()) ? 0 : Convert.ToDecimal(dr["ITM_Amount"].ToString().Trim());
                                    if (updAmt >= minAmt)
                                    {
                                        Session["s_RequiredApproval"] = "True";
                                        Session["s_ReviewNo"] = "1";
                                    }
                                }
                            }
                        }
                    }

                    bool UpdFinal = ProceedToFinalDecision("R", countAmend);

                    if (UpdFinal)
                    {
                        string remark = txtARemark.InnerText.ToString().Trim().Length > 0 ? txtARemark.InnerText.ToString() : "";
                        //Update ReferToWeb = 0
                        int recUpdated = CommonFunction.UpdateRejectedRLItem(curSelectedClient, userID);

                        if (recUpdated > 0)
                        {
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
                    }

                    //Clear all session value upon reject
                    ClearSessionValue();
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
            }

            //Clear all session value upon complete
            ClearSessionValue();

            Response.Redirect("/WebDecision", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnConfirmAccept_Click(object sender, EventArgs e)
        {
            //Refresh Listbox and amount
            //Edited by Ann Keat on 20200917:for multi level approval no need to rerun for this function
            if (!Session["s_ReviewNo"].ToString().Equals("2"))
                RefreshListBoxAndCalTotalAmt();

            decimal stubAmount = string.IsNullOrEmpty(txtTopItemAmount.Text.Trim()) ? 0 : Convert.ToDecimal(txtTopItemAmount.Text.Trim());
            decimal chqAmount = string.IsNullOrEmpty(txtBottomAmount.Text.Trim()) ? 0 : Convert.ToDecimal(txtBottomAmount.Text.Trim());
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

                    //Get this transaction 
                    bool isStubByWebExisted = false;
                    DataRow stubByWebRow = ((DataTable)Session["rejectedMainItemDT"]).Select("ITM_StubCreatedbyWeb = '1'").FirstOrDefault();
                    isStubByWebExisted = stubByWebRow != null ? true : false;

                    //CR015-20 check whether route to final,for multiple level approval
                    int countAmend = 0;
                    if (isStubByWebExisted)
                    {
                        //Select Stub only, sort by din
                        DataView sortedView = ((DataTable)Session["rejectedMainItemDT"]).Select("ITM_ItemType = 'S'").CopyToDataTable().DefaultView;
                        sortedView.Sort = "ITM_Din ASC";
                        DataTable stubUpdDT = sortedView.ToTable();
                        int totalStubCount = stubUpdDT.Rows.Count;


                        #region Update Stub
                        for (int i = 0; i < stubUpdDT.Rows.Count; i++)
                        {
                            DataRow dr = stubUpdDT.Rows[i];

                            //Restructure trans seq no
                            dr["ITM_TransSeqNum"] = i + 1;//start with 1

                            //select row with "RowUpate" = 1,loop into it and update DB
                            //Ensure update original amount also ITM_OriChqBSB, ITM_OriDepositor 
                            if (dr["ITM_StubCreatedbyWeb"].ToString().Trim().Equals("0"))
                            {
                                CommonFunction.UpdatedAmendedItems(dr);
                                countAmend++;
                            }

                            //ITM_StubCreatedByWeb = 1, loop into it and insert into DB
                            //Ensure update original amount also ITM_OriChqBSB, ITM_OriDepositor 
                            if (dr["ITM_StubCreatedbyWeb"].ToString().Trim().Equals("1"))
                            {
                                CommonFunction.InsertStubItems(dr);
                                countAmend++;
                            }
                        }
                        #endregion

                        #region Update Cheque
                        DataTable chqUpdDT = ((DataTable)Session["rejectedMainItemDT"]).Select("ITM_ItemType = 'C'").CopyToDataTable();
                        for (int i = 0; i < chqUpdDT.Rows.Count; i++)
                        {
                            DataRow dr = chqUpdDT.Rows[i];

                            //Update transeq no
                            //Restructure trans seq no
                            dr["ITM_TransSeqNum"] = totalStubCount + (i + 1);//start with 1
                            decimal amount = string.IsNullOrEmpty(dr["ITM_Amount"].ToString().Trim()) ? 0 : Convert.ToDecimal(dr["ITM_Amount"].ToString().Trim());
                            CommonFunction.UpdatedAmendedItems(dr);
                            countAmend++;
                        }
                        #endregion

                    }
                    else
                    {
                        //No Stub created by web
                        foreach (DataRow dr in ((DataTable)Session["rejectedMainItemDT"]).Rows)
                        {
                            decimal amount = string.IsNullOrEmpty(dr["ITM_Amount"].ToString().Trim()) ? 0 : Convert.ToDecimal(dr["ITM_Amount"].ToString().Trim());

                            //select row with "RowUpate" = 1,loop into it and update DB
                            //Ensure update original amount also ITM_OriChqBSB, ITM_OriDepositor 
                            if (dr["ITM_StubCreatedbyWeb"].ToString().Trim().Equals("0") && dr["RowUpdated"].ToString().Trim().Equals("1"))
                            {
                                CommonFunction.UpdatedAmendedItems(dr);
                                countAmend++;
                            }

                            //select row with "RowUpate" = 1 & ITM_StubCreatedByWeb = 1, loop into it and insert into DB
                            //Ensure update original amount also ITM_OriChqBSB, ITM_OriDepositor 
                            if (dr["ITM_StubCreatedbyWeb"].ToString().Trim().Equals("1"))
                            {
                                CommonFunction.InsertStubItems(dr);
                                countAmend++;
                            }
                        }
                    }

                    //CR015-20 check this transation required multi level approval
                    bool UpdFinal = ProceedToFinalDecision("A", countAmend);

                    if (UpdFinal)
                    {
                        //Update TBL_REJECTITEMSTATUS
                        string remark = txtARemark.InnerText.ToString().Trim().Length > 0 ? txtARemark.InnerText.ToString() : "";

                        //Update TBL_ITEM -> ITM_REJECTED = 0 , ITM_REJECTREASON = ''
                        //NOT INSERT TBL_FCMSTATUS
                        bool isMultiple = ((DataTable)Session["stubDT"]).Rows.Count > 1;
                        CommonFunction.UpdatedAcceptedItemsRLOnly(Session["selClientDBConnStr"].ToString(), remark, isMultiple, Convert.ToBoolean(Session["isBatchBPC"]));

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

                    //Clear all session value upon complete
                    ClearSessionValue();

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
                log.Message = "Click Confirm Accpet Error:" + ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/WebDecision", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        [WebMethod]
        public static string getImagePath(string id)
        {
            DataRow itemRow = ((DataTable)HttpContext.Current.Session["chequeDT"]).Rows[0];

            string curSelTopImg = id.Contains("TOP") ? HttpContext.Current.Session["CurSelectedTopImage"].ToString() :
                HttpContext.Current.Session["CurSelectedBottomImage"].ToString();

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
            else //if (id.Equals("BToggle")) 
            {
                return "/ImageHandlerBottom.ashx?id=" + randomID;
            }
        }


        //CR015-20 check for multi level approval
        public bool ProceedToFinalDecision(string decision, int amendCount)
        {
            bool updFinal = false;
            bool routeToFinal = false;
            string remark = txtARemark.InnerText.ToString().Trim().Length > 0 ? txtARemark.InnerText.ToString() : "";
            bool isForUnisys = Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(Session["s_UserForUnisys"].ToString());
            decimal minAmt = string.IsNullOrEmpty(Session["s_App_MinAmt"].ToString()) ? 0 : Convert.ToDecimal(Session["s_App_MinAmt"].ToString());
            decimal maxAmt = string.IsNullOrEmpty(Session["s_App_MaxAmt"].ToString()) ? 0 : Convert.ToDecimal(Session["s_App_MaxAmt"].ToString());
            try
            {
                if (Session["s_RequiredApproval"].ToString().Equals("True") && !isForUnisys)
                {
                    if (!Session["s_ReviewNo"].ToString().Equals("3"))
                    {
                        if (!Session["s_ReviewNo"].ToString().Equals("1"))
                        {
                            //check amount whether still hit minimum amount for multi level approval
                            bool bHitMinAmt = false;
                            foreach (DataRow dr in ((DataTable)Session["rejectedMainItemDT"]).Rows)
                            {
                                if (!dr["ITM_ItemType"].ToString().Trim().Equals("S"))
                                {
                                    decimal updAmt = string.IsNullOrEmpty(dr["ITM_Amount"].ToString().Trim()) ? 0 : Convert.ToDecimal(dr["ITM_Amount"].ToString().Trim());
                                    if (updAmt >= minAmt)
                                    {
                                        bHitMinAmt = true;
                                        break;
                                    }
                                }
                            }

                            if (bHitMinAmt)
                            {
                                routeToFinal = !Session["s_1stDecision"].ToString().Equals(decision) ? true : false;
                                updFinal = Session["s_1stDecision"].ToString().Equals(decision) ? true : false;

                                //if got amendment, route to 3rd reviewer for review
                                if (amendCount > 0)
                                {
                                    routeToFinal = true;
                                    updFinal = false;
                                }
                            }
                            else
                            {
                                decision = "S";
                                updFinal = true;
                                routeToFinal = false;
                            }
                        }
                        //only level 1 does this checking
                        //check for amount whether is amend to less than 50k
                        else
                        {
                            updFinal = true;

                            foreach (DataRow dr in ((DataTable)Session["rejectedMainItemDT"]).Rows)
                            {
                                if (!dr["ITM_ItemType"].ToString().Trim().Equals("S"))
                                {
                                    decimal updAmt = string.IsNullOrEmpty(dr["ITM_Amount"].ToString().Trim()) ? 0 : Convert.ToDecimal(dr["ITM_Amount"].ToString().Trim());
                                    if (updAmt >= minAmt)
                                    {
                                        updFinal = false;
                                        break;
                                    }
                                }
                            }

                            if (updFinal)
                                decision = "S";
                        }

                        //update decision for multi level review
                        CommonFunction.UpdateMultiLevelDecision(Session["selClientDBConnStr"].ToString(), remark, decision, Session["s_ReviewNo"].ToString(), routeToFinal);

                        string msg = string.Format(Resource.msgItemDecisionLevel, curTransNo.ToString(), Session["s_ReviewNo"].ToString(), decision);
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
            catch (Exception ex)
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
    }
}