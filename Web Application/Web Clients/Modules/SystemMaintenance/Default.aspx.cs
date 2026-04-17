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


namespace UBPCWeb.Modules.SystemMaintenance
{
    public partial class Default : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private bool isPageValid = false;
        private string strReturnMsg = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString();
            userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString();

            if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
            {
                PageValidatorResult validatorResult;

                //For password chnge, validate password by encryption class
                validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "SystemMaintenance");
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
                    string paramName = Request.Form["tableID"] == null ? string.Empty : Request.Form["tableID"];

                    if (!string.IsNullOrEmpty(paramName))
                    {
                        Response.Redirect("/System/" + paramName, false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        //Invalid ID retrieve for edit operation
                        Logger.Write(false, LogCallerID.SystemMaintenance, "", "Update Parameter", "Invalid Parameter Name retrieved for update operation", LogEventType.Error, userID);
                        Response.Redirect("/System", false);
                        Context.ApplicationInstance.CompleteRequest();

                    }
                }

                if (tagOperation.Equals("DEL"))
                {
                    //Perform Update
                    //string paramName = Request.Form["tableID"] == null ? string.Empty : Request.Form["tableID"];
                   // DataTable dtSelectedParam = CommonFunction.mainParamTable.Select(String.Format("TMPID = '{0}'", paramName.Trim())).CopyToDataTable();
                    string deletedParam = Session["MainDTParam"].ToString();

                    if (!string.IsNullOrEmpty(deletedParam))
                        DeleteParameter(deletedParam.Trim());
                    else
                    {
                        //Invalid ID retrieve for delete operation
                        Logger.Write(false, LogCallerID.SystemMaintenance, "", "Delete Parameter", "Invalid Parameter Name retrieved for delete operation", LogEventType.Error, userID);

                    }

                    Response.Redirect("/System", false);
                    Context.ApplicationInstance.CompleteRequest();

                }
            }
        }

        private void LoadControl()
        {
           
            //Initialise Grid
            //busdate, client, branch, category
            ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                string.Format("initSystemDataTable('{0}','{1}','{2}');", txtName.Text.Trim(), txtValue1.Text.Trim(), txtValue2.Text.Trim()), true);

            ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);

        }

        protected void New_Click(object sender, EventArgs e)
        {
            Response.Redirect("/System/NEW", false);
            Context.ApplicationInstance.CompleteRequest();

        }

        private void DeleteParameter(string paramName)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = string.Empty;
                stmt = Resource.stmtSQLDeleteParameter;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 50, "@ParamName", ParameterDirection.Input,paramName)
                };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.SystemMaintenance;
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

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            txtName.Text = string.Empty;
            txtValue1.Text = string.Empty;
            txtValue2.Text = string.Empty;

            ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                string.Format("initSystemDataTable('{0}','{1}','{2}');", txtName.Text.Trim(), txtValue1.Text.Trim(), txtValue2.Text.Trim()), true);

            ClientScript.RegisterStartupScript(this.GetType(), "Configure", "configureTable()", true);
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initSystemDataTable('{0}','{1}','{2}');", txtName.Text.Trim(), txtValue1.Text.Trim(), txtValue2.Text.Trim()), true);

                ClientScript.RegisterStartupScript(this.GetType(), "Configure", "configureTable()", true);
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.SystemMaintenance;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }
        }
    
    }
}