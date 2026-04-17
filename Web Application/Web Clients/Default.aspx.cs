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
//using System.Web.Security.AntiXss;

namespace UBPCWeb
{
    public partial class _Default : Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        //FL-CIMB-WebDec-22-001 do not hardcode client code
        private string clientCode = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            this.Title = "Login";

            if (Session["s_ChangePasswordSuccessMsg"] != null)
            {
                if (Session["s_ChangePasswordSuccessMsg"].ToString().Trim().Length > 0)
                {
                    lblErrMessage.Visible = false;
                    lblErrMessage.Text = string.Empty;
                    lblChangePasswordMsg.Visible = true;
                    lblChangePasswordMsg.Text = Session["s_ChangePasswordSuccessMsg"].ToString();
                    
                }
                else
                {
                    lblChangePasswordMsg.Visible = false;
                    lblChangePasswordMsg.Text = string.Empty;
                }
            }
            else 
            {
                lblChangePasswordMsg.Visible = false;
                lblChangePasswordMsg.Text = string.Empty;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                userID = txtUserID.Text.Trim();
                //Avoid user open new tab and login as other user 
                bool proceedLogin = Session["s_UserID"] == null ? true : string.IsNullOrEmpty(Session["s_UserID"].ToString().Trim()) ? true : false;

                if (proceedLogin)
                {
                    if (VerifyUserLogin())
                    {
                        lblErrMessage.Visible = false;
                        lblErrMessage.Text = string.Empty;

                        //Log success login
                        LogEntry log = new LogEntry();
                        log.Caller = LogCallerID.Login;
                        log.ClientCode = clientCode;
                        log.UserName = userID;
                        log.Severity = LogEventType.Information;
                        log.Message = "User password verification passed for :" + userID;
                        log.Write();

                        if (Session["s_UserIsLogin"] == null)
                        {
                            Session["s_UserIsLogin"] = false;
                        }

                        if (Session["s_ForcePwdChg"] == null)
                        {
                            Session["s_ForcePwdChg"] = false;
                        }

                        //user already login
                        if (Session["s_UserIsLogin"].Equals(true))
                        {
                            lblChangePasswordMsg.Visible = false;
                            lblChangePasswordMsg.Text = string.Empty;

                            lblErrMessage.Visible = true;
                            lblErrMessage.Text = Session["s_LoginMessage"].ToString();
                            //Reset user id 
                            Session["s_UserID"] = "";
                        }
                        //if force password change
                        else if (Session["s_ForcePwdChg"].Equals(true))
                        {
                            Response.Redirect("/ChangePassword", false);
                            Context.ApplicationInstance.CompleteRequest();
                        }
                        else
                        {
                            lblErrMessage.Visible = false;
                            lblErrMessage.Text = string.Empty;

                            //update user session as 1 - active
                            UpdateUserSession(userID);

                            if (Session["s_ChangePasswordSuccessMsg"] != null)
                            {
                                if (Session["s_ChangePasswordSuccessMsg"].ToString().Trim().Length > 0)
                                {
                                    Session["s_ChangePasswordSuccessMsg"] = ""; //reset back to "" after change password 
                                }
                            }

                            Response.Redirect("/Home", false);
                            Context.ApplicationInstance.CompleteRequest();

                        }
                    }
                    else
                    {
                        //Reset user id 
                        Session["s_UserID"] = "";

                        //log login failed
                        LogEntry log = new LogEntry();
                        log.Caller = LogCallerID.Login;
                        log.ClientCode = clientCode;
                        log.UserName = userID;
                        log.Severity = LogEventType.Information;
                        log.Message = "Verify Login failed.";
                        log.Write();

                        if (Session["s_LoginMessage"] == null)
                            Session["s_LoginMessage"] = "Unable to login. Please try again later";

                        //show login page
                        //show label message
                        lblErrMessage.Visible = true;
                        lblChangePasswordMsg.Visible = false;
                        lblChangePasswordMsg.Text = string.Empty;
                        lblErrMessage.Text = Session["s_LoginMessage"].ToString();
                    }
                }
                else 
                {
                    //show login page
                    //show label message
                    lblErrMessage.Visible = true;
                    lblChangePasswordMsg.Visible = false;
                    lblChangePasswordMsg.Text = string.Empty;
                    lblErrMessage.Text = "Multiple Logins not allowed in same browser.";
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Login;
                log.Severity = LogEventType.Error;
                log.Data = "VerifyUserLogin";
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                throw ex;
            }
        }

