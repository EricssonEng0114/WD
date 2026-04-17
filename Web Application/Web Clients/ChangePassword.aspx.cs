using System;
using System.Configuration;
using System.Data;
using UBPC.Encryption;
using UBPC.Web.Common;

namespace UBPCWeb
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string clientCode = string.Empty; //"OCBC';

        protected void Page_Load(object sender, EventArgs e)
        {
            userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString();
            clientCode = Session["s_UserClients"] == null ? string.Empty :
                            Session["s_UserClients"].ToString().Contains(",") ? "" :
                            Session["s_UserClients"].ToString().Trim();
        }

        protected void btnChange_Click(object sender, EventArgs e)
        {
            string strMessage = string.Empty;
            bool isValid = true;
            int intPassPos = 0;

            //If password no input
            if (txtNewPassword.Text.Trim().Length <= 0 || txtConfirmPassword.Text.Trim().Length <= 0)
            {
                isValid = false;
                strMessage = Resources.Resource.MsgAllPasswordRequired;
            }

            if (isValid)
            {
                //If password and confirm password not same value
                if (txtNewPassword.Text.Trim().Equals(txtConfirmPassword.Text.Trim()))
                {
                    PassValidatorResult validatorResult;

                    //For password chnge, validate password by encryption class
                    validatorResult = PassValidator.Validate(txtNewPassword.Text, false, GetUserPasswordHistory(ref intPassPos, Session["s_UserID"].ToString()));

                    isValid = validatorResult.Valid;

                    if (!isValid)
                    {
                        strMessage = validatorResult.ReturnMessage;
                    }
                    else
                    {
                        isValid = true;
                    }
                }
                else
                {
                    isValid = false;
                    strMessage = Resources.Resource.MsgPasswordInvalid;
                }
            }


            if (isValid)
            {
                //check if password is at position 5 - last position, reset position back to 0
                intPassPos = intPassPos.Equals(5) ? 0 : intPassPos + 1;
                UpdateUserPassword(intPassPos);

                //Reset force password change back to false
                Session["s_ForcePwdChg"] = false;
                Session["s_UserID"] = "";//RESET user id when back to login screen
                //redirect back to login page                
                Response.Redirect("/", false);
                Context.ApplicationInstance.CompleteRequest();

            }
            else
            {
                Session["s_ChangePasswordSuccessMsg"] = string.Empty;

                if (!string.IsNullOrEmpty(strMessage))
                {
                    Session["s_ChangePasswordErrMsg"] = strMessage;
                    lblErrMessage.Visible = true;
                    lblErrMessage.Text = strMessage;
                }
            }
        }

        private string[] GetUserPasswordHistory(ref int pwPosition, string userID)
        {
            //stmtGetUserLogin
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;
            string strPassword = string.Empty;
            string[] strPwdHistoryList = null;
            int intPassPos = 0;

            try
            {
                stmt = Resources.Resource.stmtGetUserLogin;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input,userID)
                };


                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);

                if (dtDB.Rows.Count > 0)
                {
                    DataRow dr = dtDB.Rows[0];
                    intPassPos = Convert.ToInt16(dr["UST_PwdPos"].ToString());

                    if (!string.IsNullOrEmpty(dr["UST_Pwd_0"].ToString().Trim()))
                        strPassword += UCrypt.Decrypt(dr["UST_Pwd_0"].ToString().Trim()) + ",";

                    if (!string.IsNullOrEmpty(dr["UST_Pwd_1"].ToString().Trim()))
                        strPassword += UCrypt.Decrypt(dr["UST_Pwd_1"].ToString().Trim()) + ",";

                    if (!string.IsNullOrEmpty(dr["UST_Pwd_2"].ToString().Trim()))
                        strPassword += UCrypt.Decrypt(dr["UST_Pwd_2"].ToString().Trim()) + ",";

                    if (!string.IsNullOrEmpty(dr["UST_Pwd_3"].ToString().Trim()))
                        strPassword += UCrypt.Decrypt(dr["UST_Pwd_3"].ToString().Trim()) + ",";

                    if (!string.IsNullOrEmpty(dr["UST_Pwd_4"].ToString().Trim()))
                        strPassword += UCrypt.Decrypt(dr["UST_Pwd_4"].ToString().Trim()) + ",";

                    if (!string.IsNullOrEmpty(dr["UST_Pwd_5"].ToString().Trim()))
                        strPassword += UCrypt.Decrypt(dr["UST_Pwd_5"].ToString().Trim()) + ",";

                    //Remove last comma
                    strPassword = strPassword.Remove(strPassword.Length - 1);
                }

                pwPosition = intPassPos;
                strPwdHistoryList = strPassword.Split(',');
            }
            catch (Exception ex)
            {
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

            return strPwdHistoryList;
        }

        private void UpdateUserPassword(int passwordPos)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = string.Empty;
                stmt = String.Format(Resources.Resource.UpdateUserPassword, passwordPos);

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input, userID),
                    dbHelperObj.CreateParameter(DbType.String, 100, "@Password", ParameterDirection.Input,  UCrypt.Encrypt(txtNewPassword.Text)),
                    dbHelperObj.CreateParameter(DbType.Int16, 0, "@PassPos", ParameterDirection.Input,  passwordPos)
                };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);

                Session["s_ChangePasswordSuccessMsg"] = Resources.Resource.MsgUserChangedPassword;

                //Log success change password to audit
                Logger.Write(false, LogCallerID.Login, clientCode, "Reset Password", String.Format(Resources.Resource.MsgUserChangedPasswordLog, userID), LogEventType.Information, userID);
            }
            catch (Exception ex)
            {
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
    }
}