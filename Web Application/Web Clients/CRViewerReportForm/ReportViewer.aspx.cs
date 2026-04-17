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

namespace UBPCWeb.CRViewerReportForm
{
    public partial class ReportViewer : System.Web.UI.Page
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
                string strReportCode = Request.Form["rptCode"] == null ? Session["CurRptSelectedRptCode"].ToString().Trim() : Request.Form["rptCode"].ToString().Trim();

                //string business date
                string strBusDate = Session["CurRptSelectedBusdate"].ToString();

                //string worksource ID
                string strWrkID = Session["CurRptSelectedWS"].ToString(); 
                                
                //string worksource Name
                string strWrkName = string.Empty;
                if (strWrkID.Equals("0"))
                {
                    strWrkName = "ALL";
                }
                else
                {
                    strWrkName = strWrkID;
                    strWrkID = strWrkID.Substring(strWrkID.Length - 4, 3);
                }

                string strClientCode = Request.Form["clientCode"] == null ? Session["CurRptSelectedRptClient"].ToString().Trim() : Request.Form["clientCode"].ToString().Trim();

                //Retrieve Report  Connection String            
                bool isAllSite = false;
                if (strReportCode.Equals("WEB01")) 
                {
                    isAllSite = true;
                }
                DataTable dtRpt = UBPCWeb.Modules.Reports.CommonFunction.GetCRReportConnectionInfo(strReportCode, strClientCode);
                string rptServer = isAllSite? dtRpt.Rows[0]["RPT_WebServerName"].ToString():dtRpt.Rows[0]["RPT_ServerName"].ToString();
                string rptDBName = isAllSite ? dtRpt.Rows[0]["RPT_WebDBName"].ToString() : dtRpt.Rows[0]["RPT_DBName"].ToString();
                string strReportPath = dtRpt.Rows[0]["RPT_ReportSrc"].ToString();
                string strReportName = dtRpt.Rows[0]["RPT_ReportName"].ToString();
                string strReportDesc = dtRpt.Rows[0]["RPT_ReportDesc"].ToString();
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

                    //Retrieve Virtual Directories Folder name from Param
                    //Get par value 2
                    string paramRptVirDir = UBPC.Web.Common.Parameters.GetParamValue(webDBConnStr, "ReportPath",false);

                    string strRptPath = string.Empty;
                    //ReportFiles/<Client Code>
                    string completePath = Path.Combine(paramRptVirDir + "//" + strClientCode, strReportName); // Server.MapPath("~/") + "ReportFiles//" + strReportName;
                    strRptPath = Server.MapPath("/" + completePath);

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
                    
                    //Using Stored Proc
                    rd.VerifyDatabase();

                    #region Loading Stored Procedure Parameter
                    //if sp run at UVRPS Web - data from all site
                    if (isAllSite)
                    {
                        DateTime busdate = Convert.ToDateTime(strBusDate);
                        string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');

                        rd.SetParameterValue("@CurBusdateStr", sqlDateTime);
                        rd.SetParameterValue("@ClientBank", strClientCode);
                        rd.SetParameterValue("@ForUnisysOps", Convert.ToBoolean(Session["s_UserForUnisys"].ToString()));


                        ////Subreport that using SP
                        //if (rd.Subreports.Count > 0)
                        //{
                        //    for (int i = 0; i < rd.Subreports.Count; i++)
                        //    {
                        //        string subReportName = rd.Subreports[i].Name;
                        //        rd.SetParameterValue("@CurBusDate", Convert.ToDateTime(strBusDate), subReportName);
                        //    }
                        //}
                    }

                    #endregion

                    #region Set Parameter Fields if existed
                    ParameterFieldDefinitions crParameterdef = rd.DataDefinition.ParameterFields;
                    if (crParameterdef.Count > 0)
                    {
                        rd.SetParameterValue("@BusDate", Convert.ToDateTime(strBusDate));
                        rd.SetParameterValue("@ClientName", strClientCode);
                        rd.SetParameterValue("@WS_ID", strWrkID);
                        rd.SetParameterValue("@WS_Name", strWrkName);

                        //rd.PrintOptions.PrinterName = "\\ ";
                        //rd.PrintToPrinter(1, true, 1, 1);

                       // rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, true, strReportCode);

                        //Set Report Source
                        CrystalReportViewer1.ReportSource = rd;
                        CrystalReportViewer1.ToolPanelView = ToolPanelViewType.None;

                        


                    }

                    #endregion



                }
                else
                {
                    Response.Write("<H2>ERROR: No Report found</H2>");
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.Reports;
                    log.Severity = LogEventType.Error;
                    log.ClientCode = strClientCode == null ? "" : strClientCode;
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
            if (Request.Form["rptCode"] != null)
            {
                Session["CurRptSelectedRptCode"] = Request.Form["rptCode"].ToString().Trim();
                Session["CurRptSelectedRptClient"] = Request.Form["clientCode"].ToString().Trim();
            }

            CrystalReportViewer1.ID = Request.Form["rptCode"] == null ? Session["CurRptSelectedRptCode"].ToString().Trim() : Request.Form["rptCode"].ToString().Trim();

            LoadReport();

            customizeToolbar();


        }

        private void customizeToolbar()
        {

            //Control ts = CrystalReportViewer1.Controls[1];

            //if (ts.ToString().Contains("ViewerToolbar"))
            //{

            //    //ImageButton btnPrintDefault = new ImageButton();

            //    //btnPrintDefault.ID = "btnPrintDfault";

            //    //btnPrintDefault.ImageUrl = "images/pdf.png";

            //    //btnPrintDefault.ToolTip = "Print Default to ...";

            //    //btnPrintDefault.Click += btnDefaultPrint_Click;

            //    //ts.Controls.Add(btnPrintDefault);

            //}

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