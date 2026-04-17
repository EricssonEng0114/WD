using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UBPC.Web.Common
{
    /// <summary>
    /// Identifies the type of Event that has cause the log.
    /// </summary>
    public enum LogEventType
    {
        /// <summary>
        /// Informational message.
        /// </summary>
        Information,
        /// <summary>
        /// Noncritical problem.
        /// </summary>
        Warning,
        /// <summary>
        /// Recoverable error.
        /// </summary>
        Error,
        /// <summary>
        /// Fatal error or application crash.
        /// </summary>
        Critical
    }
}
