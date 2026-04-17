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
                
                if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
                {
                    PageValidatorResult validatorResult;

                    //For password chnge, validate password by encryption class
                    validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "ActionedItemHistory");
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
                }

                if (isPageValid && IsPostBack)
                {
                    clientCode = ddlClient.SelectedValue.ToString();

                    string tagOperation = Request.Form["tag"] == null ? string.Empty : Request.Form["tag"];
                                  

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
                                string selNewBundleID = selectedRowParam.Split('|')[3].ToString().Trim();
                                string selActionBy = selectedRow["REJ_ACTIONEDBY"].ToString().Trim();
                                //DateTime selActionDateTime = Convert.ToDateTime(selectedRow["REJ_ACTIONEDTIME"].ToString().Trim());
                                string selActionDateTime = selectedRow["REJ_ACTIONEDTIME"].ToString().Trim();
                                string selRemark = selectedRow["REJ_REMARK"].ToString().Trim();
                                string selRejDecision = selectedRow["REJ_DECISION"].ToString().Trim();

                                //Assign row info to Session
                                Session["s_CurTransTempID"] = selectedRowID;
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
                                Session["s_CurSelectedNewBundleID"] = selNewBundleID;
                                //Action Details
                                Session["s_CurSelectedRejDecision"] = selRejDecision;
                                Session["s_CurSelectedActionby"] = selActionBy;
                                Session["s_CurSelectedActionDateTime"] = selActionDateTime;
                                Session["s_CurSelectedRemark"] = selRemark;


                                //CR015-20 assign multi level decision to session
                                Session["s_RequiredApproval"] = approval;
                                if (approval.Equals("True")) {
                                    Session["s_1stDecision"] = selectedRow["REJ_1STDECISION"].ToString().Trim();
                                    Session["s_1stReviewer"] = selectedRow["REJ_1STREVIEWER"].ToString().Trim();
                                    string sel1stActionDateTime = selectedRow["REJ_1STACTIONEDDATETIME"].ToString().Trim();
                                    Session["s_1stActionedDateTime"] = sel1stActionDateTime;
                                    Session["s_2ndDecision"] = selectedRow["REJ_2NDDECISION"].ToString().Trim();
                                    Session["s_2ndReviewer"] = selectedRow["REJ_2NDREVIEWER"].ToString().Trim();
                                    string sel2ndActionDateTime = selectedRow["REJ_2NDACTIONEDDATETIME"].ToString().Trim();
                                    Session["s_2ndActionedDateTime"] = sel2ndActionDateTime;
                                }

                                string url = string.Empty;
                                //Perform Update
                                if (selRejCategory.Equals("PV"))
                                {
                                    url = "HistoryP/" + selectedRowID.Trim().ToString();
                                }

                                if (selRejCategory.Equals("DI"))
                                {
                                    url = "HistoryD/" + selectedRowID.Trim().ToString();
                                }

                                if (selRejCategory.Equals("RL"))
                                {
                                    //if multiple mode , show multiple mode page; for OCBC only
                                    //if single/chq mode, show chq mode page; ofr unisys only (insert stub enabled), for ocbc (hide insert stub button)
                                    if (selProcMode.Equals("M") || selProcMode.Equals("N"))
                                    {
                                        url = "HistoryRM/" + selectedRowID.Trim().ToString();
                                    }
                                    else
                                    {
                                        url = "HistoryR/" + selectedRowID.Trim().ToString();
                                    }
                                }

                                //Clear session param value
                                Session["MainDTParam"] = null;

                                Response.Redirect(url, false);
                                Context.ApplicationInstance.CompleteRequest();

                            }
                            else
                            {
                                Logger.Write(false, LogCallerID.ActionedItemHistory, clientCode, "Validate Actioned Item History", "Invalid record retrieved (Record ID does not existed).", LogEventType.Error, userID);
                            }

                        }
                        else
                        {
                            //Invalid ID retrieve for validate transaction
                            Logger.Write(false, LogCallerID.ActionedItemHistory, clientCode, "Validate Actioned Item History", "Invalid record retrieved to validate Actioned Item History", LogEventType.Error, userID);
                        }
                    }
                   

                    //Refresh again
                    refreshDataTable();
                }

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ActionedItemHistory;
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
                    string.Format("initActionedHistoryDataTable('{0}','{1}','{2}','{3}','{4}');", txtDateTime.Text, ddlClient.SelectedValue.ToString(), txtPresentingBSB.Text.Trim(), "ALL", txtBatchNo.Text.Trim()), true);

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
                string.Format("initActionedHistoryDataTable('{0}','{1}','{2}','{3}','{4}');", txtDateTime.Text, ddlClient.SelectedValue.ToString(), txtPresentingBSB.Text.ToString().Trim(), ddlRejCategory.SelectedValue.ToString(),txtBatchNo.Text.Trim()), true);

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
                log.Caller = LogCallerID.ActionedItemHistory;
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
                Logger.Write(false, LogCallerID.ActionedItemHistory, "", "Validate Search", "BusDate is Empty", LogEventType.Error, userID);
            }
            else
            {                
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initActionedHistoryDataTable('{0}','{1}','{2}','{3}','{4}');", busdate, client, presentingBch, category, txtBatchNo.Text.Trim()), true);

                ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable();", true);
            }
        }

        //CR015-20 assign required approval for multi level approve
        [WebMethod]
        public static string assignSessionDT(string BatchDir, string BatchNo, string TransNo, string BundleID, string SiteName, string RequiredApproval)
        {
            //Get 1 row of DataTable record and assign to session
            HttpContext.Current.Session["MainDTParam"] = BatchDir.Trim() + "|" + BatchNo.Trim() + "|" + TransNo.Trim() + "|" + BundleID.Trim() + "|" + SiteName+"|"+RequiredApproval;
            return "1";
        }

        
        protected void ddlClient_SelectedIndexChanged1(object sender, EventArgs e)
        {
           UpdateActiveBusdate();
        }        
        
    }
}