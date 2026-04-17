using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.VisualBasic;
using UBPC.Encryption;
using UBPC.Web.Common;
using UBPCWeb.Modules;
using UBPCWeb.Model;

namespace UBPCWeb
{
    /// <summary>
    /// Summary description for WebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebService : System.Web.Services.WebService
    {
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        private string userID = string.Empty;
        private string userGroup = string.Empty;
        private string clientCode = "OCBC";

        [WebMethod(EnableSession = true)]
        public void LoadGridDataP(string ModuleName, string Param, int ParamCount)
        {
            bool isPageValid = false;


            //Check if sesssion killed and valid user
            userID = Session["s_UserID"] == null ? string.Empty : Session["s_UserID"].ToString();
            userGroup = Session["s_UserGroup"] == null ? string.Empty : Session["s_UserGroup"].ToString();

            if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(userGroup))
            {
                PageValidatorResult validatorResult;

                //For password chnge, validate password by encryption class
                validatorResult = PageValidator.Validate(webDBConnStr, userGroup, ModuleName);
                isPageValid = validatorResult.Valid;

                if (!isPageValid && ModuleName != "Dashboard")
                {
                    Context.Response.Write("You have no access right for this page.");
                }
                else
                {
                    Session["s_GeneralMsg"] = string.Empty;
                    isPageValid = true;
                    jQueryDataTableGridModel objData = new jQueryDataTableGridModel();
                    LogCallerID logID = LogCallerID.Others;
                    string[] strParamList = Param.Split('|');

                    try
                    {
                        switch (ModuleName)
                        {
                            case "AnnouncementMaintenance":
                                logID = LogCallerID.AnnouncementMaintenance;
                                if (!strParamList.Count().Equals(ParamCount))
                                {
                                    isPageValid = false;
                                    Logger.Write(false, logID, clientCode, "Load Grid", "Invalid number of parameters passed to grid", LogEventType.Error, userID);
                                    objData = null;
                                }
                                else
                                {
                                    objData = Modules.Announcement.CommonFunction.GetListingPageData(strParamList);
                                }

                                break;
                            case "OperatorMaintenance":
                                logID = LogCallerID.OperatorMaintenance;
                                if (!strParamList.Count().Equals(ParamCount))
                                {
                                    isPageValid = false;
                                    Logger.Write(false, logID, clientCode, "Load Grid", "Invalid number of parameters passed to grid", LogEventType.Error, userID);
                                    objData = null;
                                }
                                else
                                {
                                    objData = Modules.OperatorMaintenance.CommonFunction.GetListingPageData(strParamList);
                                }
                                break;

                            case "Dashboard":
                                logID = LogCallerID.Dashboard;
                                if (!strParamList.Count().Equals(ParamCount))
                                {
                                    isPageValid = false;
                                    Logger.Write(false, logID, clientCode, "Load Grid", "Invalid number of parameters passed to grid", LogEventType.Error, userID);
                                    objData = null;
                                }
                                else
                                {
                                    objData = Modules.Dashboard.CommonFunction.GetListingPageData(strParamList);
                                }
                                break;

                            case "SystemMaintenance":
                                logID = LogCallerID.SystemMaintenance;
                                if (!strParamList.Count().Equals(ParamCount))
                                {
                                    isPageValid = false;
                                    Logger.Write(false, logID, clientCode, "Load Grid", "Invalid number of parameters passed to grid", LogEventType.Error, userID);
                                    objData = null;
                                }
                                else
                                {
                                    objData = Modules.SystemMaintenance.CommonFunction.GetListingPageData(strParamList);
                                }
                                break;
                            case "BatchMaintenance":
                                logID = LogCallerID.BatchMaintenance;
                                if (!strParamList.Count().Equals(ParamCount))
                                {
                                    isPageValid = false;
                                    Logger.Write(false, logID, clientCode, "Load Grid", "Invalid number of parameters passed to grid", LogEventType.Error, userID);
                                    objData = null;
                                }
                                else
                                {
                                    objData = Modules.BatchMaintenance.CommonFunction.GetListingPageData(strParamList);
                                }
                                break;
                            case "AuditLogViewer":
                                logID = LogCallerID.AuditLogViewer;
                                if (!strParamList.Count().Equals(ParamCount))
                                {
                                    isPageValid = false;
                                    Logger.Write(false, logID, clientCode, "Load Grid", "Invalid number of parameters passed to grid", LogEventType.Error, userID);
                                    objData = null;
                                }
                                else
                                {
                                    objData = Modules.AuditLogViewer.CommonFunction.GetListingPageData(strParamList);
                                }
                                break;

                            case "Reports":
                                logID = LogCallerID.Reports;
                                if (!strParamList.Count().Equals(ParamCount))
                                {
                                    isPageValid = false;
                                    Logger.Write(false, logID, clientCode, "Load Grid", "Invalid number of parameters passed to grid", LogEventType.Error, userID);
                                    objData = null;
                                }
                                else
                                {
                                    objData = Modules.Reports.CommonFunction.GetListingPageData(strParamList);
                                }
                                break;
                            case "RejectedItemDecision":
                                logID = LogCallerID.RejectedItemDecision;
                                if (!strParamList.Count().Equals(ParamCount))
                                {
                                    isPageValid = false;
                                    Logger.Write(false, logID, clientCode, "Load Grid", "Invalid number of parameters passed to grid", LogEventType.Error, userID);
                                    objData = null;
                                }
                                else
                                {
                                    objData = Modules.RejectedItemDecision.CommonFunction.GetListingPageData(strParamList);
                                }
                                break;
                            case "ActionedItemHistory":
                                logID = LogCallerID.ActionedItemHistory;
                                if (!strParamList.Count().Equals(ParamCount))
                                {
                                    isPageValid = false;
                                    Logger.Write(false, logID, clientCode, "Load Grid", "Invalid number of parameters passed to grid", LogEventType.Error, userID);
                                    objData = null;
                                }
                                else
                                {
                                    objData = Modules.ActionedItemHistory.CommonFunction.GetListingPageData(strParamList);
                                }
                                break;

                            case "OutlookRecipientMaintenance":
                                logID = LogCallerID.OutlookRecipientMaintenance;
                                if (!strParamList.Count().Equals(ParamCount))
                                {
                                    isPageValid = false;
                                    Logger.Write(false, logID, clientCode, "Load Grid", "Invalid number of parameters passed to grid", LogEventType.Error, userID);
                                    objData = null;
                                }
                                else
                                {
                                    objData = Modules.OutlookRecipientMaintenance.CommonFunction.GetListingPageData(strParamList);
                                }
                                break;

                            case "ReportRepository":
                                logID = LogCallerID.ReportRepository;
                                if (!strParamList.Count().Equals(ParamCount))
                                {
                                    isPageValid = false;
                                    Logger.Write(false, logID, clientCode, "Load Grid", "Invalid number of parameters passed to grid", LogEventType.Error, userID);
                                    objData = null;
                                }
                                else
                                {
                                    objData = Modules.ReportRepository.CommonFunction.GetListingPageData(strParamList);
                                }
                                break;
                            //Added by boonchong PE - WD-24-003
                            case "ImageArchiveOutward":
                                logID = LogCallerID.ImageArchiveOutward;
                                if (!strParamList.Count().Equals(ParamCount))
                                {
                                    isPageValid = false;
                                    Logger.Write(false, logID, clientCode, "Load Grid", "Invalid number of parameters passed to grid", LogEventType.Error, userID);
                                    objData = null;
                                }
                                else
                                {
                                    objData = Modules.ImageArchiveOutward.CommonFunction.GetListingPageData(strParamList);
                                }
                                break;
                            //Added by boonchong PE - WD-24-003
                            case "ImageArchiveInward":
                                logID = LogCallerID.ImageArchiveInward;
                                if (!strParamList.Count().Equals(ParamCount))
                                {
                                    isPageValid = false;
                                    Logger.Write(false, logID, clientCode, "Load Grid", "Invalid number of parameters passed to grid", LogEventType.Error, userID);
                                    objData = null;
                                }
                                else
                                {
                                    objData = Modules.ImageArchiveInward.CommonFunction.GetListingPageData(strParamList);
                                }
                                break;
                        }

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = int.MaxValue;
                        Context.Response.Write(js.Serialize(objData));
                    }
                    catch (Exception ex)
                    {
                        LogEntry log = new LogEntry();
                        log.Caller = logID;
                        log.ClientCode = clientCode;
                        log.UserName = userID;
                        log.Severity = LogEventType.Error;
                        log.Message = ex.Message;
                        log.Exception = ex;
                        log.Write();


                    }
                }
            }
            else
            {
                //invalid user - back to login page
                Context.Response.Write("You have no access right for this page.");
            }

        }

