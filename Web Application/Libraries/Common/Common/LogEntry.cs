using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UBPC.Web.Common
{
    public class LogEntry
    {
        private Int32 logID;
        private LogEventType severity;
        private Boolean alert;
        private LogCallerID caller;
        private String clientCode;
        private String machineName;
        private String userName;
        private String message;
        private String data;
        private Exception exception;

        /// <summary>
        /// Initialise a new instance of a <see cref="LogEntry"/> class.
        /// </summary>
        public LogEntry()
        {
            this.logID = -1;
            this.severity = LogEventType.Information;
            this.alert = false;
            //this.caller = LogCallerID.Unknown;
            this.clientCode = String.Empty;
            this.machineName = Environment.MachineName;
            this.userName = String.Empty;
            this.message = String.Empty;
            this.data = String.Empty;
            this.exception = null;
        }

        #region Properties
        /// <summary>
        /// Get the LogID of this LogEntry (only available once the Write method is called)
        /// </summary>
        public Int32 LogID
        {
            get { return logID; }
        }

        /// <summary>
        /// Gets or sets the log entry severity as a <see cref="Severity"/> enumeration (Information, Warning, Error or Critical).
        /// </summary>
        public LogEventType Severity
        {
            get { return severity; }
            set { severity = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if this log entry is to be raised as an alert.
        /// </summary>
        public Boolean Alert
        {
            get { return alert; }
            set { alert = value; }
        }

        /// <summary>
        /// Gets or sets the log entry caller as a <see cref="LogCallerID"/> enumeration.
        /// </summary>
        public LogCallerID Caller
        {
            get { return caller; }
            set { caller = value; }
        }

        /// <summary>
        /// Gets or sets the client code relating to this log entry.
        /// </summary>
        public String ClientCode
        {
            get { return clientCode; }
            set { clientCode = value; }
        }

        /// <summary>
        /// Gets the machine name.
        /// </summary>
        public String MachineName
        {
            get { return machineName; }
        }

        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        public String UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        /// <summary>
        /// Gets or sets the message of this log entry.
        /// </summary>
        public String Message
        {
            get { return message; }
            set { message = value; }
        }

        /// <summary>
        /// Gets or sets the data string associated with this log entry
        /// </summary>
        public String Data
        {
            get { return data; }
            set { data = value; }
        }

        /// <summary>
        /// Gets or sets the exception associated with this Log Entry
        /// </summary>
        public Exception Exception
        {
            get { return exception; }
            set { exception = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Calls the static Write method of the <see cref="Logger"/> class to write the log entry.
        /// </summary>
        public void Write()
        {
            logID = Logger.Write(this);
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="LogEntry"/>, 
        /// using a default formatting template.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="LogEntry"/>.</returns>
        public override string ToString()
        {
            return String.Format(Resources.LogEntryFormatString, DateTime.Now.ToString(), severity.ToString(), caller.ToString(), clientCode, machineName, userName, message.Replace(Environment.NewLine, ";"), data, ExceptionManager.Format(exception).Replace(Environment.NewLine, ";"));
        }
        #endregion
    }



}
