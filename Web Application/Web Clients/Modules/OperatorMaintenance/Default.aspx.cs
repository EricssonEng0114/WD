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

namespace UBPCWeb.Modules.OperatorMaintenance
{
    public partial class Default : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private string clientList = string.Empty;
        private bool isPageValid = false;
        private bool isForUnisys = false;
        private string strReturnMsg = string.Empty;
        public string tempErrorMsg = string.Empty;
        
        protected void Page_Load(object sender, EventArgs e)
        {            
            string clientCode = string.Empty;

            if (!string.IsNullOrEmpty(tempErrorMsg))
            {
                Session["s_TempMsg"] = tempErrorMsg;
            }
            else
            {
                Session["s_TempMsg"] = "";
            }

            userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString().Trim();
            userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString().Trim();
            clientList = Session["s_UserClients"] == null ? string.Empty : Session["s_UserClients"].ToString().Trim();
            clientCode = !clientList.Contains(",") ? clientList.Trim() : string.Empty;//if contain ',', mean multiple client tie to this user
            isForUnisys = Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(Session["s_UserForUnisys"].ToString());

            if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
            {
                PageValidatorResult validatorResult;

                //For password chnge, validate password by encryption class
                validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "OperatorMaintenance");
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
                    string paramName = Request.Form["tableID"] == null ? string.Empty : Request.Form["tableID"].ToString().Trim();

                    string url = "Operator/" + paramName.Trim().ToString();

                    if (!string.IsNullOrEmpty(paramName))
                    {
                        Response.Redirect(url, false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        //Invalid ID retrieve for edit operator
                        Logger.Write(false, LogCallerID.OperatorMaintenance, clientCode, "Update Operator", "Invalid Operator ID (" + paramName.Trim().ToString() +") retrieved for update operation", LogEventType.Error, userID);
                        Response.Redirect("/Operator", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }

                if (tagOperation.Equals("DEL"))
                {
                    //Perform Update
                    string deletedID = Request.Form["tableID"] == null ? string.Empty : Request.Form["tableID"].ToString().Trim();
                    string deletedUsrID = Session["MainDTParam"].ToString();

                    if (!string.IsNullOrEmpty(deletedUsrID.Trim()))
                    {
                        DataTable dtSelectedUsr = CommonFunction.GetUserInfoWithUserID(deletedUsrID.Trim());

                        string deletedUsrId = dtSelectedUsr.Rows[0]["UST_UserID"].ToString();

                        //check if user is login , not allowed to delete if user is login
                        if (!CommonFunction.CheckIsUsrLogin(deletedUsrId) && (!deletedUsrId.Equals("Admin") && !deletedUsrId.Equals("SAdmin")))
                        {
                            DeleteOperator(deletedUsrId);
                        }
                        else
                        {
                            Logger.Write(false, LogCallerID.OperatorMaintenance, clientCode, "ValidateDelete", "Failed to Delete Operator because Operator is currently login.", LogEventType.Information, userID);
                            tempErrorMsg = "Unable to delete operator. " + deletedID + "is currently login.";
                        }
                    }
                    else
                    {
                        //Invalid ID retrieve for delete operation
                        Logger.Write(false, LogCallerID.OperatorMaintenance, clientCode, "Delete Operator", "Invalid Operator ID retrieved for delete operation", LogEventType.Error, userID);

                    }

                    Response.Redirect("/Operator", false);
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


                #region populate drop down for User Group
                List<string> ddllist = isForUnisys ? new List<string>() { "ALL", "1", "2", "3", "4", "5", "6", "7", "8", "9" } : new List<string>() { "ALL", "1", "2", "3", "4", "5", "6", "7", "8" };

                this.ddlUsrGroup.DataSource = from i in ddllist select new ListItem() { Text = i, Value = i };
                this.ddlUsrGroup.DataBind();
                this.ddlUsrGroup.SelectedIndex = 0;
                #endregion


                //Initialise Grid
                //busdate, client, branch, category
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initOperatorDataTable('{0}','{1}','{2}','{3}');", txtUserID.Text.Trim(), txtUsrName.Text.Trim(), ddlClient.SelectedValue.ToString(),"ALL"), true);

                ClientScript.RegisterStartupScript(this.GetType(), "Configure", "configureTable()", true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void New_Click(object sender, EventArgs e)
        {
            Response.Redirect("/Operator/NEW", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void DeleteOperator(string deletedOperID)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = string.Empty;
                stmt = Resource.stmtDeleteUser;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input,deletedOperID)
                };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);

                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OperatorMaintenance;
                log.UserName = userID;
                log.Severity = LogEventType.Information;
                log.Message = "Operator (" + deletedOperID + ") deleted.";
                log.Write();

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OperatorMaintenance;
                log.ClientCode = !clientList.Contains(",") ? clientList.Trim() : string.Empty;
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
        public static string checkUserLogin(string id)
        {
            bool isUsrLogin = CommonFunction.CheckIsUsrLogin(id);
            string returnMsg = isUsrLogin ? "1" : "0";//1 - true, 0 - false
            return returnMsg;
        }

        [WebMethod]
        public static string assignSessionDT(string selID)
        {
            //Get 1 row of DataTable record and assign to session
            HttpContext.Current.Session["MainDTParam"] = selID.Trim();
            return "1";
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            txtUserID.Text = "";
            txtUsrName.Text = "";
            ddlClient.SelectedIndex = 0;
            ddlUsrGroup.SelectedIndex = 0;

            ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                string.Format("initOperatorDataTable('{0}','{1}','{2}','{3}');", "", "", ddlClient.SelectedValue.ToString(), ddlUsrGroup.SelectedValue.ToString().Trim()), true);

            ClientScript.RegisterStartupScript(this.GetType(), "Configure", "configureTable()", true);

        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initOperatorDataTable('{0}','{1}','{2}','{3}');", txtUserID.Text.Trim(), txtUsrName.Text.Trim(), ddlClient.SelectedValue.ToString(), ddlUsrGroup.SelectedValue.ToString().Trim()), true);

                ClientScript.RegisterStartupScript(this.GetType(), "Configure", "configureTable()", true);
            }
            catch (Exception ex) 
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OperatorMaintenance;
                log.ClientCode = !clientList.Contains(",") ? clientList.Trim() : string.Empty;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }
            
        }       
         

    }
}