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

namespace UBPCWeb.Modules.OperatorMaintenance
{
    public class CommonFunction
    {
        private static SQLDBHelper dbHelperObj = new SQLDBHelper();
        private static string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();

        public static jQueryDataTableGridModel GetListingPageData(String[] paramList)
        {
            try
            {
                //1-User ID, 2-User Name, 3-CLient, 4-Group
                DataTable mainOpTable = GetAllUserListingData(paramList);

                var query = (from DataRow row in mainOpTable.Rows
                             select new UserOperatorModel
                             {
                                 TempID = row["TMPID"].ToString(),
                                 UsrGp = Convert.ToInt16(row["UST_UserGroup"].ToString()),
                                 UsrID = HttpUtility.HtmlDecode(row["UST_UserID"].ToString()),
                                 UsrName = HttpUtility.HtmlDecode(row["UST_UserName"].ToString()),
                                 UsrClnts = row["UST_Clients"].ToString(),
                                 UsrComment = HttpUtility.HtmlDecode(row["UST_Comments"].ToString()),
                                 UsrForcedPwdChq = Convert.ToBoolean(row["UST_ForcePwdChg"].ToString()),
                                 UsrNoSession = Convert.ToInt16(row["UST_NoSession"].ToString()),
                                 UsrPwdChgDays = Convert.ToInt16(row["UST_PwdChgDaysValid"].ToString()),
                                 UsrPwdChgDt = string.IsNullOrEmpty(row["UST_PwdChgLastDate"].ToString()) ? DateTime.MinValue : Convert.ToDateTime(row["UST_PwdChgLastDate"].ToString()),
                                 PwsRetryCnt = Convert.ToInt16(row["UST_PwdChgRetryCnt"].ToString()),
                                 UsrLock = Convert.ToInt16(row["UST_Locked"].ToString()),
                                 UsrLastLog = string.IsNullOrEmpty(row["UST_LastLogin"].ToString()) ? DateTime.MinValue : Convert.ToDateTime(row["UST_LastLogin"].ToString())
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

        private static DataTable GetAllUserData(String[] filterList) 
        {
            //Param List
            //1 - Busdate, 2- client, 3 - presenting branch, 4 - category            

            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                string filterCriteria = string.Empty;
                string userID = string.IsNullOrEmpty(filterList[0].ToString()) ? "" : filterList[0].ToString();
                string userName = string.IsNullOrEmpty(filterList[1].ToString()) ? "" : filterList[1].ToString();
                string client = filterList[2].ToString().Equals("ALL") ? "" : filterList[2].ToString();
                string group = filterList[3].ToString().Equals("ALL") ? "" : filterList[3].ToString();

                //Sql Parameter
                Dictionary<string, string> dictSqlParams = new Dictionary<string, string>();
                
                //generate the sql statement here
                stmt = Resource.stmtGetAllUser;

                if (userID.Trim().Length > 0)
                {
                    stmt += stmt.Contains("WHERE")? " AND UST_UserID=@UserID" : " WHERE UST_UserID=@UserID";
                    dictSqlParams.Add("@UserID", userID);
                }

                if (userName.Trim().Length > 0)
                {
                    stmt += stmt.Contains("WHERE") ? " AND UST_UserName=@UserName" : " WHERE UST_UserName=@UserName";
                    dictSqlParams.Add("@UserName", userName);
                }

                if (client.Trim().Length > 0)
                {
                    stmt += stmt.Contains("WHERE") ? " AND UST_Clients like '%' + @Client + '%'" : " WHERE UST_Clients like '%' + @Client + '%'";
                    dictSqlParams.Add("@Client", client);
                }

                if (group.Trim().Length > 0)
                {
                    stmt += stmt.Contains("WHERE") ? " AND UST_UserGroup=@Group" : " WHERE UST_UserGroup=@Group";
                    dictSqlParams.Add("@Group", group);
                }

                var sqlParameterCollection = new List<SqlParameter>();
                foreach (var parameter in dictSqlParams)
                {
                    sqlParameterCollection.Add(new SqlParameter(parameter.Key, parameter.Value));
                }

                IDbDataParameter[] param = sqlParameterCollection.ToArray();

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
        
        private static DataTable GetAllUserData()
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtGetAllUser;
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

        private static DataTable GetAllUserListingData(String[] filterList)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                string filterCriteria = string.Empty;
                string userID = string.IsNullOrEmpty(filterList[0].ToString()) ? "" : filterList[0].ToString();
                string userName = string.IsNullOrEmpty(filterList[1].ToString()) ? "" : filterList[1].ToString();
                string client = filterList[2].ToString().Equals("ALL") ? "" : filterList[2].ToString();
                string group = filterList[3].ToString().Equals("ALL") ? "" : filterList[3].ToString();

                string clientList = HttpContext.Current.Session["s_UserClients"] == null ? string.Empty : HttpContext.Current.Session["s_UserClients"].ToString();
                bool  isForUnisys = HttpContext.Current.Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString());

                stmt = "sp_GetOperatorListByClient";// Resource.stmtGetAllUserTiedToAdmin;
                IDbDataParameter[] param = new[] { 
                    dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input, userID),
                    dbHelperObj.CreateParameter(DbType.String, 20, "@UserName", ParameterDirection.Input, userName),
                    dbHelperObj.CreateParameter(DbType.String, 10, "@Client", ParameterDirection.Input, client),
                    dbHelperObj.CreateParameter(DbType.String, 1, "@Group", ParameterDirection.Input, group),
                 dbHelperObj.CreateParameter(DbType.Int16, 0, "@ForUnisysUser", ParameterDirection.Input, (isForUnisys?1:0))
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

        public static DataTable GetUserInfoWithUserID(string loginUsrID)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resources.Resource.stmtGetUserInfo;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input,loginUsrID)
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

        public static string[] GetUserPasswordHistory(ref int pwPosition, string userID)
        {
            //stmtGetUserLogin
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;
            string strPassword = string.Empty;
            string[] strPwdHistoryList = null;
            int intPassPos = 0;

            try
            {
                stmt = Resources.Resource.stmtGetUserLogin;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input,userID)
                };


                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);

                if (dtDB.Rows.Count > 0)
                {
                    DataRow dr = dtDB.Rows[0];
                    intPassPos = Convert.ToInt16(dr["UST_PwdPos"].ToString());

                    if (!string.IsNullOrEmpty(dr["UST_Pwd_0"].ToString().Trim()))
                        strPassword += UCrypt.Decrypt(dr["UST_Pwd_0"].ToString().Trim()) + ",";

                    if (!string.IsNullOrEmpty(dr["UST_Pwd_1"].ToString().Trim()))
                        strPassword += UCrypt.Decrypt(dr["UST_Pwd_1"].ToString().Trim()) + ",";

                    if (!string.IsNullOrEmpty(dr["UST_Pwd_2"].ToString().Trim()))
                        strPassword += UCrypt.Decrypt(dr["UST_Pwd_2"].ToString().Trim()) + ",";

                    if (!string.IsNullOrEmpty(dr["UST_Pwd_3"].ToString().Trim()))
                        strPassword += UCrypt.Decrypt(dr["UST_Pwd_3"].ToString().Trim()) + ",";

                    if (!string.IsNullOrEmpty(dr["UST_Pwd_4"].ToString().Trim()))
                        strPassword += UCrypt.Decrypt(dr["UST_Pwd_4"].ToString().Trim()) + ",";

                    if (!string.IsNullOrEmpty(dr["UST_Pwd_5"].ToString().Trim()))
                        strPassword += UCrypt.Decrypt(dr["UST_Pwd_5"].ToString().Trim()) + ",";

                    //Remove last comma
                    strPassword = strPassword.Remove(strPassword.Length - 1);
                }

                pwPosition = intPassPos;
                strPwdHistoryList = strPassword.Split(',');
            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.OperatorMaintenance;
                log.UserName = userID;
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

            return strPwdHistoryList;
        }
       
        public static bool CheckIsUsrLogin(string userID)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtIsUsrLogin;

                IDbDataParameter[] param = new[] { dbHelperObj.CreateParameter(DbType.String, 8, "@userID", ParameterDirection.Input, userID) };
                DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);

                if (dtDB.Rows.Count > 0)
                {
                    return Convert.ToBoolean(dtDB.Rows[0][0].ToString().Equals("1"));
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

        public static bool CheckIsUsrExist(string userID)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtUsrExisted;

                IDbDataParameter[] param = new[] { dbHelperObj.CreateParameter(DbType.String, 8, "@UserID", ParameterDirection.Input, userID) };
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
    }

    public class UserOperatorModel
    {
        //TBL_OPERATOR
        public string TempID {get;set;}
        public string UsrID { get; set; }
        public string UsrName { get; set; }
        public int UsrGp { get; set; }
        public DateTime UsrPwdChgDt { get; set; }
        public int UsrPwdChgDays { get; set; }
        public int PwsRetryCnt { get; set; }
        public string UsrComment { get; set; }
        public bool UsrForcedPwdChq { get; set; }
        public int UsrNoSession { get; set; }
        public string UsrClnts { get; set; }
        public string UsrClntsList { get; set; }
        public int UsrLock{get;set;}
        public DateTime UsrLastLog { get; set; }


    }
}