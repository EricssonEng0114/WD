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

namespace UBPCWeb.Modules.AccessMaintenance
{
    public partial class Default : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private bool isPageValid = false;
        private string strReturnMsg = string.Empty;
        public List<AccessModel> acesssList = new List<AccessModel>();
        public Dictionary<String, String> listChkboxAction = new Dictionary<String, String>();
        public bool isForUnisys = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString();
                userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString();
                isForUnisys = Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(Session["s_UserForUnisys"].ToString());

                if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
                {
                    PageValidatorResult validatorResult;

                    //For password chnge, validate password by encryption class
                    validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "AccessMaintenance");
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

                if (isPageValid)
                {
                    LoadAccessList();
                }

                if (isPageValid && IsPostBack)
                {
                    int totalUsrGroup = 8;
                    if (isForUnisys) 
                    {
                        totalUsrGroup = 9;
                    }

                    foreach (AccessModel obj in acesssList)
                    {
                        string strUpdatedGroup = string.Empty;
                        string moduleName = obj.AccessCode;
                        string formIDName = "L_" + moduleName + "_";

                        for (int i = 1; i <= totalUsrGroup; i++)
                        {
                            string isGroupChecked = Request.Form[formIDName + i.ToString()] == null ? "0" : "1";
                            strUpdatedGroup += "ACC_Group" + i.ToString() + "=" + isGroupChecked + ",";
                        }

                        strUpdatedGroup = strUpdatedGroup.Remove((strUpdatedGroup.Length - 1), 1); //remove last comma
                        UpdateAccessMaintenance(moduleName, strUpdatedGroup);
                    }

                    Response.Redirect("/Access", false);
                    Context.ApplicationInstance.CompleteRequest();
                }               
            }
            catch (Exception ex) 
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.AccessMaintenance;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                throw ex;
            }
        }

        private void LoadAccessList() 
        {
            if (acesssList.Count > 0) 
            {
                acesssList.Clear();
            } 

            DataTable dtAccessList = CommonFunction.GetAllAccessList();

            foreach (DataRow dr in dtAccessList.Rows) 
            {
                AccessModel obj = new AccessModel();
                obj.AccessCode = dr["ACC_FunctionCode"].ToString();
                obj.AccessName = dr["ACC_FunctionName"].ToString();
                obj.Group1 = Convert.ToBoolean(dr["ACC_Group1"].ToString());
                obj.Group2 = Convert.ToBoolean(dr["ACC_Group2"].ToString());
                obj.Group3 = Convert.ToBoolean(dr["ACC_Group3"].ToString());
                obj.Group4 = Convert.ToBoolean(dr["ACC_Group4"].ToString());
                obj.Group5 = Convert.ToBoolean(dr["ACC_Group5"].ToString());
                obj.Group6 = Convert.ToBoolean(dr["ACC_Group6"].ToString());
                obj.Group7 = Convert.ToBoolean(dr["ACC_Group7"].ToString());
                obj.Group8 = Convert.ToBoolean(dr["ACC_Group8"].ToString());
                obj.Group9 = Convert.ToBoolean(dr["ACC_Group9"].ToString());

                acesssList.Add(obj);
            }
        }

        private void UpdateAccessMaintenance(string moduleName, string strUpdatedGroup)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;
            try
            {
                stmt = String.Format(Resource.stmtUpdateAccess, strUpdatedGroup);

                bool isUnisys = HttpContext.Current.Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString());
                string client = isUnisys ? "USYS" : HttpContext.Current.Session["s_UserClients"].ToString().Trim();
                
                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 100, "@ModuleName", ParameterDirection.Input, moduleName),
                    dbHelperObj.CreateParameter(DbType.String, 4, "@Client", ParameterDirection.Input, client)
                    };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.AccessMaintenance;
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