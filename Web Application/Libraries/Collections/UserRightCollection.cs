using System;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;

namespace UBPC.Web.Collections
{
    public class UserRightCollection : IEnumerable
    {
        protected IDictionary<String, int> rights = new Dictionary<String, int>();

        public int Count
        {
            get { return rights.Count; }
        }

        public UserRightCollection(int userGroup)
        {
            FillUserRights(userGroup, ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString);
        }

        public UserRightCollection(int userGroup, String connectionString)
        {
            FillUserRights(userGroup, connectionString);
        }

        public Boolean ContainsKey(String Key)
        {
            return rights.ContainsKey(Key);
        }

        private void FillUserRights(int userGroup, String connectionString)
        {
            // Check that the User Group supplied is between 1 - 9
            if (userGroup < 1 || userGroup > 9)
                throw new ArgumentOutOfRangeException("userGroup", userGroup, Resources.UserGroupOutOfRange);

            // Create the connection with the specified connection string
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Open the connection
                connection.Open();

                // Create the SqlCommand to access the table
                SqlCommand command = connection.CreateCommand();
                command.CommandText = String.Format(Resources.GetUserRightsSql, userGroup);

                using (SqlDataReader sqlReader = command.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        rights.Add(sqlReader["ACS_FunctionCode"].ToString(), Convert.ToInt32(sqlReader[String.Format("ACS_Group{0}VerifyRequired", userGroup)]));
                    }
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            return rights.Values.GetEnumerator();
        }

        public int this[String key]
        {
            get { return rights[key]; }
        }
    }
}
