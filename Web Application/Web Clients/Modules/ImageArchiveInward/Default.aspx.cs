using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using UBPC.Web.Common;

namespace UBPCWeb.Modules.ImageArchiveInward
{
    public partial class Default : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private string clientCode = string.Empty;
        private bool isPageValid = false;
        private string strReturnMsg = string.Empty;
        public string tempErrorMsg = string.Empty;
        private string clientList = string.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString();
                userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString();
                clientList = Session["s_UserClients"] == null ? string.Empty : Session["s_UserClients"].ToString();
                Session["s_IsCutOffTime"] = false;

                if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
                {
                    PageValidatorResult validatorResult;

                    //For password chnge, validate password by encryption class
                    validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "ImageArchiveInward");
                    isPageValid = validatorResult.Valid;

                    if (!isPageValid)
                    {
                        Session["s_GeneralMsg"] = validatorResult.ReturnMessage;
                        Response.Redirect("/Home", false);
                        Context.ApplicationInstance.CompleteRequest();
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
                    Response.Redirect("/Login", false);
                    Context.ApplicationInstance.CompleteRequest();
                }


                //Load Control for first time
                if (isPageValid & !Page.IsPostBack)
                {
                    populateClientCode();
                    if (Session["IsClearSearchCriteria_Inward"] != null && Session["IsClearSearchCriteria_Inward"].ToString() == "False")
                    {
                        loadPrevSearchCriteria();
                    }
                    else
                    {
                        txtDateTime.Text = DateTime.Today.ToString("dd/MM/yyyy");
                    }

                    #region Get Total Count
                    string concatenatedText = txtDateTime.Text + "|" +
                                               //txtDateTimeTo.Text + "|" +
                                               txtAmount.Text + "|" +
                                               getOperate().Trim() + "|" +
                                               ddlClient.SelectedValue.ToString() + "|" +
                                               txtMicrAccNum.Text.Trim() + "|" +
                                               txtChequeNum.Text.Trim() + "|" +
                                               txtChequeBSB.Text.Trim() + "|" + "1" + "|" + "10" + "|" + "ITM_BusDate" + "|" + "ASC";

                    String[] filterList = concatenatedText.Split('|');
                    int totalRecordCount = 0;
                    //if (txtDateTimeFrom.Text != "" && txtDateTimeTo.Text != "")
                    if (txtDateTime.Text != "")
                    {
                        DataTable totalCount = CommonFunction.GetAllImageArchiveInward(filterList, "C");
                        if (totalCount.Rows.Count > 0)
                        {
                            totalRecordCount = Convert.ToInt32(totalCount.Rows[0][0]);
                        }
                        else
                        {
                            totalRecordCount = 0;
                        }
                        Session["Inward_totalRecordCount"] = totalRecordCount;
                    }
                    #endregion
                    LoadControl();
                    clientCode = ddlClient.SelectedValue.ToString();

                    //ADD START 20200826 JBCHONG CR053-19 - To have Auto Refresh at Web Decision Screen
                    string strAutoRefreshInterval = Parameters.GetParamValue(webDBConnStr, Resources.Resource.cstrAutoRefreshInterval);
                    string strAutoRefreshClientCode = Parameters.GetParamValue(webDBConnStr, Resources.Resource.cstrAutoRefreshInterval, false);
                    string isUnisysUser = Session["s_UserForUnisys"].ToString();
                    if (strAutoRefreshInterval == null || strAutoRefreshInterval == string.Empty || strAutoRefreshInterval == "")
                    {
                        Session["s_ToRefresh"] = "N";
                    }
                    else if (!clientList.Equals(strAutoRefreshClientCode) || isUnisysUser.Equals("True"))
                    {
                        Session["s_ToRefresh"] = "N";
                    }
                    else
                    {
                        Session["s_ToRefresh"] = "Y";
                    }
                    Session["s_AutoRefreshIntervalListing"] = Convert.ToInt32(strAutoRefreshInterval);
                    //ADD E N D 20200826 JBCHONG CR053-19 - To have Auto Refresh at Web Decision Screen
                }
                if (IsPostBack)
                {
                    #region Get Total Count
                    string concatenatedText = txtDateTime.Text + "|" +
                                               //txtDateTimeTo.Text + "|" +
                                               txtAmount.Text + "|" +
                                               getOperate().Trim() + "|" +
                                               ddlClient.SelectedValue.ToString() + "|" +
                                               txtMicrAccNum.Text.Trim() + "|" +
                                               txtChequeNum.Text.Trim() + "|" +
                                               txtChequeBSB.Text.Trim() + "|" + "1" + "|" + "10" + "|" + "ITM_BusDate" + "|" + "ASC";

                    String[] filterList = concatenatedText.Split('|');
                    int totalRecordCount = 0;
                    //if (txtDateTimeFrom.Text != "" && txtDateTimeTo.Text != "")
                    if (txtDateTime.Text != "")
                    {
                        DataTable totalCount = CommonFunction.GetAllImageArchiveInward(filterList, "C");
                        if (totalCount.Rows.Count > 0)
                        {
                            totalRecordCount = Convert.ToInt32(totalCount.Rows[0][0]);
                        }
                        else
                        {
                            totalRecordCount = 0;
                        }
                        Session["Inward_totalRecordCount"] = totalRecordCount;
                    }
                    #endregion

                }
                performView();
                Session["IsClearSearchCriteria_Inward"] = "True";

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

