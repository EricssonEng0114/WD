using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UBPC.Web.Collections
{
    public class RLWorksourceCollection : IEnumerable
    {
        private IDictionary<String, RLWorksourceInfo> _worksources = null;

        public int Count
        {
            get { return _worksources.Count; }
        }

        public RLWorksourceCollection(WebClientCollection clients)
        {
            _worksources = new Dictionary<String, RLWorksourceInfo>();

            FillDictionary(clients);
        }

        private static bool ColumnExists(SqlDataReader reader, string columnName)
        {
            using (var schemaTable = reader.GetSchemaTable())
            {
                if (schemaTable != null)
                    schemaTable.DefaultView.RowFilter = String.Format("ColumnName= '{0}'", columnName);

                return schemaTable != null && (schemaTable.DefaultView.Count > 0);
            }
        }

        private void FillDictionary(WebClientCollection clients)
        {
            foreach (WebClientInfo client in clients)
            {
                //skip ICPS as sICPS contain no Worksource table, skip CTB
                if (client.Code.Contains("ICPS") || client.Code.Contains("IRPS") || client.Code.Contains("CTB"))
                    continue;

                // Build the connection string based on the details
                // from the ClientInfo object
                SqlConnectionStringBuilder connString = new SqlConnectionStringBuilder();
                connString.DataSource = client.DBServerName;
                connString.InitialCatalog = client.DBName;
                connString.IntegratedSecurity = true;

                // Create and Sql Connection with the specified connection string
                using (SqlConnection connection = new SqlConnection(connString.ConnectionString))
                {
                    // Select Command will be the same for all databases
                    // as long as the table name are kept the same
                    SqlCommand command = new SqlCommand("SELECT WRK_WsID ,WRK_WorksourceName ,WRK_WorksourceAlias ,WRK_Comments ,WRK_CenterBSB ,ISNULL(WRK_AcctZone,'') AS WRK_AcctZone ,WRK_PVAssist FROM TBL_WORKSOURCE", connection);
                    connection.Open();

                    // Execute the sql command
                    SqlDataReader reader = command.ExecuteReader();

                    // Call the read method
                    while (reader.Read())
                    {
                        // Create a new RLWorksourceInfo object
                        String worksourceID = reader["WRK_WsID"] as String;
                        String worksourceName = reader["WRK_WorksourceName"] as String;
                        String worksourceAlias = reader["WRK_WorksourceAlias"] as String;
                        String comments = reader["WRK_Comments"] as String;
                        String centerBSB = reader["WRK_CenterBSB"] as String;
                        String acctZone = reader["WRK_AcctZone"] as String;

                        bool chqClr = false;
                        if (ColumnExists(reader, "WRK_ChqClearing")) 
                        {
                            chqClr = (reader["WRK_ChqClearing"] == DBNull.Value ? false : (Boolean)reader["WRK_ChqClearing"]);
                        }

                        RLWorksourceInfo worksource = new RLWorksourceInfo
                            (worksourceID, worksourceName, worksourceAlias, comments, centerBSB, acctZone,
                            reader["WRK_PVAssist"] == DBNull.Value ? false : (Boolean)reader["WRK_PVAssist"],
                           // reader["WRK_ChqClearing"] == null ? false : (reader["WRK_ChqClearing"] == DBNull.Value ? false : (Boolean)reader["WRK_ChqClearing"]), 
                            chqClr,
                            client);

                        // Add the worksource to the List
                        _worksources.Add(worksourceID, worksource);
                    }

                    // Close the reader
                    reader.Close();
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _worksources.Values.GetEnumerator();
        }

        public RLWorksourceInfo this[String worksourceID]
        {
            get { return _worksources[worksourceID]; }
        }
    }
}
