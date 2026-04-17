using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace UBPC.Web.Common
{
    public static class BusinessDate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        public static DateTime? GetActive(string clientConnStr)
        {
            DateTime? active = null;

            // Connect to the Database to query the Business Date
            using (SqlConnection connection = new SqlConnection(clientConnStr))
            {
                // Setup the SQL Command
                SqlCommand command = new SqlCommand(Resources.GetActiveDateSql, connection);

                // Open the connection
                connection.Open();

                // Execute the Query, we are only interested
                // in the first value returned
                Object obj = command.ExecuteScalar();

                // If we get null back, it means the query was
                // successful but no active date is set
                if (obj != null)
                    active = (DateTime)obj;
            }

            return active;
        }
        /// <summary>
        /// Create a tbl_busdate record for the passedin date value with inactive status
        /// </summary>
        /// <param name="businessDate"></param>

        public static void CreateInActive(DateTime businessDate, string clientConnStr)
        {
            //insert a business date record that is not active
            using (SqlConnection connection = new SqlConnection(clientConnStr))
            {
                // Setup the SQL Command
                SqlCommand command = new SqlCommand(Resources.InsertInactiveDateSql, connection);

                // Add the @BusinessDate parameter
                command.Parameters.Add("@BusinessDate", SqlDbType.DateTime).Value = businessDate.Date;

                // Open the connection
                connection.Open();

                // Execute the Query
                command.ExecuteNonQuery();
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="businessDate"></param>

        public static void SetActive(DateTime businessDate, string clientConnStr)
        {
            // If there is an active date already
            // it must cleared first
            using (SqlConnection connection = new SqlConnection(clientConnStr))
            {
                // Setup the SQL Command
                SqlCommand command = new SqlCommand(Resources.SetActiveDateSql, connection);

                // Add the @BusinessDate parameter
                command.Parameters.Add("@BusinessDate", SqlDbType.DateTime).Value = businessDate.Date;

                // Open the connection
                connection.Open();

                // Execute the Query
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// 
        /// </summary>

        public static void ClearActive(string clientConnStr)
        {
            // Set the active flag to false
            // Connect to the Database to query the Business Date
            using (SqlConnection connection = new SqlConnection(clientConnStr))
            {
                // Setup the SQL Command
                SqlCommand command = new SqlCommand(Resources.ClearActiveDateSql, connection);

                // Open the connection
                connection.Open();

                // Execute the Query
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="businessDate"></param>
        /// <returns></returns>

        public static Boolean Exists(DateTime businessDate, string clientConnStr)
        {
            // Connect to the Database to query the Business Date
            using (SqlConnection connection = new SqlConnection(clientConnStr))
            {
                // Setup the SQL Command
                SqlCommand command = new SqlCommand(Resources.DoesDateExistSql, connection);

                // Add the @BusinessDate parameter
                command.Parameters.Add("@BusinessDate", SqlDbType.DateTime).Value = businessDate.Date;

                // Open the connection
                connection.Open();

                // If the query affects 1 or more rows then the business date exists               
                if ((int)command.ExecuteScalar() == 0)
                    return false;
                else
                    return true;
            }
        }

        public static DateTime GetNextBusinessDate(DateTime businessDateEnded, String clientConnStr)
        {
            DateTime nextWeekDay = GetNextWeekday(businessDateEnded);
            Object objHoliday;
            DateTime dtHoliday;

            // Connect to the Database to query the Business Date
            using (SqlConnection connection = new SqlConnection(clientConnStr))
            {
                // Setup the SQL Command
                SqlCommand command = new SqlCommand(Resources.sqlNextHolidayDate, connection);

                // Add the @BusinessDate parameter
                command.Parameters.Add("@BusinessDate", SqlDbType.DateTime).Value = businessDateEnded.Date;

                // Open the connection
                connection.Open();

                objHoliday = command.ExecuteScalar();

                if (!(objHoliday is DBNull))
                    dtHoliday = (DateTime)objHoliday;
                else
                    dtHoliday = DateTime.MinValue;

                connection.Close();
            }

            while (nextWeekDay == dtHoliday)
            {
                nextWeekDay = GetNextBusinessDate(nextWeekDay, clientConnStr);
            }

            return nextWeekDay;
        }

        public static DateTime GetNextWeekday(DateTime dateEnded)
        {
            DateTime nextWeekDay = dateEnded.AddDays(1);

            switch (nextWeekDay.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    nextWeekDay = nextWeekDay.AddDays(2);
                    break;
                case DayOfWeek.Sunday:
                    nextWeekDay = nextWeekDay.AddDays(1);
                    break;
            }

            return nextWeekDay.Date;

        }
    }
}
