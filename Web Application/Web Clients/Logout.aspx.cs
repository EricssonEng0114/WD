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


namespace UBPCWeb
{
    public partial class Logout : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string clientCode = "OCBC";

        protected void Page_Load(object sender, EventArgs e)
        {
            //Logout success then redirect to login page
            string userID = Page.RouteData.Values["UserId"].ToString();
            string clientCode = Page.RouteData.Values["ClientCode"].ToString().Equals("C") ? "" : Page.RouteData.Values["ClientCode"].ToString();
            string siteCode = Page.RouteData.Values["SiteCode"].ToString().Equals("S") ? "" : Page.RouteData.Values["SiteCode"].ToString();
            string isTimeoutExpired = Page.RouteData.Values["expired"].ToString();
            
            bool isExpired = false;
            isExpired = string.IsNullOrEmpty(isTimeoutExpired) ? false : isTimeoutExpired.Equals("1");
            //resrt

            //update user session
            ResetInUseRejectedTransaction(userID, clientCode, siteCode);
            UpdateUserSession(userID);

            //Reset user id
            Session["s_UserID"] = string.Empty;


            if (!isExpired)
            {
                Response.Redirect("/", false);
                Context.ApplicationInstance.CompleteRequest();
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
                    dbHelperObj.CreateParameter(DbType.Int16, 0, "@UserSession", ParameterDirection.Input, 0),
                    dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input, userId)
                    };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);
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

        private void ResetInUseRejectedTransaction(string usrID, string selClientCode, string selSiteCode) 
        {
            try
            {            
                DBConnectionInfo dbConn = new DBConnectionInfo();
                dbConn.webConnStr = webDBConnStr;
                string clientCode = string.Empty;
                string site = "KL1";

                if (Session["s_CurSelectedClientRejDec"] == null)
                {
                    clientCode = selClientCode;
                }
                else 
                {
                    clientCode = !string.IsNullOrEmpty(Session["s_CurSelectedClientRejDec"].ToString())
                                 ?Session["s_CurSelectedClientRejDec"].ToString(): selClientCode;
                }

                if (Session["s_CurSelectedSiteRejDec"] == null)
                {
                    site = selSiteCode;
                }
                else
                {
                    site = !string.IsNullOrEmpty(Session["s_CurSelectedSiteRejDec"].ToString())
                                 ? Session["s_CurSelectedSiteRejDec"].ToString() : selSiteCode;
                }

                string selClientDBConnStr = dbConn.GetSiteConnectionString(true, clientCode, site);

                if (!string.IsNullOrEmpty(selClientDBConnStr))
                {
                    UBPCWeb.Modules.BatchMaintenance.CommonFunction.ResetTransactionUponLogout(selClientDBConnStr, usrID, clientCode);
                }                
            }
            catch(Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Logout;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                Session["s_UserID"] = string.Empty;

                Response.Redirect("/", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            
        }
    }
}