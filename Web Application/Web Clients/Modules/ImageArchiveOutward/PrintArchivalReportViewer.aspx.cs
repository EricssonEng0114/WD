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
using System.Globalization;
using UBPCWeb.CRViewerReportForm;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Data.SqlTypes;

namespace UBPCWeb.Modules.ImageArchiveOutward
{
    public partial class PrintArchivalReportViewer : System.Web.UI.Page
    {
        ReportDocument rd;//strReportPath
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        string pattern = @"^\d{8}$";//datetime format 20240611 yyyyMMdd
        DateTime parsedDate;

        protected void Page_Load(object sender, EventArgs e)
        {
            //do nothing

        }

        private void LoadReport()
        {
            try
            {

                List<UBPCWeb.Modules.ImageArchiveOutward.Default.SelectedData> selectedData = null;

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


                if (Regex.IsMatch(Session["CurArchivalSelectedBusdate"].ToString(), pattern))
                {
                    if (DateTime.TryParseExact(Session["CurArchivalSelectedBusdate"].ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    {
                        Session["CurArchivalSelectedBusdate"] = parsedDate;
                    }
                }
                //Following are Session Value
                //string business date
                DateTime dateBusDate = Convert.ToDateTime(Session["CurArchivalSelectedBusdate"].ToString());
                string strBusDate = dateBusDate.ToString("yyyyMMdd");

                //Selected item's batch dir
                string strBatchDir = Session["CurArchivalSelectedBatchDir"].ToString();

                //Selected Item's Batch No
                string strBatchNum = Session["CurArchivalSelectedBatchNo"].ToString();

                //Selected Item's Trans No
                string strTransNum = Session["CurArchivalSelectedTransNo"].ToString();

                //Selected Item's Din No
                string strTransSeqNum = Session["CurArchivalSelectedTransSeqNum"].ToString();

                //Selected Item's is Item Only
                string strItemOnly = Session["CurArchivalSelectedItemOnly"].ToString();

                string strIsRejected = Session["CurArchivalIsRejected"].ToString();

                string strIsMultipleCheck = Session["CurArchivalIsMultiple"].ToString();

                if (strIsMultipleCheck == "True")
                {
                    selectedData = (List<UBPCWeb.Modules.ImageArchiveOutward.Default.SelectedData>)Session["CurArchivalSelectedDataMultipleCheckBox"];
                }

                string curRepresented = string.Empty;

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
                DataTable dtRpt = GetCRReportConnectionInfo(strReportCode, clientcode);//get the report, put ALL for all bank(except CIMB)

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
                    string paramRptVirDir = UBPC.Web.Common.Parameters.GetParamValue(webDBConnStr, "ReportPath", false);

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

                    DataTable dtPrintInfo = new DataTable();
                    //Retrieve list of items in a transaction
                    if (strIsRejected == "True")
                    {
                        string strRejectCategory = Session["CurArchivalRejectCategory"].ToString();
                        dtPrintInfo = CommonFunction.GetTransactionAllItems(strClientCode, strSiteCode, "", "", strBusDate, "0", "", "", "True", Session["CurArchivalRejectCategory"].ToString());//print selected date rejected item
                        //dtPrintInfo = CommonFunction.GetImageArchiveOutwardRejectedItem(strClientCode, strRejectCategory);
                    }
                    else if (strIsMultipleCheck == "True")
                    {
                        if (dtPrintInfo == null || dtPrintInfo.Columns.Count == 0)
                        {
                            DataRow sourceRow = GetMultipleTransactionAllItems(selectedData.First().Client, selectedData.First().SiteName, selectedData.First().DirName, selectedData.First().BatchNo, selectedData.First().Date, selectedData.First().Trans, selectedData.First().TransSeqNum, strItemOnly, strIsRejected);
                            if (sourceRow != null)
                            {
                                dtPrintInfo = sourceRow.Table.Clone();

                                foreach (var data in selectedData)
                                {
                                    sourceRow = GetMultipleTransactionAllItems(data.Client, data.SiteName, data.DirName, data.BatchNo, data.Date, data.Trans, data.TransSeqNum, strItemOnly, strIsRejected);
                                    if (sourceRow != null)
                                    {
                                        DataRow newRow = dtPrintInfo.NewRow();
                                        for (int i = 0; i < dtPrintInfo.Columns.Count; i++)
                                        {
                                            if (i < sourceRow.Table.Columns.Count)
                                            {
                                                newRow[i] = sourceRow[i];
                                            }
                                        }
                                        dtPrintInfo.Rows.Add(newRow);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        dtPrintInfo = GetTransactionAllItems(strClientCode, strSiteCode, strBatchDir, strBatchNum, strBusDate, strTransNum, strTransSeqNum, strItemOnly, strIsRejected, "");
                    }

                    //Map the items's field to xsd field and link dataset to crystal report document object
                    rd.SetDataSource(GetMappedPrintTable(dtPrintInfo, strClientCode, strSiteCode).Tables[0]);

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

                        //Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1)); Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        //Response.Cache.SetNoStore();

                        ////If to directly show PDF on page
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
                //Response.End();
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


        public DataTable GetTransactionAllItems(string clientCode, string sitecode, string batchDir, 
            string batchNo, string datetime, string transNo, string transSeqNum, string isCurrentItemOnly, string isRejected, string rejectCategory)
        {
            //IDbConnection iConn = dbHelperObj.initConnection(GetClientConnectionString(clientCode,sitecode));

            string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;


            //DateTime busdate = DateTime.ParseExact(datetime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            //string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');

            try
            {
                stmt = "sp_GetPrintInfoForArchivalTransOutward";

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String,8, "@BatchDir", ParameterDirection.Input,batchDir),
                    dbHelperObj.CreateParameter(DbType.String,8, "@BatchNum", ParameterDirection.Input,batchNo),
                    dbHelperObj.CreateParameter(DbType.String,8, "@Busdate", ParameterDirection.Input,datetime),
                    dbHelperObj.CreateParameter(DbType.Int32,0, "@TransNum", ParameterDirection.Input,Convert.ToInt32(transNo)),
                    dbHelperObj.CreateParameter(DbType.String,10, "@TransSeqNum", ParameterDirection.Input, transSeqNum),
                    dbHelperObj.CreateParameter(DbType.String,10, "@isCurrentItem", ParameterDirection.Input, isCurrentItemOnly),
                    dbHelperObj.CreateParameter(DbType.String,10, "@isPrintRejected", ParameterDirection.Input, isRejected),
                    dbHelperObj.CreateParameter(DbType.String,10, "@RejectCategory", ParameterDirection.Input, rejectCategory),
                    dbHelperObj.CreateParameter(DbType.String,10, "@ClientCode", ParameterDirection.Input,clientCode),
                    dbHelperObj.CreateParameter(DbType.String,10, "@ClientSite", ParameterDirection.Input,sitecode)
                };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);
                return dtDB;
            }
            catch (Exception ex) { throw ex; }
        }

        public DataRow GetMultipleTransactionAllItems(string clientCode, string sitecode, string batchDir, 
            string batchNo, string datetime, string transNo, string transSeqNum, string isCurrentItemOnly, string isRejected)
        {
            //IDbConnection iConn = dbHelperObj.initConnection(GetClientConnectionString(clientCode,sitecode));

            string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;


            //DateTime busdate = DateTime.ParseExact(datetime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            //string sqlDateTime = busdate.Year.ToString() + busdate.Month.ToString().PadLeft(2, '0') + busdate.Day.ToString().PadLeft(2, '0');

            try
            {
                stmt = "sp_GetPrintInfoForArchivalTransOutward";

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String,8, "@BatchDir", ParameterDirection.Input,batchDir),
                    dbHelperObj.CreateParameter(DbType.String,8, "@BatchNum", ParameterDirection.Input,batchNo),
                    dbHelperObj.CreateParameter(DbType.String,8, "@Busdate", ParameterDirection.Input,datetime),
                    dbHelperObj.CreateParameter(DbType.Int32,0, "@TransNum", ParameterDirection.Input,Convert.ToInt32(transNo)),
                    dbHelperObj.CreateParameter(DbType.String,10, "@TransSeqNum", ParameterDirection.Input, transSeqNum),
                    dbHelperObj.CreateParameter(DbType.String,10, "@isCurrentItem", ParameterDirection.Input, isCurrentItemOnly),
                    dbHelperObj.CreateParameter(DbType.String,10, "@isPrintRejected", ParameterDirection.Input, isRejected),
                    dbHelperObj.CreateParameter(DbType.String,10, "@RejectCategory", ParameterDirection.Input, ""),
                    dbHelperObj.CreateParameter(DbType.String,10, "@ClientCode", ParameterDirection.Input,clientCode),
                    dbHelperObj.CreateParameter(DbType.String,10, "@ClientSite", ParameterDirection.Input,sitecode)
                };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);
                if(dtDB.Rows.Count > 0)
                {
                    return dtDB.Rows[0];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex) { throw ex; }
        }

        ArchivalPrintItem dsPrint;
        private DataSet GetMappedPrintTable(DataTable dtArchivalTransItm, string strClientCode, string strSiteCode)
        {
            try
            {
                if (Regex.IsMatch(Session["s_CurSelectedBusdateRejDec"].ToString(), pattern))
                {
                    if (DateTime.TryParseExact(Session["s_CurSelectedBusdateRejDec"].ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    {
                        Session["s_CurSelectedBusdateRejDec"] = parsedDate;
                    }
                }
                DateTime curBusdate = Convert.ToDateTime(Session["s_CurSelectedBusdateRejDec"]);
                string curIISVirtualPath = Session["s_CurSelectedIISVirtualPathImgArc"].ToString() + "//" + curBusdate.ToString("yyyyMMdd");
                //string curTransportID = Session["s_CurSelectedTransportIDImgArc"].ToString().PadLeft(2, '0');

                string fileDirectory = string.Empty;// = "//" + curIISVirtualPath + "//IFS_" + curTransportID + "//";


                ArchivalPrintItem.PrintOutwardTransItemDataTable dtPrint;

                //virtual directories name --> REMEMBER: change to correct virtual dic name 
                //string IFSPath = Server.MapPath(@"/ImgArchivePath_Out/20240611/IFS_01/");
                //string IFSPath = Server.MapPath(fileDirectory);

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

                    string transportID = drTransItem["ITM_TransportID"].ToString().Trim().PadLeft(2, '0');
                    string curBatchDir = drTransItem["ITM_BatchDirectory"].ToString().Trim();
                    string uic = drTransItem["ITM_UIC"].ToString().Trim();
                    string dateString = drTransItem["ITM_BusDate"].ToString().Trim();

                    DateTime dateValue = DateTime.Parse(drTransItem["ITM_BusDate"].ToString().Trim(), CultureInfo.InvariantCulture);
                    dr.ITM_BusDate = dateValue.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    //dr.ITM_BusDate = Convert.ToDateTime(drTransItem["ITM_BusDate"].ToString("dd/MM/yyyy").Trim());

                    dr.ITM_WsID = drTransItem["ITM_WsID"].ToString().Trim();
                    dr.ITM_ProcessingSite = drTransItem["ITM_ProcessingSite"].ToString().Trim();
                    dr.ITM_TransportID = transportID;
                    dr.ITM_BatchDirectory = drTransItem["ITM_BatchDirectory"].ToString().Trim();
                    dr.ITM_BatchNum = drTransItem["ITM_BatchNum"].ToString().Trim();
                    dr.ITM_TransNum = drTransItem["ITM_TransNum"].ToString().Trim();
                    dr.ITM_DIN = drTransItem["ITM_DIN"].ToString().Trim();
                    dr.ITM_Represented = drTransItem["ITM_REPRESENTED"].ToString().Trim();
                    dr.ITM_ItemType = drTransItem["ITM_ItemType"].ToString().Trim();
                    dr.ITM_ProcMode = drTransItem["ITM_ProcMode"].ToString().Trim();
                    dr.ITM_Fld1 = drTransItem["ITM_Fld1"].ToString().Trim();
                    dr.ITM_Fld2 = drTransItem["ITM_Fld2"].ToString().Trim();
                    dr.ITM_Fld3 = drTransItem["ITM_Fld3"].ToString().Trim();
                    dr.ITM_Fld4 = drTransItem["ITM_Fld4"].ToString().Trim();
                    dr.ITM_Fld5 = drTransItem["ITM_Fld5"].ToString().Trim();
                    dr.ITM_Fld6 = drTransItem["ITM_Fld6"].ToString().Trim();
                    dr.ITM_Fld7 = drTransItem["ITM_Fld7"].ToString().Trim();
                    dr.ITM_Fld8 = drTransItem["ITM_Fld8"].ToString().Trim();
                    dr.ITM_Fld9 = drTransItem["ITM_Fld9"].ToString().Trim();
                    dr.ITM_Fld10 = (drTransItem["ITM_ItemType"].ToString() == "B") ? drTransItem["ITM_Fld1"].ToString().Trim() : drTransItem["ITM_Fld10"].ToString().Trim();
                    dr.ITM_Fld11 = drTransItem["ITM_Fld11"].ToString().Trim();
                    dr.ITM_Fld12 = drTransItem["ITM_Fld12"].ToString().Trim();
                    dr.ITM_Amount = drTransItem["ITM_ItemType"].ToString() == "B" ? "" : decimal.Parse(drTransItem["ITM_Amount"].ToString().Trim()).ToString("N2");
                    dr.ITM_BundleID = drTransItem["ITM_BundleID"].ToString().Trim();
                    dr.ITM_CheckType = drTransItem["ITM_CheckType"].ToString().Trim();
                    dr.ITM_UIC = drTransItem["ITM_UIC"].ToString().Trim();
                    dr.ITM_RefNum = drTransItem["ITM_RefNum"].ToString().Trim();
                    dr.ITM_DI_Format = drTransItem["ITM_DI_Format"].ToString().Trim();
                    dr.ITM_DI_Desc = drTransItem["ITM_DI_Desc"].ToString().Trim();
                    dr.ITM_Returned = drTransItem["ITM_Returned"].ToString().Trim();
                    dr.ITM_ReturnedDate = drTransItem["ITM_ReturnedDate"].ToString().Trim() != string.Empty ? Convert.ToDateTime(drTransItem["ITM_ReturnedDate"].ToString().Trim()) : DateTime.MinValue;
                    dr.ITM_ReturnedReason = drTransItem["ITM_ReturnedReason"].ToString().Trim();
                    dr.ITM_Ref1 = drTransItem["ITM_Ref1"].ToString().Trim();
                    dr.ITM_Ref2 = drTransItem["ITM_Ref2"].ToString().Trim();
                    dr.ITM_Ref3 = drTransItem["ITM_Ref3"].ToString().Trim();
                    dr.ITM_Ref4 = drTransItem["ITM_Ref4"].ToString().Trim();
                    dr.ITM_Ref5 = drTransItem["ITM_Ref5"].ToString().Trim();
                    dr.ITM_Ref6 = drTransItem["ITM_Ref6"].ToString().Trim();
                    dr.ITM_Ref7 = drTransItem["ITM_Ref7"].ToString().Trim();
                    dr.ITM_Ref8 = drTransItem["ITM_Ref8"].ToString().Trim();
                    dr.ITM_Ref9 = drTransItem["ITM_Ref9"].ToString().Trim();
                    dr.ITM_Ref10 = drTransItem["ITM_Ref10"].ToString().Trim();
                    dr.ITM_NCF = drTransItem["ITM_NCF"].ToString().Trim();
                    dr.ITM_ImageFileName = drTransItem["ITM_ImageFileName"].ToString().Trim();
                    dr.ITM_Fr_ImgOffset = drTransItem["ITM_Fr_ImgOffset"].ToString().Trim();
                    dr.ITM_Fr_ImgSize = drTransItem["ITM_Fr_ImgSize"].ToString().Trim();
                    dr.ITM_Rr_ImgOffset = drTransItem["ITM_Rr_ImgOffset"].ToString().Trim();
                    dr.ITM_ProcessChkType = drTransItem["ITM_ProcessChkType"].ToString().Trim();
                    dr.PrimaryName = drTransItem["PrimaryName"].ToString().Trim();
                    dr.SecondaryName1 = drTransItem["SecondaryName1"].ToString().Trim();
                    dr.SecondaryName2 = drTransItem["SecondaryName2"].ToString().Trim();
                    dr.SecondaryName3 = drTransItem["SecondaryName3"].ToString().Trim();

                    dr.SaveDate = DateTime.Today.ToString("dd/MM/yyyy").Trim();
                    dr.Indicator = "Outward";

                    dr.ITM_TransSeqNum = drTransItem["ITM_TransSeqNum"].ToString().Trim(); 
                    dr.UniqueIDPair = drTransItem["ITM_BatchDirectory"].ToString().Trim() + drTransItem["ITM_BatchNum"].ToString().Trim() + 
                                      drTransItem["ITM_TransNum"].ToString().Trim() + drTransItem["ITM_TransSeqNum"].ToString().Trim();

                    dr.ITM_PERunNum = drTransItem["ITM_PERunNum"].ToString().Trim();
                    dr.ITM_RunNum = drTransItem["ITM_RunNum"].ToString().Trim();
                    dr.ITM_Rejected = drTransItem["ITM_Rejected"].ToString() == "True" ? "Rejected" : "OK";
                    dr.ITM_RejectReason = drTransItem["ITM_RejectReason"].ToString().Trim();
                    dr.ITM_Represented = drTransItem["ITM_REPRESENTED"].ToString().Trim();
                    #region dr.ProductType
                    if (drTransItem["ITM_DI_Format"].ToString() != ""  && Convert.ToInt32(drTransItem["ITM_DI_Format"]) > 1)
                    {
                        dr.ProductType = "Lockbox";
                    }
                    else if (drTransItem["BST_IsBPC"].ToString() == "True")
                    {
                        dr.ProductType = "BPC";
                    }
                    else if (drTransItem["ITM_WsID"].ToString() == "021")
                    {
                        dr.ProductType = "Normal";
                    }
                    else if (drTransItem["ITM_WsID"].ToString() == "022")
                    {
                        dr.ProductType = "MOPO";
                    }
                    else if (drTransItem["ITM_WsID"].ToString() == "023")
                    {
                        dr.ProductType = "NCI";
                    }
                    else if (drTransItem["ITM_WsID"].ToString() == "024")
                    {
                        dr.ProductType = "Foreign Cheque";
                    }
                    else
                    {
                        dr.ProductType = "N/A";
                    }
                    #endregion

                    //added by boonchong 20251013 CIMB CR008-25
                    if (strClientCode == "CIMB")
                    {
                        dr.ITM_Rejected = drTransItem["ITM_Rejected"].ToString() == "True" ? "Rejected(CIMB)" : "OK(CIMB)";

                        if (drTransItem.Table.Columns.Contains("ITM_ReferToWeb") && drTransItem.Table.Columns.Contains("REJ_ActionByUnisys"))
                        {
                            //cannot refer to ITM_ReferToWeb because after accept/reject in WD it will become 0
                            dr.ITM_ReferToWeb = !string.IsNullOrEmpty(drTransItem["REJ_ActionByUnisys"].ToString().Trim()) ? "Pushed to WD" : "Not Pushed to WD";
                        }
                        if (drTransItem.Table.Columns.Contains("REJ_ActionByUnisys"))
                        {
                            dr.REJ_ActionByUnisys = drTransItem["REJ_ActionByUnisys"].ToString().Trim();
                        }
                        else
                        {
                            dr.REJ_ActionByUnisys = "";
                        }
                    }


                    byte[] imgFrontByte = null;
                    byte[] imgRearByte = null;
                    string imgFrontOffsetTiff = drTransItem["ITM_Fr_ImgOffset"].ToString().Trim();// "int64";
                    string imgFrontSizeTiff = drTransItem["ITM_Fr_ImgSize"].ToString().Trim();//"int32"
                    string imgRearOffsetTiff = drTransItem["ITM_Rr_ImgOffset"].ToString().Trim();
                    string imgRearSizeTiff = drTransItem["ITM_Rr_ImgSize"].ToString().Trim();

                    string itmFileName = drTransItem["ITM_ImageFileName"].ToString().Trim();//06010001\80070146    

                    string _TempDirectoryWithBusDate = GetTempDirectory(@"C:\Temp");
                    fileDirectory = "//" + curIISVirtualPath + "//IFS_" + transportID + "//";
                    string IFSPath = Server.MapPath(fileDirectory);
                    #region Front Tiff
                    try
                    {
                        if (drTransItem["ITM_REPRESENTED"].ToString().Trim() == "True")
                        {//represented Tiff
                            string virtualDirectory = "~/" + curIISVirtualPath + "\\IFS_" + transportID + "\\";
                            string physicalPath = HttpContext.Current.Server.MapPath(virtualDirectory);
                            string outwardImagePath = physicalPath + curBatchDir;

                            ProcessImage(uic, outwardImagePath, uic, strSiteCode, strClientCode);
                            if (Session["ImageArchiveInwardFrontImg"].ToString() == null)
                            {
                                string noImgFileName = Server.MapPath("~/Content/images/NoImage.jpg");
                                imgFrontByte = System.IO.File.ReadAllBytes(noImgFileName);
                            }
                            else
                            {
                                imgFrontOffsetTiff = Session["ImageArchiveInwardFrontImg"].ToString().Trim();
                                imgFrontByte = (byte[])Session["ImageArchiveInwardFrontImg"];
                            }

                        }
                        else
                        {//normal tiff
                            if (imgFrontOffsetTiff == "0")
                            {
                                string noImgFileName = Server.MapPath("~/Content/images/NoImage.jpg");
                                imgFrontByte = System.IO.File.ReadAllBytes(noImgFileName);
                            }
                            else
                            {
                                String FIMfileName = Path.Combine(IFSPath, itmFileName + ".FIM");

                                System.IO.FileStream imagefile = new System.IO.FileStream(FIMfileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                                imagefile.Seek(Convert.ToInt64(imgFrontOffsetTiff) - 1, System.IO.SeekOrigin.Begin);
                                byte[] bytes = System.IO.File.ReadAllBytes(FIMfileName);
                                imagefile.Read(bytes, 0, Convert.ToInt32(imgFrontSizeTiff) - 1);
                                imgFrontByte = bytes;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string noImgFileName = Server.MapPath("~/Content/images/NoImage.jpg");
                        imgFrontByte = System.IO.File.ReadAllBytes(noImgFileName);
                    }
                    #endregion

                    #region Rear Tiff
                    try
                    {
                        //Rear Tiff
                        if (drTransItem["ITM_REPRESENTED"].ToString().Trim() == "True")
                        {//represented tiff
                            string virtualDirectory = "~/" + curIISVirtualPath + "\\IFS_" + transportID + "\\";
                            string physicalPath = HttpContext.Current.Server.MapPath(virtualDirectory);
                            string outwardImagePath = physicalPath + curBatchDir;

                            ProcessImage(uic, outwardImagePath, uic, strSiteCode, strClientCode);
                            if (Session["ImageArchiveInwardRearImg"].ToString() == null)
                            {
                                string noImgFileName = Server.MapPath("~/Content/images/NoImage.jpg");
                                imgRearByte = System.IO.File.ReadAllBytes(noImgFileName);
                            }
                            else
                            {
                                imgFrontOffsetTiff = Session["ImageArchiveInwardRearImg"].ToString().Trim();
                                imgRearByte = (byte[])Session["ImageArchiveInwardRearImg"];
                            }

                        }
                        else
                        {//normal tiff
                            if (imgRearOffsetTiff == "0")
                            {
                                string noImgFileName = Server.MapPath("~/Content/images/NoImage.jpg");
                                imgRearByte = System.IO.File.ReadAllBytes(noImgFileName);
                            }
                            else
                            {
                                String RIMfileName = Path.Combine(IFSPath, itmFileName + ".RIM");

                                System.IO.FileStream imagefile = new System.IO.FileStream(RIMfileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                                imagefile.Seek(Convert.ToInt64(imgRearOffsetTiff) - 1, System.IO.SeekOrigin.Begin);
                                byte[] bytes = System.IO.File.ReadAllBytes(RIMfileName);
                                imagefile.Read(bytes, 0, Convert.ToInt32(imgRearSizeTiff) - 1);
                                imgRearByte = bytes;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string noImgFileName = Server.MapPath("~/Content/images/NoImage.jpg");
                        imgRearByte = System.IO.File.ReadAllBytes(noImgFileName);
                    }
                    #endregion

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

        private void ProcessImage(string filename, string fileDirectory, string UIC, string curSite, string clientList)
        {

            DateTime curBusdate = Convert.ToDateTime(Session["s_CurSelectedBusdateRejDec"]);
            DBConnectionInfo dbConn = new DBConnectionInfo();
            dbConn.webConnStr = webDBConnStr;
            //Session["selClientDBConnStr"] = dbConn.GetSiteConnectionString(true, Session["s_CurSelectedClientImgArc"].ToString(), curSite);
            string outputPath = @"C:\Temp";
            string fullName = filename + ".IMG";

            try
            {
                if (!string.IsNullOrEmpty(outputPath))
                {
                    try
                    {
                        if (File.Exists(Path.Combine(fileDirectory, fullName)))
                        {
                            //listOfImgFile.Add(Path.Combine(fileDirectory, fullName));
                            string path = Path.Combine(fileDirectory, fullName);
                            System.Drawing.Image tifImg = System.Drawing.Image.FromFile(path);
                            Guid objguid = tifImg.FrameDimensionsList[0];
                            FrameDimension fd = new FrameDimension(objguid);
                            int page = tifImg.GetFrameCount(fd);
                            int index = 0;
                            string outputFileName = string.Empty;
                            string directory = "C:\\Temp\\" + ((DateTime)curBusdate).ToString("yyyyMMdd");
                            if (!Directory.Exists(directory))
                                Directory.CreateDirectory(directory);

                            Session.Remove("ImageArchiveInwardFrontJPEG");
                            Session.Remove("ImageArchiveInwardFrontImg");
                            Session.Remove("ImageArchiveInwardRearImg");

                            for (index = 0; index < page; index++)
                            {
                                tifImg.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Page, index);
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    if (index == 0)
                                    {
                                        tifImg.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                        outputFileName = UIC + ".jpg";
                                        File.WriteAllBytes(directory + "\\" + outputFileName, ms.ToArray());
                                        Session["ImageArchiveInwardFrontJPEG"] = ms.ToArray();
                                    }
                                    else
                                    {
                                        if (index == 1)
                                        {
                                            tifImg.Save(ms, System.Drawing.Imaging.ImageFormat.Tiff);
                                            outputFileName = UIC + ".FIM";
                                            File.WriteAllBytes(directory + "\\" + outputFileName, ms.ToArray());
                                            Session["ImageArchiveInwardFrontImg"] = ms.ToArray();
                                        }
                                        else
                                        {
                                            tifImg.Save(ms, System.Drawing.Imaging.ImageFormat.Tiff);
                                            outputFileName = UIC + ".RIM";
                                            File.WriteAllBytes(directory + "\\" + outputFileName, ms.ToArray());
                                            Session["ImageArchiveInwardRearImg"] = ms.ToArray();
                                        }
                                    }

                                    ms.Dispose();
                                }

                            }

                            tifImg.Dispose();

                            string sqlDateTime = ((DateTime)curBusdate).ToString("yyyyMMdd");
                            string ImgPath = sqlDateTime + "\\" + UIC;
                            ChequeViewerCtrl chqImgObj = new ChequeViewerCtrl();
                            //string IFSPath, bool isFront, bool isJPEG, String frontImgPath, String rearImgPath, String frontJpegImgPath

                            if (Directory.Exists(directory))
                            {
                                Directory.Delete(directory, true);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogEntry log = new LogEntry();
                        log.Caller = LogCallerID.ImageArchiveOutward;
                        log.Severity = LogEventType.Error;
                        log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                        log.Alert = true;
                        log.Message = ex.Message;
                        log.Write();

                    }
                }

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.ImageArchiveOutward;
                log.Severity = LogEventType.Error;
                log.ClientCode = (clientList.Contains(",") ? "" : clientList.Trim().ToString());
                log.Alert = true;
                log.Message = ex.Message;
                log.Write();
            }
        }

        public DataTable InitializeDataTable()
        {
            // Initialize the DataTable
            DataTable dtPrintInfo = new DataTable();

            // Add columns to the DataTable with appropriate data types
            dtPrintInfo.Columns.Add("ITM_BusDate", typeof(DateTime));
            dtPrintInfo.Columns.Add("ITM_WsID", typeof(string));
            dtPrintInfo.Columns.Add("ITM_ProcessingSite", typeof(string));
            dtPrintInfo.Columns.Add("ITM_TransportID", typeof(string));
            dtPrintInfo.Columns.Add("ITM_BatchDirectory", typeof(string));
            dtPrintInfo.Columns.Add("ITM_BatchNum", typeof(string));
            dtPrintInfo.Columns.Add("ITM_TransNum", typeof(string));
            dtPrintInfo.Columns.Add("ITM_DIN", typeof(string));
            dtPrintInfo.Columns.Add("ITM_ItemType", typeof(string));
            dtPrintInfo.Columns.Add("ITM_ProcMode", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Fld1", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Fld2", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Fld3", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Fld4", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Fld5", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Fld6", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Fld7", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Fld8", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Fld9", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Fld10", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Fld11", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Fld12", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Amount", typeof(string)); // Changed from decimal to string if you are formatting the amount
            dtPrintInfo.Columns.Add("ITM_BundleID", typeof(string));
            dtPrintInfo.Columns.Add("ITM_CheckType", typeof(string));
            dtPrintInfo.Columns.Add("ITM_UIC", typeof(string));
            dtPrintInfo.Columns.Add("ITM_RefNum", typeof(string));
            dtPrintInfo.Columns.Add("ITM_DI_Format", typeof(string));
            dtPrintInfo.Columns.Add("ITM_DI_Desc", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Returned", typeof(string));
            dtPrintInfo.Columns.Add("ITM_ReturnedDate", typeof(DateTime));
            dtPrintInfo.Columns.Add("ITM_ReturnedReason", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Ref1", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Ref2", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Ref3", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Ref4", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Ref5", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Ref6", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Ref7", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Ref8", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Ref9", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Ref10", typeof(string));
            dtPrintInfo.Columns.Add("ITM_NCF", typeof(string));
            dtPrintInfo.Columns.Add("ITM_ImageFileName", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Fr_ImgOffset", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Fr_ImgSize", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Rr_ImgOffset", typeof(string));
            dtPrintInfo.Columns.Add("ITM_ProcessChkType", typeof(string));
            dtPrintInfo.Columns.Add("PrimaryName", typeof(string));
            dtPrintInfo.Columns.Add("SecondaryName1", typeof(string));
            dtPrintInfo.Columns.Add("SecondaryName2", typeof(string));
            dtPrintInfo.Columns.Add("SecondaryName3", typeof(string));
            dtPrintInfo.Columns.Add("SaveDate", typeof(string)); // Date as string if you are formatting it as "dd/MM/yyyy"
            dtPrintInfo.Columns.Add("Indicator", typeof(string));
            dtPrintInfo.Columns.Add("ITM_TransSeqNum", typeof(string));
            dtPrintInfo.Columns.Add("UniqueIDPair", typeof(string));
            dtPrintInfo.Columns.Add("ITM_PERunNum", typeof(string));
            dtPrintInfo.Columns.Add("ITM_RunNum", typeof(string));
            dtPrintInfo.Columns.Add("ITM_Rejected", typeof(string));
            dtPrintInfo.Columns.Add("ITM_RejectReason", typeof(string));
            dtPrintInfo.Columns.Add("ProductType", typeof(string));

            return dtPrintInfo;
        }
    }
}