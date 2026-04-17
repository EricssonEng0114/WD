using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.FriendlyUrls;

namespace UBPCWeb
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            var settings = new FriendlyUrlSettings();
            settings.AutoRedirectMode = RedirectMode.Off;// RedirectMode.Permanent;
            routes.EnableFriendlyUrls(settings);

            //Re-route url 
            routes.MapPageRoute("Login", "", "~/Default.aspx");
            routes.MapPageRoute("BackLogin", "Login", "~/Default.aspx");

            routes.MapPageRoute("Homepage", "Home", "~/Modules/Dashboard/Default.aspx");

            //Maintenance task
            routes.MapPageRoute("AnnoucementMaintenance", "Announcement", "~/Modules/Announcement/Default.aspx");
            routes.MapPageRoute("AnnoucementMaintenanceUpdate", "Announcement/{Id}", "~/Modules/Announcement/Upsert.aspx");

            routes.MapPageRoute("AccessMaintenance", "Access", "~/Modules/AccessMaintenance/Default.aspx");

            routes.MapPageRoute("OperatorMaintenance", "Operator", "~/Modules/OperatorMaintenance/Default.aspx");
            routes.MapPageRoute("OperatorMaintenanceUpdate", "Operator/{Id}", "~/Modules/OperatorMaintenance/Upsert.aspx");

            routes.MapPageRoute("SystemMaintenance", "System", "~/Modules/SystemMaintenance/Default.aspx");
            routes.MapPageRoute("SystemMaintenanceUpdate", "System/{Id}", "~/Modules/SystemMaintenance/Upsert.aspx");

            routes.MapPageRoute("BatchMaintenance", "Batch", "~/Modules/BatchMaintenance/Default.aspx");

            //Rejected Item Decision
            routes.MapPageRoute("RejectedItemDecision", "WebDecision", "~/Modules/RejectedItemDecision/Default.aspx");
            routes.MapPageRoute("RejectedItemDecisionValidateRLChequeMode", "WebDecisionR/{Id}", "~/Modules/RejectedItemDecision/ValidateRLReject.aspx");
            routes.MapPageRoute("RejectedItemDecisionValidateRLMultipleMode", "WebDecisionRM/{Id}", "~/Modules/RejectedItemDecision/ValidateRLRejectMultipleMode.aspx");
            routes.MapPageRoute("RejectedItemDecisionValidateDI", "WebDecisionD/{Id}", "~/Modules/RejectedItemDecision/ValidateDIReject.aspx");
            routes.MapPageRoute("RejectedItemDecisionValidatePV", "WebDecisionP/{Id}", "~/Modules/RejectedItemDecision/ValidatePVReject.aspx");


            //Actioned Item HIstory
            routes.MapPageRoute("ActionedItemHistory", "History", "~/Modules/ActionedItemHistory/Default.aspx");
            routes.MapPageRoute("ActionedHistoryValidateRLChequeMode", "HistoryR/{Id}", "~/Modules/ActionedItemHistory/ViewRLHistory.aspx");
            routes.MapPageRoute("ActionedHistoryValidateRLMultipleMode", "HistoryRM/{Id}", "~/Modules/ActionedItemHistory/ViewRLHistoryMultipleMode.aspx");
            routes.MapPageRoute("ActionedHistoryValidateDI", "HistoryD/{Id}", "~/Modules/ActionedItemHistory/ViewDIHistory.aspx");
            routes.MapPageRoute("ActionedHistoryValidatePV", "HistoryP/{Id}", "~/Modules/ActionedItemHistory/ViewPVHistory.aspx");

            //Outlook Recipient Maintenance
            routes.MapPageRoute("OutlookRecipientMaintenance", "OutlookRecipient", "~/Modules/OutlookRecipientMaintenance/Default.aspx");
            routes.MapPageRoute("OutlookRecipientMaintenanceUpdate", "OutlookRecipient/{Id}", "~/Modules/OutlookRecipientMaintenance/Detail.aspx");

            //BPO Outlook Module
            routes.MapPageRoute("BPOOutlookModule", "BPOOutlook", "~/Modules/BPOOutlookModule/Default.aspx");

            //Monitoring
            routes.MapPageRoute("AuditLog", "Log", "~/Modules/AuditLogViewer/Default.aspx");

            //Operator
            routes.MapPageRoute("Report", "Reports", "~/Modules/Reports/Default.aspx");

            //Profile
            routes.MapPageRoute("ViewProfile", "Profile", "~/Modules/Profile/Default.aspx");

            //Handle Logout
            routes.MapPageRoute("LogoutTimeout", "LogoutTimeout/{UserId}/{ClientCode}/{SiteCode}/{expired}",
                                 "~/Logout.aspx");

            //Report
            routes.MapPageRoute("ViewReport", "ViewReport", "~/CRViewerReportForm/ReportViewer.aspx");
            //Added by boonchong PE - WD-24-003
            routes.MapPageRoute("PrintArchivalReport", "PrintArchivalReport", "~/CRViewerReportForm/PrintArchivalReportViewer.aspx");

            //ReportRepository 
            routes.MapPageRoute("ReportRepository", "ReportRepository", "~/Modules/ReportRepository/Default.aspx");

            //Image Archive(Outward)//Added by boonchong PE - WD-24-003
            routes.MapPageRoute("ImageArchiveOutward", "ImageArchiveOutward", "~/Modules/ImageArchiveOutward/Default.aspx");
            routes.MapPageRoute("ImageArchiveOutwardDetail", "ImageArchiveOutwardDetail", "~/Modules/ImageArchiveOutward/ImageArchiveOutwardDetail.aspx");

            //Image Archive(Inward)//Added by boonchong PE - WD-24-003
            routes.MapPageRoute("ImageArchiveInward", "ImageArchiveInward", "~/Modules/ImageArchiveInward/Default.aspx");
            routes.MapPageRoute("ImageArchiveInwardDetail", "ImageArchiveInwardDetail", "~/Modules/ImageArchiveInward/ImageArchiveInwardDetail.aspx");

        }
    }
}
