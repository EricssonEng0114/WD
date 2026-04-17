using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using UBPC.Web.Common;
using UBPCWeb.Model;

namespace UBPCWeb.Modules.BPOOutlookModule
{
    public class CommonFunction
    {
        // public static string selClientDBConnStr = string.Empty;
        private static SQLDBHelper dbHelperObj = new SQLDBHelper();

        private static string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
    
        public static DataTable GetReportListByClient(string clientCode,DateTime busDate)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;
            try
            {
                stmt = Resource.stmtGetClientReportList;

                IDbDataParameter[] param =
                    new[] { 
                        dbHelperObj.CreateParameter(DbType.String, 10, "@ClientCode", ParameterDirection.Input, clientCode),
                        dbHelperObj.CreateParameter(DbType.DateTime, 99, "@BusDate", ParameterDirection.Input, busDate),
                        dbHelperObj.CreateParameter(DbType.String, 50, "@ArchivalDBName", ParameterDirection.Input, string.Empty)
                    };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);

                return dtDB;
            }
            catch(Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.BPOOutlookModule;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                throw ex;
            }
            finally
            {
                iConn.Close();
            }
        }
    }
}