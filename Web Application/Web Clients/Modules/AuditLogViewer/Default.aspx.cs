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
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.IO;

namespace UBPCWeb.Modules.AuditLogViewer
{
    public partial class Default : System.Web.UI.Page
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private bool isPageValid = false;
        private bool isUnisysUsr = false;
        private string clientList = string.Empty;

        private ReportDocument rd;//strReportPath


        protected void Page_Load(object sender, EventArgs e)
        {           
            userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString();
            userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString();
            isUnisysUsr = HttpContext.Current.Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString());
            //Client drop down should filter by user assigned client list
            clientList = Session["s_UserClients"] == null ? string.Empty : Session["s_UserClients"].ToString();

            if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
            {
                PageValidatorResult validatorResult;

                //For password chnge, validate password by encryption class
                validatorResult = PageValidator.Validate(webDBConnStr, userGroup, "AuditLogViewer");
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

            if (isPageValid & !Page.IsPostBack) 
            {
                LoadControl();
            }
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            if (rd != null)
            {
                rd.Close();
                rd.Dispose();
                //GC.Collect();
            }
        }


        private void LoadControl() 
        {
            //Edited Shinyi on PE-ALL-Web-2019-002 Ops Support request to save or print audit log for error investigation
            lnkSave.Visible = isUnisysUsr;
            lnkPrint.Visible = isUnisysUsr;

            //Load Client Drop Down - If unisys admin, show all client available, else show only filtered list
            if (Convert.ToBoolean(Convert.ToBoolean(Session["s_UserForUnisys"].ToString()).Equals(true)) && userGroup.Equals("9"))
            {
                string[] cltArr = clientList.Split(',');
                this.ddlClient.DataSource =  CommonFunction.GetClientInfoList(true);
                this.ddlClient.DataTextField = "CLT_ClientCode";
                this.ddlClient.DataValueField = "CLT_ClientCode";
                this.ddlClient.DataBind();
                this.ddlClient.SelectedIndex = 0;
            }
            else
            {
                int selectedIdx = this.ddlClient.SelectedIndex;

                string clientList = HttpContext.Current.Session["s_UserClients"] == null ? string.Empty : HttpContext.Current.Session["s_UserClients"].ToString();
                string[] cltArr = clientList.Split(',');
                List<ListItem> cltitems = new List<ListItem>();
                foreach (string clientCode in cltArr) cltitems.Add(new ListItem(clientCode, clientCode));
                this.ddlClient.DataSource = from i in cltitems select new ListItem() { Text = i.Text, Value = i.Value };
                this.ddlClient.DataTextField = "Text";
                this.ddlClient.DataValueField = "Value";
                this.ddlClient.DataBind();

                this.ddlClient.SelectedIndex = selectedIdx < 0 ? 0 : selectedIdx;
            }

            //Load Caller Drop down
            List<ListItem> callerItems = new List<ListItem>();
            foreach (string caller in (Enum.GetNames(typeof(UBPC.Web.Common.LogCallerID)).ToList())) 
                callerItems.Add(new ListItem(caller, caller));

            this.ddlCaller.DataSource = from i in callerItems select new ListItem() { Text = i.Text, Value = i.Value };
            this.ddlCaller.DataTextField = "Text";
            this.ddlCaller.DataValueField = "Value";
            this.ddlCaller.DataBind();

            //Load Severity Drop down
            List<ListItem> sevItems = new List<ListItem>();
            sevItems.Add(new ListItem("ALL", "ALL"));
            sevItems.Add(new ListItem("Critical", "Critical"));
            sevItems.Add(new ListItem("Error", "Error"));
            sevItems.Add(new ListItem("Information", "Information"));

            this.ddlSeverity.DataSource = from i in sevItems select new ListItem() { Text = i.Text, Value = i.Value };
            this.ddlSeverity.DataTextField = "Text";
            this.ddlSeverity.DataValueField = "Value";
            this.ddlSeverity.DataBind();

            //Log always show current date
            txtDateTime.Text = DateTime.Now.ToShortDateString();
        }