        private void LoadControl()
        {
            try
            {
                DBConnectionInfo db = new DBConnectionInfo();
                db.webConnStr = webDBConnStr;
                string mainUVRPSConnection = db.GetSiteConnectionString(true, ddlClient.SelectedValue.ToString(), db.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), ddlClient.SelectedValue.ToString().Trim()));


                string amount = string.Empty;
                string selectedOperator = string.Empty;
                amount = txtAmount.Text.Trim();

                if (txtDateTime.Text != "")
                {
                    if (string.IsNullOrWhiteSpace(txtMicrAccNum.Text) && string.IsNullOrWhiteSpace(txtChequeNum.Text)
                    && string.IsNullOrWhiteSpace(txtChequeBSB.Text) && string.IsNullOrWhiteSpace(txtAmount.Text))
                    {

                    }
                    else
                    {
                        var totalCount = Session["Inward_totalRecordCount"].ToString();
                        ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                            string.Format("initImageArchiveInwardDataTable('{0}','{1}','{2}','{3}','{4}','{5}','{6}', '{7}');",
                            txtDateTime.Text, amount.Trim(), getOperate().Trim(), ddlClient.SelectedValue.ToString(),
                            txtMicrAccNum.Text.Trim(), txtChequeNum.Text.Trim(), txtChequeBSB.Text.Trim(), totalCount), true);
                    }
                }

                ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void refreshDataTable()
        {
            //Initialise Grid
            //busdate, client, branch, category
            string amount = string.Empty;

            amount = txtAmount.Text.Trim();

            if (txtDateTime.Text != "" && Request.Form[btnClear.UniqueID] == null)
            {
                var totalCount = Session["Inward_totalRecordCount"].ToString();
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initImageArchiveInwardDataTable('{0}','{1}','{2}','{3}','{4}','{5}','{6}', '{7}');",
                    txtDateTime.Text, amount.Trim(), getOperate().Trim(), ddlClient.SelectedValue.ToString(),
                    txtMicrAccNum.Text.Trim(), txtChequeNum.Text.Trim(), txtChequeBSB.Text.Trim(), totalCount), true);
            }

            ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);

        }

        private void UpdateActiveBusdate()
        {
            try
            {
                DBConnectionInfo db = new DBConnectionInfo();
                db.webConnStr = webDBConnStr;
                string mainUVRPSConnection = db.GetSiteConnectionString(true, ddlClient.SelectedValue.ToString(), db.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), ddlClient.SelectedValue.ToString().Trim()));

                DateTime curActiveBusdate = DateTime.Today;
                if (BusinessDate.GetActive(mainUVRPSConnection) != null)
                {
                    curActiveBusdate = Convert.ToDateTime(BusinessDate.GetActive(mainUVRPSConnection));
                }

                //txtDateTimeFrom.Text = curActiveBusdate.ToShortDateString();
                //txtDateTimeTo.Text = curActiveBusdate.ToShortDateString();
                //Session["CurActionSelectedBusdateFrom"] = curActiveBusdate.ToShortDateString();
                //Session["CurActionSelectedBusdateTo"] = curActiveBusdate.ToShortDateString();

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveInward;
                log.ClientCode = ddlClient.SelectedValue == null ? "" : ddlClient.SelectedValue.ToString();
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string busdate = string.Empty;
            string busdateTo = string.Empty;
            string client = string.Empty;
            string presentingBch = string.Empty;
            string category = string.Empty;
            string searchCriteria = string.Empty;

            //Session["CurActionSelectedBusdateFrom"] = txtDateTimeFrom.Text;
            //Session["CurActionSelectedBusdateTo"] = txtDateTimeTo.Text;

            //Store current search criteria
            Session["SearchCri_Inward_ClientCode"] = ddlClient.SelectedValue.ToString();
            Session["SearchCri_Inward_BusDate"] = txtDateTime.Text;
            //Session["SearchCri_Inward_BusDateTo"] = txtDateTimeTo.Text;
            Session["SearchCri_Inward_MicrAccNum"] = txtMicrAccNum.Text.Trim();
            Session["SearchCri_Inward_ChequeNum"] = txtChequeNum.Text.Trim();
            Session["SearchCri_Inward_ChequeBSB"] = txtChequeBSB.Text.Trim();
            Session["SearchCri_Inward_Amount"] = txtAmount.Text.Trim();
            Session["SearchCri_Inward_Operate"] = getOperate();

            string amount = string.Empty;

            busdate = txtDateTime.Text.Trim();
            //busdateTo = txtDateTimeTo.Text.Trim();
            client = ddlClient.SelectedValue.ToString().Trim();
            amount = txtAmount.Text.Trim();

            if (validateTextField())
            {
                dateErrorMsg.Style["display"] = "none";
                if (string.IsNullOrEmpty(busdate.Trim()))
                {
                    Logger.Write(false, LogCallerID.ImageArchiveInward, "", "Validate Search", "BusDate is Empty", LogEventType.Error, userID);
                }
                else
                {
                    if (txtDateTime.Text != "")
                    {
                        var totalCount = Session["Inward_totalRecordCount"].ToString();
                        ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                            string.Format("initImageArchiveInwardDataTable('{0}','{1}','{2}','{3}','{4}','{5}','{6}', '{7}');",
                            txtDateTime.Text, amount.Trim(), getOperate().Trim(), ddlClient.SelectedValue.ToString(),
                            txtMicrAccNum.Text.Trim(), txtChequeNum.Text.Trim(), txtChequeBSB.Text.Trim(), totalCount), true);
                    }

                    ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);
                }
            }
            else
            {

            }







            //if (string.IsNullOrEmpty(busdateFrom.Trim()) || string.IsNullOrEmpty(busdateTo.Trim()))
            //{
            //    Logger.Write(false, LogCallerID.ImageArchiveInward, "", "Validate Search", "BusDate is Empty", LogEventType.Error, userID);
            //}
            //else
            //{
            //    if (txtDateTimeFrom.Text != "" && txtDateTimeTo.Text != "")
            //    {
            //        ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
            //            string.Format("initImageArchiveInwardDataTable('{0}','{1}','{2}','{3}','{4}','{5}','{6}', {7});",
            //            txtDateTimeFrom.Text, txtDateTimeTo.Text, amount.Trim(), getOperate().Trim(), ddlClient.SelectedValue.ToString(),
            //            txtMicrAccNum.Text.Trim(), txtChequeNum.Text.Trim(), txtChequeBSB.Text.Trim()), true);
            //    }

            //    ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);
            //}
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            //Clear Search Criteria
            txtDateTime.Text = DateTime.Today.ToString("dd/MM/yyyy");
            //txtDateTimeTo.Text = ""; //DateTime.Today.ToString("dd/MM/yyyy");
            txtMicrAccNum.Text = string.Empty;
            txtChequeNum.Text = string.Empty;
            txtChequeBSB.Text = string.Empty;
            txtAmount.Text = string.Empty;
            rblAmountType.SelectedValue = "smaller";
            getOperate();
            dateErrorMsg.Style["display"] = "none";
            ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);
        }



        [WebMethod]
        public static string assignSessionDT(string BatchNo, string BusDate, string CheckNum, string UIC)
        {
            //Get 1 row of DataTable record and assign to session
            HttpContext.Current.Session["Inward_MainDTParam"] = BatchNo.Trim() + "|" + BusDate + "|" + CheckNum + "|" + UIC;
            return "1";
        }

        protected void ddlClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            //UpdateActiveBusdate();
        }

        protected void rblAmountType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Handle the change in amount type selection

        }

        public string getOperate()
        {
            string operate = string.Empty;
            string selectedOperator = string.Empty;
            string selectedValue = string.Empty;
            Session["SearchCri_Inward_Operate"] = rblAmountType.SelectedValue;
            selectedValue = Session["SearchCri_Inward_Operate"].ToString();

            switch (selectedValue)
            {
                case "smaller":
                    selectedOperator = "<=";
                    break;
                case "fixed":
                    selectedOperator = "=";
                    break;
                case "larger":
                    selectedOperator = ">=";
                    break;
                default:
                    break;
            }
            operate = selectedOperator;
            return operate;
        }

        public void loadPrevSearchCriteria()
        {
            if (Session["SearchCri_Inward_ClientCode"] != null)
            {
                ddlClient.SelectedValue = Session["SearchCri_Inward_ClientCode"].ToString();
            }

            if (Session["SearchCri_Inward_BusDate"] != null)
            {
                txtDateTime.Text = Session["SearchCri_Inward_BusDate"].ToString();
            }
            else
            {
                txtDateTime.Text = DateTime.Today.ToString("dd/MM/yyyy");
            }

            if (Session["SearchCri_Inward_MicrAccNum"] != null)
            {
                txtMicrAccNum.Text = Session["SearchCri_Inward_MicrAccNum"].ToString();
            }

            if (Session["SearchCri_Inward_ChequeNum"] != null)
            {
                txtChequeNum.Text = Session["SearchCri_Inward_ChequeNum"].ToString();
            }

            if (Session["SearchCri_Inward_ChequeBSB"] != null)
            {
                txtChequeBSB.Text = Session["SearchCri_Inward_ChequeBSB"].ToString();
            }

            if (Session["SearchCri_Inward_Amount"] != null)
            {
                txtAmount.Text = Session["SearchCri_Inward_Amount"].ToString();
            }

            if (Session["SearchCri_Inward_Operate"] != null)
            {
                rblAmountType.SelectedValue = Session["SearchCri_Inward_Operate"].ToString();
            }

        }

        public void performView()
        {
            if (isPageValid && IsPostBack)
            {
                clientCode = ddlClient.SelectedValue.ToString();

                string tagOperation = Request.Form["tag"] == null ? string.Empty : Request.Form["tag"];

                if (tagOperation.Equals("VALIDATE"))
                {
                    string selectedRowParam = Session["Inward_MainDTParam"].ToString();
                    string selectedRowID = Request.Form["tableID"] == null ? string.Empty : Request.Form["tableID"].ToString().Trim();

                    if (!string.IsNullOrEmpty(selectedRowParam))
                    {
                        DBConnectionInfo dbConn = new DBConnectionInfo();
                        dbConn.webConnStr = webDBConnStr;
                        //string clientDB = dbConn.GetSiteConnectionString(true, ddlClient.SelectedValue.ToString().Trim(), "KL1");

                        DataTable checkDT = CommonFunction.GetSelectedImgArchiveInward(webDBConnStr, selectedRowParam.Split('|'), Session["SearchCri_Inward_ClientCode"].ToString().Trim());

                        //get TBL_ARCHIVE_SETTING
                        DataTable getImageArchivePath = CommonFunction.GetImageArchivePath(selectedRowParam.Split('|'), ddlClient.SelectedValue.ToString().Trim());
                        //Param List
                        //1 - Batch Dir, 2- Batch No, 3 - Trans NO, 4 - New Bundle ID , 5- Site name,6-Required Approval           
                        //
                        List<string> paramList = selectedRowParam.Split('|').ToList();
                        string approval = paramList.Last();

                        if (getImageArchivePath.Rows.Count > 0)
                        {
                            DataRow selectedRow = getImageArchivePath.Rows[0];
                            string selYearMonth = selectedRow["Archive_YearMonth"].ToString().Trim();
                            string selTableName = selectedRow["Archive_TableName"].ToString().Trim();
                            string selArchivePath = selectedRow["Archive_ArchivePath"].ToString().Trim();
                            string selIISVirtualPath = selectedRow["Archive_IISVirtualPath"].ToString().Trim();
                            string selType = selectedRow["Archive_Type"].ToString().Trim();

                            Session["s_CurSelectedYearMonthImgArc"] = selYearMonth;
                            Session["s_CurSelectedTableNameImgArc"] = selTableName;
                            Session["s_CurSelectedArchivePathImgArc"] = selArchivePath;
                            Session["s_CurSelectedIISVirtualPathImgArc"] = selIISVirtualPath;
                            Session["s_CurSelectedTypeImgArc"] = selType;
                        }

                        if (checkDT.Rows.Count > 0)
                        {
                            DataRow selectedRow = checkDT.Rows[0];

                            string selBusdate = selectedRow["BusDate"].ToString().Trim();
                            string selBatchNum = selectedRow["BatchNum"].ToString().Trim();
                            string selIssuingBankType = selectedRow["IssuingBankType"].ToString().Trim();
                            string selIssuingBank = selectedRow["IssuingBank"].ToString().Trim();
                            string selIssuingBranch = selectedRow["IssuingBranch"].ToString().Trim();
                            string selCheckNo = selectedRow["CheckNo"].ToString().Trim();
                            string selCheckDigit = selectedRow["CheckDigit"].ToString().Trim();
                            string selTRCode = selectedRow["TRCode"].ToString().Trim();//S: SINGLE, M/N: MULTIPLE, C: CHEQUES ONLY
                            string selAcctNo = selectedRow["AcctNo"].ToString().Trim();
                            string selAmount = selectedRow["Amount"].ToString().Trim();
                            string selNCF = selectedRow["NCF"].ToString().Trim();
                            string selUIC = selectedRow["UIC"].ToString().Trim();
                            string selReturnCount = selectedRow["ReturnCount"].ToString().Trim();
                            string selTransactionType = selectedRow["TransactionType"].ToString().Trim();
                            string selImageFolder = selectedRow["ImageFolder"].ToString().Trim();


                            //Assign row info to Session
                            Session["s_CurSelectedBusdateRejDec"] = Convert.ToDateTime(selBusdate);
                            Session["s_CurSelectedBatchNoRejDec"] = selBatchNum;
                            Session["s_CurSelectedIssuingBankTypeImgArc"] = selIssuingBankType;
                            Session["s_CurSelectedIssuingBankImgArc"] = selIssuingBank;
                            Session["s_CurSelectedIssuingBranchImgArc"] = selIssuingBranch;
                            Session["s_CurSelectedCheckNoImgArc"] = selCheckNo;
                            Session["s_CurSelectedCheckDigitImgArc"] = selCheckDigit;
                            Session["s_CurSelectedTRCodeImgArc"] = selTRCode;
                            Session["s_CurSelectedAcctNoImgArc"] = selAcctNo;
                            Session["s_CurSelectedAmountImgArc"] = selAmount;
                            Session["s_CurSelectedNCFImgArc"] = selNCF;
                            Session["s_CurSelectedUICImgArc"] = selUIC;
                            Session["s_CurSelectedReturnCountImgArc"] = selReturnCount;
                            Session["s_CurSelectedTransactionTypeImgArc"] = selTransactionType;
                            Session["s_CurSelectedImageFolderImgArc"] = selImageFolder;
                            Session["s_CurSelectedClientImgArc"] = Session["SearchCri_Inward_ClientCode"].ToString();


                            string url = string.Empty;
                            url = "ImageArchiveInwardDetail/";

                            //Clear session param value
                            Session["Inward_MainDTParam"] = null;

                            Response.Redirect(url, false);
                            Context.ApplicationInstance.CompleteRequest();

                        }
                        else
                        {
                            Logger.Write(false, LogCallerID.ImageArchiveInward, clientCode, "View Image Archive Inward", "Invalid record retrieved (Record ID does not existed).", LogEventType.Error, userID);
                        }

                    }
                    else
                    {
                        //Invalid ID retrieve for validate transaction
                        Logger.Write(false, LogCallerID.ImageArchiveInward, clientCode, "View Image Archive Inward", "Invalid record retrieved to view Image Archive Inward", LogEventType.Error, userID);
                    }
                }

                //Refresh again
                if (validateTextField())
                {
                    refreshDataTable();
                }
            }
        }

        public bool validateTextField()
        {
            if (txtDateTime.Text == "")// || txtDateTimeTo.Text == "")
            {
                if (IsPostBack)
                {
                    dateErrorMsg.Text = "Please select date.";
                    dateErrorMsg.Style["display"] = "block";
                    dateErrorMsg.Style["font-size"] = "13px";
                }
                return false;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtMicrAccNum.Text) && string.IsNullOrWhiteSpace(txtChequeNum.Text)
                && string.IsNullOrWhiteSpace(txtChequeBSB.Text) && string.IsNullOrWhiteSpace(txtAmount.Text))
                {
                    dateErrorMsg.Text = "Please enter one more search option to proceed";
                    dateErrorMsg.Style["display"] = "block";
                    dateErrorMsg.Style["font-size"] = "13px";
                    return false;
                }
                else
                {
                    dateErrorMsg.Style["display"] = "none";
                    return true;
                }
            }
        }

        protected void populateClientCode()
        {
            //Load Client Drop Down
            string[] cltArr = clientList.Split(',');
            List<ListItem> cltitems = new List<ListItem>();
            foreach (string clientCode in cltArr) cltitems.Add(new ListItem(clientCode, clientCode));
            this.ddlClient.DataSource = from i in cltitems select new ListItem() { Text = i.Text, Value = i.Value };
            this.ddlClient.DataTextField = "Text";
            this.ddlClient.DataValueField = "Value";
            this.ddlClient.DataBind();
            this.ddlClient.SelectedIndex = 0;
        }
    }
}