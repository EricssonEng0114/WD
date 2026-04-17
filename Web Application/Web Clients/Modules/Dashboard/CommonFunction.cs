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
using UBPC.Encryption;

namespace UBPCWeb.Modules.Dashboard
{
    public class CommonFunction
    {
        private static SQLDBHelper dbHelperObj = new SQLDBHelper();
        private static string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();

        public static jQueryDataTableGridModel GetListingPageData(String[] paramList)
        {
            try
            {
                //1-Client
                DataTable mainOpTable = GetDashboardMonitorData(paramList);

                var query = (from DataRow row in mainOpTable.Rows
                             select new DashboardMonitorModel
                             {
                                 ProcessingBranch = row["ProcessingBranch"].ToString(),
                                 SOD = row["SOD"].ToString(),
                                 EOD = row["EOD"].ToString(),
                                 NCF = row["NCF"].ToString(),
                                 TotalBatches = Convert.ToInt32(row["TotalBatches"].ToString()),
                                 EmptyBatches = Convert.ToInt32(row["EmptyBatches"].ToString())
                             });

                HttpContext.Current.Session["StartOfDay"] = query.Where(x => x.SOD == "True").Count().ToString();
                HttpContext.Current.Session["EndOfDay"] = query.Where(x => x.EOD == "True").Count().ToString();
                HttpContext.Current.Session["NCF"] = query.Where(x => x.NCF == "True").Count().ToString();
                HttpContext.Current.Session["TotalBranches"] = query.Count().ToString();

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

        private static DataTable GetDashboardMonitorData(String[] filterList)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = Resource.stmtSqlGetNCFBranchesListing;

            try
            {
                var client = filterList[0].ToString();
                var busDate = Convert.ToDateTime(filterList[1]);

                IDbDataParameter[] param = new[] {
                    dbHelperObj.CreateParameter(DbType.DateTime, 10, "@BusDate", ParameterDirection.Input, busDate),
                    dbHelperObj.CreateParameter(DbType.String, 20, "@ClientCode", ParameterDirection.Input, client)
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
    }

    public class DashboardMonitorModel
    {
        public string ProcessingBranch { get; set; }
        public string SOD { get; set; }
        public string EOD { get; set; }
        public string NCF { get; set; }
        public int TotalBatches { get; set; }
        public int EmptyBatches { get; set; }
    }
}