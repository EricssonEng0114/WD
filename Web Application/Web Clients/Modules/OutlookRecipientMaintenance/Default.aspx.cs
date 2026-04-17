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

namespace UBPCWeb.Modules.OutlookRecipientMaintenance
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
                    validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "OutlookRecipientMaintenance");
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
                }

                if (isPageValid && IsPostBack)
                {
                    string tagOperation = Request.Form["tag"] == null ? string.Empty : Request.Form["tag"];
                    if (tagOperation.Equals("EDIT"))
                    {
                        //Perform Update
                        string paramName = Request.Form["tableID"] == null ? string.Empty : Request.Form["tableID"].ToString().Trim();

                        string url = "OutlookRecipient/" + paramName.Trim().ToString();

                        if (!string.IsNullOrEmpty(paramName))
                        {
                            Response.Redirect(url, false);
                            Context.ApplicationInstance.CompleteRequest();
                        }
                        else
                        {
                            //Invalid ID retrieve for edit operator
                            Logger.Write(false, LogCallerID.OutlookRecipientMaintenance, clientCode, "Update Outlook Recipient", "Invalid Outlook Recipient ID (" + paramName.Trim().ToString() + ") retrieved for update operation", LogEventType.Error, userID);
                            Response.Redirect("/OutlookRecipient", false);
                            Context.ApplicationInstance.CompleteRequest();
                        }
                    }

                    if (tagOperation.Equals("DEL"))
                    {
                        //Perform Update
                        string deletedID = Request.Form["tableID"] == null ? string.Empty : Request.Form["tableID"].ToString().Trim();
                        string deletedRecipientID = Session["MainDTParam"].ToString();

                        if (!string.IsNullOrEmpty(deletedRecipientID.Trim()))
                        {
                            deleteRecipientInfo(deletedRecipientID);
                        }
                        else
                        {
                            //Invalid ID retrieve for delete operation
                            Logger.Write(false, LogCallerID.OutlookRecipientMaintenance, clientCode, "Delete Outlook Recipient", "Invalid Outlook Recipient ID retrieved for delete operation", LogEventType.Error, userID);
                        }

                        Response.Redirect("/OutlookRecipient", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OutlookRecipientMaintenance;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                Response.Redirect("/OutlookRecipient", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void LoadControl()
        {
            try
            {
                //Load Client Drop Down
                string[] cltArr = clientList.Split(',');
                List<ListItem> cltitems = new List<ListItem>();
                foreach (string clientCode in cltArr) cltitems.Add(new ListItem(clientCode, clientCode));
                //Edited Shinyi:Add Unisys as one of the selection PE-WD-24-001
                cltitems.Add(new ListItem("UNISYS", "USYS"));

                this.ddlClient.DataSource = from i in cltitems select new ListItem() { Text = i.Text, Value = i.Value };
                this.ddlClient.DataTextField = "Text";
                this.ddlClient.DataValueField = "Value";
                this.ddlClient.DataBind();
                this.ddlClient.SelectedIndex = 0;




                //Initialise Grid
                //client, email address
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                     string.Format("initOutlookRecpMaintDataTable('{0}','{1}','{2}');", ddlClient.SelectedValue.Trim(),
                     txtEmailAddr.Text.Trim(), ddlSite.SelectedValue.Trim()), true);

                ClientScript.RegisterStartupScript(this.GetType(), "Configure", "configureTable()", true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                //Initialise Grid
                //client, email address
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initOutlookRecpMaintDataTable('{0}','{1}','{2}');", ddlClient.SelectedValue.Trim(),
                    txtEmailAddr.Text.Trim(), ddlSite.SelectedValue.Trim()), true);

                ClientScript.RegisterStartupScript(this.GetType(), "Configure", "configureTable()", true);
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OutlookRecipientMaintenance;
                log.ClientCode = !clientList.Contains(",") ? clientList.Trim() : string.Empty;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }
        }

        protected void New_Click(object sender, EventArgs e)
        {
            Response.Redirect("/OutlookRecipient/NEW", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            txtEmailAddr.Text = "";
            ddlClient.SelectedIndex = 0;

            //Initialise Grid
            //client, email address
            ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initOutlookRecpMaintDataTable('{0}','{1}','{2}');", ddlClient.SelectedValue.Trim(),
                    txtEmailAddr.Text.Trim(), ddlSite.SelectedValue.Trim()), true);

            ClientScript.RegisterStartupScript(this.GetType(), "Configure", "configureTable()", true);
        }

        private void deleteRecipientInfo(string id)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = string.Empty;
                stmt = Resource.stmtDelOutlookRecipient;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 50, "@Recp_ID", ParameterDirection.Input,id)
                };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OutlookRecipientMaintenance;
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

        [WebMethod]
        public static string assignSessionDT(string selID)
        {
            //Get 1 row of DataTable record and assign to session
            HttpContext.Current.Session["MainDTParam"] = selID.Trim();
            return "1";
        }
    }
}