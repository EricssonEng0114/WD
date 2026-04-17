using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.VisualBasic;
using UBPC.Web.Common;

namespace UBPC.Web.Common
{
    public class DBConnectionInfo
    {

        #region Variables
        //Database connection
        private SQLDBHelper dbHelperObj = new SQLDBHelper();
        private System.Data.IDbTransaction dbTranObj;
        private IDbConnection dbConnObj;

        private string _clientConnStr = string.Empty;
        private string _clientCode = string.Empty;
        private string _webConnStr = string.Empty;

        #endregion

        #region Properties Implementation
        public string webConnStr
        {
            set { _webConnStr = value; }
            get { return _webConnStr; }
        }

        public string clientConnStr
        {
            set { _clientConnStr = value; }
            get { return _clientConnStr; }
        }

        public string clientCode
        {
            set { _clientCode = value; }
            get { return _clientCode; }
        }

        #endregion

        public String BuildClientConnectionString()
        {
            if (!string.IsNullOrEmpty(_webConnStr) && !string.IsNullOrEmpty(_clientCode) && !string.IsNullOrEmpty(_clientConnStr))
            {
                IDbConnection iConn = dbHelperObj.initConnection(_webConnStr);
                string stmt = string.Empty;

                try
                {
                    stmt = Resources.stmtGetClientConnectionString;

                    IDbDataParameter[] param = new[]{
                        dbHelperObj.CreateParameter(DbType.String, 10, "@ClientCode", ParameterDirection.Input, _clientCode)
                    };

                    DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);
                    return dtDB.Rows.Count > 0 ? dtDB.Rows[0][0].ToString() : null;
                }
                catch (Exception ex)
                {
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.Others;
                    log.ClientCode = clientCode;
                    log.Severity = LogEventType.Error;
                    log.Message = ex.Message;
                    log.Exception = ex;
                    log.Write();

                    return null;
                }
                finally
                {
                    iConn.Close();
                }
            }
            else 
            {
                return null;
            }
        }

        public String GetSiteConnectionString(bool isClientDB,string clientBank, string siteCode) 
        {
            if (!string.IsNullOrEmpty(_webConnStr))
            {
                IDbConnection iConn = dbHelperObj.initConnection(_webConnStr);
                string stmt = string.Empty;

                try
                {
                    stmt = Resources.stmtGetSiteClientConnectionString;

                    IDbDataParameter[] param = new[]{
                        dbHelperObj.CreateParameter(DbType.String, 3, "@SiteName", ParameterDirection.Input, siteCode),
                        dbHelperObj.CreateParameter(DbType.String, 10, "@Bank", ParameterDirection.Input, clientBank)

                    };

                    DataTable dtDB = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);
                    return dtDB.Rows.Count > 0 ? (isClientDB ? dtDB.Rows[0]["DB_ConnString_Client"].ToString() : dtDB.Rows[0]["DB_ConnString_UVRPS"].ToString()) : "";
                }
                catch (Exception ex)
                {
                    LogEntry log = new LogEntry();
                    log.Caller = LogCallerID.Others;
                    log.ClientCode = clientCode;
                    log.Severity = LogEventType.Error;
                    log.Message = ex.Message;
                    log.Exception = ex;
                    log.Write();

                    return null;
                }
                finally
                {
                    iConn.Close();
                }
            }
            else
            {
                return null;
            }
        }

        public String GetMainSite(string mainSiteParam, string clientCode) 
        {
            string mainSite = string.Empty;

            //KL1:OCBC|KL2:RHB,CIMB,MBB,CTB
            string[] siteList = mainSiteParam.Split('|');

            foreach (string siteInfo in siteList) 
            {
                if (siteInfo.Contains(clientCode.Trim())) 
                {
                    mainSite = siteInfo.Split(':')[0].ToString();
                    break;
                }
            }

            return mainSite;
        }
    }
}
