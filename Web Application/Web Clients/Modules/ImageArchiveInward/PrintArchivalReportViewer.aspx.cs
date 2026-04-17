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
using UBPCWeb.Modules.ImageArchiveOutward;
using UBPC.Web.Common;
using UBPCWeb.CRViewerReportForm;
using System.Globalization;
using System.Drawing.Imaging;

namespace UBPCWeb.Modules.ImageArchiveInward
{
    public partial class PrintArchivalReportViewer : System.Web.UI.Page
    {
        ReportDocument rd;//strReportPath
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private SQLDBHelper dbHelperObj = new SQLDBHelper();


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

                //Following are Query String Param 
                //Client Code
                string strClientCode = Request.Form["clientCode"] == null ? Session["CurArchivalSelectedRptClient"].ToString().Trim() : Request.Form["clientCode"].ToString().Trim();

                //Report code
                string strReportCode = Request.Form["rptcode"] == null ? Session["CurArchivalSelectedRptCode"].ToString().Trim() : Request.Form["rptcode"].ToString().Trim();

                //Site code
                string strSiteCode = Request.Form["sitecode"] == null ? Session["CurArchivalSelectedSiteCode"].ToString().Trim() : Request.Form["sitecode"].ToString().Trim(); 
                                
                //Following are Session Value
                //string business date
                DateTime dateBusDate = Convert.ToDateTime(Session["CurArchivalSelectedBusdate"].ToString());
                string strBusDate = dateBusDate.ToString("yyyyMMdd");

                //Selected Item's Batch No
                string strBatchNum = Session["CurArchivalSelectedBatchNo"].ToString();

                //Selected item's Check No
                string strCheckNo = Session["CurArchivalSelectedCheckNo"].ToString();

                //Selected item's UIC
                string strUIC = Session["CurArchivalSelectedUIC"].ToString();

                string curIISVirtualPath = Session["s_CurSelectedIISVirtualPathImgArc"].ToString();
                string curTransactionType = Session["s_CurSelectedTransactionTypeImgArc"].ToString();
                string curImageFolder = Session["s_CurSelectedImageFolderImgArc"].ToString();

                string clientcode = strClientCode.ToString().Trim();
                if (clientcode.Equals("CIMB"))
                {
                    clientcode = "CIMB";
                }
                else
                {
                    clientcode = "ALL";
                }

                //Initialise DB connection to crystal report object
                DataTable dtRpt = GetCRReportConnectionInfo(strReportCode, clientcode);

                string rptServer = dtRpt.Rows[0]["RPT_ServerName"].ToString(); //Report file DB Server Name
                string rptDBName = dtRpt.Rows[0]["RPT_DBName"].ToString(); //Report File Database Name
                string strReportPath = dtRpt.Rows[0]["RPT_ReportSrc"].ToString();//Report File Path, in TBL_Report's RPT_ReportSrc
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
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    rd = new ReportDocument();//strReportPath

                    //Retrieve Virtual Directories Folder name from Param
                    //Get par value 2
                    string paramRptVirDir = UBPC.Web.Common.Parameters.GetParamValue(webDBConnStr, "ReportPath",false);

                    string strRptPath = string.Empty;
                    //ReportFiles/<Client Code>
                    string completePath = Path.Combine(paramRptVirDir + "//" + clientcode, strReportName); // Server.MapPath("~/") + "ReportFiles//" + strReportName;
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

                    DataTable dtPrintInfo;
                    //Retrieve list of items in a transaction
                    dtPrintInfo = GetTransactionAllItems(strClientCode, strSiteCode, strBusDate, strCheckNo, strBatchNum, strUIC);
                    //Map the items's field to xsd field and link dataset to crystal report document object
                    rd.SetDataSource(GetMappedPrintTable(dtPrintInfo, curIISVirtualPath, strBusDate, curTransactionType, curImageFolder).Tables[0]);

                    // set the Report Title
                    rd.SummaryInfo.ReportTitle = string.Format("{0} {1}", strReportCode, strReportDesc);
                    
                    //Using Stored Proc
                    rd.VerifyDatabase();
                                   
                    #region Set Parameter Fields if existed
                    ParameterFieldDefinitions crParameterdef = rd.DataDefinition.ParameterFields;
                    if (crParameterdef.Count > 0)
                    {
                        rd.SetParameterValue("@BusDate", dateBusDate);
                        rd.SetParameterValue("@ClientName", strClientCode);

                        //If to directly show PDF on page
                        rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, strReportCode);

