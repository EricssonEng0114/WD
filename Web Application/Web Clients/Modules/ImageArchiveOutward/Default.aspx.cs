using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Management;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using UBPC.Web.Common;
using UBPCWeb.Modules.Reports;

namespace UBPCWeb.Modules.ImageArchiveOutward
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
        private string clientList = string.Empty; protected string ReportCode { get; set; }
        protected string ReportClient { get; set; }
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
                    validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "ImageArchiveOutward");
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
                string eventTarget = Request.Params["__EVENTTARGET"];
                if (eventTarget != btnPrintRejected.UniqueID)
                {
                    if (isPageValid & !Page.IsPostBack)
                    {
                        populateItemTypeDropDownList();
                        if (Session["IsClearSearchCriteria_Outward"] != null && Session["IsClearSearchCriteria_Outward"].ToString() == "False")
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
                                                   txtDepositorAcc.Text + "|" +
                                                   getOperate().Trim() + "|" +
                                                   txtMicrAccNum.Text + "|" +
                                                   txtMicrBSB.Text + "|" +
                                                   txtChequeNum.Text + "|" +
                                                   ddlItemType.SelectedValue.ToString() + "|" +
                                                   txtPresentingBSB.Text.Trim() + "|" +
                                                   ddlClient.SelectedValue.ToString() + "|" +
                                                   "1" + "|" + "10" + "|" + "ITM_BusDate" + "|" + "ASC";

                        String[] filterList = concatenatedText.Split('|');
                        int totalRecordCount = 0;
                        //if (txtDateTimeFrom.Text != "" && txtDateTimeTo.Text != "")
                        if (txtDateTime.Text != "")
                        {
                            DataTable totalCount = CommonFunction.GetAllImageArchiveOutwardListing(filterList, "C");
                            if (totalCount.Rows.Count > 0)
                            {
                                totalRecordCount = Convert.ToInt32(totalCount.Rows[0][0]);
                            }
                            else
                            {
                                totalRecordCount = 0;
                            }
                            Session["Outward_totalRecordCount"] = totalRecordCount;
                        }
                        #endregion

                        LoadControl();

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


                    }

                    if (IsPostBack)
                    {
                        #region Get Total Count
                        string concatenatedText = txtDateTime.Text + "|" +
                                                   //txtDateTimeTo.Text + "|" +
                                                   txtAmount.Text + "|" +
                                                   txtDepositorAcc.Text + "|" +
                                                   getOperate().Trim() + "|" +
                                                   txtMicrAccNum.Text + "|" +
                                                   txtMicrBSB.Text + "|" +
                                                   txtChequeNum.Text + "|" +
                                                   ddlItemType.SelectedValue.ToString() + "|" +
                                                   txtPresentingBSB.Text.Trim() + "|" +
                                                   ddlClient.SelectedValue.ToString() + "|" +
                                                   "1" + "|" + "10" + "|" + "ITM_BusDate" + "|" + "ASC";

                        String[] filterList = concatenatedText.Split('|');
                        int totalRecordCount = 0;
                        //if (txtDateTimeFrom.Text != "" && txtDateTimeTo.Text != "")
                        if (txtDateTime.Text != "")
                        {
                            DataTable totalCount = CommonFunction.GetAllImageArchiveOutwardListing(filterList, "C");
                            if (totalCount.Rows.Count > 0)
                            {
                                totalRecordCount = Convert.ToInt32(totalCount.Rows[0][0]);
                            }
                            else
                            {
                                totalRecordCount = 0;
                            }
                            Session["Outward_totalRecordCount"] = totalRecordCount;
                        }
                        #endregion
                    }

                    performView();

                    string amount = txtAmount.Text.Trim();
                    //if (validateTextField())
                    //{
                    if (txtDateTime.Text != "")
                    {
                        if (!string.IsNullOrWhiteSpace(txtMicrAccNum.Text) && !string.IsNullOrWhiteSpace(txtMicrBSB.Text)
                           && !string.IsNullOrWhiteSpace(txtChequeNum.Text) && !string.IsNullOrWhiteSpace(txtPresentingBSB.Text)
                           && !string.IsNullOrWhiteSpace(txtDepositorAcc.Text) && !string.IsNullOrWhiteSpace(txtAmount.Text))
                        {
                            var totalCount = Session["Outward_totalRecordCount"].ToString();
                            ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                                string.Format("initImageArchiveOutwardDataTable('{0}','{1}','{2}','{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}');",
                                txtDateTime.Text, amount.Trim(), txtDepositorAcc.Text.Trim(), getOperate(),
                            txtMicrAccNum.Text.Trim(), txtMicrBSB.Text.Trim(), txtChequeNum.Text.Trim(),
                            ddlItemType.SelectedValue.ToString(), txtPresentingBSB.Text.Trim(), ddlClient.SelectedValue.ToString(), totalCount), true);
                        }
                    }
                }
                else
                {
                    if ((Session["IsClearSearchCriteria_Outward"] != null && Session["IsClearSearchCriteria_Outward"].ToString() == "False") || eventTarget == btnPrintRejected.UniqueID)
                    {
                        loadPrevSearchCriteria();
                    }
                    else
                    {
                        txtDateTime.Text = DateTime.Today.ToString("dd/MM/yyyy");
                    }
                    var totalCount = Session["Outward_totalRecordCount"].ToString();
                    ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                                string.Format("initImageArchiveOutwardDataTable('{0}','{1}','{2}','{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}');",
                                txtDateTime.Text, txtAmount.Text.Trim(), txtDepositorAcc.Text.Trim(), getOperate(),
                            txtMicrAccNum.Text.Trim(), txtMicrBSB.Text.Trim(), txtChequeNum.Text.Trim(),
                            ddlItemType.SelectedValue.ToString(), txtPresentingBSB.Text.Trim(), ddlClient.SelectedValue.ToString(), totalCount), true);
                }
                //}
                Session["IsClearSearchCriteria_Outward"] = "True";
                //ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);

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

        private void LoadControl()
        {            
            try
            {
                DBConnectionInfo db = new DBConnectionInfo();
                db.webConnStr = webDBConnStr;
                string mainUVRPSConnection = db.GetSiteConnectionString(true, ddlClient.SelectedValue.ToString(), db.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), ddlClient.SelectedValue.ToString().Trim()));

                string amount = string.Empty;
                string operate = string.Empty;
                amount = txtAmount.Text.Trim();
                operate = getOperate();

                if (txtDateTime.Text != "")
                {
                    if (string.IsNullOrWhiteSpace(txtMicrAccNum.Text) && string.IsNullOrWhiteSpace(txtMicrBSB.Text)
                       && string.IsNullOrWhiteSpace(txtChequeNum.Text) && string.IsNullOrWhiteSpace(txtPresentingBSB.Text)
                       && string.IsNullOrWhiteSpace(txtDepositorAcc.Text) && string.IsNullOrWhiteSpace(txtAmount.Text))
                    {
                    }
                    else
                    {
                        var totalCount = Session["Outward_totalRecordCount"].ToString();
                        ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                            string.Format("initImageArchiveOutwardDataTable('{0}','{1}','{2}','{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}');",
                            txtDateTime.Text, amount.Trim(), txtDepositorAcc.Text.Trim(), getOperate(),
                        txtMicrAccNum.Text.Trim(), txtMicrBSB.Text.Trim(), txtChequeNum.Text.Trim(),
                        ddlItemType.SelectedValue.ToString(), txtPresentingBSB.Text.Trim(), ddlClient.SelectedValue.ToString(), totalCount), true);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void refreshDataTable()
        {
            string amount = string.Empty;

            amount = txtAmount.Text.Trim();
            //Initialise Grid
            //busdate, client, branch, category
            if (validateTextField())
            {
                if (txtDateTime.Text != "" && Request.Form[btnClear.UniqueID] == null)
                {
                    var totalCount = Session["Outward_totalRecordCount"].ToString();
                    ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                            string.Format("initImageArchiveOutwardDataTable('{0}','{1}','{2}','{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}');",
                            txtDateTime.Text, amount.Trim(), txtDepositorAcc.Text.Trim(), getOperate(),
                        txtMicrAccNum.Text.Trim(), txtMicrBSB.Text.Trim(), txtChequeNum.Text.Trim(),
                        ddlItemType.SelectedValue.ToString(), txtPresentingBSB.Text.Trim(), ddlClient.SelectedValue.ToString(), totalCount), true);
                }
            }

            //ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);

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

                txtDateTime.Text = curActiveBusdate.ToShortDateString();
                //txtDateTimeTo.Text = curActiveBusdate.ToShortDateString();
                //Session["CurActionSelectedBusdateFrom"] = curActiveBusdate.ToShortDateString();
                //Session["CurActionSelectedBusdateTo"] = curActiveBusdate.ToShortDateString();

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveOutward;
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
            //string busdateTo = string.Empty;
            string client = string.Empty;
            string presentingBch = string.Empty;
            string category = string.Empty;
            string searchCriteria = string.Empty;

            NoDataMsg.Style["display"] = "none";

            //Store current search criteria
            Session["SearchCri_Outward_ClientCode"] = ddlClient.SelectedValue.ToString();
            Session["SearchCri_Outward_BusDate"] = txtDateTime.Text;
            //Session["SearchCri_Outward_BusDateTo"] = txtDateTimeTo.Text;
            Session["SearchCri_Outward_DepAcc"] = txtDepositorAcc.Text.Trim();
            Session["SearchCri_Outward_MicrAccNum"] = txtMicrAccNum.Text.Trim();
            Session["SearchCri_Outward_MicrBSB"] = txtMicrBSB.Text.Trim();
            Session["SearchCri_Outward_ChequeNum"] = txtChequeNum.Text.Trim();
            Session["SearchCri_Outward_ItemType"] = ddlItemType.SelectedValue.ToString();
            Session["SearchCri_Outward_PresentingBSB"] = txtPresentingBSB.Text.Trim();
            Session["SearchCri_Outward_Amount"] = txtAmount.Text.Trim();
            Session["SearchCri_Outward_Operate"] = getOperate();
            ddlPrintRejected.SelectedValue = "Normal";

            busdate = txtDateTime.Text.Trim();
            //busdateTo = txtDateTimeTo.Text.Trim();
            client = ddlClient.SelectedValue.ToString().Trim();

            string amount = string.Empty;

            if (string.IsNullOrEmpty(busdate.Trim()))
            {
                Logger.Write(false, LogCallerID.ImageArchiveOutward, "", "Validate Search", "BusDate is Empty", LogEventType.Error, userID);
            }
            else
            {
                amount = txtAmount.Text.Trim();
                if (validateTextField())
                {
                    if (txtDateTime.Text != "")
                    {
                        var totalCount = Session["Outward_totalRecordCount"].ToString();
                        if(totalCount != "0")
                        {
                            footerContainer.Visible = true;
                            bodyContainer.Style["margin-bottom"] = "0px !important";
                        }
                        else
                        {
                            footerContainer.Visible = false;
                            bodyContainer.Style["margin-bottom"] = "0px !important";
                        }
                        ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                            string.Format("initImageArchiveOutwardDataTable('{0}','{1}','{2}','{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}');",
                            txtDateTime.Text, amount.Trim(), txtDepositorAcc.Text.Trim(), getOperate(),
                        txtMicrAccNum.Text.Trim(), txtMicrBSB.Text.Trim(), txtChequeNum.Text.Trim(),
                        ddlItemType.SelectedValue.ToString(), txtPresentingBSB.Text.Trim(), ddlClient.SelectedValue.ToString(), totalCount), true);
                    }
                }

                ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);


            }

        }


        protected void btnClear_Click(object sender, EventArgs e)
        {
            //Clear search criteria
            txtDateTime.Text = DateTime.Today.ToString("dd/MM/yyyy");
            //txtDateTimeTo.Text = "";
            txtDepositorAcc.Text = string.Empty;
            txtMicrAccNum.Text = string.Empty;
            txtMicrBSB.Text = string.Empty;
            txtChequeNum.Text = string.Empty;
            ddlItemType.SelectedValue = "All";
            txtPresentingBSB.Text = string.Empty;
            txtAmount.Text = string.Empty;
            rblAmountType.SelectedValue = "smaller";
            ddlPrintRejected.SelectedValue = "Normal";
            getOperate();

            footerContainer.Visible = false;
            bodyContainer.Style["margin-bottom"] = "0px !important";

            dateErrorMsg.Style["display"] = "none";

            ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);

        }

        protected void btnPrintRejected_Click(object sender, EventArgs e)
        {
            try
            {
                DBConnectionInfo dbConn = new DBConnectionInfo();
                dbConn.webConnStr = webDBConnStr;

                //string selClientDBConnStr = dbConn.GetSiteConnectionString(true, clientCode.Trim(), dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim()));
                string clientSite = dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim());

                DateTime busdate = DateTime.ParseExact(Session["SearchCri_Outward_BusDate"].ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');
                Session["CurArchivalRejectCategory"] = ddlPrintRejected.SelectedValue.ToString().Trim();

                DataTable rejectedItem = CommonFunction.GetTransactionAllItems(ddlClient.SelectedValue.ToString().Trim(), clientSite, "", "", sqlDateTime, "0", "", "", "True", Session["CurArchivalRejectCategory"].ToString());//print selected rejected item
                //DataTable rejectedItem = CommonFunction.GetImageArchiveOutwardRejectedItem(ddlClient.SelectedValue.ToString().Trim(), ddlPrintRejected.SelectedValue.ToString().Trim());
                if (rejectedItem.Rows.Count > 0)
                {
                    NoDataMsg.Style["display"] = "none";
                    string selectedRowParam = "0|0|0|0|0|" + sqlDateTime + "|" + clientSite;
                    DataTable getImageArchivePath = CommonFunction.GetImageArchivePath(selectedRowParam.Split('|'), ddlClient.SelectedValue.ToString().Trim());
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

                    string clientCode = ddlClient.SelectedValue.ToString().Trim();
                    if (clientCode.Equals("CIMB"))
                    {
                        clientCode = "CIMB";
                    }
                    else
                    {
                        clientCode = "ALL";
                    }

                    DataTable reportInfo = CommonFunction.GetArchivalReportConnectionInfo("PAR01", clientCode);
                    DataRow itemRow = reportInfo.Rows[0];
                    Session["CurArchivalSelectedRptClient"] = ddlClient.SelectedValue.ToString(); //Session["s_CurSelectedClientImgArc"];
                    Session["CurArchivalSelectedRptCode"] = itemRow["RPT_ReportCode"];
                    Session["CurArchivalSelectedSiteCode"] = clientSite;//Session["s_CurSelectedSiteImgArc"];
                                                                        //Assign Report Session
                    Session["s_CurSelectedBusdateRejDec"] = busdate;
                    Session["CurArchivalSelectedBusdate"] = busdate; //Session["s_CurSelectedBusdateRejDec"];
                    Session["CurArchivalSelectedBatchDir"] = "";// Session["s_CurSelectedBatchDirImgArc"];
                    Session["CurArchivalSelectedBatchNo"] = ""; // Session["s_CurSelectedBatchNoRejDec"];
                    Session["CurArchivalSelectedTransNo"] = "0";// Session["currentTransNum"];
                    Session["CurArchivalSelectedTransSeqNum"] = "";//Session["s_CurSelectedTransSeqNumImgArc"];
                    Session["CurArchivalSelectedItemOnly"] = false;
                    Session["CurArchivalIsRejected"] = true;
                    Session["CurArchivalIsMultiple"] = "False";
                    Session["CurArchivalRejectCategory"] = ddlPrintRejected.SelectedValue.ToString(); //Normal, MOPO(081), FCC

                    ReportCode = Session["CurArchivalSelectedRptCode"].ToString(); // Replace with actual retrieval logic
                    ReportClient = Session["CurArchivalSelectedRptClient"].ToString(); // Replace with actual retrieval logic

                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadCR", "openCR();", true);
                }
                else
                {
                    NoDataMsg.Style["display"] = "inline"; 
                    string script = "setTimeout(function() { window.scrollTo(0, document.body.scrollHeight); }, 100);";
                    ClientScript.RegisterStartupScript(this.GetType(), "ScrollToBottom", script, true);
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

        [WebMethod]
        public static string PrintFunction(List<SelectedData> selectedData)
        {
            string url = string.Empty;
            DBConnectionInfo dbConn = new DBConnectionInfo();
            //dbConn.webConnStr = webDBConnStr;

            //string selClientDBConnStr = dbConn.GetSiteConnectionString(true, clientCode.Trim(), dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim()));
            //string clientSite = dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), "clientCode.Trim()");

            //DateTime busdate = DateTime.ParseExact("txtDateTime.text", "dd/MM/yyyy", CultureInfo.InvariantCulture);
            //string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');

            string selectedRowParam = "0|0|0|0|0|" + selectedData[0].Date + "|" + selectedData[0].SiteName;
            DataTable getImageArchivePath = CommonFunction.GetImageArchivePath(selectedRowParam.Split('|'), selectedData[0].Client);
            if (getImageArchivePath.Rows.Count > 0)
            {
                DataRow selectedRow = getImageArchivePath.Rows[0];
                string selYearMonth = selectedRow["Archive_YearMonth"].ToString().Trim();
                string selTableName = selectedRow["Archive_TableName"].ToString().Trim();
                string selArchivePath = selectedRow["Archive_ArchivePath"].ToString().Trim();
                string selIISVirtualPath = selectedRow["Archive_IISVirtualPath"].ToString().Trim();
                string selType = selectedRow["Archive_Type"].ToString().Trim();

                HttpContext.Current.Session["s_CurSelectedYearMonthImgArc"] = selYearMonth;
                HttpContext.Current.Session["s_CurSelectedTableNameImgArc"] = selTableName;
                HttpContext.Current.Session["s_CurSelectedArchivePathImgArc"] = selArchivePath;
                HttpContext.Current.Session["s_CurSelectedIISVirtualPathImgArc"] = selIISVirtualPath;
                HttpContext.Current.Session["s_CurSelectedTypeImgArc"] = selType;
            }

            string clientcode = selectedData[0].Client.ToString().Trim();
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
            HttpContext.Current.Session["CurArchivalSelectedRptClient"] = selectedData[0].Client; //Session["s_CurSelectedClientImgArc"];
            HttpContext.Current.Session["CurArchivalSelectedRptCode"] = itemRow["RPT_ReportCode"];
            HttpContext.Current.Session["CurArchivalSelectedSiteCode"] = selectedData[0].SiteName;//Session["s_CurSelectedSiteImgArc"];
            //Assign Report Session
            HttpContext.Current.Session["s_CurSelectedBusdateRejDec"] = selectedData[0].Date;
            HttpContext.Current.Session["CurArchivalSelectedBusdate"] = selectedData[0].Date; //Session["s_CurSelectedBusdateRejDec"];
            HttpContext.Current.Session["CurArchivalSelectedBatchDir"] = "";// Session["s_CurSelectedBatchDirImgArc"];
            HttpContext.Current.Session["CurArchivalSelectedBatchNo"] = ""; // Session["s_CurSelectedBatchNoRejDec"];
            HttpContext.Current.Session["CurArchivalSelectedTransNo"] = "0";// Session["currentTransNum"];
            HttpContext.Current.Session["CurArchivalSelectedTransSeqNum"] = "";//Session["s_CurSelectedTransSeqNumImgArc"];
            HttpContext.Current.Session["CurArchivalSelectedItemOnly"] = true;
            HttpContext.Current.Session["CurArchivalIsRejected"] = false;
            HttpContext.Current.Session["CurArchivalIsMultiple"] = "True";

            HttpContext.Current.Session["CurArchivalSelectedDataMultipleCheckBox"] = selectedData;

            //string ReportCode = HttpContext.Current.Session["CurArchivalSelectedRptCode"].ToString(); // Replace with actual retrieval logic
            //string ReportClient = HttpContext.Current.Session["CurArchivalSelectedRptClient"].ToString(); // Replace with actual retrieval logic

            //ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadCR", "openCR();", true);
            return "Success";
        }


        [WebMethod]
        public static string getReportUrl()
        {
            string clientcode = HttpContext.Current.Session["CurArchivalSelectedRptClient"].ToString().Trim();
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
            //string clientcode = HttpContext.Current.Session["CurArchivalSelectedRptClient"].ToString();
            //string reportcode = HttpContext.Current.Session["CurArchivalSelectedRptCode"].ToString();
            //string sitecode = HttpContext.Current.Session["CurArchivalSelectedSiteCode"].ToString();
            ////Assign Report Session
            //string busdate = HttpContext.Current.Session["CurArchivalSelectedBusdate"].ToString();
            //string batchdir = HttpContext.Current.Session["CurArchivalSelectedBatchDir"].ToString();
            //string batchnum = HttpContext.Current.Session["CurArchivalSelectedBatchNo"].ToString();
            //string transnum = HttpContext.Current.Session["CurArchivalSelectedTransNo"].ToString();
            string isCurrentItemOnly = HttpContext.Current.Session["CurArchivalSelectedItemOnly"].ToString();


            string url = string.Empty;
            if (isCurrentItemOnly == "true")
            {
                url = "/Modules/ImageArchiveOutward/PrintArchivalReportViewer.aspx?v=" + DateTime.Now.Ticks;
            }
            else
            {
                url = "/Modules/ImageArchiveOutward/PrintArchivalReportViewer.aspx?v=" + DateTime.Now.Ticks;
            }

            string fullUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + url;
            return fullUrl;
        }



        //CR015-20 assign required approval for multi level approve
        [WebMethod]
        public static string assignSessionDT(string BatchDir, string BatchNo, string TransNo, string Din, string ClientCode, string BusDate, string ClientSite, string TransSeqNum, string ItemType)
        {
            //Get 1 row of DataTable record and assign to session
            HttpContext.Current.Session["Outward_MainDTParam"] = BatchDir.Trim() + "|" + BatchNo.Trim() + "|" + TransNo.Trim() + "|" + Din.Trim() + "|" + ClientCode.Trim() + "|" + BusDate.Trim() + "|" + ClientSite.Trim() + "|" + TransSeqNum.Trim() + "|" + ItemType.Trim();
            return "1";
        }

        protected void ddlClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlPrintRejected.Items.Clear();
            if(ddlClient.SelectedValue.ToString() == "OCBC")
            {
                //Normal Rejected”, “MOPO rejected” and “FCC rejected”
                ddlPrintRejected.Items.Add(new ListItem("Normal Rejected", "Normal"));
                ddlPrintRejected.Items.Add(new ListItem("MOPO Rejected", "MOPO"));
                ddlPrintRejected.Items.Add(new ListItem("FCC Rejected", "FCC"));//Foreign
                ddlPrintRejected.SelectedValue = "Normal";
            }
            else
            {
                //Normal Rejected”, “MOPO rejected” 
                ddlPrintRejected.Items.Add(new ListItem("Normal Rejected", "Normal"));
                ddlPrintRejected.Items.Add(new ListItem("MOPO Rejected", "MOPO"));
                ddlPrintRejected.SelectedValue = "Normal";
            }
        }

        protected void ddlItemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void populateItemTypeDropDownList()
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

            //ddlItemType.Items.Clear();
            ddlItemType.Items.Add(new ListItem("All", "All"));
            ddlItemType.Items.Add(new ListItem("S", "S"));
            ddlItemType.Items.Add(new ListItem("C", "C"));

            if (ddlClient.SelectedValue.ToString() == "OCBC")
            {
                //Normal Rejected”, “MOPO rejected” and “FCC rejected”
                ddlPrintRejected.Items.Add(new ListItem("Normal Rejected", "Normal"));
                ddlPrintRejected.Items.Add(new ListItem("MOPO Rejected", "MOPO"));
                ddlPrintRejected.Items.Add(new ListItem("FCC Rejected", "FCC"));//Foreign
                ddlPrintRejected.SelectedValue = "Normal";
            }
            else
            {
                //Normal Rejected”, “MOPO rejected” 
                ddlPrintRejected.Items.Add(new ListItem("Normal Rejected", "Normal"));
                ddlPrintRejected.Items.Add(new ListItem("MOPO Rejected", "MOPO"));
                ddlPrintRejected.SelectedValue = "Normal";
            }
        }


        // Validate input for fixed amount search
        private bool IsValidAmountInput()
        {
            return !string.IsNullOrEmpty(txtAmount.Text.Trim());
        }

        // Helper method to check if a string is numeric
        private bool IsNumeric(string str)
        {
            double output;
            return double.TryParse(str, out output);
        }

        protected void rblAmountType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Handle the change in amount type selection

        }
        public string getOperate()
        {
            bool isOperateChange;
            string operate = string.Empty;
            string selectedOperator = string.Empty;
            string selectedValue = string.Empty;


            Session["SearchCri_Outward_Operate"] = rblAmountType.SelectedValue;
            selectedValue = Session["SearchCri_Outward_Operate"] == null ? rblAmountType.SelectedValue : Session["SearchCri_Outward_Operate"].ToString();

            switch (selectedValue)
            {
                case "fixed":
                    selectedOperator = "=";
                    break;
                case "larger":
                    selectedOperator = ">=";
                    break;
                case "smaller":
                    selectedOperator = "<=";
                    break;
                default:
                    break;
            }
            operate = selectedOperator;
            return operate;
        }

        public void loadPrevSearchCriteria()
        {
            if (Session["SearchCri_Outward_ClientCode"] != null)
            {
                ddlClient.SelectedValue = Session["SearchCri_Outward_ClientCode"].ToString();
            }

            if (Session["SearchCri_Outward_BusDate"] != null)
            {
                txtDateTime.Text = Session["SearchCri_Outward_BusDate"].ToString();
            }
            else
            {
                txtDateTime.Text = DateTime.Today.ToString("dd/MM/yyyy");
            }

            if (Session["SearchCri_Outward_DepAcc"] != null)
            {
                txtDepositorAcc.Text = Session["SearchCri_Outward_DepAcc"].ToString();
            } 
            
            if (Session["SearchCri_Outward_MicrAccNum"] != null)
            {
                txtMicrAccNum.Text = Session["SearchCri_Outward_MicrAccNum"].ToString();
            } 
            
            if (Session["SearchCri_Outward_MicrBSB"] != null)
            {
                txtMicrBSB.Text = Session["SearchCri_Outward_MicrBSB"].ToString();
            } 
            
            if (Session["SearchCri_Outward_ChequeNum"] != null)
            {
                txtChequeNum.Text = Session["SearchCri_Outward_ChequeNum"].ToString();
            } 
            
            if (Session["SearchCri_Outward_ItemType"] != null)
            {
                ddlItemType.SelectedValue = Session["SearchCri_Outward_ItemType"].ToString();
            } 
            
            if (Session["SearchCri_Outward_PresentingBSB"] != null)
            {
                txtPresentingBSB.Text = Session["SearchCri_Outward_PresentingBSB"].ToString();
            }  
            
            if (Session["SearchCri_Outward_Amount"] != null)
            {
                txtAmount.Text = Session["SearchCri_Outward_Amount"].ToString();
            }

            if (Session["SearchCri_Outward_Operate"] != null)
            {
                rblAmountType.SelectedValue = Session["SearchCri_Outward_Operate"].ToString();
            }
            if (Session["Outward_totalRecordCount"] != null && Session["Outward_totalRecordCount"].ToString() != "0")
            {
                footerContainer.Visible = true;
                bodyContainer.Style["margin-bottom"] = "0px !important";
            }
            else
            {
                footerContainer.Visible = false;
                bodyContainer.Style["margin-bottom"] = "0px !important";
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
                    string selectedRowParam = Session["Outward_MainDTParam"].ToString();
                    string selectedRowID = Request.Form["tableID"] == null ? string.Empty : Request.Form["tableID"].ToString().Trim();

                    if (!string.IsNullOrEmpty(selectedRowParam))
                    {
                        DBConnectionInfo dbConn = new DBConnectionInfo();
                        dbConn.webConnStr = webDBConnStr;
                        //string clientDB = dbConn.GetSiteConnectionString(true, ddlClient.SelectedValue.ToString().Trim(), "KL1");//selectedRowParam.Split('|')[3].ToString().Trim());
                        String[] filterList = selectedRowParam.Split('|');
                        Session["s_CurSelectedBatchDirImgArc"] = filterList[0].ToString();
                        Session["s_CurSelectedBatchNoRejDec"] = filterList[1].ToString();
                        Session["s_CurSelectedTransNoImgArc"] = filterList[2].ToString();
                        Session["s_CurSelectedBusdateRejDec"] = filterList[5].ToString();
                        Session["s_CurSelectedSiteImgArc"] = filterList[6].ToString();
                        Session["s_CurSelectedTransSeqNumImgArc"] = filterList[7].ToString();
                        Session["s_CurSelectedItemTypeImgArc"] = filterList[8].ToString();
                        Session["s_CurSelectedClientImgArc"] = Session["SearchCri_Outward_ClientCode"].ToString();



                        DataTable checkDT = CommonFunction.GetSelectedImgArchiveOutward(webDBConnStr.ToString(), false);
                        //DataTable checkDT = CommonFunction.GetSelectedImgArchiveOutward(webDBConnStr, selectedRowParam.Split('|'), ddlClient.SelectedValue.ToString().Trim());

                        //get TBL_ARCHIVE_SETTING
                        DataTable getImageArchivePath = CommonFunction.GetImageArchivePath(selectedRowParam.Split('|'), Session["SearchCri_Outward_ClientCode"].ToString().Trim());
                        //CR015-20:get required approval param to check whether this transaction required different level approval
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
                            DataRow selectedRow = checkDT.Rows[0];//sqlStmtGetSelectedImageArchiveOutward

                            //Image Archive Outward detail
                            string selBusdate = selectedRow["ITM_BusDate"].ToString().Trim();
                            string selBatchDir = selectedRow["ITM_BatchDirectory"].ToString().Trim();
                            string selBatchNum = selectedRow["ITM_BatchNum"].ToString().Trim();
                            string selTransNum = selectedRow["ITM_TransNum"].ToString().Trim();
                            string selBundleID = selectedRow["ITM_BundleID"].ToString().Trim();
                            string selProcMode = selectedRow["ITM_ProcMode"].ToString().Trim();//S: SINGLE, M/N: MULTIPLE, C: CHEQUES ONLY
                            string selSite = selectedRow["ITM_ProcessingSite"].ToString().Trim();

                            string selPresentingBSB = selectedRow["ITM_Fld9"].ToString().Trim();
                            string selWorksource = selectedRow["ITM_WsID"].ToString().Trim();
                            string selItemType = selectedRow["ITM_ItemType"].ToString().Trim();

                            string selRepresented = selectedRow["ITM_REPRESENTED"].ToString().Trim();
                            string selUIC = selectedRow["ITM_UIC"].ToString().Trim();
                            string selDin = selectedRow["ITM_DIN"].ToString().Trim();

                            string selTransportID = selectedRow["ITM_TransportID"].ToString().Trim();
                            string selTransSeqNum = selectedRow["ITM_TransSeqNum"].ToString().Trim();

                            //Assign row info to Session
                            Session["s_CurSelectedBusdateRejDec"] = selBusdate;
                            Session["s_CurSelectedBatchDirImgArc"] = selBatchDir;
                            Session["s_CurSelectedBatchNoRejDec"] = selBatchNum;
                            Session["s_CurSelectedTransNoImgArc"] = selTransNum;
                            Session["s_CurSelectedSiteImgArc"] = selSite;
                            Session["s_CurSelectedPresentingBSBImgArc"] = selPresentingBSB;
                            Session["s_CurSelectedClientImgArc"] = Session["SearchCri_Outward_ClientCode"].ToString();//ddlClient.SelectedValue.ToString();
                            Session["s_CurSelectedWsIDImgArc"] = selWorksource;
                            Session["s_CurSelectedNewBundleImgArc"] = selBundleID;
                            Session["s_CurSelectedRepresentedImgArc"] = selRepresented;
                            Session["s_CurSelectedUICImgArc"] = selUIC;
                            Session["s_CurSelectedProcModeImgArc"] = selProcMode;
                            Session["s_CurSelectedItemTypeImgArc"] = selItemType;
                            Session["s_CurSelectedDinImgArc"] = selDin;
                            Session["s_CurSelectedTransportIDImgArc"] = selTransportID;
                            Session["s_CurSelectedTransSeqNumImgArc"] = selTransSeqNum;


                            string url = string.Empty;
                            url = "ImageArchiveOutwardDetail/";

                            //Clear session param value
                            Session["Outward_MainDTParam"] = null;

                            Response.Redirect(url, false);
                            Context.ApplicationInstance.CompleteRequest();

                        }
                        else
                        {
                            Logger.Write(false, LogCallerID.ImageArchiveOutward, clientCode, "Validate Image Archive Outward", "Invalid record retrieved (Record ID does not existed).", LogEventType.Error, userID);
                        }

                    }
                    else
                    {
                        //Invalid ID retrieve for validate transaction
                        Logger.Write(false, LogCallerID.ImageArchiveOutward, clientCode, "Validate Image Archive Outward", "Invalid record retrieved to validate Image Archive Outward", LogEventType.Error, userID);
                    }
                }


                //Refresh again
                refreshDataTable();
            }
        }

        public bool validateTextField()
        {
            if (txtDateTime.Text == "")
            {
                if (IsPostBack)
                {
                    dateErrorMsg.Text = "Please select date.";
                    dateErrorMsg.Style["display"] = "block";
                }
                footerContainer.Visible = false;
                bodyContainer.Style["margin-bottom"] = "0px !important";
                return false;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtMicrAccNum.Text) && string.IsNullOrWhiteSpace(txtMicrBSB.Text)
                   && string.IsNullOrWhiteSpace(txtChequeNum.Text) && string.IsNullOrWhiteSpace(txtPresentingBSB.Text)
                   && string.IsNullOrWhiteSpace(txtDepositorAcc.Text) && string.IsNullOrWhiteSpace(txtAmount.Text))
                {
                    dateErrorMsg.Text = "Please enter one more search option to proceed";
                    dateErrorMsg.Style["display"] = "block";
                    dateErrorMsg.Style["font-size"] = "13px";

                    footerContainer.Visible = false;
                    bodyContainer.Style["margin-bottom"] = "0px !important";
                    return false;
                }
                else
                {
                    dateErrorMsg.Style["display"] = "none";
                    return true;
                }
            }
        }

        public class SelectedData
        {
            public string Date { get; set; }
            public string DirName { get; set; }
            public string BatchNo { get; set; }
            public string Trans { get; set; }
            public string TransSeqNum { get; set; }
            public string Din { get; set; }
            public string Client { get; set; }
            public string SiteName { get; set; }
        }
    }
}