        //Edited Shinyi on PE-ALL-Web-2019-002 Ops Support request to save or print audit log for error investigation
        private void LoadReport()
        {
            try
            {
                bool isValid = true;

                //Server root path
                String rootPath = Server.MapPath("~/");

                //Report code
                string strReportCode = "AUD02";

                //string business date
                string strBusDate = txtDateTime.Text.Trim();

                //string severity
                string strSeverity = ddlSeverity.SelectedValue.ToString().Trim();

                //string caller
                string strCaller = ddlCaller.SelectedValue.ToString().Trim();

                //string client code
                string strClientCode = ddlClient.SelectedValue.ToString().Trim();

                //Retrieve TBL_Report  Connection String           
                DataTable dtRpt = UBPCWeb.Modules.AuditLogViewer.CommonFunction.GetAuditReportConnectionInfo(strReportCode);

                string rptServer = dtRpt.Rows[0]["RPT_WebServerName"].ToString();
                string rptDBName = dtRpt.Rows[0]["RPT_WebDBName"].ToString();
                string strReportPath = dtRpt.Rows[0]["RPT_ReportSrc"].ToString();
                string strReportName = dtRpt.Rows[0]["RPT_ReportName"].ToString();
                string strReportDesc = dtRpt.Rows[0]["RPT_ReportDesc"].ToString();
                //0 = Default meaning no need export, P= export to PDF , E = Export to Excel
                string strReportType = dtRpt.Rows[0]["RPT_ReportType"].ToString();

                this.Title = string.Format("{0} {1}", strReportCode, strReportDesc); ;//PageTitle

                if (string.IsNullOrEmpty(strReportName)) // Checking is Report name provided or not
                {
                    isValid = false;
                }

                //For view report, export and print report
                if (isValid) // If Report Name provided then do other operation
                {
                    rd = new ReportDocument();//strReportPath

                    #region Retrieve Report File from Report path

                    //Retrieve Virtual Directories Folder name from Param
                    //Get par value 2
                    string paramRptVirDir = UBPC.Web.Common.Parameters.GetParamValue(webDBConnStr, "ReportPath", false);

                    string strRptPath = string.Empty;
                    //ReportFiles/<Client Code>
                    string completePath = Path.Combine(paramRptVirDir + "//ALL", strReportName);
                    strRptPath = Server.MapPath("/" + completePath);

                    #endregion

                    #region Retrieve Exported Path

                    //Get par value 2 = virtual dir folder name sit in wwwroot
                    string paramRptExpVirDir = UBPC.Web.Common.Parameters.GetParamValue(webDBConnStr, "AuditLogReportPath", false);
                    string strRptExpPath = string.Empty;
                    //ReportExportedPath/yyyymmdd/<Client Code>
                    DateTime selectedDate = Convert.ToDateTime(strBusDate);
                    string busdateFolderName = selectedDate.Year.ToString() + selectedDate.Month.ToString().PadLeft(2, '0') + selectedDate.Day.ToString().PadLeft(2, '0');

                    string completeExpPath = Path.Combine(paramRptExpVirDir + "//" + busdateFolderName, Session["s_UserID"].ToString());
                    strRptExpPath = Server.MapPath("/" + completeExpPath);

                    //AUD02.pdf
                    //Use directory info to create directories to regen
                    DirectoryInfo dirInfo = new DirectoryInfo(strRptExpPath);
                    if (!dirInfo.Exists)
                    {
                        dirInfo.Create();
                    }
                   
                   // string fullExportPathRptName = strRptExpPath + "//" + "AUD02.pdf";

                    #endregion

                    //Loading Report
                    rd.Load(strRptPath);

                    //Assign DB Connection information for reports
                    ConnectionInfo connection = new ConnectionInfo();
                    connection.DatabaseName = rptDBName;
                    connection.ServerName = rptServer;
                    connection.IntegratedSecurity = true;

                    //assign the connection to all tables in the main report
                    #region Assign Report Connection to all table in main report
                    foreach (CrystalDecisions.CrystalReports.Engine.Table rptTable in rd.Database.Tables)
                    {
                        // Cache the logon info block
                        TableLogOnInfo logOnInfo = rptTable.LogOnInfo;
                        // Set the connection
                        logOnInfo.ConnectionInfo = connection;
                        // Apply the connection to the table!
                        rptTable.ApplyLogOnInfo(logOnInfo);
                    }
                    #endregion

                    #region Assign Report Connection to all table in sub report if subreport existed
                    /* If there are any subreports - you will need to check all sections in the report 
               and then assign the connection info again to all tables in each subreport. */

                    foreach (Section section in rd.ReportDefinition.Sections)
                    {
                        // In each section we need to loop through all the reporting objects
                        foreach (ReportObject reportObject in section.ReportObjects)
                        {
                            if (reportObject.Kind == ReportObjectKind.SubreportObject)
                            {
                                SubreportObject subreport = (SubreportObject)reportObject;
                                ReportDocument subDocument = subreport.OpenSubreport(subreport.SubreportName);

                                foreach (CrystalDecisions.CrystalReports.Engine.Table table in subDocument.Database.Tables)
                                {
                                    // Cache the logon info block
                                    TableLogOnInfo logOnInfo = table.LogOnInfo;
                                    // Set the connection
                                    logOnInfo.ConnectionInfo = connection;
                                    // Apply the connection to the table!
                                    table.ApplyLogOnInfo(logOnInfo);
                                }
                            }
                        }
                    }
                    #endregion


                    // set the Report Title
                    rd.SummaryInfo.ReportTitle = string.Format("{0} {1}", strReportCode, strReportDesc);

                    ////Using Stored Proc
                  //  rd.VerifyDatabase();

                    #region Set Parameter Fields if existed
                    ParameterFieldDefinitions crParameterdef = rd.DataDefinition.ParameterFields;
                    if (crParameterdef.Count > 0)
                    {
                        rd.SetParameterValue("@BusDate", Convert.ToDateTime(strBusDate));
                        rd.SetParameterValue("@ClientName", strClientCode);
                    }

                    #region Loading Stored Procedure Parameter
                    DateTime busdate = Convert.ToDateTime(strBusDate);
                    string sqlStartDateTime = strBusDate + " 00:00:00";
                    string sqlEndDateTime = strBusDate + " 23:59:59";

                    rd.SetParameterValue("@StartDateTime", sqlStartDateTime);
                    rd.SetParameterValue("@EndDateTime", sqlEndDateTime);
                    rd.SetParameterValue("@Severity", strSeverity);
                    rd.SetParameterValue("@Caller", strCaller);
                    rd.SetParameterValue("@ClientCode", strClientCode);

                    #endregion
                    #endregion

                    
                    ExportOptions rptExportOption;
                    DiskFileDestinationOptions rptFileDestOption = new DiskFileDestinationOptions();
                    PdfRtfWordFormatOptions rptFormatOption = new PdfRtfWordFormatOptions();
                    ExcelFormatOptions rptExcelFormatOption = new ExcelFormatOptions();

                    if (strReportType.Equals("P"))
                    {
                        rptFileDestOption.DiskFileName = strRptExpPath + "//" + "AUD02.pdf";

                        rptExportOption = rd.ExportOptions;
                        {
                            rptExportOption.ExportDestinationType = ExportDestinationType.DiskFile;
                            rptExportOption.ExportFormatType = ExportFormatType.PortableDocFormat;
                            rptExportOption.ExportDestinationOptions = rptFileDestOption;
                            rptExportOption.ExportFormatOptions = rptFormatOption;
                        }
                    }
                    else 
                    {
                        rptFileDestOption.DiskFileName = strRptExpPath + "//" + "AUD02.xls";

                        rptExportOption = rd.ExportOptions;
                        {
                            rptExportOption.ExportDestinationType = ExportDestinationType.DiskFile;
                            rptExportOption.ExportFormatType = ExportFormatType.Excel;
                            rptExportOption.ExportDestinationOptions = rptFileDestOption;
                            rptExportOption.ExportFormatOptions = rptExcelFormatOption;
                        }
                    }

                    //if (toExport)
                    //{
                        rd.Export(rptExportOption);
                    //Only working to print to web server default printers
                    //}
                    //else
                    //{
                    //    rd.PrintToPrinter(1, true, 0, 0);
                    //}

                    //Log if Report Exported
                    LogEntry rptlog = new LogEntry();
                    rptlog.Caller = LogCallerID.AuditLogViewer;
                    rptlog.Severity = LogEventType.Information;
                    rptlog.UserName = Session["s_UserID"].ToString();
                    rptlog.Data = string.Format("Selected Date={0}|Severity={1}|Caller={2}|Client={3}", busdateFolderName, strSeverity, strCaller, strClientCode);

                    rptlog.Message = "Audit Log Report Exported.";
                    rptlog.Write();
                }
                else
                {
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.AuditLogViewer;
                    log.Severity = LogEventType.Error;

                    log.Message = "Failed to generate Audit Report: Missing Report";
                    log.Write();
                }
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.AuditLogViewer;
                log.Severity = LogEventType.Error;
                log.Exception = ex;
                log.Message = "Failed to Generate Audit Report" + ex.Message;
                log.Write();
            }
        }


