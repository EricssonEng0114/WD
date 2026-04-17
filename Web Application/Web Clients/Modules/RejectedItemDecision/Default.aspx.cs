using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using UBPC.Web.Common;

namespace UBPCWeb.Modules.RejectedItemDecision
{
    public partial class Default : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        protected string clientCode = string.Empty;
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
                    Response.Redirect("/Login", false);
                    Context.ApplicationInstance.CompleteRequest();
                }

                //Load Control for first time
                if (isPageValid & !Page.IsPostBack)
                {
                    LoadControl();
                    clientCode = ddlClient.SelectedValue.ToString();

                    //Check cut off time
                    lblCutOffTimeMsg.Visible = checkCutOffTime();

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

                if (isPageValid && IsPostBack)
                {
                    clientCode = ddlClient.SelectedValue.ToString();

                    string tagOperation = Request.Form["tag"] == null ? string.Empty : Request.Form["tag"];

                    //Check Cut off
                    if (!checkCutOffTime())
                    {
                        lblCutOffTimeMsg.Visible = false;

                        if (tagOperation.Equals("VALIDATE"))
                        {
                            string selectedRowParam = Session["MainDTParam"].ToString();
                            string selectedRowID = Request.Form["tableID"] == null ? string.Empty : Request.Form["tableID"].ToString().Trim();

                            if (!string.IsNullOrEmpty(selectedRowParam))
                            {
                                DBConnectionInfo dbConn = new DBConnectionInfo();
                                dbConn.webConnStr = webDBConnStr;
                                string clientDB = dbConn.GetSiteConnectionString(true, ddlClient.SelectedValue.ToString().Trim(), selectedRowParam.Split('|')[4].ToString().Trim());

                                DataTable checkDT = CommonFunction.GetSelectedRejTrans(clientDB, Convert.ToDateTime(txtDateTime.Text), selectedRowParam.Split('|'));
                                //CR015-20:get required approval param to check whether this transaction required different level approval
                                //Param List
                                //1 - Batch Dir, 2- Batch No, 3 - Trans NO, 4 - New Bundle ID , 5- Site name,6-Required Approval           
                                //
                                List<string> paramList = selectedRowParam.Split('|').ToList();
                                string approval = paramList.Last();

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

                                    string selRejReason = selectedRow["REJ_REASON"].ToString().Trim();

                                    //Assign row info to Session
                                    Session["s_CurSelectedBusdateRejDec"] = Convert.ToDateTime(selBusdate);
                                    Session["s_CurSelectedBatchDirRejDec"] = selBatchDir;
                                    Session["s_CurSelectedBatchNoRejDec"] = selBatchNo;
                                    Session["s_CurSelectedTransNoRejDec"] = selTranNo;
                                    Session["s_CurSelectedSiteRejDec"] = selSite;
                                    Session["s_CurSelectedPresentingRejDec"] = selPresentingBSB;
                                    Session["s_CurSelectedClientRejDec"] = ddlClient.SelectedValue.ToString();
                                    Session["s_CurSelectedAllowBreakdown"] = selCanBreakdown;
                                    Session["s_CurSelectedAllowTolerance"] = selCanTolerate;
                                    Session["s_CurSelectedRejectReason"] = selRejReason;
                                    Session["s_CurSelectedWsIDRejDec"] = selectedRow["ITM_WsID"].ToString().Trim();
                                    Session["s_CurSelectedNewBundleRejDec"] = selectedRow["REJ_NEW_BUNDLEID"].ToString().Trim();
                                    //CR015-20
                                    //check required approval column value and assign review level for current transaction
                                    string ReviewerNo = "0";
                                    if (approval.Equals("True"))
                                    {
                                        if (!(bool)(selectedRow["REJ_ROUTETOFINAL"]))
                                        {
                                            if (selectedRow["REJ_1STDECISION"].ToString() == "W")
                                            {
                                                ReviewerNo = "1";
                                            }
                                            else
                                            {
                                                ReviewerNo = "2";
                                            }
                                        }
                                        else
                                            ReviewerNo = "3";
                                    }

                                    Session["s_RequiredApproval"] = approval;
                                    Session["s_1stDecision"] = selectedRow["REJ_1STDECISION"].ToString().Trim();
                                    Session["s_2ndDecision"] = selectedRow["REJ_2NDDECISION"].ToString().Trim();
                                    Session["s_ReviewNo"] = ReviewerNo;
                                    Session["s_RouteToFinal"] = (bool)(selectedRow["REJ_ROUTETOFINAL"]);
                                    Session["s_RejectedRemark"] = selectedRow["REJ_REMARK"].ToString().Trim();

                                    string url = string.Empty;
                                    //Perform Update
                                    if (selRejCategory.Equals("PV"))
                                    {
                                        url = "WebDecisionP/" + selectedRowID.Trim().ToString();
                                    }

                                    if (selRejCategory.Equals("DI"))
                                    {
                                        url = "WebDecisionD/" + selectedRowID.Trim().ToString();
                                    }

                                    if (selRejCategory.Equals("RL"))
                                    {
                                        //if multiple mode , show multiple mode page; for OCBC only
                                        //if single/chq mode, show chq mode page; ofr unisys only (insert stub enabled), for ocbc (hide insert stub button)
                                        if (selProcMode.Equals("M") || selProcMode.Equals("N"))
                                        {
                                            url = "WebDecisionRM/" + selectedRowID.Trim().ToString();
                                        }
                                        else
                                        {
                                            url = "WebDecisionR/" + selectedRowID.Trim().ToString();
                                        }
                                    }

                                    //Clear session param value
                                    Session["MainDTParam"] = null;

                                    Response.Redirect(url, false);
                                    Context.ApplicationInstance.CompleteRequest();

                                }
                                else
                                {
                                    Logger.Write(false, LogCallerID.RejectedItemDecision, clientCode, "Validate Rejected Item Decision", "Invalid record retrieved (Record ID does not existed).", LogEventType.Error, userID);
                                }

                            }
                            else
                            {
                                //Invalid ID retrieve for validate transaction
                                Logger.Write(false, LogCallerID.RejectedItemDecision, clientCode, "Validate Rejected Item Decision", "Invalid record retrieved to validate rejected item decision", LogEventType.Error, userID);
                            }
                        }
                    }
                    else
                    {
                        lblCutOffTimeMsg.Visible = true;
                    }

                    //Refresh again
                    refreshDataTable();
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

        private bool checkCutOffTime()
        {
            try
            {
                //need to check if web decision cut off
                //If web decision cut off dy
                //Build connection string for site and set curDataTable
                //DBConnectionInfo dbConn = new DBConnectionInfo();
                //dbConn.webConnStr = webDBConnStr;
                //Session["selClientDBConnStr"] = dbConn.GetSiteConnectionString(true, ddlClient.SelectedValue.ToString().Trim(), dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), ddlClient.SelectedValue.ToString().Trim()));
                string webDecCutOff = GetCutOffTime();//Parameters.GetParamValue(Session["selClientDBConnStr"].ToString(), Resources.Resource.cstrCutOffTime);

                TimeSpan cutOffTime;
                if (!TimeSpan.TryParse(webDecCutOff, out cutOffTime))
                {
                    Logger.Write(false, LogCallerID.RejectedItemDecision, clientCode, "Validate Rejected Item Decision", "Invalid Cut off time detected.", LogEventType.Error, userID);
                }
                else
                {
                    DateTime dateTimeNow = Convert.ToDateTime(DateTime.Now);
                    DateTime dateTimeCutOff = Convert.ToDateTime(webDecCutOff);
                    if (dateTimeNow.TimeOfDay.Ticks > dateTimeCutOff.TimeOfDay.Ticks)
                    {
                        //After Cut Off Time
                        Session["s_IsCutOffTime"] = true;
                        Logger.Write(false, LogCallerID.RejectedItemDecision, clientCode, "Validate Rejected Item Decision", "Web Decision only can be done after Cut Off time.", LogEventType.Error, userID);
                    }
                    else
                    {
                        Session["s_IsCutOffTime"] = false;
                        string url = string.Empty;
                    }
                }

                return Convert.ToBoolean(Session["s_IsCutOffTime"]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetCutOffTime()
        {
            using (SqlConnection connection = new SqlConnection(webDBConnStr))
            {
                // Setup the SQL Command
                SqlCommand command = new SqlCommand(Resources.Resource.GetWebDecCutOFfTime, connection);
                command.CommandType = CommandType.StoredProcedure;
                // Add the search parameter key
                command.Parameters.Add("@ClientCode", SqlDbType.VarChar).Value = ddlClient.SelectedValue.ToString().Trim();

                // Open the connection
                connection.Open();

                // Execute the Query 
                Object obj = null;
                obj = command.ExecuteScalar();

                if (obj != null && obj != DBNull.Value)
                    return obj.ToString();
            }

            return string.Empty;
        }

        private void LoadControl()
        {
            try
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


                //Initialise Grid
                //busdate, client, branch, category
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initRejDecisionDataTable('{0}','{1}','{2}','{3}','{4}');", txtDateTime.Text, ddlClient.SelectedValue.ToString(), txtPresentingBSB.Text.Trim(), "ALL", txtBatchNo.Text.Trim()), true);

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
            ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                string.Format("initRejDecisionDataTable('{0}','{1}','{2}','{3}','{4}');", txtDateTime.Text, ddlClient.SelectedValue.ToString(), txtPresentingBSB.Text.ToString().Trim(), ddlRejCategory.SelectedValue.ToString(), txtBatchNo.Text.Trim()), true);

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
                log.Caller = LogCallerID.RejectedItemDecision;
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
            string client = string.Empty;
            string presentingBch = string.Empty;
            string category = string.Empty;

            //Edited Shinyi -display active busdate upon client selected
            Session["CurActionSelectedBusdate"] = txtDateTime.Text;

            busdate = txtDateTime.Text.Trim();
            presentingBch = txtPresentingBSB.Text.ToString().Trim();
            category = ddlRejCategory.SelectedValue.ToString().Trim();
            client = ddlClient.SelectedValue.ToString().Trim();


            if (string.IsNullOrEmpty(busdate.Trim()))
            {
                Logger.Write(false, LogCallerID.RejectedItemDecision, "", "Validate Search", "BusDate is Empty", LogEventType.Error, userID);
            }
            else
            {
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initRejDecisionDataTable('{0}','{1}','{2}','{3}','{4}');", busdate, client, presentingBch, category, txtBatchNo.Text.Trim()), true);

                ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);
            }

        }

        //CR015-20 assign required approval for multi level approve
        [WebMethod]
        public static string assignSessionDT(string BatchDir, string BatchNo, string TransNo, string BundleID, string SiteName, string RequiredApproval)
        {
            //Get 1 row of DataTable record and assign to session
            HttpContext.Current.Session["MainDTParam"] = BatchDir.Trim() + "|" + BatchNo.Trim() + "|" + TransNo.Trim() + "|" + BundleID.Trim() + "|" + SiteName + "|" + RequiredApproval;
            return "1";
        }

        protected void ddlClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateActiveBusdate();
        }

    }

}