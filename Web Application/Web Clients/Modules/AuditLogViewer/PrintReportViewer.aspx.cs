using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Configuration;
using Microsoft.VisualBasic;
using System.IO;
using System.Data;
using CrystalDecisions.Web;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net;
using UBPCWeb.Modules.Reports;
using UBPC.Web.Common;

namespace UBPCWeb.Modules.AuditLogViewer
{
    public partial class PrintReportViewer : System.Web.UI.Page
    {
        ReportDocument rd;//strReportPath
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();

        protected void Page_Load(object sender, EventArgs e)
        {
            //do nothing
        }

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
                string strBusDate = Request.QueryString["strDT"].ToString().Trim();// txtDateTime.Text.Trim();

                //string severity
                string strSeverity = Request.QueryString["sev"].ToString().Trim(); //ddlSeverity.SelectedValue.ToString().Trim();

                //string caller
                string strCaller = Request.QueryString["caller"].ToString().Trim(); //ddlCaller.SelectedValue.ToString().Trim();

                //string client code
                string strClientCode = Request.QueryString["client"].ToString().Trim(); //ddlClient.SelectedValue.ToString().Trim();

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

                    PrintCrystalReportViewer1.ReportSource = rd;
                    PrintCrystalReportViewer1.ToolPanelView = ToolPanelViewType.None;

                    #endregion

                    //Log if Report Exported
                    LogEntry rptlog = new LogEntry();
                    rptlog.Caller = LogCallerID.AuditLogViewer;
                    rptlog.Severity = LogEventType.Information;
                    rptlog.UserName = Session["s_UserID"].ToString();
                    rptlog.Data = string.Format("Selected Date={0}|Severity={1}|Caller={2}|Client={3}", busdateFolderName, strSeverity, strCaller, strClientCode);

                    rptlog.Message = "Audit Log Report Ready to Print";
                    rptlog.Write();

                }
                else
                {
                    Response.Write("<H2>ERROR: No Report found</H2>");
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.Reports;
                    log.Severity = LogEventType.Error;

                    log.Message = "Failed to generate Report: Missing Report";
                    log.Write();

                }
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception ex)
            {               
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Reports;
                log.Severity = LogEventType.Error;
                log.Exception = ex;
                log.Message = "Failed Generate Report" + ex.Message;
                log.Write();

                Response.Write("<H2>ERROR:Failed to Load Report File</H2>");

            }
        }


        private String GetTempDirectory(string tempDir)
        {
            String tempDirectory = tempDir;
            if (Directory.Exists(tempDirectory) == false)
            {//temporary directory doesnot exist, create directory
                Directory.CreateDirectory(tempDirectory);
            }
            return tempDirectory;
        }


        protected void Page_Init(object sender, EventArgs e)
        {
            PrintCrystalReportViewer1.ID = Request.QueryString["ReportCode"];
            LoadReport();
        }       

        protected void btnDefaultPrint_Click(object sender, ImageClickEventArgs e) 
        {
            rd.PrintToPrinter(1, true, 1, 1);
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
    }
}