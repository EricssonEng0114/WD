using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
//using UBPC.Web.Common.ExceptionManager;


namespace UBPC.Web.Common
{
    /// <summary>
    /// Provides static methods for logging events to the database. 
    /// </summary>
    public static class Logger
    {
        public static int Write(Boolean alert, LogCallerID caller, String clientCode, String data, Exception e, String message, LogEventType severity, String userName)
        {
            LogEntry log = new LogEntry();
            log.Alert = alert;
            log.Caller = caller;
            log.ClientCode = clientCode;
            log.Data = data;
            log.Exception = e;
            log.Message = message;
            log.Severity = severity;
            log.UserName = userName;
            return Write(log);
        }

        public static int Write(Boolean alert, LogCallerID caller, String clientCode, String data, String message, LogEventType severity, String userName)
        {
            LogEntry log = new LogEntry();
            log.Alert = alert;
            log.Caller = caller;
            log.ClientCode = clientCode;
            log.Data = data;
            log.Message = message;
            log.Severity = severity;
            log.UserName = userName;
            return Write(log);
        }

        public static int Write(LogCallerID caller, String message)
        {
            return Write(caller, message, String.Empty);
        }

        public static int Write(LogCallerID caller, String message, String data)
        {
            return Write(caller, String.Empty, message, data);
        }

        public static int Write(LogCallerID caller, String clientCode, String message, String data)
        {
            return Write(caller, clientCode, message, data, String.Empty);
        }

        public static int Write(LogCallerID caller, String clientCode, String message, String data, String userName)
        {
            return Write(caller, clientCode, message, data, LogEventType.Information, userName);
        }

        public static int Write(LogCallerID caller, String message, LogEventType severity)
        {
            return Write(caller, message, String.Empty, severity);
        }

        public static int Write(LogCallerID caller, String message, LogEventType severity, String userName)
        {
            return Write(caller, message, String.Empty, severity, userName);
        }

        public static int Write(LogCallerID caller, String message, String data, LogEventType severity)
        {
            return Write(caller, message, data, severity, String.Empty);
        }

        public static int Write(LogCallerID caller, String message, String data, LogEventType severity, String userName)
        {
            return Write(caller, String.Empty, message, data, severity, userName);
        }

        public static int Write(LogCallerID caller, String clientCode, String message, String data, LogEventType severity)
        {
            return Write(caller, clientCode, message, data, severity, String.Empty);
        }

        public static int Write(LogCallerID caller, String clientCode, String message, String data, LogEventType severity, String userName)
        {
            return Write(false, caller, clientCode, data, null, message, severity, userName);
        }

        /// <summary>
        /// Writes a LogEntry object to the database.
        /// </summary>
        /// <param name="log">The LogEntry object to write to the database.</param>
        /// <returns>The LogID of the inserted row.</returns>
        public static int Write(LogEntry log)
        {
            int logID = -1;

            try
            {
                // Attempt to log to the Database
                logID = WriteDatabase(log);
            }
            catch
            {
                // We don't care about what happened, we just 
                // know that there was a problem writing the
                // log entry to the database so just revert
                // to a text file.
                //try
                //{
                WriteTextFile(log);
                //}
                //catch
                //{
                //    // Just in case writing to a Text File also fails
                //    // catch and swallow the Exception (I know, its not
                //    // a good thing to do but i don't want it to crash
                //    // the application...)
                //}
            }

            return logID;
        }

        /// <summary>
        /// Performs the underlying write of the LogEntry object to the database.
        /// </summary>
        /// <param name="log">The LogEntry object to write to the database.</param>
        /// <returns>The LogID of the inserted row.</returns>
        private static int WriteDatabase(LogEntry log)
        {
            int logID = -1;

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString))
            {
                // Create the command
                SqlCommand command = CreateSqlCommand(connection.CreateCommand(), log);

                // Open the connection
                connection.Open();

                // Execute the stored procedure
                command.ExecuteNonQuery();

                // Get the Log ID that was auto assigned
                logID = (int)command.Parameters["@LogID"].Value;
            }

            return logID;
        }

        /// <summary>
        /// Performs the underlying write of the LogEntry object to a text file.
        /// </summary>
        /// <param name="log">The LogEntry object to write to the text file.</param>
        private static void WriteTextFile(LogEntry log)
        {
            // Build the log file name based on the application path and
            // file name from the AppDomain.CurrentDomain object
            String fileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, Path.ChangeExtension(AppDomain.CurrentDomain.SetupInformation.ApplicationName, ".log"));

            // Set a flag if the log file exists or not so we can
            // decide whether we need to write the file header
            Boolean exists = File.Exists(fileName);

            // Write the log entry to the log file
            using (TextWriter textWriter = new StreamWriter(fileName, true))
            {
                if (!exists)
                    textWriter.WriteLine(Resources.LogEntryHeaderString);

                textWriter.WriteLine(log.ToString());
            }
        }

        /// <summary>
        /// Configures an SqlCommand from the LogEntry object to perform the database insert with.
        /// </summary>
        /// <param name="command">The SqlCommand to configure.</param>
        /// <param name="log">The LogEntry object to create the SqlCommand from.</param>
        /// <returns>The configured SqlCommand</returns>
        private static SqlCommand CreateSqlCommand(SqlCommand command, LogEntry log)
        {
            // Setup the SqlCommand to execute
            command.CommandText = "sp_WriteLog";
            command.CommandType = CommandType.StoredProcedure;

            // Setup the input parameters
            SqlParameter param = null;

            param = new SqlParameter("@Severity", SqlDbType.NVarChar, 32);
            param.Value = log.Severity.ToString();
            command.Parameters.Add(param);


            param = new SqlParameter("@Caller", SqlDbType.NVarChar, 32);
            param.Value = log.Caller.ToString();
            command.Parameters.Add(param);

            param = new SqlParameter("@ClientCode", SqlDbType.NChar);
            if (log.ClientCode == String.Empty)
                param.Value = DBNull.Value;
            else
                param.Value = log.ClientCode;
            command.Parameters.Add(param);

            param = new SqlParameter("@UserName", SqlDbType.NVarChar, 32);
            if ((log.UserName == String.Empty) || (log.UserName == null))
                param.Value = DBNull.Value;
            else
                param.Value = log.UserName;
            command.Parameters.Add(param);

            param = new SqlParameter("@Message", SqlDbType.NVarChar, 512);
            if (log.Message == String.Empty)
            {
                if (log.Exception != null)
                    param.Value = log.Exception.Message;
                else
                    param.Value = DBNull.Value;
            }
            else
                param.Value = log.Message;
            command.Parameters.Add(param);

            param = new SqlParameter("@Data", SqlDbType.NVarChar, 1024);
            if (log.Data == String.Empty)
                param.Value = DBNull.Value;
            else
                param.Value = log.Data;
            command.Parameters.Add(param);

            param = new SqlParameter("@Exception", SqlDbType.VarBinary);
            if (log.Exception == null)
                param.Value = DBNull.Value;
            else
                param.Value = ExceptionManager.Serialize(log.Exception);
            command.Parameters.Add(param);

            // Setup the output parameter which will return the LogID
            param = new SqlParameter("@LogID", SqlDbType.Int);
            param.Direction = ParameterDirection.Output;
            command.Parameters.Add(param);

            return command;
        }
    }
}
