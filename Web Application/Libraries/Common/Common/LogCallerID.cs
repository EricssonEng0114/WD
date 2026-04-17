using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UBPC.Web.Common
{
    /// <summary>
    /// Identifes the caller who has requested the log.
    /// </summary>
    public enum LogCallerID
    {
        ALL,
        Reports,
        AnnouncementMaintenance,
        AuditLogViewer,
        AccessMaintenance,
        BatchMaintenance,
        SystemMaintenance,
        OperatorMaintenance,
        RejectedItemDecision,
        ActionedItemHistory,
        OutlookRecipientMaintenance,
        ReportRepository,
        BPOOutlookModule,
        ImageArchiveOutward,
        ImageArchiveInward,
        Dashboard,
        Login,
        Logout,
        Others
    }
}
