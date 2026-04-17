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

namespace UBPCWeb.Modules.Announcement
{
    public class CommonFunction
    {
        private static SQLDBHelper dbHelperObj = new SQLDBHelper();

        private static string webDBConnStr = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();

        public static jQueryDataTableGridModel GetListingPageData(String[] paramList)
        {
            try
            {
                DataTable dtDB = GetAllAnouncement(paramList);

                var query = (from DataRow row in dtDB.Rows
                             select new AnnouncementModel
                             {
                                 AnnID =row["ANM_ID"].ToString(),
                                 AnnType = row["ANM_Type"].ToString(),
                                 Client = row["ANM_Client"].ToString(),
                                 Title = HttpUtility.HtmlDecode(row["ANM_Title"].ToString()),
                                 Desc = HttpUtility.HtmlDecode(row["ANM_Description"].ToString()),
                                 DateFrom = Convert.ToDateTime(row["ANM_CreatedFrom"].ToString()),
                                 DateTo = Convert.ToDateTime(row["ANM_CreatedTo"].ToString()),
                                 CreatedBy = row["ANM_CreatedBy"].ToString()
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

        public static DataTable GetAllAnouncement(String[] filterList)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                string client = string.IsNullOrEmpty(filterList[0].ToString()) ? "" : filterList[0].ToString();
                string title = string.IsNullOrEmpty(filterList[1].ToString()) ? "" : filterList[1].ToString();
                string desc = string.IsNullOrEmpty(filterList[2].ToString()) ? "" : filterList[2].ToString();
              //  string group = filterList[3].ToString().Equals("ALL") ? "" : filterList[3].ToString();


                string clientList = HttpContext.Current.Session["s_UserClients"] == null ? string.Empty : HttpContext.Current.Session["s_UserClients"].ToString();
                bool isForUnisys = HttpContext.Current.Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString());

                //Sql Parameter
                Dictionary<string, string> dictSqlParams = new Dictionary<string, string>();
                stmt = Resource.stmtGetAllAnnouncement;

                dictSqlParams.Add("@IsUnisys", (isForUnisys ? "1" : "0"));

                if (client.Trim().Length > 0)
                {
                    stmt += " AND ANM_Client=@Client";
                    dictSqlParams.Add("@Client", client);
                }

                if (title.Trim().Length > 0)
                {
                    stmt += " AND ANM_Title LIKE '%' + @Title + '%'";
                    dictSqlParams.Add("@Title", title);
                }

                if (desc.Trim().Length > 0)
                {
                    stmt += " AND ANM_Description LIKE '%' + @Desc +'%'";
                    dictSqlParams.Add("@Desc", desc);
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

        public static DataTable GetTodayAnouncement()
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {

                string clientList = HttpContext.Current.Session["s_UserClients"] == null ? string.Empty : HttpContext.Current.Session["s_UserClients"].ToString();
                bool isForUnisys = HttpContext.Current.Session["s_UserForUnisys"] == null ? false : Convert.ToBoolean(HttpContext.Current.Session["s_UserForUnisys"].ToString());

                stmt = Resource.stmtGetAnnouncementToday;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.Int16, 0, "@IsUnisys", ParameterDirection.Input, (isForUnisys?1:0)),
                    dbHelperObj.CreateParameter(DbType.String, 1000, "@ClientList", ParameterDirection.Input, clientList)
                };


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


        public static DataTable GetAnnounrcementInfoByID(string announceID)
        {
            IDbConnection iConn = dbHelperObj.initConnection(webDBConnStr);
            string stmt = string.Empty;

            try
            {
                stmt = Resource.stmtGetAnnouncementByID;

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.Int64, 0, "@ANMID", ParameterDirection.Input, announceID)
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

    public class AnnouncementModel
    {
        //TBL_ANNOUNCEMENT
        public string AnnID { get; set; }
        public string AnnType { get; set; }
        public string Title { get; set; }
        public string Client { get; set; }
        public string Desc { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string CreatedBy { get; set; }
    }
}