        #region Helper Methods
     
        private bool VerifyUserLogin() 
        {
            int intLoginTry = 0;
            int intPasswordValidityPeriod = 0;
            int intLastLoginValidityPeriod = 0;
            int intMaxLogonSession = 0;
            int intMaxDownloadDay = 0;

            bool isValidUser = false;

            try
            {
                //intialise empty string to session
                Session["s_UserID"] = string.Empty;
                Session["s_UserClients"] = string.Empty;
                Session["s_UserGroup"] = string.Empty;
                Session["s_UserForUnisys"] = false;

                //Used Upon timeout logout - will be assigned when selecting a trans in rejected item decision
                //Need assign default value else redirect to logout URL will hit error
                Session["s_CurSelectedClientRejDec"] = "C";
                Session["s_CurSelectedSiteRejDec"] = "S";


                //get max login try value & last login validity period from Param
                intLoginTry = Convert.ToInt16(Parameters.GetParamValue(webDBConnStr, Resources.Resource.cstrMAXLOGONTRY));
                intLastLoginValidityPeriod = Convert.ToInt32(Parameters.GetParamValue(webDBConnStr, Resources.Resource.cstrLastLoginValidityPeriod));
                intMaxLogonSession =  Convert.ToInt16(Parameters.GetParamValue(webDBConnStr, Resources.Resource.cstrMaxLogonSession));

                //check if user existed
                DataTable dtUserInfo = GetUserInfoWithUserID(txtUserID.Text.Trim());

                if (dtUserInfo.Rows.Count > 0)
                {
                    //Get password validity period from user record
                    string usrPwdValidityPeriod = dtUserInfo.Rows[0]["UST_PwdChgDaysValid"].ToString().Trim().Length > 0 ? dtUserInfo.Rows[0]["UST_PwdChgDaysValid"].ToString() : "0";
                    intPasswordValidityPeriod = Convert.ToInt32(usrPwdValidityPeriod);
                    int usrRetryCount = Convert.ToInt32(dtUserInfo.Rows[0]["UST_PwdChgRetryCnt"].ToString().Trim());
                    int usrLocked = Convert.ToInt16(dtUserInfo.Rows[0]["UST_Locked"].ToString());

                    //FL-CIMB-WebDec-22-001 client code refer to db for bank user,unisys user leave it empty
                    if(dtUserInfo.Rows[0]["UST_ForUnisys"].ToString().Trim().Equals("1"))
                    {
                        clientCode = string.Empty;
                    }
                    else
                    {
                        clientCode = dtUserInfo.Rows[0]["UST_Clients"].ToString().Trim();
                    }

                    //PCI DSS - Store minimum 6 password history
                    if (isValidPassword(dtUserInfo))
                    {
                        DateTime? dtLastLogin = null;
                        if (dtUserInfo.Rows[0]["UST_LastLogin"] != null)
                        {
                            if (!string.IsNullOrEmpty(dtUserInfo.Rows[0]["UST_LastLogin"].ToString().Trim()))
                            {
                                dtLastLogin = Convert.ToDateTime(dtUserInfo.Rows[0]["UST_LastLogin"].ToString());
                            }
                        }

                        //check difference date
                        int lastLoginDiff = 0;
                        if (dtLastLogin.HasValue)
                        {
                            lastLoginDiff = (DateTime.Now.Date - Convert.ToDateTime(dtLastLogin).Date).Days;
                        }

                        //Check user login retry count
                        if (usrRetryCount >= intLoginTry)
                        {
                            UpdateUserLockedAccount();
                            Session["s_LoginMessage"] = Resources.Resource.MsgMaxLogonTry;
                            isValidUser = false;
                            return false;
                            
                        }
                        //PCI DSS , If detected last login is > 90 days, automically locked the user account
                        //Modified by JBCHONG on 20220427 - PE-ALL-22-001
                        else if (dtUserInfo.Rows[0]["UST_UserGroup"].ToString() != "9" && dtLastLogin.HasValue && (lastLoginDiff > intLastLoginValidityPeriod) && !usrLocked.Equals(2))
                        {
                            UpdateUserLockedAccount();
                            Session["s_LoginMessage"] = Resources.Resource.MsgInactiveAccountLocked;
                            isValidUser = false;
                            return false;
                        }
                        //User is locked, not allowed to login
                        else if (dtUserInfo.Rows[0]["UST_Locked"].ToString().Trim().Equals("1"))
                        {
                            Session["s_LoginMessage"] = Resources.Resource.MsgMaxLogonTry;
                            isValidUser = false;
                            return false;
                        }
                        else
                        {
                            //Valid user and password - reset count = 0
                            //Update retry count & last login
                            UpdateRetryCountAndLastLogin(0);

                            //keep user info into Session
                            Session["s_UserID"] = dtUserInfo.Rows[0]["UST_UserID"].ToString().Trim();
                            Session["s_UserClients"] = dtUserInfo.Rows[0]["UST_Clients"].ToString().Trim();
                            Session["s_UserGroup"] = dtUserInfo.Rows[0]["UST_UserGroup"].ToString().Trim();
                            Session["s_UserForUnisys"] = Convert.ToInt16(dtUserInfo.Rows[0]["UST_ForUnisys"].ToString().Trim()).Equals(1);

                            //Set Main Site - centralise site 
                            Session["s_MainSite"] = Parameters.GetParamValue(webDBConnStr, Resources.Resource.MainSiteParam);

                            //Edited Shinyi - CR1086 - Get Max download days for Client - if is unisys, then no need check
                            //If is unisys - then set the download day = 0
                            Session["s_CurClientMaxDownloadDays"] = Session["s_UserForUnisys"].ToString() == "1" ? "0" : Parameters.GetParamValue(webDBConnStr, Resources.Resource.cstrMAXDownloadDayForClient);

                            Session["s_LoginMessage"] = string.Empty;
                            isValidUser = true;
                        }

                        //Check if required forced change password or change password validy reached
                        bool isPwdForcedChange = Convert.ToBoolean(dtUserInfo.Rows[0]["UST_ForcePwdChg"].ToString());
                        if (isPwdForcedChange) 
                        {
                           Session["s_ForcePwdChg"] = true;
                           Session["s_LoginMessage"] = Resources.Resource.MsgPasswordForceChg;
                        }
                        else if ((DateTime.Now.Date - (Convert.ToDateTime(dtUserInfo.Rows[0]["UST_PwdChgLastDate"].ToString())).Date).Days > intPasswordValidityPeriod)
                        {
                            Session["s_ForcePwdChg"] = true;
                            Session["s_LoginMessage"] = Resources.Resource.MsgPasswordExpired;
                        }
                        else 
                        {
                            Session["s_ForcePwdChg"] = false;
                        }

                        //check log in session - only allowed 1 session active at a time
                        int usrLoginSession = Convert.ToInt16(dtUserInfo.Rows[0]["UST_NoSession"].ToString());
                        if (usrLoginSession >= intMaxLogonSession)
                        {
                            Session["s_UserIsLogin"] = true;
                            Session["s_LoginMessage"] = Resources.Resource.MsgMaxLogonSession;
                        }
                        else 
                        {
                            Session["s_UserIsLogin"] = false;
                        }
                    }
                    else 
                    {
                        //Check user login retry count
                        if (usrRetryCount >= intLoginTry)
                        {
                            UpdateUserLockedAccount();
                            Session["s_LoginMessage"] = Resources.Resource.MsgMaxLogonTry;
                            isValidUser = false;
                            return false;

                        }
                        else
                        {
                            //User existed but invalid password
                            UpdateRetryCountAndLastLogin(usrRetryCount + 1);
                            Session["s_LoginMessage"] = Resources.Resource.MsgInvalidUserID;
                            isValidUser = false;
                        }
                    }
                }
                else
                {
                    //User not existed
                    Session["s_LoginMessage"] = Resources.Resource.MsgInvalidUserID;
                    isValidUser = false;
                }              

            }
            catch (Exception ex) 
            {
                Session["s_UserID"] = "";

                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Login;
                log.ClientCode = clientCode;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                isValidUser = false;            
            }

            return isValidUser;

        }

