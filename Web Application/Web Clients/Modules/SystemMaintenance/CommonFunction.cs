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


namespace UBPCWeb.Modules.SystemMaintenance
{
    public class CommonFunction
    {
        private static SQLDBHelper dbHelperObj = new SQLDBHelper();
        private static string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();

        public static jQueryDataTableGridModel GetListingPageData(String[] paramList)
        {
            try
            {
                DataTable mainParamTable = GetAllParameters(paramList);

                var query = (from DataRow row in mainParamTable.Rows
                             select new ParamModel
                             {
                                 TempID = row["TMPID"].ToString(),
                                 ParamName = HttpUtility.HtmlDecode(row["PAR_ParamName"].ToString()),
                                 Value1 = HttpUtility.HtmlDecode(row["PAR_Value1"].ToString()),
                                 Value2 = HttpUtility.HtmlDecode(row["PAR_Value2"].ToString()),
                                 Comment = HttpUtility.HtmlDecode(row["PAR_Comment"].ToString())                                
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

        public static DataTable GetAllParameters(String[] filterList)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                string parName = string.IsNullOrEmpty(filterList[0].ToString()) ? "" : filterList[0].ToString();
                string parVal1 = string.IsNullOrEmpty(filterList[1].ToString()) ? "" : filterList[1].ToString();
                string parVal2 = string.IsNullOrEmpty(filterList[2].ToString()) ? "" : filterList[2].ToString();

                //Sql Parameter
                Dictionary<string, string> dictSqlParams = new Dictionary<string, string>();

                stmt = Resource.stmtGetAllParameter;

                if (parName.Trim().Length > 0)
                {
                    stmt += stmt.Contains("WHERE") ? " AND PAR_ParamName LIKE '%' + @ParName + '%'" : " WHERE PAR_ParamName LIKE '%' + @ParName + '%'";
                    dictSqlParams.Add("@ParName", parName);
                }

                if (parVal1.Trim().Length > 0)
                {
                    stmt += stmt.Contains("WHERE") ? " AND PAR_Value1 LIKE '%' + @ParVal1 + '%'" : " WHERE PAR_Value1 LIKE '%' + @ParVal1 + '%'";
                    dictSqlParams.Add("@ParVal1", parVal1);
                }

                if (parVal2.Trim().Length > 0)
                {
                    stmt += stmt.Contains("WHERE") ? " AND PAR_Value2 LIKE '%' + @ParVal2 + '%'" : " WHERE PAR_Value2 LIKE '%' + @ParVal2 + '%'";
                    dictSqlParams.Add("@ParVal2", parVal2);
                }

                var sqlParameterCollection = new List<SqlParameter>();
                foreach (var parameter in dictSqlParams)
                {
                    sqlParameterCollection.Add(new SqlParameter(parameter.Key, parameter.Value));
                }

                IDbDataParameter[] param = sqlParameterCollection.ToArray();

                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt,param);

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

        public static DataTable GetParamInfoByID(string paramName)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtGetParameterInfo;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 50, "@ParamName", ParameterDirection.Input, paramName)
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
    }

    public class ParamModel
    {
        //TBL_PARAM
        public string TempID { get; set; }
        public string ParamName { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Comment { get; set; }
    }
}