        [WebMethod(EnableSession = true)]
        public void LoadSecondImg()
        {

            //type|frontOffset|frontLength|rearOffset|rearLength|batch No
            //   String id = context.Request.QueryString["Id"];

            //  string userid =  context.Session["s_UserID"].ToString();



            //write your handler implementation here.
            //load from fim rim
            byte[] imgByte;

            string rlpsImgPath = HttpContext.Current.Server.MapPath("/IFS_UVRPS");//virtual directories
            string initTempPath = @"C:\Temp";
            bool isFront = true;
            bool isJpeg = false;// HttpContext.Current.Session["isJpeg"].Equals("1");// true;

            //front tiff
            //string frontOffset = "4837";
            //string frontLength = "8022";
            //string rearOffset = "3701";
            //string rearLength = "3210";
            //string imgFilName = "40400001";

            //front jpeg
            string frontOffset = isJpeg ? "32893" : "4837";
            string frontLength = isJpeg ? "16660" : "8022";
            string rearOffset = isJpeg ? "13180" : "3701";
            string rearLength = isJpeg ? "10035" : "3210";
            string imgFilName = "40400001";

            //IFS Path  - [s_CurIFSPath]
            //Session Value - [s_TopImage] = isJpeg;isFront;frontOffset;frontSize;rearOffset;rearSize;batchNo
            //Session Value - [s_BottomImage] = isJpeg;isFront;frontOffset;frontSize;rearOffset;rearSize;batchNo

            ChequeViewerCtrl chqImgObj = new ChequeViewerCtrl();
            imgByte = chqImgObj.LoadImage(isFront, isJpeg, rlpsImgPath.Trim(), imgFilName.Trim(), Convert.ToInt64(frontOffset.Trim()),
                 Convert.ToInt64(rearOffset.Trim()), Convert.ToInt32(frontLength.Trim()), Convert.ToInt32(rearLength.Trim()),
                 Convert.ToDateTime("2019-02-13 00:00:00.000"), "73070123", 1,
                 initTempPath);

            //response.AddHeader("content-disposition", "inline;filename=myimage.jpeg");
            HttpContext.Current.Response.AppendHeader("Content-Length", imgByte.Length.ToString());
            HttpContext.Current.Response.ContentType = "image/tiff";
            HttpContext.Current.Response.OutputStream.Write(imgByte, 0, imgByte.Length);
        }

        [WebMethod(EnableSession = true)]
        public string UpdateLabels()
        {
            var data = new
            {
                StartOfDay = HttpContext.Current.Session["StartOfDay"]?.ToString() ?? "0",
                EndOfDay = HttpContext.Current.Session["EndOfDay"]?.ToString() ?? "0",
                NCF = HttpContext.Current.Session["NCF"]?.ToString() ?? "0",
                TotalBranches = HttpContext.Current.Session["TotalBranches"]?.ToString() ?? "0"
            };

            return new JavaScriptSerializer().Serialize(data);
        }

    }
}
