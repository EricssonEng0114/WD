using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;

namespace UBPC.Encryption
{
    public class PassValidatorResult
    {
        private Boolean _valid;
        private String _returnMessage = string.Empty;
        
        public PassValidatorResult()
        {
            _valid = true;
            _returnMessage = string.Empty;
        }

        public Boolean Valid
        {
            get
            {
                return _valid;
            }
            set
            {
                _valid = value;
            }
        }

        public String ReturnMessage
        {
            get
            {
                return _returnMessage;
            }
            set
            {
                _returnMessage = value;
            }
        }
    }

    public static class PassValidator
    {
        const int MinPasswordLength = 7;

        public static PassValidatorResult Validate(string password, Boolean isEncrypted)
        {
            return Validate(password, isEncrypted, null);
        }

        public static PassValidatorResult Validate(string password, Boolean isEncrypted, string[] historyPasswords)
        {
            PassValidatorResult validatorResult = new PassValidatorResult();

            var MinPasswordLength = 12;
            string connectionString = ConfigurationManager.ConnectionStrings["WebConnectionString"].ConnectionString.ToString();
            var stmt = Resource.GetParamVal1Sql;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Open the connection
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(stmt, conn))
                {
                    // Add parameter to prevent SQL Injection
                    cmd.Parameters.AddWithValue("@ParamKey", "MinPswdLen");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int columnIndex = reader.GetOrdinal("PAR_VALUE1"); // Get column index dynamically

                            if (!reader.IsDBNull(columnIndex)) // Check for NULL values
                            {
                                string value = reader.GetString(columnIndex).Trim(); // Read as string

                                if (int.TryParse(value, out int parsedValue)) // Safe conversion
                                {
                                    MinPasswordLength = parsedValue;
                                }
                            }
                        }
                    }
                }
            }

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasLowerChar = new Regex(@"[a-z]+");
            bool num = false;
            bool UpperChar = false;
            bool LowerChar = false;

            password = password.Trim();

            if (isEncrypted)
            {
                password = UCrypt.Decrypt(password);
            }

            if (hasNumber.IsMatch(password))
            {
                num = true;
            }

            if (hasUpperChar.IsMatch(password))
            {
                UpperChar = true;
            }

            if (hasLowerChar.IsMatch(password))
            {
                LowerChar = true;
            }

            if (password.Length < MinPasswordLength)
            {
                //password entered is less then min length required
                validatorResult.ReturnMessage = $"At least {MinPasswordLength} characters required for the password. Please try again.";
                validatorResult.Valid = false;
            }

            if (!(num && UpperChar && LowerChar))
            {
                validatorResult.ReturnMessage = "Password failed complexity check. Password must consist of (uppercase, lowercase and numeric) characters.";
                validatorResult.Valid = false;
            }

            if (historyPasswords != null && historyPasswords.Length > 0)
            {
                foreach (string historyPassword in historyPasswords)
                {
                    if (password.Equals(historyPassword.Trim()))
                    {
                        validatorResult.ReturnMessage = "Password cannot be the same as your previous 6 passwords. Please try again.";
                        validatorResult.Valid = false;
                    }
                }
            }

            return validatorResult;
        }
    }
}
