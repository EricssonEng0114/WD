using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace UBPC.Web.Collections
{
    public class ClientInfo
    {
        private String _code;
        private String _module;
        private String _name;
        private String _alias;
        private String _dbName;
        private String _dbServerName;

        public ClientInfo()
        {
        }

        public ClientInfo(String clientCode,String clientModule, String clientName, String clientAlias, String clientDBName, String clientDBServerName)
        {
            _code = clientCode;
            _module = clientModule;
            _name = clientName;
            _alias = clientAlias;
            _dbName = clientDBName;
            _dbServerName = clientDBServerName;
        }

        public virtual void Fill(DataRow row)
        {
            // Fill the member variables with those from the DataRow
            _code = row["CLT_ClientCode"] as String;
            _module = row["CLT_ClientModule"] as String;
            _name = row["CLT_ClientName"] as String;
            _alias = row["CLT_ClientAlias"] as String;
            _dbName = row["CLT_ClientDBName"] as String;
            _dbServerName = row["CLT_ClientDBServerName"] as String;
        }

        public String Code
        {
            get { return _code.Trim(); }
        }

        public String Module
        {
            get { return _module.Trim(); }
        }

        public String Name
        {
            get { return _name; }
        }

        public String Alias
        {
            get { return _alias; }
        }

        public String DBName
        {
            get { return _dbName; }
        }

        public String DBServerName
        {
            get { return _dbServerName; }
        }

        public String SqlConnectionString
        {
            get
            {
                SqlConnectionStringBuilder connString = new SqlConnectionStringBuilder();
                connString.DataSource = _dbServerName;
                connString.InitialCatalog = _dbName;
                connString.IntegratedSecurity = true;

                return connString.ConnectionString;
            }
        }

        

    }
}