        private bool isValidPassword(DataTable dtUserInfo) 
        {
            bool isValid = false;
            int pwdPos = 0;
            string pwdDecrypt = string.Empty;

            try
            {
                pwdPos = Convert.ToInt16(dtUserInfo.Rows[0]["UST_PwdPos"].ToString());

                switch (pwdPos)
                {
                    case 0:
                        pwdDecrypt = UCrypt.Decrypt(dtUserInfo.Rows[0]["UST_Pwd_0"].ToString());
                        break;
                    case 1:
                        pwdDecrypt = UCrypt.Decrypt(dtUserInfo.Rows[0]["UST_Pwd_1"].ToString());
                        break;
                    case 2:
                        pwdDecrypt = UCrypt.Decrypt(dtUserInfo.Rows[0]["UST_Pwd_2"].ToString());
                        break;
                    case 3:
                        pwdDecrypt = UCrypt.Decrypt(dtUserInfo.Rows[0]["UST_Pwd_3"].ToString());
                        break;
                    case 4:
                        pwdDecrypt = UCrypt.Decrypt(dtUserInfo.Rows[0]["UST_Pwd_4"].ToString());
                        break;
                    case 5:
                        pwdDecrypt = UCrypt.Decrypt(dtUserInfo.Rows[0]["UST_Pwd_5"].ToString());
                        break;
                }

                isValid = (pwdDecrypt.Equals(txtpassword.Text.Trim()));

            }
            catch (Exception ex) 
            {
                Session["s_UserID"] = "";

                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Login;
                log.ClientCode = clientCode;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                isValid = false;
            }

            return isValid;
        }

