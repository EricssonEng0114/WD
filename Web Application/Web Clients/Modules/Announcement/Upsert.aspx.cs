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


namespace UBPCWeb.Modules.Announcement
{
    public partial class Upsert : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private string clientList = string.Empty;
        private bool isPageValid = false;
        private string strReturnMsg = string.Empty;
        private bool isEditMode = false;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString().Trim();
                userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString().Trim();
                clientList = Session["s_UserClients"] == null ? string.Empty : Session["s_UserClients"].ToString().Trim();

                if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
                {
                    #region Validate Access Right on this Page
                    PageValidatorResult validatorResult;

                    //For password chnge, validate password by encryption class
                    validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "AnnouncementMaintenance");
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

               string clientCode = ddlClient.SelectedValue ==null 
                    ? (clientList.Contains(",")? "": clientList.Trim().ToString()) 
                    : ddlClient.SelectedValue.Trim().ToString();
            }
            catch (Exception ex) 
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.AnnouncementMaintenance;
                log.ClientCode = ddlClient.SelectedValue == null ? (clientList.Contains(",") ? "" : clientList.Trim().ToString()) : ddlClient.SelectedValue.ToString();
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
            Response.Redirect("/Announcement", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
          string clientCode = ddlClient.SelectedValue == null ? "" : ddlClient.SelectedValue.ToString();

            try
            {
                //Load javascript spinner
                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadSpinner", "loadSpinner()", true);
                //checkSaveData
                if (ValidateInsertUpdate())
                {
                    if (!isEditMode)
                    {
                        InsertAnnouncement();

                        Logger.Write(false, LogCallerID.AnnouncementMaintenance, clientCode, "Insert", "New Announcement Added.", LogEventType.Information, userID);
                    }
                    else
                    {
                        UpdateAnnouncement(Page.RouteData.Values["Id"].ToString().Trim());
                        Logger.Write(false, LogCallerID.AnnouncementMaintenance, clientCode, "Announcement ID =" + Page.RouteData.Values["Id"].ToString().Trim(), "Announcement Updated.", LogEventType.Information, userID);

                    }
                }
                else
                {
                    //Log Failed to update due to validate failed
                    Logger.Write(false, LogCallerID.AnnouncementMaintenance, clientCode, "ValidateInsertUpdate", "Failed to Add/Update Announcement because Validation failed.", LogEventType.Information, userID);
                }                
            }
            catch (Exception ex) 
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.AnnouncementMaintenance;
                log.ClientCode = clientCode;
                log.UserName = userID;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();
            }

            Response.Redirect("/Announcement", false);
            Context.ApplicationInstance.CompleteRequest();

        }

        #region Helpher Methods
        private void LoadControls()
        {
            try
            {
                string opParam = Page.RouteData.Values["Id"].ToString().Trim();
                isEditMode = opParam.Equals("NEW") ? false : true;
                lblTitle.Text = opParam.Equals("NEW") ? "Create New Announcement" : "Update Announcement";

                //Reassign Selected Param VName
                opParam = isEditMode? Session["MainDTParam"].ToString():"NEW";
                #region populate drop down for Announcement Type
                List<ListItem> items = new List<ListItem>();
                items.Add(new ListItem("Information", "info"));
                items.Add(new ListItem("Alert", "alert"));
                items.Add(new ListItem("Success", "success"));
                items.Add(new ListItem("Error", "error"));

                this.ddlType.DataSource = from i in items select new ListItem() { Text = i.Text, Value = i.Value };
                this.ddlType.DataTextField = "Text";
                this.ddlType.DataValueField = "Value";
                this.ddlType.DataBind();

                string [] cltArr = clientList.Split(',');
                List<ListItem> cltitems = new List<ListItem>();
                foreach (string clientCode in cltArr) 
                {
                    cltitems.Add(new ListItem(clientCode, clientCode));
                }

                this.ddlClient.DataSource = from i in cltitems select new ListItem() { Text = i.Text, Value = i.Value };
                this.ddlClient.DataTextField = "Text";
                this.ddlClient.DataValueField = "Value";
                this.ddlClient.DataBind();

                #endregion

                if (!isEditMode)
                {
                    txtTitle.Text = string.Empty;
                    txtDesc.Text = string.Empty;
                    txtDateFrom.Text = string.Empty;
                    txtDateTo.Text = string.Empty;
                    this.ddlType.SelectedValue = "alert";
                }
                else
                {
                    //select from tbl_announcrement to get data
                    //Get User ID to retrieve user info and populate into page
                    DataTable dtSelectedAnnouncement = CommonFunction.GetAnnounrcementInfoByID(opParam.Trim());

                    if (dtSelectedAnnouncement.Rows.Count > 0)
                    {
                        txtTitle.Text = HttpUtility.HtmlDecode(dtSelectedAnnouncement.Rows[0]["ANM_Title"].ToString().Trim());
                        txtDesc.Text = HttpUtility.HtmlDecode(dtSelectedAnnouncement.Rows[0]["ANM_Description"].ToString());
                        txtDateFrom.Text = Convert.ToDateTime(dtSelectedAnnouncement.Rows[0]["ANM_CreatedFrom"].ToString()).ToShortDateString();
                        txtDateTo.Text = Convert.ToDateTime(dtSelectedAnnouncement.Rows[0]["ANM_CreatedTo"].ToString()).ToShortDateString();
                        this.ddlType.SelectedValue = dtSelectedAnnouncement.Rows[0]["ANM_Type"].ToString().Trim();

                    }
                    else
                    {
                        //nabigate back to listing page
                        Response.Redirect("/Announcement", false);
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

                if (txtTitle.Text.Trim().Length <= 0 || txtDesc.Text.Trim().Length <= 0
                    || txtDateTo.Text.Trim().Length <= 0 || txtDateFrom.Text.Trim().Length <= 0)
                {
                    isValid = false;
                }

                if (Convert.ToDateTime(txtDateFrom.Text.Trim()) > Convert.ToDateTime(txtDateFrom.Text.Trim()))
                {
                    isValid = false;
                }
            }
            catch (Exception ex) 
            {
                isValid = false;
                throw ex;
            }

            return isValid;
        }
                    
        private void InsertAnnouncement() 
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = string.Empty;
                stmt = Resource.stmtSQLInsertAnnouncement;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 100, "@Title", ParameterDirection.Input,  txtTitle.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 0, "@Description", ParameterDirection.Input, txtDesc.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 100, "@Type", ParameterDirection.Input, ddlType.SelectedValue.ToString().Trim()),
                    dbHelperObj.CreateParameter(DbType.DateTime, 0, "@CreatedFrom", ParameterDirection.Input, Convert.ToDateTime(txtDateFrom.Text.Trim())),
                    dbHelperObj.CreateParameter(DbType.DateTime, 0, "@CreatedTo", ParameterDirection.Input,  Convert.ToDateTime(txtDateTo.Text.Trim())),
                    dbHelperObj.CreateParameter(DbType.String, 50, "@CreatedBy", ParameterDirection.Input, userID),
                    dbHelperObj.CreateParameter(DbType.String, 10, "@Client", ParameterDirection.Input, ddlClient.SelectedValue.ToString().Trim())

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

        private void UpdateAnnouncement(string announceID) 
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);

            try
            {
                string stmt = string.Empty;
                stmt = Resource.stmtSQLUpdAnnouncement;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 100, "@Title", ParameterDirection.Input,txtTitle.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.String, 0, "@Description", ParameterDirection.Input, txtDesc.Text.Trim()),
                    dbHelperObj.CreateParameter(DbType.Int64,0 , "@ID", ParameterDirection.Input, Convert.ToInt64(announceID)),
                    dbHelperObj.CreateParameter(DbType.String, 100, "@Type", ParameterDirection.Input, ddlType.SelectedValue.ToString().Trim()),
                    dbHelperObj.CreateParameter(DbType.DateTime, 0, "@CreatedFrom", ParameterDirection.Input,  Convert.ToDateTime(txtDateFrom.Text.Trim())),
                    dbHelperObj.CreateParameter(DbType.DateTime, 0, "@CreatedTo", ParameterDirection.Input,  Convert.ToDateTime(txtDateTo.Text.Trim())),
                    dbHelperObj.CreateParameter(DbType.String, 10, "@Client", ParameterDirection.Input,ddlClient.SelectedValue.ToString().Trim())
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
    }
}