        protected void btnSearch_Click(object sender, EventArgs e)        
        {
            string dateTime = string.Empty;
            string severity = string.Empty;
            string caller = string.Empty;
            string client = string.Empty;

            dateTime = txtDateTime.Text.Trim();
            severity = ddlSeverity.SelectedValue.ToString().Trim();
            caller = ddlCaller.SelectedValue.ToString().Trim();
            client = ddlClient.SelectedValue.ToString().Trim();

            if (string.IsNullOrEmpty(dateTime.Trim())) 
            {
                Logger.Write(false, LogCallerID.AuditLogViewer, "", "Validate Search", "Date is Empty", LogEventType.Error, userID);
            }
            else{

                //date, severity, caller,client
                ClientScript.RegisterStartupScript(this.GetType(), "LoadGrid",
                    string.Format("initAuditLogDataTable('{0}','{1}','{2}','{3}');", dateTime, severity, caller, client), true);

                //date, severity, caller,client
                ClientScript.RegisterStartupScript(this.GetType(), "ConfigureGrid", "configureTable()", true);

            }           
        }
      
        protected void lnkSave_Click(object sender, EventArgs e)
        {
            LoadReport();

            //Refresh grid
            btnSearch_Click(sender, e);
        }

        protected void lnkPrint_Click(object sender, EventArgs e)
        {
           // LoadReport(false);
            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "loadCR", "openCR();", true);

            //Refresh grid
            btnSearch_Click(sender, e);
        }

        [WebMethod]
        public static string getReportUrl(string busdate, string clientCode, string severity, string caller)
        {
            string url = string.Empty;
            url = "/Modules/AuditLogViewer/PrintReportViewer.aspx?strDT=" + busdate + "&sev=" + severity + "&caller=" + caller + "&client=" + clientCode;

            string fullUrl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + url;
            return fullUrl;
        }
    }
}