using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UBPC.Encryption;
using UBPC.Web.Common;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.VisualBasic;
using System.Web.Services;
using System.Web.Script.Services;
using UBPCWeb.Model;
using System.Web.Script.Serialization;

namespace UBPCWeb.Modules.Reports
{
    public partial class Default : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private bool isPageValid = false;
        private bool isForUnisys = false;
        private string clientList = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString().Trim();
            userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString().Trim();
            isForUnisys = Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(Session["s_UserForUnisys"].ToString());
            clientList = Session["s_UserClients"] == null ? string.Empty : Session["s_UserClients"].ToString().Trim();

            if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
            {
                PageValidatorResult validatorResult;

                //For password chnge, validate password by encryption class
                validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "Reports");
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

            if (isPageValid & !Page.IsPostBack)
            {
                LoadControl();
            }
        }

        private void LoadControl()
        {

            //Load Client Drop Down
            //if (isForUnisys)
            //{
            //    DataTable dtClientInfo = CommonFunction.GetClientInfoList(Convert.ToBoolean(Session["s_UserForUnisys"].ToString()));
            //    DataView dv = new DataView(dtClientInfo);
            //    this.ddlClient.DataSource = dtClientInfo;
            //    this.ddlClient.DataTextField = "CLT_ClientCode";
            //    this.ddlClient.DataValueField = "CLT_ClientCode";
            //    this.ddlClient.DataBind();
            //}
            //else
            //{
                string[] cltArr = clientList.Split(',');
                List<ListItem> cltitems = new List<ListItem>();
                foreach (string clientCode in cltArr)
                {
                    cltitems.Add(new ListItem(clientCode, clientCode));
                }

                this.ddlClient.DataSource = from i in cltitems select new ListItem() { Text = i.Text, Value = i.Value };
                this.ddlClient.DataTextField = "Text";
                this.ddlClient.DataValueField = "Value";
                this.ddlClient.DataBind();
            //}


            if (this.ddlClient.Items.Count > 0)
            {
                this.ddlClient.Items[0].Selected = true;
            }
            
            //Build connection string for site and set curDataTable
            DBConnectionInfo dbConn = new DBConnectionInfo();
            dbConn.webConnStr = webDBConnStr;
            
            //Load worksource lsit
            DataTable dtWsList = CommonFunction.GetWorksourceList(userID, ddlClient.SelectedValue.ToString().Trim());
            this.ddlWorksource.DataSource = dtWsList;
            this.ddlWorksource.DataTextField ="WsName";
            this.ddlWorksource.DataValueField = "WsName";
            this.ddlWorksource.DataBind();
            ddlWorksource.SelectedIndex = 0;

            DateTime curActiveBusdate = String.IsNullOrEmpty(Session["CurBusdate"].ToString()) ? DateTime.Now : Convert.ToDateTime(Session["CurBusdate"]);
            txtDateTime.Text = curActiveBusdate.ToShortDateString();

            //Initialise selected busdate
            if (Session["CurActionSelectedBusdate"] == null)
            {
                Session["CurActionSelectedBusdate"] = curActiveBusdate.ToShortDateString();

            }
            else
            {
                Session["CurActionSelectedBusdate"] = "";
            }

            Session["CurRptSelectedBusdate"] = txtDateTime.Text;
            Session["CurRptSelectedWS"] = "0";

            

            //Initialise Grid
            //busdate, client, branch, category
            ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                string.Format("initReportDataTable('{0}','{1}','{2}');", txtDateTime.Text, ddlClient.SelectedValue.ToString(), "ALL"), true);

            ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);

        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string dateTime = string.Empty;
            string client = string.Empty;
            string worksource = string.Empty;
            
            dateTime = txtDateTime.Text.Trim();
            client = ddlClient.SelectedValue.ToString().Trim();
            worksource = ddlWorksource.SelectedValue.ToString().Trim().Equals("ALL") ? "0" : ddlWorksource.SelectedValue.ToString().Trim();

            //Shinyi:Used in Report Viewer to fiilter by Selected Busdate and worksource
            Session["CurActionSelectedBusdate"] = dateTime;
            Session["CurRptSelectedBusdate"] = dateTime;
            Session["CurRptSelectedWS"] = worksource;
            
            if (string.IsNullOrEmpty(dateTime.Trim()))
            {
                Logger.Write(false, LogCallerID.Reports, client, "Validate Search", "Date is Empty", LogEventType.Error, userID);
            }
            else
            {
                //date, severity, caller,client
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initReportDataTable('{0}','{1}','{2}');", dateTime, client, worksource), true);

                //date, severity, caller,client
                ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);

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

                txtDateTime.Text = curActiveBusdate.ToShortDateString();
                Session["CurActionSelectedBusdate"] = curActiveBusdate.ToShortDateString();

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Reports;
                log.ClientCode = ddlClient.SelectedValue == null ? "" : ddlClient.SelectedValue.ToString();
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

        }

        [WebMethod]
        public static string getReportUrl(string rptCode, string cltCode, string wsID, string busdate)
        {
            string url = string.Empty;
            url = "/CRViewerReportForm/ReportViewer.aspx?ReportCode=" + rptCode + "&ReportClt=" + cltCode + "&ReportDate=" + busdate + "&WrkSourceID=" + wsID;

           string fullUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + url;
            return fullUrl;
        }

        //protected void ddlClient_SelectedIndexChanged(object sender, EventArgs e)
        //{
           
        //}

        protected void ddlClient_SelectedIndexChanged1(object sender, EventArgs e)
        {
            //Set Valid navigation
            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "setValidNav", "setValidNavigation();", true);

            UpdateActiveBusdate();

            //load again worksource list
            if (ddlWorksource.Items.Count > 0)
                ddlWorksource.Items.Clear();

            //Load worksource lsit
            DataTable dtWsList = CommonFunction.GetWorksourceList(userID, ddlClient.SelectedValue.ToString().Trim());
            this.ddlWorksource.DataSource = dtWsList;
            this.ddlWorksource.DataTextField = "WsName";
            this.ddlWorksource.DataValueField = "WsName";
            this.ddlWorksource.DataBind();
            ddlWorksource.SelectedIndex = 0;
        }

    }
}