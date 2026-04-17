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
using System.Web.Services;


namespace UBPCWeb.Modules.Announcement
{
    public partial class Default : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private string clientList = string.Empty;
        private bool isPageValid = false;
        private string strReturnMsg = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString();
            userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString();
            clientList = Session["s_UserClients"] == null ? string.Empty : Session["s_UserClients"].ToString();

            if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
            {
                PageValidatorResult validatorResult;

                //For password chnge, validate password by encryption class
                validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "AnnouncementMaintenance");
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

            if (isPageValid && !IsPostBack)
            {
                LoadControl();
            }


            if (isPageValid && IsPostBack) 
            {
                string tagOperation = Request.Form["tag"] == null ? string.Empty : Request.Form["tag"];

                if (tagOperation.Equals("EDIT")) 
                {
                    //Perform Update
                   // string announID = Request.Form["tableID"] == null ? string.Empty : Request.Form["tableID"];
                    string announID = Session["MainDTParam"].ToString();

                    if (!string.IsNullOrEmpty(announID))
                    {
                        Response.Redirect("/Announcement/" + announID, false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        //Invalid ID retrieve for edit operation
                        Logger.Write(false, LogCallerID.AnnouncementMaintenance, "", "Update Announcement", "Invalid Announcement ID retrieved for update operation", LogEventType.Error, userID);
                        Response.Redirect("/Announcement", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }

                if (tagOperation.Equals("DEL"))
                {
                    //Perform Update
                  //  string announID = Request.Form["tableID"] == null ? string.Empty : Request.Form["tableID"];
                    string deletedAnnID = Session["MainDTParam"].ToString();

                    if (!string.IsNullOrEmpty(deletedAnnID))
                    {
                        DeleteAnnouncement(deletedAnnID);
                        Logger.Write(false, LogCallerID.AnnouncementMaintenance, ddlClient.SelectedValue.ToString(), "Announcement ID =" + deletedAnnID, "Announcement Deleted.", LogEventType.Information, userID);

                    }
                    else
                    {
                        //Invalid ID retrieve for delete operation
                        Logger.Write(false, LogCallerID.AnnouncementMaintenance, ddlClient.SelectedValue.ToString(), "Delete Announcement", "Invalid Announcement ID retrieved for delete operation", LogEventType.Error, userID);
                    }

                    Response.Redirect("/Announcement", false);
                    Context.ApplicationInstance.CompleteRequest();

                }
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

                txtTitle.Text = "";
                txtDesc.Text = "";
                
                //Initialise Grid
                //busdate, client, branch, category
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initAnnouncementDataTable('{0}','{1}','{2}');", ddlClient.SelectedValue.ToString(), txtTitle.Text.Trim(), txtDesc.Text.Trim()), true);

                ClientScript.RegisterStartupScript(this.GetType(), "Configure", "configureTable()", true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void New_Click(object sender, EventArgs e)
        {
            Response.Redirect("/Announcement/NEW", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void DeleteAnnouncement(string announceID)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = string.Empty;
                stmt = Resource.stmtSQLDeleteAnnouncement;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.Int64, 0, "@ID", ParameterDirection.Input,Convert.ToInt64(announceID))
                };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);


            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.AnnouncementMaintenance;
                log.ClientCode = "";
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                throw ex;
            }
            finally
            {
                iConn.Close();
            }
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            txtTitle.Text = "";
            txtDesc.Text = "";
            ddlClient.SelectedIndex = 0;

            ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                string.Format("initAnnouncementDataTable('{0}','{1}','{2}');", ddlClient.SelectedValue.ToString(), txtTitle.Text.Trim(), txtDesc.Text.Trim()), true);

            ClientScript.RegisterStartupScript(this.GetType(), "Configure", "configureTable()", true);
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initAnnouncementDataTable('{0}','{1}','{2}');", ddlClient.SelectedValue.ToString(), txtTitle.Text.Trim(), txtDesc.Text.Trim()), true);

                ClientScript.RegisterStartupScript(this.GetType(), "Configure", "configureTable()", true);
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.AnnouncementMaintenance;
                log.ClientCode = !clientList.Contains(",") ? clientList.Trim() : string.Empty;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }
        }

        [WebMethod]
        public static string assignSessionDT(string selID)
        {
            //Get 1 row of DataTable record and assign to session
            HttpContext.Current.Session["MainDTParam"] = selID.Trim();
            return "1";
        }
    }
}