                        //If to display crystal report viewer screen on page 
                        //Set Report Source
                        //CrystalReportViewer1.ReportSource = rd;
                        //CrystalReportViewer1.ToolPanelView = ToolPanelViewType.None;

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
                    log.Message = "Failed to Print Archival Transaction: Missing Report";
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
                log.Message = "Failed Print Archival Transaction" + ex.Message;
                log.Write();

                Response.Write("<H2>ERROR:Failed to Load Archival Report File</H2>");
            }
            finally
            {
                rd.Close();
                rd.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public string GetClientConnectionString(string clientcode, string sitecode) 
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = "SELECT DB_ConnString_Client FROM TBL_SITEDB  WHERE DB_Bank = @ClientCode and DB_Site = @ClientSite";

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 10, "@ClientCode", ParameterDirection.Input, clientcode),
                    dbHelperObj.CreateParameter(DbType.String, 3, "@ClientSite", ParameterDirection.Input, sitecode)

                };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);
                return dtDB.Rows.Count > 0 ? dtDB.Rows[0][0].ToString() : null;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            finally
            {
                iConn.Close();
            }

        }

        public DataTable GetCRReportConnectionInfo(string reportCode, string clientCode)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = UBPCWeb.Modules.Reports.Resource.stmtGetCRReportInfo;
                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 10, "@ReportCode", ParameterDirection.Input,reportCode),
                    dbHelperObj.CreateParameter(DbType.String, 10, "@ClientCode", ParameterDirection.Input,clientCode)
                };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);
                return dtDB;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                iConn.Close();
            }
        }


        public DataTable GetTransactionAllItems(string clientCode, string sitecode, string busdate, string checkNo, string batchNo, string uic)
        {
            //IDbConnection iConn = dbHelperObj.initConnection(GetClientConnectionString(clientCode,sitecode));

            string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            DBConnectionInfo dbConn = new DBConnectionInfo();
            dbConn.webConnStr = webDBConnStr;

            string clientSite = dbConn.GetMainSite(HttpContext.Current.Session["s_MainSite"].ToString(), clientCode.Trim());

            try
            {
                stmt = "sp_GetPrintInfoForArchivalTransInward";

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String,10, "@BatchNum", ParameterDirection.Input,batchNo),
                    dbHelperObj.CreateParameter(DbType.String,8, "@Busdate", ParameterDirection.Input,busdate),
                    dbHelperObj.CreateParameter(DbType.String,10, "@CheckNum", ParameterDirection.Input,checkNo),
                    dbHelperObj.CreateParameter(DbType.String,30, "@UIC", ParameterDirection.Input,uic),
                    dbHelperObj.CreateParameter(DbType.String,10, "@ClientCode", ParameterDirection.Input,clientCode),
                    dbHelperObj.CreateParameter(DbType.String,10, "@ClientSite", ParameterDirection.Input,clientSite),

                };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);
                return dtDB;
            }
            catch (Exception ex) { throw ex; }
        }

        ArchivalPrintItem dsPrint;
        private DataSet GetMappedPrintTable(DataTable dtArchivalTransItm, string curIISVirtualPath, string curBusDate, string curTransactionType, string curImageFolder)
        {
            try
            {
                ArchivalPrintItem.PrintOutwardTransItemDataTable dtPrint;

                //virtual directories name --> REMEMBER: change to correct virtual dic name 
                //string IFSPath = Server.MapPath("/IFS_XX");

                string virtualDirectory = "~/" + curIISVirtualPath;

                // Retrieve the physical path of the directory
                string physicalPath = HttpContext.Current.Server.MapPath(virtualDirectory);

                string inwardImagePath = curBusDate + "\\" + curTransactionType + "\\" + curImageFolder + "\\archives";
                //string inwardImagePath = physicalPath + "\\" + curBusDate + "\\" + curTransactionType + "\\" + curImageFolder + "\\archives";
                string IFSPath = Server.MapPath(inwardImagePath);

                if (dsPrint == null)
                {
                    dsPrint = new ArchivalPrintItem();
                }

                dtPrint = dsPrint.PrintOutwardTransItem;
                if (dtPrint == null)
                {
                    dtPrint = new ArchivalPrintItem.PrintOutwardTransItemDataTable();
                    dsPrint.Tables.Add(dtPrint);
                }

                //Clear Main Talbe Rows and Insert as new set of rows
                dtPrint.Rows.Clear();

                //Populate data to table
                foreach (DataRow drTransItem in dtArchivalTransItm.Rows)
                {

                    ArchivalPrintItem.PrintOutwardTransItemRow dr = dtPrint.NewPrintOutwardTransItemRow();
                    string dateString = drTransItem["ITM_BusDate"].ToString().Trim();
                    DateTime dateValue = DateTime.Parse(drTransItem["ITM_BusDate"].ToString().Trim(), CultureInfo.InvariantCulture);

                    dr.ITM_BusDate = dateValue.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    //dr.ITM_BusDate = Convert.ToDateTime(drTransItem["ITM_BusDate"].ToString().Trim());
                    dr.ITM_Fld10 = drTransItem["ITM_IssuingBank"].ToString().Trim() + drTransItem["ITM_IssuingBranch"].ToString().Trim();
                    dr.ITM_BatchNum = drTransItem["ITM_BatchNum"].ToString().Trim();
                    dr.ITM_Fld12 = drTransItem["ITM_AcctNo"].ToString().Trim();
                    dr.ITM_Amount = decimal.Parse(drTransItem["ITM_Amount"].ToString().Trim()).ToString("N2");
                    dr.SaveDate = DateTime.Today.ToString("dd/MM/yyyy").Trim();
                    dr.Indicator = "Inward";
                    dr.ITM_CheckNo = drTransItem["ITM_CheckNo"].ToString().Trim();
                    dr.ITM_ReturnCount = (string.IsNullOrEmpty(drTransItem["ITM_ReturnCount"].ToString().TrimStart('0')) || string.IsNullOrEmpty(drTransItem["ITM_ReturnCount"].ToString().Trim())) ? "0" : drTransItem["ITM_ReturnCount"].ToString().TrimStart('0');
                    dr.ITM_TransactionType = drTransItem["ITM_TransactionType"].ToString().Trim();
                    dr.ITM_NCF = drTransItem["ITM_NCF"].ToString().Trim();

                    

                    byte[] imgFrontByte = null;
                    byte[] imgRearByte = null;

                    string _TempDirectoryWithBusDate = GetTempDirectory(@"C:\Temp");
                    

                    try
                    {
                        //Front Tiff
                        if (Session["ImageArchiveInwardFrontImg"].ToString() == null)
                        {
                            string noImgFileName = Server.MapPath("~/Content/images/NoImage.jpg");
                            imgFrontByte = System.IO.File.ReadAllBytes(noImgFileName);
                        }
                        else
                        {
                            string imgFrontOffsetTiff = Session["ImageArchiveInwardFrontImg"].ToString().Trim();
                            imgFrontByte = (byte[])Session["ImageArchiveInwardFrontImg"];
                        }
                    }
                    catch (Exception ex)
                    {
                        string noImgFileName = Server.MapPath("~/Content/images/NoImage.jpg");
                        imgFrontByte = System.IO.File.ReadAllBytes(noImgFileName);
                    }

                    try
                    {
                        //Rear Tiff
                        if (Session["ImageArchiveInwardRearImg"].ToString() == null)
                        {
                            string noImgFileName = Server.MapPath("~/Content/images/NoImage.jpg");
                            imgFrontByte = System.IO.File.ReadAllBytes(noImgFileName);
                        }
                        else
                        {
                            string imgRearOffsetTiff = Session["ImageArchiveInwardRearImg"].ToString().Trim();
                            imgRearByte = (byte[])Session["ImageArchiveInwardRearImg"];
                        }
                    }
                    catch (Exception ex)
                    {
                        string noImgFileName = Server.MapPath("~/Content/images/NoImage.jpg");
                        imgRearByte = System.IO.File.ReadAllBytes(noImgFileName);
                    }


                    dr.ImgFront = imgFrontByte;
                    dr.ImgRear = imgRearByte;


                    dtPrint.Rows.Add(dr);
                }

                return dsPrint;
            }
            catch (Exception ex)
            {
                throw ex;
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
            if (Request.Form["rptcode"] != null)
            {
                Session["CurArchivalSelectedRptCode"] = Request.Form["rptcode"].ToString().Trim();
                Session["CurArchivalSelectedRptClient"] = Request.Form["clientCode"].ToString().Trim();
            }

            CrystalReportViewer1.ID = Request.Form["rptcode"] == null ? Session["CurArchivalSelectedRptCode"].ToString().Trim() : Request.Form["rptcode"].ToString().Trim();

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