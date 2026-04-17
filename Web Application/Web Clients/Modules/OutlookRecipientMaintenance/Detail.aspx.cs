using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using UBPC.Web.Common;

namespace UBPCWeb.Modules.OutlookRecipientMaintenance
{
    public partial class Detail : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private string clientCode = string.Empty;
        private bool isPageValid = false;
        public bool isEditMode = false;
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

                if (isPageValid)
                {
                    string opParam = Page.RouteData.Values["Id"].ToString();
                    isEditMode = opParam.Equals("NEW") ? false : true;
                }

                //Load Control for first time
                if (isPageValid & !Page.IsPostBack)
                {
                    LoadControl();
                    //clientCode = ddlClient.SelectedValue.ToString();
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

                Response.Redirect("/WebDecision", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void LoadControl()
        {
            try
            {
                string opParam = Page.RouteData.Values["Id"].ToString();
                lblTitle.Text = opParam.Equals("NEW") ? "Create New Outlook Recipient" : "Update Outlook Recipient";

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

                if (opParam.Equals("NEW"))
                {
                    hfActionType.Value = "A";
                    hfRecpID.Value = "0";
                }
                else
                {
                    hfActionType.Value = "E";
                    hfRecpID.Value = opParam;
                }

                if (hfActionType.Value.Equals("E"))
                {
                    DataTable dt = CommonFunction.GetRecipientInfo(opParam);
                    if (dt.Rows.Count > 0)
                    {
                        txtEmailAddress.Text = dt.Rows[0]["Recp_EmailAddr"].ToString();
                        hfPrevEmailAddr.Value = dt.Rows[0]["Recp_EmailAddr"].ToString();
                        ddlClient.SelectedValue = dt.Rows[0]["Recp_ClientCode"].ToString();
                        ddlSite.SelectedValue = dt.Rows[0]["Recp_Site"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                //Load javascript spinner
                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadSpinner", "loadSpinner()", true);
                if (validateInsertUpdate())
                {
                    if (hfActionType.Value == "A")
                    {
                        InsertNewRecipient();
                    }
                    else
                    {
                        UpdateRecipient();
                    }
                }
                else
                {
                    //Log Failed to update due to validate failed
                    Logger.Write(false, LogCallerID.OutlookRecipientMaintenance, (clientList.Contains(",") ? "" : clientList.Trim().ToString()), "ValidateInsertUpdate", "Failed to Add/Update Email Recipient because Validation failed.", LogEventType.Information, userID);
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
            }

            Response.Redirect("/OutlookRecipient", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected bool validateInsertUpdate()
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(txtEmailAddress.Text))
            {
                isValid = false;
            }

            Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", RegexOptions.IgnoreCase);
            if (!regex.IsMatch(txtEmailAddress.Text))
            {
                isValid = false;
            }

            bool exist = CommonFunction.CheckisEmailAddrExist(hfRecpID.Value, txtEmailAddress.Text.ToString(), ddlClient.SelectedValue.ToString());
            if (exist)
            {
                isValid = false;
            }

            return isValid;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadSpinner", "loadSpinner()", true);
            Response.Redirect("/OutlookRecipient", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        [WebMethod]
        public static string validateEmailAddr(string id, string emailAddr, string clientCode)
        {
            bool isEmailAddrExist = CommonFunction.CheckisEmailAddrExist(id, emailAddr, clientCode);
            string returnMsg = isEmailAddrExist ? "1" : "0";//1 - true, 0 - false
            return returnMsg;
        }


        protected void InsertNewRecipient()
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = Resource.stmtInsOutlookRecipient;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 4, "@Recp_ClientCode", ParameterDirection.Input, ddlClient.SelectedValue.Trim().ToString()),
                    dbHelperObj.CreateParameter(DbType.String, 50, "@Recp_EmailAddr", ParameterDirection.Input, txtEmailAddress.Text.ToString()),
                    dbHelperObj.CreateParameter(DbType.DateTime, 50, "@Recp_CreationDate", ParameterDirection.Input, DateTime.Now),
                    dbHelperObj.CreateParameter(DbType.String, 50, "@Recp_CreatedBy", ParameterDirection.Input, userID),
                    dbHelperObj.CreateParameter(DbType.String, 4, "@Recp_Site", ParameterDirection.Input, ddlSite.SelectedValue.Trim().ToString())
                };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);

                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OutlookRecipientMaintenance;
                log.UserName = userID;
                log.Severity = LogEventType.Information;
                log.Message = "New Recipient (" + txtEmailAddress.Text.Trim() + ") for " + ddlClient.SelectedValue.ToString() + " created";
                log.Write();
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

                throw ex;
            }
            finally
            {
                iConn.Close();
            }
        }

        protected void UpdateRecipient()
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = Resource.stmtUpdOutlookRecipient;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 4, "@Recp_ClientCode", ParameterDirection.Input, ddlClient.SelectedValue.ToString()),
                    dbHelperObj.CreateParameter(DbType.String, 50, "@Recp_EmailAddr", ParameterDirection.Input, txtEmailAddress.Text.ToString()),
                    dbHelperObj.CreateParameter(DbType.String, 50, "@Recp_ID", ParameterDirection.Input, hfRecpID.Value.ToString().Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 4, "@Recp_Site", ParameterDirection.Input, ddlSite.SelectedValue.ToString().Trim()),
                };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);

                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OutlookRecipientMaintenance;
                log.UserName = userID;
                log.Severity = LogEventType.Information;
                log.Message = "Update Recipient from (" + hfPrevEmailAddr.Value.Trim() + ") to (" + txtEmailAddress.Text.Trim() + ") for " + ddlClient.SelectedValue.ToString();
                log.Write();
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

                throw ex;
            }
            finally
            {
                iConn.Close();
            }
        }
    }
}