        private DataTable GetUserInfoWithUserID(string loginUsrID)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resources.Resource.stmtGetUserInfo;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input,loginUsrID)
                };


                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);

                return dtDB;

            }
            catch (Exception ex)
            {
                Session["s_UserID"] = "";

                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Login;
                log.ClientCode = clientCode;
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

        private void UpdateUserLockedAccount() 
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = string.Empty;
                stmt = Resources.Resource.UpdateLockedAccount;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.Int16, 0, "@Locked", ParameterDirection.Input, 1),
                    dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input, userID)
                    };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);
            }
            catch (Exception ex)
            {
                Session["s_UserID"] = "";

                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Login;
                log.ClientCode = clientCode;
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

        private void UpdateRetryCountAndLastLogin(int retryCount) 
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = string.Empty;
                stmt = Resources.Resource.UpdateLastLogin;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.Int16, 0, "@Count", ParameterDirection.Input, retryCount),
                    dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input, userID)
                    };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);
            }
            catch (Exception ex)
            {
                Session["s_UserID"] = "";

                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Login;
                log.ClientCode = clientCode;
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

        private void UpdateUserSession(string userId)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = string.Empty;
                stmt = Resources.Resource.UpdateUserSession;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.Int16, 0, "@UserSession", ParameterDirection.Input, 1),
                    dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input, userId)
                    };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);
            }
            catch (Exception ex)
            {
                Session["s_UserID"] = "";

                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Login;
                log.ClientCode = clientCode;
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
        #endregion
    }
}