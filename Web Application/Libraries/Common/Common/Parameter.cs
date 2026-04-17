using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UBPC.Web.Common
{
    public static class Parameters
    {
        /// <summary>
        /// Checks if the given ParamKey exists in the Client DB TBL_PARAM
        /// </summary>
        /// <param name="ClientConnectionStr">client DB connection string</param>
        /// <param name="ParamKey">required parameter key</param>
        /// <returns></returns>
        public static Boolean Exists(string ClientConnectionStr, string ParamKey)
        {
            // Connect to the Client specific database to query the parameter table
            using (SqlConnection connection = new SqlConnection(ClientConnectionStr))
            {
                // Setup the SQL Command
                SqlCommand command = new SqlCommand(Resources.DoesParamExistSql, connection);

                // Add the search parameter key
                command.Parameters.Add("@ParamKey", SqlDbType.VarChar).Value = ParamKey;

                // Open the connection
                connection.Open();

                // If the query affects 1 or more rows then the business date exists               
                if ((int)command.ExecuteScalar() == 0)
                    return false;
                else
                    return true;
            }
        }
        /// <summary>
        ///Retrieves the first or second parameter value from the Client DB TBL_PARAM of ParamKey. 
        /// Returns null if the ParamKey is not defined.
        /// </summary>
        /// <param name="ClientConnectionStr">client DB connection string</param>
        /// <param name="ParamKey">required parameter key</param>
        /// <returns></returns>
        public static string GetParamValue(string ClientConnectionStr, string ParamKey)
        {
            return GetParamValue(ClientConnectionStr, ParamKey, true);
        }

        /// <summary>
        /// Retrieves the first or second parameter value from the Client DB TBL_PARAM of ParamKey. 
        /// Returns null if the ParamKey is not defined.
        /// </summary>
        /// <param name="ClientConnectionStr">client DB connection string</param>
        /// <param name="ParamKey">required parameter key</param>
        /// <param name="FirstValue">if FirstValue is true, returns the PAR_VALUE1, else returns PAR_VALUE2</param>
        /// <returns></returns>
        public static string GetParamValue(string ClientConnectionStr, string ParamKey, bool FirstValue)
        {
            string ParamVal = string.Empty;

            // Connect to the Database to query the Business Date
            using (SqlConnection connection = new SqlConnection(ClientConnectionStr))
            {
                // Setup the SQL Command
                SqlCommand command = new SqlCommand((FirstValue ? Resources.GetParamVal1Sql : Resources.GetParamVal2Sql), connection);
                // Add the search parameter key
                command.Parameters.Add("@ParamKey", SqlDbType.VarChar).Value = ParamKey;

                // Open the connection
                connection.Open();

                // Execute the Query 
                Object obj = null;
                obj = command.ExecuteScalar();

                if (obj != null)
                    ParamVal = (string)obj;
            }

            return ParamVal;
        }

        public static void UpdateParamValue(string ClientConnectionStr, string ParamKey, string FirstValue, string SecondValue)
        {
            // Connect to the Database to query the Business Date
            using (SqlConnection connection = new SqlConnection(ClientConnectionStr))
            {
                // Setup the SQL Command
                SqlCommand command = new SqlCommand(Resources.UpdateParamVal, connection);
                // Add the search parameter key
                command.Parameters.Add("@PAR_VALUE1", SqlDbType.VarChar).Value = FirstValue;
                command.Parameters.Add("@PAR_VALUE2", SqlDbType.VarChar).Value = SecondValue;
                command.Parameters.Add("@ParamKey", SqlDbType.VarChar).Value = ParamKey;

                // Open the connection
                connection.Open();

                // Execute the Query 
                command.ExecuteNonQuery();
                command.Dispose();
            }
        }
    }
}
