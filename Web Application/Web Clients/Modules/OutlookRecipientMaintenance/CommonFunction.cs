using System;
using System.Configuration;
using System.Data;
using System.Linq;
using UBPC.Web.Common;
using UBPCWeb.Model;

namespace UBPCWeb.Modules.OutlookRecipientMaintenance
{
    public class CommonFunction
    {
        // public static string selClientDBConnStr = string.Empty;
        private static SQLDBHelper dbHelperObj = new SQLDBHelper();

        private static string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();

        public static jQueryDataTableGridModel GetListingPageData(String[] paramList)
        {
            //Param List
            //0 - client code, 1 - email address

            try
            {
                DataTable mainTransDt = GetAllRecp(paramList);

                var query = (from DataRow row in mainTransDt.Rows
                             select new OutlookRecpMaintModel
                             {
                                 Id = row["Recp_ID"].ToString(),
                                 ClientCode = row["Recp_ClientCode"].ToString(),
                                 EmailAddr = row["Recp_EmailAddr"].ToString(),
                                 Site = row["Recp_Site"].ToString()
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

        public static DataTable GetAllRecpByClient(string clientCode, string[] siteLst)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtGetRecpByClient;
                var selectedSiteLst = (siteLst != null && siteLst.Length > 0) ?
                    string.Join(",", siteLst) :
                    null;

                IDbDataParameter[] param =
                    new[] {
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ClientCode", ParameterDirection.Input, clientCode),
                        dbHelperObj.CreateParameter(DbType.String, 4000, "@SiteList",
                        ParameterDirection.Input,  string.IsNullOrEmpty(selectedSiteLst) ? (object)DBNull.Value : selectedSiteLst)
                    };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);

                return dtDB;
            }
            catch (Exception ex)
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


        public static DataTable GetAllRecp(String[] filterList)
        {
            //Param List
            //0 - Client Code,1 - Email Address

            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                string filterCriteria = string.Empty;
                string clientCode = filterList[0].ToString();
                string emailAddr = filterList[1].ToString();
                string site = filterList[2].ToString();

                stmt = Resource.stmtGetAllRecipient;

                IDbDataParameter[] param =
                    new[] {
                        dbHelperObj.CreateParameter(DbType.String, 4, "@ClientCode", ParameterDirection.Input, clientCode),
                        dbHelperObj.CreateParameter(DbType.String, 40, "@EmailAddr", ParameterDirection.Input, emailAddr),
                        dbHelperObj.CreateParameter(DbType.String, 4, "@Site", ParameterDirection.Input, site)
                    };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.StoredProcedure, stmt, param);

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

        public static DataTable GetRecipientInfo(string recpID)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtGetRecipient;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.Int64, 8, "@Recp_ID", ParameterDirection.Input,recpID)
                };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);

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

        public static bool CheckisEmailAddrExist(string id, string emailAddr, string clientCode)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtEmailAddrExist;

                IDbDataParameter[] param = new[] {
                    dbHelperObj.CreateParameter(DbType.Int64, 8, "@ID", ParameterDirection.Input, id),
                    dbHelperObj.CreateParameter(DbType.String, 4, "@ClientCode", ParameterDirection.Input, clientCode),
                    dbHelperObj.CreateParameter(DbType.String, 50, "@EmailAddr", ParameterDirection.Input, emailAddr),
                };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);

                if (dtDB.Rows.Count > 0)
                {
                    return Convert.ToInt16(dtDB.Rows[0][0].ToString()) > 0;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                iConn.Close();
            }
        }

        public class OutlookRecpMaintModel
        {
            //TBL_REJECTITEMSTATUS
            public string Id { get; set; }
            public string ClientCode { get; set; }
            public string EmailAddr { get; set; }
            public string Site { get; set; }
        }
    }
}