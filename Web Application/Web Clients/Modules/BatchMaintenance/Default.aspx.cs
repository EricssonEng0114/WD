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
namespace UBPCWeb.Modules.BatchMaintenance
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
                //Session["s_IsCutOffTime"] = false;

                if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
                {
                    PageValidatorResult validatorResult;

                    //For password chnge, validate password by encryption class
                    validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "RejectedItemDecision");
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
                    Session["s_UserID"] = "";
                    Response.Redirect("/Login", false);
                    Context.ApplicationInstance.CompleteRequest();
                }

                //Load Control for first time
                if (isPageValid & !Page.IsPostBack)
                {
                    LoadControl();
                    clientCode = ddlClient.SelectedValue.ToString();

                    

                    DBConnectionInfo dbConn = new DBConnectionInfo();
                    dbConn.webConnStr = webDBConnStr;
                    Session["selClientDBConnStr"] = dbConn.GetSiteConnectionString(true, ddlClient.SelectedValue.ToString().Trim(), dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), ddlClient.SelectedValue.ToString().Trim()));
                    
                    //Check cut off time
                    lblCutOffTimeMsg.Visible = false;
                }

                if (isPageValid && IsPostBack)
                {
                    clientCode = ddlClient.SelectedValue.ToString();

                    string tagOperation = Request.Form["tag"] == null ? string.Empty : Request.Form["tag"];

                    lblCutOffTimeMsg.Visible = false;

                    //Validate - Mean to reset
                    if (tagOperation.Equals("VALIDATE"))
                    {
                        string selectedRowParam = Session["MainDTParam"].ToString();
                        string selectedRowID = Request.Form["tableID"] == null ? string.Empty : Request.Form["tableID"].ToString().Trim();

                        if (!string.IsNullOrEmpty(selectedRowID))
                        {
                            DBConnectionInfo dbConn = new DBConnectionInfo();
                            dbConn.webConnStr = webDBConnStr;
                            string clientDB = dbConn.GetSiteConnectionString(true, ddlClient.SelectedValue.ToString().Trim(), selectedRowParam.Split('|')[4].ToString().Trim());

                            //Reassign Selected Row Connection String
                            Session["selClientDBConnStr"] = clientDB;

                            DataTable checkDT = CommonFunction.GetSelectedRejTrans(clientDB, Convert.ToDateTime(txtDateTime.Text), selectedRowParam.Split('|'));
                                                               
                            if (checkDT.Rows.Count > 0)
                            {
                                DataRow selectedRow = checkDT.Rows[0];
                                string selBusdate = selectedRow["REJ_BUSDATE"].ToString().Trim();
                                string selBatchDir = selectedRow["ITM_BatchDirectory"].ToString().Trim();
                                string selBatchNo = selectedRow["REJ_BATCHNUM"].ToString().Trim();
                                string selTranNo = selectedRow["REJ_TRANSNUM"].ToString().Trim();
                                string selPresentingBSB = selectedRow["ITM_FLD10"].ToString().Trim();
                                string selSite = selectedRow["REJ_SITECODE"].ToString().Trim();
                                string selRejCategory = selectedRow["REJ_CATEGORY"].ToString().Trim();
                                string selProcMode = selectedRow["PROC_MODE"].ToString().Trim();//S: SINGLE, M/N: MULTIPLE, C: CHEQUES ONLY
                                bool selCanBreakdown = Convert.ToBoolean(selectedRow["REJ_ALLOWBREAKDOWN"]);
                                bool selCanTolerate = Convert.ToBoolean(selectedRow["REJ_ALLOWTOLERANCE"]);
                                string selNewBundleID = selectedRow["REJ_NEW_BUNDLEID"].ToString().Trim();                                

                                string selRejReason = selectedRow["REJ_REASON"].ToString().Trim();

                                //Reset batch n show reset message
                                //Get Current Action Date Time
                                bool isAllowedReset = true;
                                int resetIntervalInMS = Convert.ToInt32(Parameters.GetParamValue(webDBConnStr, Resources.Resource.cstrResetInterval));
                                DateTime latestActionDateTime = selectedRow["REJ_ACTIONEDTIME"] == null ? DateTime.MinValue : string.IsNullOrEmpty(selectedRow["REJ_ACTIONEDTIME"].ToString()) ? DateTime.MinValue: Convert.ToDateTime(selectedRow["REJ_ACTIONEDTIME"].ToString().Trim());
                                                                             
                                int intervalTime = resetIntervalInMS;

                                if (!latestActionDateTime.Equals(DateTime.MinValue))
                                {
                                    intervalTime = (int)DateTime.Now.Subtract(latestActionDateTime).TotalMilliseconds;
                                }

                                //Retrieve latest status 
                                string selStatus = CommonFunction.RetrieveDBLatestStatus(clientDB, clientCode, Convert.ToDateTime(selBusdate), selBatchNo, Convert.ToInt32(selTranNo), selNewBundleID);

                                //If status = In Use/In progress
                                if (selStatus.Equals("I"))
                                {
                                    //Check if reset is allowed after transaction idle for 10minutes
                                    isAllowedReset = (intervalTime >= resetIntervalInMS);
                                }

                                if (isAllowedReset)
                                {
                                    //Only allowed to reset if status "I"
                                    if (selStatus.Equals("I"))
                                    {
                                        int recAffected = CommonFunction.ResetTransactionStatus(clientDB, ddlClient.SelectedValue.ToString(), "W", selectedRow["REJ_ACTIONEDBY"].ToString().Trim(), Convert.ToDateTime(selBusdate), selBatchNo, Convert.ToInt16(selTranNo), selNewBundleID);

                                        if (recAffected > 0)
                                        {
                                            //Success 
                                            //Log the Trans Status Update Details
                                            string msgFormta = "BusDate={0}|BatchNo={1}|BatchDir={2}|Transaction={3}";

                                            LogEntry log = new LogEntry();
                                            log.Caller = LogCallerID.BatchMaintenance;
                                            log.ClientCode = clientCode;
                                            log.UserName = userID;
                                            log.Severity = LogEventType.Information;
                                            log.Message = string.Format("Reset Transaction for Trans No {0}.", selTranNo);
                                            log.Data = string.Format(msgFormta, selBusdate, selBatchNo, selBatchDir, selTranNo);
                                            log.Write();

                                            ClientScript.RegisterStartupScript(this.GetType(), "PromptMsg",
                                                string.Format("displaymsg('{0}','{1}');", "Batch Maintenance", "Reset Successfully."), true);

                                            //Refresh 
                                            // refreshDataTable();

                                        }
                                    }
                                    else if (selStatus.Equals("C"))
                                    {
                                        //Completed  showMsg("@TempData["title"]","@TempData["message"]");
                                        ClientScript.RegisterStartupScript(this.GetType(), "PromptMsg",
                                            string.Format("displaymsg('{0}','{1}');", "Batch Maintenance", "Unable to reset a transaction that is already clear to GWC."), true);

                                        //Refresh see
                                        //  refreshDataTable();
                                    }
                                    else 
                                    {
                                        //if still "W"
                                        //Completed  showMsg("@TempData["title"]","@TempData["message"]");
                                        ClientScript.RegisterStartupScript(this.GetType(), "PromptMsg",
                                            string.Format("displaymsg('{0}','{1}');", "Batch Maintenance", "Unable to reset a transaction that is already being reset."), true);

                                        //Refresh see
                                        // refreshDataTable();
                                    }
                                }
                                else 
                                {
                                    //Not allowed to reset
                                    int minutes = Convert.ToInt32(TimeSpan.FromMilliseconds(resetIntervalInMS - intervalTime).TotalMinutes);

                                    string message = string.Empty;

                                    if (minutes > 0)
                                    {
                                        message = "Transaction is actively in-used. Please try again " + minutes.ToString() + " minute(s) later.";
                                    }
                                    else
                                    {
                                        message = "Transaction is actively in-used. Please try again 1 minute later.";
                                    }

                                    ClientScript.RegisterStartupScript(this.GetType(), "PromptMsg",
                                        string.Format("displaymsg('{0}','{1}');", "Batch Maintenance", message), true);                    
                                }
                            }
                            else
                            {
                                Logger.Write(false, LogCallerID.BatchMaintenance, clientCode, "Reset Transaction", "Invalid record retrieved (Record ID does not existed).", LogEventType.Error, userID);
                            }
                               
                        }
                        else
                        {
                            //Invalid ID retrieve for validate transaction
                            Logger.Write(false, LogCallerID.BatchMaintenance, clientCode, "Reset Transaction", "Invalid record retrieved to reset", LogEventType.Error, userID);
                        }
                    }
                    

                    refreshDataTable();
                }

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.BatchMaintenance;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                refreshDataTable();
            }
        }     

        private void LoadControl()
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

            //Load Reject Category Drop down
            List<ListItem> categoryItems = new List<ListItem>();
            categoryItems.Add(new ListItem("ALL", "ALL"));
            categoryItems.Add(new ListItem("PV Reject", "PV"));
            categoryItems.Add(new ListItem("DI Reject", "DI"));
            categoryItems.Add(new ListItem("RL Reject", "RL"));
            this.ddlRejCategory.DataSource = from i in categoryItems select new ListItem() { Text = i.Text, Value = i.Value };
            this.ddlRejCategory.DataTextField = "Text";
            this.ddlRejCategory.DataValueField = "Value";
            this.ddlRejCategory.DataBind();

            DBConnectionInfo db = new DBConnectionInfo();
            db.webConnStr = webDBConnStr;
            string mainUVRPSConnection = db.GetSiteConnectionString(true, ddlClient.SelectedValue.ToString(), db.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), ddlClient.SelectedValue.ToString().Trim()));

            DateTime curActiveBusdate = DateTime.Today;
            if (BusinessDate.GetActive(mainUVRPSConnection) != null)
            {
                curActiveBusdate = Convert.ToDateTime(BusinessDate.GetActive(mainUVRPSConnection));
            }

            Session["CurBusdate"] = curActiveBusdate;

            txtDateTime.Text = curActiveBusdate.ToShortDateString();// "12/26/2018";

            //Initialise selected busdate
            if (Session["CurActionSelectedBusdate"] == null)
            {
                Session["CurActionSelectedBusdate"] = curActiveBusdate.ToShortDateString();
            }
            else
            {
                Session["CurActionSelectedBusdate"] = "";
            }

            //Initialise Grid
            //busdate, client, branch, category
            ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                string.Format("initBatchMaintDataTable('{0}','{1}','{2}','{3}','{4}');", txtDateTime.Text, ddlClient.SelectedValue.ToString(), txtPresentingBSB.Text.Trim(), "ALL", txtBatchNo.Text), true);

            ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);

        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string busdate = string.Empty;
            string client = string.Empty;
            string presentingBch = string.Empty;
            string category = string.Empty;
            string batchNo = string.Empty;

            Session["CurActionSelectedBusdate"] = txtDateTime.Text.Trim();


            busdate = txtDateTime.Text.Trim();
            presentingBch = txtPresentingBSB.Text.ToString().Trim();
            category = ddlRejCategory.SelectedValue.ToString().Trim();
            client = ddlClient.SelectedValue.ToString().Trim();
            batchNo = txtBatchNo.Text.ToString().Trim();

            if (string.IsNullOrEmpty(busdate.Trim()))
            {
                Logger.Write(false, LogCallerID.BatchMaintenance, "", "Validate Search", "BusDate is Empty", LogEventType.Error, userID);
            }
            else
            {
                //busdate, client, branch, category
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initBatchMaintDataTable('{0}','{1}','{2}','{3}','{4}');", busdate, client, presentingBch, category, batchNo), true);

                ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);

            }

        }

        private void refreshDataTable() 
        {
            //Initialise Grid
            //busdate, client, branch, category
            ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                string.Format("initBatchMaintDataTable('{0}','{1}','{2}','{3}','{4}');", txtDateTime.Text, ddlClient.SelectedValue.ToString(), txtPresentingBSB.Text.ToString().Trim(),ddlRejCategory.SelectedValue.ToString(), txtBatchNo.Text.Trim()), true);

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

                txtDateTime.Text = curActiveBusdate.ToShortDateString();

                Session["CurActionSelectedBusdate"] = curActiveBusdate.ToShortDateString();

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.BatchMaintenance;
                log.ClientCode = ddlClient.SelectedValue == null ? "" : ddlClient.SelectedValue.ToString();
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

        }

        [WebMethod]
        public static string assignSessionDT(string BatchDir, string BatchNo, string TransNo, string BundleID, string SiteName)
        {
            //Get 1 row of DataTable record and assign to session
            HttpContext.Current.Session["MainDTParam"] = BatchDir.Trim() + "|" + BatchNo.Trim() + "|" + TransNo.Trim() + "|" + BundleID.Trim() + "|" + SiteName;
            return "1";
        }

        protected void ddlClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateActiveBusdate();
        }        

    }
}