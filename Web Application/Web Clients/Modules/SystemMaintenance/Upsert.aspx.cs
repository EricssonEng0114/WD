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
using System.Text.RegularExpressions;
//using System.Web.Security.AntiXss;

namespace UBPCWeb.Modules.SystemMaintenance
{
    public partial class Upsert : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private bool isPageValid = false;
        private string strReturnMsg = string.Empty;
        private bool isEditMode = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString().Trim();
                userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString().Trim(); 

                if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
                {
                    #region Validate Access Right on this Page
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
                    #endregion
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
                    string tagPage = Page.RouteData.Values["Id"].ToString().Trim();

                    isEditMode = !tagPage.Equals("NEW");

                    //First time load - reset control
                    if (!IsPostBack && isPageValid)
                        LoadControls();
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.SystemMaintenance;
                //log.ClientCode = clientCode;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }
        }


        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadSpinner", "loadSpinner()", true);
            Response.Redirect("/System", false);
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
                        InsertParameter();
                    }
                    else
                    {
                        UpdateParameter();
                    }
                }
                else
                {
                    //Log Failed to update due to validate failed
                    Logger.Write(false, LogCallerID.SystemMaintenance, "", "ValidateInsertUpdate", "Failed to Add/Update Parameters because Validation failed.", LogEventType.Information, userID);
                }
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.SystemMaintenance;
                //log.ClientCode = clientCode;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

            Response.Redirect("/System", false);
            Context.ApplicationInstance.CompleteRequest();

        }

        #region Helpher Methods
        private void LoadControls()
        {
            try
            {
                string opParam = Page.RouteData.Values["Id"].ToString().Trim();
                lblTitle.Text = opParam.Equals("NEW") ? "Create New Parameter" : "Update Parameter";

                //Reassign Selected Param VName
                opParam = isEditMode ? Session["MainDTParam"].ToString() : "NEW";

                if (!isEditMode)
                {
                    txtParamName.Text = string.Empty;
                    txtParamName.ReadOnly = false;
                    txtValue1.Text = string.Empty;
                    txtValue2.Text = string.Empty;
                    txtComment.InnerText = string.Empty;
                }
                else
                {
                    txtParamName.ReadOnly = true;
                    //DataTable dtSelectedParam = CommonFunction.mainParamTable.Select(String.Format("TMPID = '{0}'", opParam.Trim())).CopyToDataTable();
                                        
                    DataTable dtSelectedParam = CommonFunction.GetParamInfoByID(opParam.Trim());

                    if (dtSelectedParam.Rows.Count > 0)
                    {
                        txtParamName.Text = dtSelectedParam.Rows[0]["PAR_ParamName"].ToString().Trim();
                        txtValue1.Text = dtSelectedParam.Rows[0]["PAR_Value1"].ToString();
                        txtValue2.Text = dtSelectedParam.Rows[0]["PAR_Value2"].ToString();
                        txtComment.InnerText = dtSelectedParam.Rows[0]["PAR_Comment"].ToString();
                    }
                    else
                    {
                        //nabigate back to listing page
                        Response.Redirect("/System", false);
                        Context.ApplicationInstance.CompleteRequest();

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool ValidateInsertUpdate()
        {
            bool isValid = true;
            try
            {

                if (txtParamName.Text.Trim().Length <= 0)
                {
                    isValid = false;
                }

                if (!txtParamName.ReadOnly)
                {
                    if (!UBPC.Web.Common.CommonFunction.isAlphanumeric(txtParamName.Text.Trim()))
                    {
                        isValid = false;
                    }
                }
            }
            catch (Exception ex)
            {
                isValid = false;
                throw ex;
            }

            return isValid;
        }

        private void InsertParameter()
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = string.Empty;
                stmt = Resource.stmtSQLInsertParameter;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 50, "@ParamName", ParameterDirection.Input, txtParamName.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 100, "@ParValue1", ParameterDirection.Input, txtValue1.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 50, "@ParValue2", ParameterDirection.Input,  txtValue2.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 70, "@Comments", ParameterDirection.Input, txtComment.InnerText.Trim())
                };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                iConn.Close();
            }
        }

        private void UpdateParameter()
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = string.Empty;
                stmt = Resource.stmtSQLUpdParameter;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 50, "@ParamName", ParameterDirection.Input, txtParamName.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 100, "@ParValue1", ParameterDirection.Input, txtValue1.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 50, "@ParValue2", ParameterDirection.Input,  txtValue2.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 70, "@Comments", ParameterDirection.Input, txtComment.InnerText.Trim())
                };

                dbHelperObj.executeNonQuery(iConn, CommandType.Text, stmt, param);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                iConn.Close();
            }
        }

        #endregion

        protected void txtParamName_TextChanged(object sender, EventArgs e)
        {
            //TextBox obj = (TextBox)sender;
            //if (!UBPC.Web.Common.CommonFunction.isAlphanumeric(obj.Text.Trim())) 
            //{
            //    return;
            //}
        }
    }
}