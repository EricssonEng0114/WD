using System;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace UBPC.Web.Collections
{

    public class ClientCollection<TClientInfo> : IEnumerable<TClientInfo>
          where TClientInfo : ClientInfo, new()
    {
        protected IDictionary<String, TClientInfo> _clients;

        public int Count
        {
            get { return _clients.Count; }
        }

        public ClientCollection()
        {//this will get all clients in the database
            _clients = new Dictionary<String, TClientInfo>();

            // No conenction string specified so just use the
            // default defined in the applications config file
            FillDictionary(ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString);
        }

        public ClientCollection(String userID)
        {
            _clients = new Dictionary<String, TClientInfo>();

            // No conenction string specified so just use the
            // default defined in the applications config file
            FillDictionary(ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString, userID);
        }

        public ClientCollection(String connectionString, String userID)
        {
            _clients = new Dictionary<String, TClientInfo>();

            FillDictionary(connectionString, userID);
        }

        private void FillDictionary(String connectionString, String userID)
        {
            if (userID == "Admin")
            {//this is our super user... it's not in the TBL_Operator table, must get everything
                FillDictionary(connectionString);
                return;
            }

            //Make the query to database to get client list for this user
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString);
            connection.Open();
            SqlCommand cmd = new SqlCommand(Resources.GetSelectedClientSql, connection);
            cmd.Parameters.Add("@userID", SqlDbType.Char).Value = userID;
            object result = cmd.ExecuteScalar();
            String[] selClientList = null;
            if (result != null)
            {
                String clientList;
                clientList = (String)result;
                selClientList = clientList.Split(',');

                // Make the query to the Database
                DataTable table = QueryDatabase(connectionString);

                // Work through each row and add a new Client
                foreach (DataRow row in table.Rows)
                {
                    // Create the ClientInfo based object
                    TClientInfo info = new TClientInfo();

                    foreach (String client in selClientList)
                    {
                        if (row["CLT_ClientCode"].ToString().Trim().Contains(client))
                        {
                            // Call the objects Fill method to get
                            // the Client data from the DataRow
                            info.Fill(row);

                            // Add the object to the List
                            _clients.Add(info.Code, info);
                        }
                    }
                }
            }
        }

        private void FillDictionary(String connectionString)
        {
            // Make the query to the Database
            DataTable table = QueryDatabase(connectionString);

            // Work through each row and add a new Client
            foreach (DataRow row in table.Rows)
            {
                // Create the ClientInfo based object
                TClientInfo info = new TClientInfo();

                // Call the objects Fill method to get
                // the Client data from the DataRow
                info.Fill(row);

                // Add the object to the List
                _clients.Add(info.Code, info);
            }
        }

        public System.Collections.Generic.IEnumerator<TClientInfo> GetEnumerator()
        {
            return _clients.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private DataTable QueryDatabase(String connectionString)
        {
            // Create the connection with the specified connection string
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter();

                // Select Command will be the same for all databases
                // as long as the table name are kept the same
                adapter.SelectCommand = new SqlCommand("SELECT * FROM TBL_CLIENTINFO", connection);

                // Fill a DataTable object with the Client Data
                DataTable table = new DataTable();
                adapter.Fill(table);

                // Return the DataTable for processing
                return table;
            }
        }

        public TClientInfo this[String clientCode]
        {
            //CLT_ClientDBName
                       // get { return _clients[clientCode]; }

            get { return _clients[clientCode]; }
        }
    }
}
