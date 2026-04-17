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
using UBPC.Web.Collections;

namespace UBPCWeb.Modules.Reports
{
    public class CommonFunction
    {
        private static WebClientCollection clients = null;
        private static RLWorksourceCollection worksources = null;
        private static SQLDBHelper dbHelperObj = new SQLDBHelper();
        private static string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();

        public static jQueryDataTableGridModel GetListingPageData(String[] paramList)
        {
            try
            {
                DataTable dtDB = GetAllReport(paramList);

                var query = (from DataRow row in dtDB.Rows
                             select new ReportModel
                             {
                                 ReportCode = row["RPT_ReportCode"].ToString(),
                                 ReportName = row["RPT_ReportName"].ToString(),
                                 ReportDesc = row["RPT_ReportDesc"].ToString(),
                                 ReportSrc = row["RPT_ReportSrc"].ToString(),
                                 ReportClient = row["RPT_ClientCode"].ToString()

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

        public static DataTable GetAllReport(String[] filterList)
        {
            //Param List
            //1 - Date Time, 2- client, 3 - worksource            

            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                string filterCriteria = string.Empty;
                string dateTime = filterList[0].ToString();
                string client = filterList[1].ToString();
                string worksource = filterList[2].ToString();

                //generate the sql statement here
                stmt = Resource.stmtGetAllReport;// "SELECT * from TBL_LOG WITH(NOLOCK) where TimeStamp>=@startDateTime AND TimeStamp <=@endDateTime";

                Dictionary<string, string> dictSqlParams = new Dictionary<string, string>();
                if (client.ToLower() != "all")
                {
                    stmt += " AND RPT_ClientCode=@client";
                    dictSqlParams.Add("@client", client);
                }
                

                var sqlParameterCollection = new List<SqlParameter>();
                foreach (var parameter in dictSqlParams)
                {
                    sqlParameterCollection.Add(new SqlParameter(parameter.Key, parameter.Value));
                }

                IDbDataParameter[] paramRpt = sqlParameterCollection.ToArray();

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, paramRpt);

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
        
        public static DataTable GetCRReportConnectionInfo(string reportCode, string clientCode)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtGetCRReportInfo;
                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 10, "@ReportCode", ParameterDirection.Input,reportCode),
                    dbHelperObj.CreateParameter(DbType.String, 10, "@ClientCode", ParameterDirection.Input,clientCode)
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

        public static String BuildConnectionString(string clientCode)
        {
            IDbConnection iConn = dbHelperObj.initConnection(ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString());
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtGetClientConnectionString;
                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 10, "@ClientCode", ParameterDirection.Input,clientCode)
                };

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);
                return dtDB.Rows.Count > 0 ? dtDB.Rows[0][0].ToString() : null;
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

        public static DataTable GetWorksourceList(string userID, string clientCode) 
        {
            clients = new WebClientCollection(userID);
            worksources = new RLWorksourceCollection(clients);


            DataTable wsDt = new DataTable();
            wsDt.Columns.Add("WsID");
            wsDt.Columns.Add("WsName");
            DataRow _row1 = wsDt.NewRow();
            _row1[0] = "ALL";
            _row1[1] = "ALL";
            wsDt.Rows.Add(_row1);

            List<String> wrkList = new List<String>();

            //get the selected clients details
            clients = new WebClientCollection(HttpContext.Current.Session["s_UserID"].ToString());

            WebClientInfo client = clients[clientCode];
            RLWorksourceInfo info = new RLWorksourceInfo("0", "ALL", "", "", "", "", false, false, client);
            info.FormatString = "{0}\t{1}";
            
            foreach (RLWorksourceInfo worksource in worksources)
            {
                if (worksource.Client.Code.Equals(client.Code))
                {
                    DataRow _row = wsDt.NewRow();

                    if (client.Code.Equals("OCBC") && (worksource.WorksourceID.Equals("022") || worksource.WorksourceID.Equals("023") || worksource.WorksourceID.Equals("024")))
                    {
                        //dont add for MOPO, NCI and FX
                    }
                    else if (client.Code.Equals("MBB") && (worksource.WorksourceID.Equals("009")))
                    {
                        //dont add for NCI
                    }
                    else if (client.Code.Equals("RHB") && (worksource.WorksourceID.Equals("038") || worksource.WorksourceID.Equals("039") || worksource.WorksourceID.Equals("041")))
                    {
                        //dont add for MOPO, NCI
                    }
                    else if (client.Code.Equals("CIMB") && (worksource.WorksourceID.Equals("081") || worksource.WorksourceID.Equals("082") || worksource.WorksourceID.Equals("083")))
                    {
                        //dont add for MOPO, NCI
                    }
                    else 
                    {
                        _row[0] = worksource.WorksourceID;
                        _row[1] = worksource.WorksourceName + "(" + worksource.WorksourceID + ")";
                        wsDt.Rows.Add(_row);
                    }

                }
            }

            return wsDt;
        }
    }

    public class ReportModel
    {
        //TBL_REPORT
        public string ReportCode { get; set; }
        public string ReportDesc { get; set; }
        public string ReportName { get; set; }
        public string ReportSrc { get; set; }
        public string ReportClient { get; set; }

    }
}