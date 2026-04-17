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


namespace UBPCWeb.Modules.AccessMaintenance
{
    public class CommonFunction
    {
        private static SQLDBHelper dbHelperObj = new SQLDBHelper();
        private static string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
        
        public static DataTable GetAllAccessList()
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtGetAll;

                bool isUnisys = HttpContext.Current.Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString());
                string client = isUnisys ? "USYS" : HttpContext.Current.Session["s_UserClients"].ToString().Trim();

                IDbDataParameter[] param =
                    new[] { 
                        dbHelperObj.CreateParameter(DbType.String, 4, "@Client", ParameterDirection.Input, client),
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


        public static DataTable GetAllAccessByGroup(string userGroup)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = string.Format(Resource.stmtGetAccessLevelModule,userGroup);

                bool isUnisys = HttpContext.Current.Session["s_UserForUnisys"] == null? false: Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString());
                string client = isUnisys ? "USYS" : HttpContext.Current.Session["s_UserClients"].ToString().Trim();

                IDbDataParameter[] param =
                  new[] { 
                        dbHelperObj.CreateParameter(DbType.String, 4, "@Client", ParameterDirection.Input, client)
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


        public static List<AccessModel> GetAccessFunctionName(DataTable allAccessDT, string filterMessage)
        {
            List<AccessModel> funcNameList = new List<AccessModel>();
            DataRow[] othRow = allAccessDT.Select(filterMessage);
            foreach (DataRow dr in othRow)
            {
                AccessModel accesObj = new AccessModel();
                accesObj.Category = dr["ACC_Category"].ToString();
                accesObj.ModuleName = dr["ModuleName"].ToString();
                accesObj.ModuleLink = dr["ModuleLink"].ToString();

                if (funcNameList.Contains(accesObj))
                    funcNameList.Remove(accesObj);

                funcNameList.Add(accesObj);
            }

            return funcNameList;

        }

    }

    public class AccessModel
    {
        //TBL_ACCESLEVEL
        public string AccessCode { get; set; }
        public string AccessName { get; set; }
        public string ModuleName { get; set; } // used in sidebar menu
        public string ModuleLink { get; set; } // used in sidebar menu
        public string Category { get; set; } // used in sidebar menu
        public bool Group1 { get; set; }
        public bool Group2 { get; set; }
        public bool Group3 { get; set; }
        public bool Group4 { get; set; }
        public bool Group5 { get; set; }
        public bool Group6 { get; set; }
        public bool Group7 { get; set; }
        public bool Group8 { get; set; }
        public bool Group9 { get; set; }

    }
}