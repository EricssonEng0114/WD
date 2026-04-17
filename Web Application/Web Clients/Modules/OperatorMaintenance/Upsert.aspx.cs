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
using System.Net;

namespace UBPCWeb.Modules.OperatorMaintenance
{
    public partial class Upsert : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private string clientList = string.Empty;

        private bool isPageValid = false;
        public bool isEditMode = false;
        public bool isPasswordEditable = false;
        private int curPwdPosition = 0;

        private bool isForUnisys = false;

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
                validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "OperatorMaintenance");
                isPageValid = validatorResult.Valid;

                if (!isPageValid)
                {
                    Session["s_GeneralMsg"] = validatorResult.ReturnMessage;
                    Response.Redirect("/Home", false);
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

            if (isPageValid) 
            {
                string opParam = Page.RouteData.Values["Id"].ToString();
                isEditMode = opParam.Equals("NEW") ? false : true;

            }

            //First time load - reset control
            if(!IsPostBack &&  isPageValid)
                LoadControls();

        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadSpinner", "loadSpinner()", true);
            Response.Redirect("/Operator", false);
            Context.ApplicationInstance.CompleteRequest();

        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                //Load javascript spinner
                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadSpinner", "loadSpinner()", true);
                //checkSaveData
                if (ValidateInsertUpdate())
                {
                    if (!isEditMode)
                    {
                       //insert operator
                        InsertNewOperator();
                    }
                    else
                    {
                        //update operator
                        UpdateOperator();

                        if (txtPassword.Text.Trim().Length > 0)
                        {
                            RefreshSelectedOpInfo();
                            curPwdPosition = curPwdPosition.Equals(5) ? 0 : curPwdPosition + 1;
                            UpdateOperatorPassword(curPwdPosition);
                        }
                    }
                }
                else
                {
                    //Log Failed to update due to validate failed
                    Logger.Write(false, LogCallerID.OperatorMaintenance, (clientList.Contains(",") ? "" : clientList.Trim().ToString()), "ValidateInsertUpdate", "Failed to Add/Update Operator because Validation failed.", LogEventType.Information, userID);
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OperatorMaintenance;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

            Response.Redirect("/Operator", false);
            Context.ApplicationInstance.CompleteRequest();

        }

        private void RefreshSelectedOpInfo() 
        {
            string selectedRowParam = Session["MainDTParam"].ToString();
            DataTable dtSelectedUsr = CommonFunction.GetUserInfoWithUserID(selectedRowParam.Trim());
            curPwdPosition = Convert.ToInt16(dtSelectedUsr.Rows[0]["UST_PwdPos"].ToString());
        }

        private void LoadControls()
        {
            string opParam = Page.RouteData.Values["Id"].ToString();
            string currentUser = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString().Trim();

            //Only admin group allowed to enter random password to reset
            isPasswordEditable = (!isForUnisys && isEditMode && userGroup.Equals("8")) || (isForUnisys && isEditMode && userGroup.Equals("9")) || (opParam.Equals("NEW"));            
                             
            #region populate drop down for User Group
            List<string> ddllist  = isForUnisys ? new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9" } : new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8" };
            
            this.ddlUserGroup.DataSource = from i in ddllist select new ListItem() { Text = i, Value = i };
            this.ddlUserGroup.DataBind();
            #endregion

            #region populate list box for client

            if (isForUnisys)
            {
                DataTable dtClientInfo = GetClientInfoList();
                DataView dv = new DataView(dtClientInfo);
                this.lstBoxClients.DataSource = dtClientInfo;
                this.lstBoxClients.DataTextField = "CLT_ClientCode";
                this.lstBoxClients.DataValueField = "CLT_ClientCode";
                this.lstBoxClients.DataBind();
            }
            else
            {
                string[] cltArr = clientList.Split(',');
                List<ListItem> cltitems = new List<ListItem>();
                foreach (string clientCode in cltArr)
                {
                    cltitems.Add(new ListItem(clientCode, clientCode));
                }

                this.lstBoxClients.DataSource = from i in cltitems select new ListItem() { Text = i.Text, Value = i.Value };
                this.lstBoxClients.DataTextField = "Text";
                this.lstBoxClients.DataValueField = "Value";
                this.lstBoxClients.DataBind();
            }

            #endregion

            lblTitle.Text = opParam.Equals("NEW") ? "Create New Operator" : "Update Operator";
            
            if (opParam.Equals("NEW"))
            {
                lblPassword.Visible = true;
                lblPassword.Text = "*Password";
                txtPassword.Enabled = true;
                txtPassword.Visible = true;

                //hide login checkbox
                chkIsLogin.Checked = false;
                chkIsLogin.Visible = false;
                chkIsLogin.Enabled = false;
                lblIsLogin.Visible = false;

                //disable force password change checkbox for create new
                chkForcePasswordChange.Checked = true;
                chkForcePasswordChange.Visible = true;
                chkForcePasswordChange.Enabled = false;
                lblForcedPwdChanged.Visible = true;

                txtUsrID.Text = string.Empty;
                txtUsrID.ReadOnly = false;
                txtUsrName.Text = string.Empty;
                txtPassword.Text = string.Empty;
                txtAreaComment.InnerText = string.Empty;
                this.ddlUserGroup.SelectedValue = "1";
                if (this.lstBoxClients.Items.Count > 0) 
                {
                    this.lstBoxClients.Items[0].Selected = true;
                }
            }
            else 
            {
                txtUsrID.ReadOnly = true;

                lblPassword.Visible = false;
                txtPassword.Enabled = false;
                txtPassword.Visible = false;

                //login checkbox
                chkIsLogin.Visible = true;
                chkIsLogin.Enabled = true;
                lblIsLogin.Visible = true;

                chkForcePasswordChange.Visible = true;
                chkForcePasswordChange.Enabled = true;
                lblForcedPwdChanged.Visible = true;
 
                //Get User ID to retrieve user info and populate into page
                string selectedRowParam = Session["MainDTParam"].ToString();
                DataTable dtSelectedUsr = CommonFunction.GetUserInfoWithUserID(selectedRowParam.Trim());

                bool isPasswordForcedChangebefore = false;

                if (dtSelectedUsr.Rows.Count > 0)
                {
                    curPwdPosition = Convert.ToInt16(dtSelectedUsr.Rows[0]["UST_PwdPos"].ToString());

                    txtUsrID.Text = HttpUtility.HtmlDecode(dtSelectedUsr.Rows[0]["UST_UserID"].ToString());
                    txtUsrName.Text = WebUtility.HtmlDecode(dtSelectedUsr.Rows[0]["UST_UserName"].ToString());
                    txtAreaComment.InnerText = HttpUtility.HtmlDecode(dtSelectedUsr.Rows[0]["UST_Comments"].ToString());
                    ddlUserGroup.SelectedValue = dtSelectedUsr.Rows[0]["UST_UserGroup"].ToString();
                    isPasswordForcedChangebefore = (string.IsNullOrEmpty(dtSelectedUsr.Rows[0]["UST_LastLogin"].ToString()) ? false : true) &&                        
                        (string.IsNullOrEmpty(dtSelectedUsr.Rows[0]["UST_PwdChgLastDate"].ToString()) ? false : true);

                    //checkbox value
                    chkForcePasswordChange.Checked = Convert.ToBoolean(dtSelectedUsr.Rows[0]["UST_ForcePwdChg"].ToString());                   
                    chkForcePasswordChange.Enabled = isPasswordForcedChangebefore;//if previously set to force password change, cannot undo

                    bool isLogin = Convert.ToBoolean(dtSelectedUsr.Rows[0]["UST_NoSession"].ToString().Equals("1"));
                    bool isLocked = Convert.ToBoolean(dtSelectedUsr.Rows[0]["UST_Locked"].ToString().Equals("1"));
                    lblIsLogin.Text = (isLogin && isLocked)? "Is Login & Locked"
                        : (isLogin && !isLocked) ? "Is Login" : "Locked";

                    chkIsLogin.Checked = isLogin || isLocked;

                    chkIsLogin.Enabled = !(currentUser.Trim() == txtUsrID.Text.Trim());


                    if (this.lstBoxClients.Items.Count > 0)
                    {
                        string selectedClient = dtSelectedUsr.Rows[0]["UST_Clients"].ToString();

                        for (int i = 0; i < lstBoxClients.Items.Count; i++)
                        {
                            if (selectedClient.Contains(lstBoxClients.Items[i].Text.Trim().ToString()))
                            {
                                this.lstBoxClients.Items[i].Selected = true;
                            }
                            else 
                            {
                                this.lstBoxClients.Items[i].Selected = false;
                            }
                        }                       
                    }
                    
                    //if group 9 - Admin - allowed to change password
                    if (isPasswordEditable)
                    {
                        lblPassword.Text = "Reset Password";
                        lblPassword.Visible = true;
                        txtPassword.Visible = true;
                        txtPassword.Enabled = true;
                    }
                    else
                    {
                        lblPassword.Visible = false;
                        txtPassword.Visible = false;
                        txtPassword.Enabled = false;
                    }
                }
                else 
                {
                    //nabigate back to listing page
                    Response.Redirect("Operator",false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
        }
      

        private void InsertNewOperator()
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                List<ListItem> selectedClientList = lstBoxClients.Items.Cast<ListItem>().Where(i => i.Selected).ToList();
                string strSelectedClient = string.Empty;

                foreach (ListItem lstItem in selectedClientList)
                {
                    strSelectedClient += lstItem.Text.Trim() + ",";
                }

                strSelectedClient = strSelectedClient.Remove(strSelectedClient.Length - 1);

                string stmt = string.Empty;
                stmt = Resource.stmtInsertUser;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input, txtUsrID.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 20, "@UserName", ParameterDirection.Input, txtUsrName.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.Int16, 0, "@UserGroup", ParameterDirection.Input, Convert.ToInt16(ddlUserGroup.SelectedValue.ToString().Trim())),
                    dbHelperObj.CreateParameter(DbType.String, 100, "@Password", ParameterDirection.Input,  UCrypt.Encrypt(txtPassword.Text)),
                    dbHelperObj.CreateParameter(DbType.String, 50, "@Comments", ParameterDirection.Input, txtAreaComment.InnerText.ToString().Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 200, "@Clients", ParameterDirection.Input, strSelectedClient),
                    dbHelperObj.CreateParameter(DbType.Int16, 0, "@ForUnisys", ParameterDirection.Input, isForUnisys)

                    };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);

                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OperatorMaintenance;
                log.UserName = userID;
                log.Severity = LogEventType.Information;
                log.Message = "New Operator ("+txtUsrID.Text.Trim()+") created";
                log.Write();

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OperatorMaintenance;
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

        private void UpdateOperator()
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {

              List<ListItem> selectedClientList = lstBoxClients.Items.Cast<ListItem>().Where(i => i.Selected).ToList();
              string strSelectedClient = string.Empty;

              foreach (ListItem lstItem in selectedClientList) 
              {
                  strSelectedClient += lstItem.Text.Trim() + ",";
              }

             strSelectedClient = strSelectedClient.Remove(strSelectedClient.Length - 1);
                
                string stmt = string.Empty;
                stmt = Resource.stmtUpdateUser;

                bool isLocked = false;
                if (lblIsLogin.Text.Contains("Locked"))
                {
                    isLocked = chkIsLogin.Checked? true:false;
                }              

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input, txtUsrID.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 20, "@UserName", ParameterDirection.Input, txtUsrName.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.Int16, 0, "@UserGroup", ParameterDirection.Input, Convert.ToInt16(ddlUserGroup.SelectedValue.ToString().Trim())),
                    dbHelperObj.CreateParameter(DbType.String, 50, "@Comment", ParameterDirection.Input, txtAreaComment.InnerText.ToString().Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 200, "@Clients", ParameterDirection.Input, strSelectedClient),
                    dbHelperObj.CreateParameter(DbType.Int16, 0, "@ForUnisys", ParameterDirection.Input, isForUnisys),
                    dbHelperObj.CreateParameter(DbType.Int16, 0, "@IsLocked", ParameterDirection.Input, isLocked),
                    dbHelperObj.CreateParameter(DbType.Int16, 0, "@IsSession", ParameterDirection.Input, lblIsLogin.Text.Trim().Equals("Locked")?0: (chkIsLogin.Checked ? 1 : 0)),
                    dbHelperObj.CreateParameter(DbType.Int16, 0, "@ForcedPwdChanged", ParameterDirection.Input, chkForcePasswordChange.Checked ? 1 : 0)
                    };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);

                Logger.Write(false, LogCallerID.Login, (clientList.Contains(",") ? "" : clientList.Trim().ToString()), "Update Operator", "Operator ( " + txtUsrID.Text + " ) has been successfully updated by " + userID, LogEventType.Information, userID);

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OperatorMaintenance;
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
            

        private void UpdateOperatorPassword(int passwordPos)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = string.Empty;
                stmt = String.Format(Resource.stmtUpdateOperPassword, passwordPos);

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input, txtUsrID.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 100, "@Password", ParameterDirection.Input,  UCrypt.Encrypt(txtPassword.Text)),
                    dbHelperObj.CreateParameter(DbType.Int16, 0, "@PassPos", ParameterDirection.Input,  passwordPos)
                };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);

                //Log success change password to audit
                Logger.Write(false, LogCallerID.Login, (clientList.Contains(",") ? "" : clientList.Trim().ToString()), "Reset Password", "Password for " + txtUsrID.Text + " has been reset and changed by" + userID, LogEventType.Information, userID);
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OperatorMaintenance;
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

        private bool ValidateInsertUpdate()
        {
            PassValidatorResult validatorResult;
            int intPassPos = 0;
            bool isValid = true;

            //Validate mandatory
            if (txtUsrID.Text.Trim().Length < 6 || txtUsrName.Text.Trim().Length <= 0) 
            {
                isValid = false;
            }

            if (!isEditMode) 
            {
                isValid = txtPassword.Text.Trim().Length > 0;

                if (!UBPC.Web.Common.CommonFunction.isAlphanumeric(txtUsrID.Text.Trim()))
                {
                    isValid = false;
                }
            }

            //validate user id for newly created operator
            if (!isEditMode && CommonFunction.CheckIsUsrExist(txtUsrID.Text.Trim()))
            {
                isValid = false;
            }

            
            //If password is entered, validate password
            if (txtPassword.Text.Trim().Length > 0) 
            {
                string[] passwordHistory = CommonFunction.GetUserPasswordHistory(ref intPassPos, txtUsrID.Text.Trim().ToString());

                //For password chnge, validate password by encryption class
                validatorResult = PassValidator.Validate(txtPassword.Text, false, passwordHistory);
                isValid = validatorResult.Valid;              
            } 

            return isValid;
        }

        private DataTable GetClientInfoList()
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = isForUnisys? Resource.stmtGetClientInfoList: Resource.stmtGetClientInfoListExcUnisys;
                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt);

                return dtDB;

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OperatorMaintenance;
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

        [WebMethod]
        public static string validatePassword(string param, string id) 
        {
            PassValidatorResult validatorResult;
            int intPassPos = 0;
            bool isValid = false;
            string returnMsg = string.Empty;

            //For password chnge, validate password by encryption class
            validatorResult = PassValidator.Validate(param, false, CommonFunction.GetUserPasswordHistory(ref intPassPos, id));
            isValid = validatorResult.Valid;
            returnMsg = !isValid? validatorResult.ReturnMessage: "";

            return returnMsg;
        }

        [WebMethod]
        public static string checkUserLogin(string id) 
        {
           bool isUsrLogin = CommonFunction.CheckIsUsrLogin(id);
           string returnMsg = isUsrLogin ? "1" : "0";//1 - true, 0 - false
           return returnMsg;          
        }        

        [WebMethod]
        public static string validateUsrID(string id) 
        {
           bool isUsrExist = CommonFunction.CheckIsUsrExist(id);
           string returnMsg = isUsrExist ? "1" : "0";//1 - true, 0 - false
           return returnMsg;          
        }            

        protected void btnForcedSave_Click(object sender, EventArgs e)
        {
            Logger.Write(false, LogCallerID.OperatorMaintenance, (clientList.Contains(",") ? "" : clientList.Trim().ToString()), "Update Operator", "User" + txtUsrID.Text.Trim() + " is currently login. Confirmed to proceed update by " + userID, LogEventType.Information, userID);

            //Force update even user is actively login
            btnSave_Click(sender, e);
        }
    }
}