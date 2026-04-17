using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UBPC.Web.Common
{
    public static class RejDecValidation
    {
        private static SQLDBHelper dbHelperObj = new SQLDBHelper();
        private static string clientConnString = string.Empty;

        //For OCBC only
        public static RejDecValidatorResult ValidateCDV(string connStr, string accountNo) 
        {
            RejDecValidatorResult validatorResult = new RejDecValidatorResult();
            bool isvalid = false;
            clientConnString = connStr;

            string accountType = string.Empty;
            string OcbcCDVWeightage = "2987654321";

            if (accountNo.Trim().Length.Equals(10))
            {
                isvalid = true;

                //Edited Shinyi CR 019-19 - 10 digit account with 4th Digit is "4" 
                if (accountNo.Trim().Substring(3, 1).Equals("4"))
                {
                    accountType = "LOAN";
                }
                else
                {
                    accountType = "CASA";
                }
            }
            else if (accountNo.Trim().Length.Equals(15))
            {
                isvalid = true;
                accountType = "LOAN";
            }
            else if (accountNo.Trim().Length.Equals(16))
            {
                isvalid = true;
                accountType = "CREDIT";
            }
            else
            {
                isvalid = false;
                validatorResult.ReturnMessage = "Invalid Account Number Length.";
            }

            if (isvalid)
            {
                switch (accountType)
                {
                    case "CASA":
                        if (accountNo.Trim().Length.Equals(10))
                        {
                            char[] ocbcChr = OcbcCDVWeightage.ToCharArray();
                            char[] acctChr = accountNo.ToCharArray();
                            int totalCal = 0;

                            for (int i = 0; i < acctChr.Length; i++)
                            {
                                int acctCharVal = Convert.ToInt16(acctChr[i].ToString());
                                int ocbcCharVal = Convert.ToInt16(ocbcChr[i].ToString());

                                int multiplyVal = acctCharVal * ocbcCharVal;

                                totalCal += multiplyVal;
                            }

                            int finalResult = (totalCal % 11);

                            if (!finalResult.Equals(0))
                            {
                                isvalid = false;
                                validatorResult.ReturnMessage = "Invalid Check Digit Validation for CA/SA Account.";
                            }
                            else 
                            {
                                isvalid = true;
                                validatorResult.ReturnMessage = ""; 
                            }
                        }

                        break;
                    case "LOAN":
                        if (accountNo.Trim().Length.Equals(15) || (accountNo.Trim().Length.Equals(10) && accountNo.Trim().Substring(3,1).Equals("4")))
                        {
                            char[] ocbcChr = OcbcCDVWeightage.ToCharArray();
                            char[] acctChr = accountNo.Substring(0, 10).ToCharArray();
                            int totalCal = 0;

                            for (int i = 0; i < acctChr.Length; i++)
                            {
                                int acctCharVal = Convert.ToInt16(acctChr[i].ToString());
                                int ocbcCharVal = Convert.ToInt16(ocbcChr[i].ToString());

                                int multiplyVal = acctCharVal * ocbcCharVal;

                                totalCal += multiplyVal;
                            }
                           
                            int finalResult = (totalCal % 11);

                            if (!finalResult.Equals(0))
                            {
                                isvalid = false;
                                validatorResult.ReturnMessage = "Invalid Check Digit Validation for Loan Account.";
                            }
                            else 
                            {
                                isvalid = true;
                                validatorResult.ReturnMessage = ""; 
                            }
                        }

                        break;
                    case "CREDIT":
                        if (accountNo.Trim().Length.Equals(16))
                        {
                            //Check first 6 digit exist in BIN
                            string First6Digit = accountNo.Substring(0, 6);
                            bool isValidBin = ValidateCreditCardBin(First6Digit);

                            if (!isValidBin)
                            {
                                isvalid = false;
                                validatorResult.ReturnMessage = "Invalid Check Digit Validation for Credit Card Account.";
                            }
                            else 
                            {
                                isvalid = true;
                                validatorResult.ReturnMessage = ""; 
                            }
                        }
                        break;
                }
            }

            validatorResult.Valid = isvalid;

            return validatorResult;
        }

        public static bool ValidateCreditCardBin(string accountNo) 
        {
            IDbConnection iConn = dbHelperObj.initConnection(clientConnString);
            string stmt = string.Empty;

            try
            {
                stmt = Resources.stmtGetCCBinForCDVChecking;
                IDbDataParameter[] param = new[] { 
                        dbHelperObj.CreateParameter(DbType.String, 6, "@BinNo", ParameterDirection.Input, accountNo)
                    };

                DataTable binDT = dbHelperObj.executeDataTable(iConn, CommandType.Text, stmt, param);
                return binDT.Rows.Count > 0;
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

        #region temp to remove
        //public static RejDecValidatorResult ValidateBSB(string connStr, string bsb)
        //{
        //    RejDecValidatorResult validatorResult = new RejDecValidatorResult();
        //    clientConnString = connStr;

        //    // Connect to the Client specific database to query the parameter table
        //    using (SqlConnection connection = new SqlConnection(clientConnString))
        //    {
        //        // Setup the SQL Command
        //        SqlCommand command = new SqlCommand(Resources.stmtGetValidBSB, connection);

        //        // Add the search parameter key
        //        command.Parameters.Add("@Bsb", SqlDbType.VarChar).Value = bsb;

        //        // Open the connection
        //        connection.Open();
                               
        //        if ((int)command.ExecuteScalar() == 0)
        //        {
        //            validatorResult.Valid = false;
        //            validatorResult.ReturnMessage = "Invalid BSB. Please enter a valid BSB.";
        //        }
        //        else
        //        {
        //            validatorResult.Valid = true;
        //            validatorResult.ReturnMessage = ""; 
        //        }
        //    }

        //    return validatorResult;
        //}
        #endregion

        public static RejDecValidatorResult ValidateAcceptedTransaction(string connStr, string acctNoOrbsb, bool isStub, bool isBPC, string oriChqBSB, string oriStubAcctNo, string oriStubFld10, string otherStubAcctNo)
        {
            RejDecValidatorResult validatorResult = new RejDecValidatorResult();
            clientConnString = connStr;

            // Connect to the Client specific database to query the parameter table
            using (SqlConnection connection = new SqlConnection(clientConnString))
            {
                // Setup the SQL Command
                SqlCommand command = new SqlCommand("sp_ValidateAcceptedTransactionForWeb", connection);

                // Add the search parameter key
                command.Parameters.Add("@IsBPC", SqlDbType.Bit).Value = isBPC;
                command.Parameters.Add("@IsStub", SqlDbType.Bit).Value = isStub;
                command.Parameters.Add("@AcctNumOrBSB", SqlDbType.VarChar).Value = acctNoOrbsb;
                command.Parameters.Add("@OrigChqBSB", SqlDbType.VarChar).Value = oriChqBSB;
                command.Parameters.Add("@OrigStubAcctNo", SqlDbType.VarChar).Value = oriStubAcctNo;

                //ITM_FLD10
                command.Parameters.Add("@OrigStubFld10", SqlDbType.VarChar).Value = oriStubFld10;        
                //Other Stub Account No
                command.Parameters.Add("@OtherStubAcctNo", SqlDbType.VarChar).Value = otherStubAcctNo;

                command.Parameters.Add("@ReturnMessage", SqlDbType.NVarChar, 500);
                command.Parameters["@ReturnMessage"].Direction = ParameterDirection.Output;
                

                // Open the connection
                connection.Open();

                command.CommandType = CommandType.StoredProcedure;
                command.ExecuteNonQuery();

                string outputmsg = command.Parameters["@ReturnMessage"].Value.ToString();


                if (!string.IsNullOrEmpty(outputmsg))
                {
                    validatorResult.Valid = false;
                    validatorResult.ReturnMessage = outputmsg;
                }
                else
                {
                    validatorResult.Valid = true;
                    validatorResult.ReturnMessage = "";
                }
            }

            return validatorResult;
        }
     
    }

    public class RejDecValidatorResult
    {
        private Boolean _valid;
        private String _returnMessage = string.Empty;

        public RejDecValidatorResult()
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
}
