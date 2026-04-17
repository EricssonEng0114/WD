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
    public partial class ValidateRLRejectMultipleMode : System.Web.UI.Page
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
                AssignSessionValue();
                btnAccept.Visible = true;

                //if (isForUnisys)
                //{
                //    btnAccept.Visible = false;  
                //}
                //else
                //{
                //    btnAccept.Visible = true;
                //}

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
                }
                else
                {
                    //Set Valid navigation
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "setValidNav", "setValidNavigation();", true);
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
                //Reset current index = 0 for first time
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
                    DataTable dtMainDetail = CommonFunction.GetRLDetailItemInfo(Session["selClientDBConnStr"].ToString());
                    Session["rejectedMainItemDT"] = dtMainDetail;

                    if (((DataTable)Session["rejectedMainItemDT"]).Rows.Count > 0)
                    {
                        //update reject reason title
                        lblRejReason.Text = "REJECT REASON: " + Session["s_CurSelectedRejectReason"].ToString();

                        //Populate Item to Listbox
                        LoadListBox();

                        //Get Virtual Directory for IFS Path
                        int curTransportID = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Rows[0]["ITM_TransportID"].ToString().PadLeft(2, '0'));

                       Session["VirDirForIFSPath"] = CommonFunction.GetDirectoryForIFSPath(curTransportID, curSite);

                        Session["curTransItemsCount"] = ((DataTable)Session["rejectedMainItemDT"]).Rows.Count;
                        Session["topItemPtr"] = 0;
                        //Show Data on screen
                        if (!Session["s_ReviewNo"].ToString().Equals("1"))
                        {
                            txtARemark.InnerText = Session["s_RejectedRemark"].ToString();

                            if (Session["s_ReviewNo"].ToString().Equals("3"))
                            {
                                txtTopItem1.Attributes.Add("readonly", "readonly");
                                txtTopItemAmount.Attributes.Add("readonly", "readonly");
                            }
                        }

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
                    Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Validate Rejected Item Decision", "Invalid session value for PV reject re.", LogEventType.Error, userID);
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
                if (CommonFunction.IsTransReset())
                {
                    Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Tranaction Reset", "Unable to proceed as selected transaction is already reset", LogEventType.Error, userID);
                    Response.Redirect("/WebDecision", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
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
                    //Added Shinyi - store Stub Ori Dep Acct No and Cheq Ori BSB - CIMB need for validation upon accepting trans
                    Session["CurOriStubDepAcctNo"] = itemRow["ITM_OriDepositor"] == null? string.Empty: itemRow["ITM_OriDepositor"].ToString();
                    Session["CurOriChequeBSB"] = itemRow["ITM_OriChqBSB"] == null? string.Empty: itemRow["ITM_OriChqBSB"].ToString();

                    //Shinyi - store Batch bsb - RHB need for Validate GL Account Number
                    Session["CurOriBathchBSB"] = itemRow == null ? itemRow["ITM_FLD10"].ToString() : itemRow["ITM_FLD10"].ToString();
                    //Shinyi - store other transaction stub dep account - CIMB does not allow cross product in a batch for different transactions
                    Session["CurOtherStubDepAcct"] = string.IsNullOrEmpty(Session["CurOtherStubDepAcct"].ToString().Trim()) ?
                        CommonFunction.GetOtherStubAccountNo(Session["selClientDBConnStr"].ToString(), (itemRow["ITM_DIN"] == null ? "0" : itemRow["ITM_DIN"].ToString().Trim()))
                        : Session["CurOtherStubDepAcct"].ToString().Trim();


                    //calculate total stub n cheque count & amount
                    int curTotalOriStubCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='S'"));
                    int curTotalOriChqCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='C'"));

                    //MSMC Scenario - total count and amount need to skip rejected stub
                    if (curTotalOriStubCount > 1 && curTotalOriChqCount > 1)  
                    {
                        curTotalStubAmt = Convert.ToDecimal(((DataTable)Session["rejectedMainItemDT"]).Compute("SUM(ITM_Amount)", "ITM_ItemType='S' AND StubReject = 0"));
                        curTotalStubCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='S' AND StubReject = 0"));
                    }
                    else
                    {
                        curTotalStubAmt = Convert.ToDecimal(((DataTable)Session["rejectedMainItemDT"]).Compute("SUM(ITM_Amount)", "ITM_ItemType='S'"));
                        curTotalStubCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='S'"));
                    }

                    curTotalChqAmt = Convert.ToDecimal(((DataTable)Session["rejectedMainItemDT"]).Compute("SUM(ITM_Amount)", "ITM_ItemType='C'"));
                    curTotalChqCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='C'"));

                    lblTotalStubCount.Text = "Total Stub Count: " + curTotalStubCount.ToString();
                    txtTotalStubAmt.Text = curTotalStubAmt.Equals(0) ? "0.00" : String.Format("{0:N}", curTotalStubAmt);
                    lblTotalChequeCount.Text = "Total Cheque Count: " + curTotalChqCount.ToString();
                    txtTotalChequeAmt.Text = curTotalChqAmt.Equals(0) ? "0.00" : String.Format("{0:N}", curTotalChqAmt);


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

                    if (selItemIsStub)
                    {
                        Session["isBatchBPC"] = Convert.ToBoolean(itemRow["BST_IsBPC"].ToString().Trim());
                        //Check if it's BPC IN ocbc then show "BSB"
                        lblTopItem1.Text = Convert.ToBoolean(itemRow["BST_IsBPC"].ToString().Trim()).Equals(true) && Session["s_CurSelectedClientRejDec"].ToString().Equals("OCBC") ?
                                          "Presenting BSB:": "Depositor Account No.:";
                        Session["curSelectedStub"] = Session["curItemPtr"];
                        txtTopItem1.Text = itemRow["ITM_FLD12"].ToString().Trim();
                        //txtTopItem1.MaxLength =  Convert.ToBoolean(itemRow["BST_IsBPC"].ToString().Trim()).Equals(true) ? 7: 16;
                    }
                    else
                    {
                        lblTopItem1.Text = "Cheque BSB:";
                        txtTopItem1.Text = itemRow["ITM_FLD4"].ToString().Trim();
                        //txtTopItem1.MaxLength = 7;
                    }

                    //PE-WD-24-002 - Added by BOONCHONG 
                    if (Convert.ToBoolean(Session["isBatchBPC"]))
                    {
                        txtTopItem1.Enabled = false;
                        txtTopItemAmount.Enabled = false;

                        lblRejReason.Style["color"] = "red";
                        lblRejReason.Style["font-weight"] = "bold";
                        lblRejReason.Style["background-color"] = "yellow";
                    }

                    txtTopItemAmount.Text = (Convert.ToDecimal(itemRow["ITM_Amount"].ToString()).Equals(0))
                        ?"0.00": String.Format("{0:N}", Convert.ToDecimal(itemRow["ITM_Amount"].ToString()));

                    //Display reject reason 
                    lblItemRejReason.Text = itemRow["ITM_RejectReason"] == null ? "" :                         
                        string.IsNullOrEmpty(itemRow["ITM_RejectReason"].ToString().Trim())? "" :
                        "* " + itemRow["ITM_RejectReason"].ToString().Trim();

                    if (string.IsNullOrEmpty(lblItemRejReason.Text))
                    {
                        lblItemRejReason.Font.Size = 3;
                    }
                    else 
                    {
                        lblItemRejReason.Font.Size = 12;
                    }

                    //Disable editable fields for rejected stub in MSMC
                    //check title got "*", if "*" mean its valid stub reject to web
                    //If not containing "*" but with reject reason - disable the field
                    if (curTotalOriStubCount > 1 && curTotalOriChqCount > 1)
                    {
                        if (itemRow["ITM_ItemType"].ToString().Trim().Equals("S") && (!itemRow["ListBoxTitle"].ToString().Trim().Contains("*") && !string.IsNullOrEmpty(itemRow["ITM_RejectReason"].ToString().Trim())))
                        {
                            txtTopItem1.ReadOnly = true;
                            txtTopItemAmount.ReadOnly = true;                           
                        }
                        else 
                        {
                            txtTopItem1.ReadOnly = false;
                            txtTopItemAmount.ReadOnly = false;
                        }
                    }
                    else 
                    {
                        txtTopItem1.ReadOnly = false;
                        txtTopItemAmount.ReadOnly = false;
                    }
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

        private void LoadListBox() 
        {
            try
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

                //foreach (ListItem li in LstBSelectItem.Items)
                //{
                //    if (li.Text.Trim().Contains("*"))
                //    {
                //        this.LstBSelectItem.Items[curSelectedIndex].Attributes.Add("style", "color: red");
                //    }
                //}
                //
            }
            catch (Exception ex) 
            {
                throw ex;
            }

        }

        private bool ValidateAccept(out string oerrMsg, string bsbOrAccntNo, decimal amount,bool isStub)
        {
            try
            {
                bool isValidInput = false;
                oerrMsg = string.Empty;
                
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
                else if (!(string.IsNullOrEmpty(bsbOrAccntNo.Trim()) || bsbOrAccntNo.Trim().Equals("0")) && amount.Equals(0))
                {
                    txtTopItemAmount.Focus();
                    isValidInput = false;
                    oerrMsg = "Please enter valid amount greater than zero.";
                }
                else
                {
                    isValidInput = true;
                }

                RejDecValidatorResult rejDecValidator;

                rejDecValidator = RejDecValidation.ValidateAcceptedTransaction(Session["selClientDBConnStr"].ToString(), bsbOrAccntNo, isStub, Convert.ToBoolean(Session["isBatchBPC"]), Session["CurOriChequeBSB"].ToString().Trim(), Session["CurOriStubDepAcctNo"].ToString().Trim(),Session["CurOriBathchBSB"].ToString().Trim(), Session["CurOtherStubDepAcct"].ToString().Trim());
                isValidInput = rejDecValidator.Valid;
                oerrMsg = rejDecValidator.ReturnMessage;

                if (Session["s_CurSelectedClientRejDec"].ToString().Equals("OCBC"))
                {
                    //stub depositor account require cdv checking
                    if (isValidInput && isStub && !Convert.ToBoolean(Session["isBatchBPC"]))
                    {
                        //CDV Checking
                        rejDecValidator = RejDecValidation.ValidateCDV(Session["selClientDBConnStr"].ToString(), bsbOrAccntNo);
                        isValidInput = rejDecValidator.Valid;
                        oerrMsg = rejDecValidator.ReturnMessage;
                    }

                    //Bsb validation for BPC's Stub & Cheque Items
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
            int curIndex = Convert.ToInt16(Session["curSelListBoxItemIdx"]);
            DataRow rowBefore = ((DataTable)Session["rejectedMainItemDT"]).Rows[curIndex];//Select(filterRow).FirstOrDefault();

            //Check if current stub count  = 1 only
            //Update the stub first
            #region Update Stub/cheque

            decimal amount = 0;
            amount = string.IsNullOrEmpty(txtTopItemAmount.Text.Trim()) ? 0 : Convert.ToDecimal(txtTopItemAmount.Text.Trim());
            if (rowBefore != null)
            {
                rowBefore["RowUpdated"] = 1;
                rowBefore["ITM_Amount"] = amount;

                if (rowBefore["ITM_ItemType"].ToString().Equals("S"))
                {
                    rowBefore["ITM_Fld12"] = txtTopItem1.Text.Trim();
                }
                else
                {
                    rowBefore["ITM_Fld4"] = txtTopItem1.Text.Trim();
                }
            }

            string listBoxTitle = string.Empty;
            if (!rowBefore["ListBoxTitle"].ToString().Trim().Contains("*"))
            {
                listBoxTitle = rowBefore["ITM_ItemType"].ToString().Trim() + ": " + "RM " + String.Format("{0:n}", amount);
            }
            else
            {
               listBoxTitle =  rowBefore["ITM_ItemType"].ToString().Trim() + ": " + "RM " + String.Format("{0:n}", amount) + "*";
            }

            rowBefore["ListBoxTitle"] = listBoxTitle;
            
            LoadListBox();
            #endregion                        

            //calculate total stub n cheque count & amount
            int curTotalOriStubCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='S'"));
            int curTotalOriChqCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='C'"));

            //MSMC Scenario - total count and amount need to skip rejected stub
            if (curTotalOriStubCount > 1 && curTotalOriChqCount > 1)
            {
                curTotalStubAmt = Convert.ToDecimal(((DataTable)Session["rejectedMainItemDT"]).Compute("SUM(ITM_Amount)", "ITM_ItemType='S' AND StubReject = 0"));
                curTotalStubCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='S' AND StubReject = 0"));
            }
            else
            {
                curTotalStubAmt = Convert.ToDecimal(((DataTable)Session["rejectedMainItemDT"]).Compute("SUM(ITM_Amount)", "ITM_ItemType='S'"));
                curTotalStubCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='S'"));
            }

            curTotalChqAmt = Convert.ToDecimal(((DataTable)Session["rejectedMainItemDT"]).Compute("SUM(ITM_Amount)", "ITM_ItemType='C'"));
            curTotalChqCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='C'"));

            lblTotalStubCount.Text = "Total Stub Count: " + curTotalStubCount.ToString();
            txtTotalStubAmt.Text = curTotalStubAmt.Equals(0) ? "0.00" : String.Format("{0:N}", curTotalStubAmt);

            lblTotalChequeCount.Text = "Total Cheque Count: " + curTotalChqCount.ToString();
            txtTotalChequeAmt.Text = curTotalChqAmt.Equals(0) ? "0.00" : String.Format("{0:N}", curTotalChqAmt);
        }

        private bool UpdateCurrentSelectedItem(out string errMsg)
        {
            bool isValidInput = false;
            errMsg = string.Empty;

            //If it's not readonly - mean for editable then need to update value to datatable
            bool isUpdForAmended = !txtTopItemAmount.ReadOnly;

            if (isUpdForAmended)
            {
                //Update current table value first
                string curAcctNoOrBSB = string.Empty;
                decimal curAmount = 0;
                int curIndex = Convert.ToInt16(Session["curSelListBoxItemIdx"]);
                string curIDValue = LstBSelectItem.Items[curIndex].Value;

                string filterRow = String.Format("TMPID='{0}'", curIDValue);
                DataRow row = ((DataTable)Session["rejectedMainItemDT"]).Select(filterRow).FirstOrDefault();
                bool isRowChange = false;
                decimal amount = string.IsNullOrEmpty(txtTopItemAmount.Text.Trim()) ? 0 : Convert.ToDecimal(txtTopItemAmount.Text.Trim());

                if (row != null)
                {
                    if (ValidateAccept(out errMsg, txtTopItem1.Text.Trim(), amount, lblTopItem1.Text.Contains("Depositor")))
                    {
                        isRowChange = Convert.ToBoolean(row["RowUpdated"]);

                        curAcctNoOrBSB = row["ITM_ItemType"].ToString().Trim().Equals("S") ? row["ITM_FLD12"].ToString().Trim() : row["ITM_FLD4"].ToString().Trim();
                        curAmount = Convert.ToDecimal(row["ITM_Amount"].ToString().Trim());

                        if (!isRowChange)
                        {
                            if ((!curAcctNoOrBSB.Equals(txtTopItem1.Text.Trim())) || !(curAmount.Equals(Convert.ToDecimal(txtTopItemAmount.Text.Trim()))))
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

                }

                if (isValidInput)
                {
                    if (isRowChange)
                    {
                        row["RowUpdated"] = 1;

                        if (row["ITM_ItemType"].ToString().Equals("S"))
                        {
                            row["ITM_Fld12"] = txtTopItem1.Text.Trim();
                        }
                        else
                        {
                            row["ITM_Fld4"] = txtTopItem1.Text.Trim();
                        }

                        row["ITM_Amount"] = Convert.ToDecimal(txtTopItemAmount.Text.Trim());

                        //title need to change
                        string listBoxTitle = string.Empty;
                        if (!row["ListBoxTitle"].ToString().Trim().Contains("*"))
                        {
                            listBoxTitle = row["ITM_ItemType"].ToString().Trim() + ": " + "RM " + String.Format("{0:n}", amount);
                        }
                        else
                        {
                            listBoxTitle = row["ITM_ItemType"].ToString().Trim() + ": " + "RM " + String.Format("{0:n}", amount) + "*";
                        }

                        row["ListBoxTitle"] = listBoxTitle;

                        LoadListBox();
                    }
                    else
                    {
                        if (!row["RowUpdated"].ToString().Trim().Equals("1"))
                            row["RowUpdated"] = 0;
                    }
                }
                else
                {
                    isValidInput = false;
                }
            }
            else 
            {
                isValidInput = true;
            }

            return isValidInput;
        }

        #endregion

        #region Event Handling

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
                if (CommonFunction.IsTransReset())
                {
                    Logger.Write(false, LogCallerID.RejectedItemDecision, curSelectedClient, "Tranaction Reset", "Unable to proceed as selected transaction is already reset", LogEventType.Error, userID);
                    Response.Redirect("/WebDecision", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    string errMsg = string.Empty;
                    //User might want to navigate at first time enter and reject
                    //Validate only upon accept button click
                    bool isValidChanged = UpdateCurrentSelectedItem(out errMsg);
                    isValidChanged = true;//need no do validation but need to update value changed
                    int curIndex = Convert.ToInt16(Session["curSelListBoxItemIdx"]);

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
            if (string.IsNullOrEmpty(txtTopItem1.Text.Trim()))
            {
                txtTopItem1.Text = "";
            }
            else
            {
                if (!UBPC.Web.Common.CommonFunction.isNumberOnly(txtTopItem1.Text.Trim()))
                {
                    txtTopItem1.Text = "";
                }                
            }

            RefreshListBoxAndCalTotalAmt();
        }    
              
        protected void btnAccept_Click(object sender, EventArgs e)
        {            
            string errMsg;
            decimal amount = string.IsNullOrEmpty(txtTopItemAmount.Text.Trim()) ? 0 : Convert.ToDecimal(txtTopItemAmount.Text.Trim());
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
                    //itemRow["ListBoxTitle"]"ITM_ItemType='S' AND ITM_Rejected = 0"));
                    DataTable dtTable = (DataTable)Session["rejectedMainItemDT"];
                    int curTotalValidStubCount = Convert.ToInt32(dtTable.Compute("COUNT(ITM_Amount)", "ITM_ItemType='S' AND StubReject = 0"));
                    int curTotalValidChqCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='C'"));

                    //Not allow multiple stub and multiple cheque scenario
                    if (curTotalValidStubCount > 1 && curTotalValidChqCount > 1)
                    {
                        isValidAccept = false;
                        errMsg = "You are not allow to accept. It will cause invalid multiple stubs and multiple cheques transaction.";
                    }
                    else
                    {
                        isValidAccept = true;
                        errMsg = "";
                    }

                    if (isValidAccept)
                    {
                        //differnet stub, different account no
                        //amount cannot be zero
                        //validate by cdv: account no cannot be zero
                        if (ValidateAccept(out errMsg, txtTopItem1.Text.Trim(), amount, lblTopItem1.Text.Contains("Depositor")))
                        {
                            //validate total stub amount and total cheque amount
                            if ((txtTotalChequeAmt.Text.ToString().Trim().Equals(txtTotalStubAmt.Text.ToString().Trim())) && Convert.ToDecimal(txtTotalChequeAmt.Text.ToString().Trim()) > 0)
                            {
                                //calculate total stub n cheque count & amount
                                int curTotalOriStubCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='S'"));
                                int curTotalOriChqCount = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='C'"));

                                bool isMSMC = (curTotalOriStubCount > 1 && curTotalOriChqCount > 1);

                                //select row with amendment, "RowUpate" = 1, loop into it and update DB
                                //Ensure update original value also ITM_OriChqBSB, ITM_OriDepositor 
                                foreach (DataRow dr in ((DataTable)Session["rejectedMainItemDT"]).Rows)
                                {
                                    decimal updAmt = string.IsNullOrEmpty(dr["ITM_Amount"].ToString().Trim()) ? 0 : Convert.ToDecimal(dr["ITM_Amount"].ToString().Trim());
                                    string acctNoOrBSB = dr["ITM_ItemType"].ToString().Trim().Equals("S") ? dr["ITM_Fld12"].ToString().Trim() : dr["ITM_Fld4"].ToString().Trim();

                                    if (ValidateAccept(out errMsg, acctNoOrBSB, updAmt, dr["ITM_ItemType"].ToString().Trim().Equals("S")))
                                    {
                                        bool toCheck = true;
                                        if (isMSMC)
                                        {
                                            toCheck = dr["StubReject"].ToString().Trim().Equals("0");
                                        }

                                        if (toCheck)
                                        {
                                            //decimal updAmt = string.IsNullOrEmpty(dr["ITM_Amount"].ToString().Trim()) ? 0 : Convert.ToDecimal(dr["ITM_Amount"].ToString().Trim());

                                            if (updAmt <= 0)
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
                                            isValidAccept = true;
                                        }

                                    }
                                    else
                                    {
                                        isValidAccept = false;
                                        break;
                                    }
                                }

                                if (isValidAccept)
                                {
                                    isValidAccept = UpdateCurrentSelectedItem(out errMsg);
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
                    int countAmend = 0;
                    bool isForUnisys = Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(Session["s_UserForUnisys"].ToString());
                    bool bMultiApproval = Convert.ToBoolean(Session["s_ClientRequiredApproval"].ToString());
                    decimal minAmt = string.IsNullOrEmpty(Session["s_App_MinAmt"].ToString()) ? 0 : Convert.ToDecimal(Session["s_App_MinAmt"].ToString());
                    if(!isForUnisys)
                    {
                        //check whether the cheque amount is over min amount for multi level approval
                        if(!Session["s_RequiredApproval"].ToString().Equals("True") && bMultiApproval)
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

                    //Clear all session value upon back to listing
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
                log.Message = "Click Reject Error:" + ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/WebDecision", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void btnConfirmAccept_Click(object sender, EventArgs e)
        {
            try 
            {
                int countAmend = 0;
                foreach (DataRow dr in ((DataTable)Session["rejectedMainItemDT"]).Rows)
                {
                    decimal updAmt = string.IsNullOrEmpty(dr["ITM_Amount"].ToString().Trim()) ? 0 : Convert.ToDecimal(dr["ITM_Amount"].ToString().Trim());

                    if (dr["RowUpdated"].ToString().Trim().Equals("1"))
                    {
                        countAmend++;
                        CommonFunction.UpdatedAmendedItems(dr);
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
                    bool isMultiple = Convert.ToInt32(((DataTable)Session["rejectedMainItemDT"]).Compute("COUNT(ITM_Amount)", "ITM_ItemType='S'")) > 1;
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
                //Clear all session value upon accepted
                ClearSessionValue();

                Response.Redirect("/WebDecision", false);
                Context.ApplicationInstance.CompleteRequest();                
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

                            if(bHitMinAmt)
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
                                if(!dr["ITM_ItemType"].ToString().Trim().Equals("S"))
                                {
                                    decimal updAmt = string.IsNullOrEmpty(dr["ITM_Amount"].ToString().Trim()) ? 0 : Convert.ToDecimal(dr["ITM_Amount"].ToString().Trim());
                                    if(updAmt >= minAmt)
                                    {
                                        updFinal = false;
                                        break;
                                    }
                                }
                            }

                            if(updFinal)
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