using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UBPCWeb.Model;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.VisualBasic;
using UBPC.Web.Common;

namespace UBPCWeb.Modules.AuditLogViewer
{
    public class CommonFunction
    {
        private static SQLDBHelper dbHelperObj = new SQLDBHelper();

        private static string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();

        public static jQueryDataTableGridModel GetListingPageData(String[] paramList)
        {
            //Param List
            //1 - Date Time, 2- severity, 3 - caller, 4 - client            

            try
            {
                DataTable dtDB = GetAllLogs(paramList);


                var query = (from DataRow row in dtDB.Rows
                             select new LogModel
                             {
                                 LogID = Convert.ToInt32(row["LogID"].ToString()),
                                 Caller = row["Caller"].ToString(),
                                 Client = row["ClientCode"].ToString(),
                                 Data = row["Data"].ToString(),
                                 //Exception = (byte[])row["Exception"],
                                 ExpMsg = row["Exception"] == System.DBNull.Value ?
                                     "" : ExceptionManager.Deserialize((byte[])row["Exception"]).ToString(),
                                 Msg = row["Message"].ToString(),
                                 Status = row["Severity"].ToString(),
                                 DateTime = (DateTime)row["Timestamp"],
                                 UsrName = row["UserName"].ToString()
   
                             });

                var gridData = new jQueryDataTableGridModel()
                {
                    aaData = query.ToList(),
                    iTotalDisplayRecords = query.Count(),
                    iTotalRecords = query.Count()
                };

                return gridData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable GetAllLogs()
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtGetAllAuditLog;
                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt);

                return dtDB;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                iConn.Close();
            }
        }

        public static DataTable GetAllLogs(String[] filterList)
        {
            //Param List
            //1 - Date Time, 2- severity, 3 - caller, 4 - client            

            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                string filterCriteria = string.Empty;
                string dateTime = filterList[0].ToString();
                string severity = filterList[1].ToString();
                string caller = filterList[2].ToString();
                string clientCode = filterList[3].ToString();
                
                string startDateTime = dateTime + " 00:00:00";
                string endDateTime = dateTime + " 23:59:59";

                //generate the sql statement here
                stmt = Resource.stmtGetAllAuditLogFilter;

                Dictionary<string, string> dictSqlParams = new Dictionary<string, string>();
                dictSqlParams.Add("@startDatetime", startDateTime);
                dictSqlParams.Add("@endDateTime", endDateTime);

                if (severity.ToLower() != "all")
                {
                    stmt += " AND severity=@severity";
                    dictSqlParams.Add("@severity", severity);
                }

                if (caller.ToLower() != "all")
                {
                    stmt += " AND caller=@caller";
                    dictSqlParams.Add("@caller", caller);
                }

                if (clientCode.ToLower() != "all")
                {
                    stmt += " AND clientCode=@clientCode";
                    dictSqlParams.Add("@clientCode", clientCode);
                }

                var sqlParameterCollection = new List<SqlParameter>();
                foreach (var parameter in dictSqlParams)
                {
                    sqlParameterCollection.Add(new SqlParameter(parameter.Key, parameter.Value));
                }

                IDbDataParameter[] paramAuditLog = sqlParameterCollection.ToArray();

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, paramAuditLog);

                return dtDB;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                iConn.Close();
            }
        }

        public static DataTable GetClientInfoList(bool isForUnisys)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = isForUnisys ? Resource.stmtGetClientInfoList : Resource.stmtGetClientInfoListExcUnisys;
                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt);

                return dtDB;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                iConn.Close();
            }
        }
     
        //Edited Shinyi CR
        public static DataTable GetAuditReportConnectionInfo(string reportCode)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtGetPrintAuditReportInfo;
                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 10, "@ReportCode", ParameterDirection.Input,reportCode)
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


    }

    public class LogModel
    {
        //TBL_AUDITLOG
        public int LogID { get; set; }
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
        public string Caller { get; set; }
        public string Client { get; set; }
        public string UsrName { get; set; }
        public string Msg { get; set; }
        public string Data { get; set; }
        public Byte[] Exception { get; set; }
        public string ExpMsg { get; set; }
    }
}