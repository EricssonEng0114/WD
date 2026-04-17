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
using UBPC.Encryption;
using UBPC.Web.Common;

namespace UBPCWeb.Modules.Profile
{
    public partial class Default : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private string clientCode = "OCBC";

        protected void Page_Load(object sender, EventArgs e)
        {
            userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString();
            userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString();

            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(userGroup))
            {
                //invalid user - back to login page
                Session["s_UserID"] = "";
                Response.Redirect("/Login", false);
                Context.ApplicationInstance.CompleteRequest();

            }
            else 
            {
                PopulateProfileInfo();
            }
        }

        private void PopulateProfileInfo() 
        {
            DataTable dtUsrInfor = GetUserInfoWithUserID(userID);

            if (dtUsrInfor.Rows.Count > 0)
            {
                txtUsrID.Text = HttpUtility.HtmlDecode(dtUsrInfor.Rows[0]["UST_UserID"].ToString());
                txtUsrName.Text = HttpUtility.HtmlDecode(dtUsrInfor.Rows[0]["UST_UserName"].ToString());
                txtUsrGp.Text = dtUsrInfor.Rows[0]["UST_UserGroup"].ToString();
                txtAreaClnt.Value = dtUsrInfor.Rows[0]["UST_Clients"].ToString();

                txtLastPwsChg.Text = dtUsrInfor.Rows[0]["UST_PwdChgLastDate"] != null? Convert.ToDateTime(dtUsrInfor.Rows[0]["UST_PwdChgLastDate"].ToString()).ToShortDateString():string.Empty;
                txtLastLogin.Text = dtUsrInfor.Rows[0]["UST_LastLogin"] != null ? Convert.ToDateTime(dtUsrInfor.Rows[0]["UST_LastLogin"].ToString()).ToShortDateString() : string.Empty;
            }
            else 
            {               
                //Show Invalid User found
                Session["s_LoginMessage"] = Resources.Resource.MsgInvalidUserID;
                //Redirect to Login page
                Session["s_UserID"] = "";
                Response.Redirect("/Login", false);
                Context.ApplicationInstance.CompleteRequest();

            }
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