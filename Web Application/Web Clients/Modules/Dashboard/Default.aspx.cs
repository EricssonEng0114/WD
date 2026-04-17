using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.VisualBasic;
using UBPC.Web.Common;
using UBPCWeb.Modules.Announcement;
using System.Web.Script.Serialization;
using System.Web.Services;
//using System.Web.Security.AntiXss;

namespace UBPCWeb.Modules.Dashboard
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private string strReturnMsg = string.Empty;

        public List<AnnouncementModel> annList = new List<AnnouncementModel>();
        public DataTable activeUsrDT = new DataTable();
        public DataTable rejStatusDT = new DataTable();
        //Edited by AK on 2020-09-08:new datatable to store the client whether required multiple level approval
        public DataTable appApprovalDt = new DataTable();
        public bool isUnisys = false;
        public string selectedClientDropDown = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString();
            userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString();
            isUnisys = Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString());

            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(userGroup))
            {
                //invalid user - back to login page
                Session["s_UserID"] = "";
                Response.Redirect("/Login", false);
                Context.ApplicationInstance.CompleteRequest();

            }
            else 
            {
                LoadControl(); 

                LoadAnnouncement();
                LoadActiveUserList();
                //Edited by AK on 2020-09-08:get multi level approval indicator
                UpdateRequiredApproval();
                LoadRejectedItemStatus();
                if (!Page.IsPostBack)
                {
                    var client = ddlClient.SelectedValue == null ? string.Empty : ddlClient.SelectedValue.ToString().Trim();
                    MonitorDiv.Style["display"] = !isUnisys && client == "RHB" ? "block" : "none";
                    LoadBranchData();
                }
            }
        }

        private void LoadBranchData()
        {
            ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                string.Format("initMonitorDataTable('{0}','{1}');", ddlClient.SelectedValue.ToString(), txtDateTime.Text), true);

            ClientScript.RegisterStartupScript(this.GetType(), "Configure", "configureTable()", true);
        }

        private void LoadAnnouncement() 
        {
            if (annList.Count > 0)
                annList.Clear();

            DataTable dtDB = UBPCWeb.Modules.Announcement.CommonFunction.GetTodayAnouncement();

            foreach (DataRow dr in dtDB.Rows) 
            {
                AnnouncementModel annObj = new AnnouncementModel();
                annObj.AnnID = dr["ANM_ID"].ToString();
                annObj.AnnType = dr["ANM_Type"].ToString();
                annObj.Title = HttpUtility.HtmlDecode(dr["ANM_Title"].ToString());
                annObj.Desc = HttpUtility.HtmlDecode(dr["ANM_Description"].ToString());
                annObj.DateFrom = Convert.ToDateTime(dr["ANM_CreatedFrom"].ToString());
                annObj.DateTo = Convert.ToDateTime(dr["ANM_CreatedTo"].ToString());
                annObj.CreatedBy = dr["ANM_CreatedBy"].ToString();
                annObj.Client = dr["ANM_Client"].ToString();

                annList.Add(annObj);
            }
        }
        
        private void LoadActiveUserList() 
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                string clientList = HttpContext.Current.Session["s_UserClients"] == null ? string.Empty : HttpContext.Current.Session["s_UserClients"].ToString();
                bool isForUnisys = HttpContext.Current.Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString());

                stmt = isForUnisys ? Resource.stmtSqlGetActiveUserLoginForUnisys : Resource.stmtSqlGetActiveUserLoginForClient;
                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 10, "@ClientCode", ParameterDirection.Input, clientList)
                };

                activeUsrDT = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                iConn.Close();
            }
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

                txtDateTime.Text = curActiveBusdate.ToShortDateString(); ;
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Dashboard;
                log.ClientCode = ddlClient.SelectedValue == null ? "" : ddlClient.SelectedValue.ToString();
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

        }


        //Edited by AK on 2020-09-08:check whether client need multiple level approval
        private void UpdateRequiredApproval()
        {
            string stmt = string.Empty;
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            try
            {
                stmt = Resource.sqlStmtGetAppApprove;

                IDbDataParameter[] param = new[]{
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ClientBank", ParameterDirection.Input, ddlClient.SelectedValue.ToString().Trim()),
                };

                appApprovalDt = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);

                Session["s_ClientRequiredApproval"] = appApprovalDt.Rows.Count > 0 ? true : false;
                Session["s_App_MinAmt"] = appApprovalDt.Rows.Count > 0 ? appApprovalDt.Rows[0]["APP_MINAMT"]: string.Empty;
                Session["s_App_MaxAmt"] = appApprovalDt.Rows.Count > 0 ? appApprovalDt.Rows[0]["APP_MAXAMT"] : string.Empty;
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Dashboard;
                log.ClientCode = ddlClient.SelectedValue == null ? "" : ddlClient.SelectedValue.ToString();
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }
        }

        private void LoadRejectedItemStatus() 
        {
            Boolean requiredApproval = Convert.ToBoolean(Session["s_ClientRequiredApproval"].ToString());
            //ALL
            txtAllWaiting.Text = "0";
            //CR015-20 pending approval count
            txtAllPending.Text = "0";
            txtAllInUse.Text = "0";
            txtAllCompleted.Text = "0";
            txtAllTotal.Text = "0";

            if (isUnisys)
            {
                tdlblAllPending.Visible = false;
                tdtxtAllPending.Visible = false;
                tdtxtAllWaiting.Attributes.Add("colspan", "3");
            }
            else {
                tdlblAllPending.Visible = requiredApproval ? true : false;
                tdtxtAllPending.Visible = requiredApproval ? true : false;

                if(requiredApproval)
                    tdtxtAllWaiting.Attributes.Remove("colspan");
                else
                    tdtxtAllWaiting.Attributes.Add("colspan", "3");
            }

            //PV
            txtPVWaiting.Text = "0";
            //CR015-20 pending approval count
            txtPVPending.Text = "0";
            txtPVInUse.Text = "0";
            txtPVCompleted.Text = "0";
            txtPVTotal.Text = "0";

            if (isUnisys)
            {
                tdlblPVPending.Visible = false;
                tdtxtPVPending.Visible = false;
                tdtxtPVWaiting.Attributes.Add("colspan", "3");
            }
            else
            {
                tdlblPVPending.Visible = requiredApproval ? true : false;
                tdtxtPVPending.Visible = requiredApproval ? true : false;

                if (requiredApproval)
                    tdtxtPVWaiting.Attributes.Remove("colspan");
                else
                    tdtxtPVWaiting.Attributes.Add("colspan", "3");
            }

            //DI
            txtDIWaiting.Text = "0";
            //CR015-20 pending approval count
            txtDIPending.Text = "0";
            txtDIInUse.Text = "0";
            txtDICompleted.Text = "0";
            txtDITotal.Text = "0";

            if (isUnisys)
            {
                tdlblDIPending.Visible = false;
                tdtxtDIPending.Visible = false;
                tdtxtDIWaiting.Attributes.Add("colspan", "3");
            }
            else
            {
                tdlblDIPending.Visible = requiredApproval ? true : false;
                tdtxtDIPending.Visible = requiredApproval ? true : false;

                if (requiredApproval)
                    tdtxtDIWaiting.Attributes.Remove("colspan");
                else
                    tdtxtDIWaiting.Attributes.Add("colspan", "3");
            }

            //RL
            txtRLWaiting.Text = "0";
            //CR015-20 pending approval count
            txtRLPending.Text = "0";
            txtRLInUse.Text = "0";
            txtRLCompleted.Text = "0";
            txtRLTotal.Text = "0";

            if (isUnisys)
            {
                tdlblRLPending.Visible = false;
                tdtxtRLPending.Visible = false;
                tdtxtRLWaiting.Attributes.Add("colspan", "3");
            }
            else
            {
                tdlblRLPending.Visible = requiredApproval ? true : false;
                tdtxtRLPending.Visible = requiredApproval ? true : false;

                if (requiredApproval)
                    tdtxtRLWaiting.Attributes.Remove("colspan");
                else
                    tdtxtRLWaiting.Attributes.Add("colspan", "3");
            }

            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                //Edited by AK on 2020-09-08:add check whether this client bank need multiple level approval
                
                bool isForUnisys = HttpContext.Current.Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString());
                
                DateTime busdate = Convert.ToDateTime(txtDateTime.Text.Trim());
                string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');

                stmt = Resource.stmtSqlGetAllRejTransaction;
                IDbDataParameter[] param = new[]{
                        dbHelperObj.CreateParameter(DbType.String, 8, "@BusDate", ParameterDirection.Input, sqlDateTime),
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ClientBank", ParameterDirection.Input, ddlClient.SelectedValue.ToString().Trim()),
                        dbHelperObj.CreateParameter(DbType.Boolean, 0, "@ForUnisysOps", ParameterDirection.Input, Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString()))

                };

                rejStatusDT = dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);

                if (rejStatusDT.Rows.Count > 0)
                {
                    //ALL
                    DataRow allDR = rejStatusDT.Select("TEMP_CATEGORY='ALL'").FirstOrDefault();
                    txtAllWaiting.Text = allDR["Waiting"].ToString();
                    //CR015-20 pending approval count
                    txtAllPending.Text = allDR["Pending"].ToString();
                    txtAllInUse.Text = allDR["InUse"].ToString();
                    txtAllCompleted.Text = allDR["Completed"].ToString();
                    txtAllTotal.Text = allDR["TotalCount"].ToString();

                    //PV
                    DataRow pvDR = rejStatusDT.Select("TEMP_CATEGORY='PV'").FirstOrDefault();
                    if (pvDR != null)
                    {
                        txtPVWaiting.Text = pvDR["Waiting"].ToString();
                        //CR015-20 pending approval count
                        txtPVPending.Text = pvDR["Pending"].ToString();
                        txtPVInUse.Text = pvDR["InUse"].ToString();
                        txtPVCompleted.Text = pvDR["Completed"].ToString();
                        txtPVTotal.Text = pvDR["TotalCount"].ToString();
                    }

                    //DI
                    DataRow diDR = rejStatusDT.Select("TEMP_CATEGORY='DI'").FirstOrDefault();
                    if (diDR != null)
                    {
                        txtDIWaiting.Text = diDR["Waiting"].ToString();
                        //CR015-20 pending approval count
                        txtDIPending.Text = diDR["Pending"].ToString();
                        txtDIInUse.Text = diDR["InUse"].ToString();
                        txtDICompleted.Text = diDR["Completed"].ToString();
                        txtDITotal.Text = diDR["TotalCount"].ToString();
                    }

                    //RL
                    DataRow rlDR = rejStatusDT.Select("TEMP_CATEGORY='RL'").FirstOrDefault();
                    if (rlDR != null)
                    {
                        txtRLWaiting.Text = rlDR["Waiting"].ToString();
                        //CR015-20 pending approval count
                        txtRLPending.Text = rlDR["Pending"].ToString();
                        txtRLInUse.Text = rlDR["InUse"].ToString();
                        txtRLCompleted.Text = rlDR["Completed"].ToString();
                        txtRLTotal.Text = rlDR["TotalCount"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Dashboard;
                log.ClientCode = ddlClient.SelectedValue == null ? "" : ddlClient.SelectedValue.ToString();
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }
            finally
            {
                iConn.Close();
            }
        }

        private void LoadControl() 
        {
            //Load Client Drop Down
            int selectedIdx = this.ddlClient.SelectedIndex;

            string clientList = HttpContext.Current.Session["s_UserClients"] == null ? string.Empty : HttpContext.Current.Session["s_UserClients"].ToString();
            string[] cltArr = clientList.Split(',');
            List<ListItem> cltitems = new List<ListItem>();
            foreach (string clientCode in cltArr) cltitems.Add(new ListItem(clientCode, clientCode));
            this.ddlClient.DataSource = from i in cltitems select new ListItem() { Text = i.Text, Value = i.Value };
            this.ddlClient.DataTextField = "Text";
            this.ddlClient.DataValueField = "Value";
            this.ddlClient.DataBind();            

            this.ddlClient.SelectedIndex = selectedIdx <0 ? 0 : selectedIdx;

            DBConnectionInfo db = new DBConnectionInfo();
            db.webConnStr = webDBConnStr;
            string mainUVRPSConnection = db.GetSiteConnectionString(true, ddlClient.SelectedValue.ToString(), db.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), ddlClient.SelectedValue.ToString().Trim()));
            
            DateTime curActiveBusdate = DateTime.Today;
            if (BusinessDate.GetActive(mainUVRPSConnection) != null)
            {
                curActiveBusdate = Convert.ToDateTime(BusinessDate.GetActive(mainUVRPSConnection));
            }

            Session["CurBusdate"] = curActiveBusdate;

            if(txtDateTime.Text.Trim().Length <=0)
                txtDateTime.Text = curActiveBusdate.ToShortDateString();
             
        }

        protected void txtDateTime_TextChanged(object sender, EventArgs e)
        {
            LoadRejectedItemStatus();
        }

        protected void ddlClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateActiveBusdate();
            //Edited by AK on 2020-09-08:get multi level approval indicator from db
            UpdateRequiredApproval();
            LoadRejectedItemStatus();
            ToogleMonitorDiv();
        }

        private void ToogleMonitorDiv()
        {
            string selectedClient = ddlClient.SelectedValue;

            if (!isUnisys && selectedClient == "RHB")
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowMonitorDiv", "showMonitorDiv();", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideMonitorDiv", "hideMonitorDiv();", true);
            }
        }
    }


   
}