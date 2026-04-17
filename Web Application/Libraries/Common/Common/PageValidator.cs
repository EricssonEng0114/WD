using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;


namespace UBPC.Web.Common
{
    public class PageValidatorResult
    {
        private Boolean _valid;
        private String _returnMessage = string.Empty;

        public PageValidatorResult()
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

    public static class PageValidator
    {
        private static SQLDBHelper dbHelperObj = new SQLDBHelper();

        public static PageValidatorResult Validate(string connectionString, string userGroup, string module)
        {
            PageValidatorResult validatorResult = new PageValidatorResult();

            //Check if user group is allowed to access this module
            if (GetUserAccessRightModules(connectionString, Convert.ToInt16(userGroup), module))
            {
                validatorResult.ReturnMessage = "";
                validatorResult.Valid = true;
            }
            else
            {
                validatorResult.ReturnMessage = "You are not authorised to access the page: " + module;
                validatorResult.Valid = false;
            }

            return validatorResult;
        }

        private static bool GetUserAccessRightModules(string connStr, int userGroup, string moduleName)
        {
            IDbConnection iConn = dbHelperObj.initConnection(connStr);
            string stmt = string.Empty;

            try
            {
                stmt = String.Format(Resources.GetUserAccessRightModules,userGroup);

                IDbDataParameter[] param = new[]{
                    dbHelperObj.CreateParameter(DbType.String, 100, "@ModuleName", ParameterDirection.Input,moduleName)
                };

                Object obj = dbHelperObj.executeScalar(iConn, CommandType.Text, stmt, param);

                int recCount = (obj == null) ? 0 : Convert.ToInt32(obj);
                return recCount>0;

            }
            catch (Exception ex)
            {
                LogEntry log = new LogEntry();
                log.Caller = LogCallerID.Login;
                log.Severity = LogEventType.Error;
                log.Message = ex.Message;
                log.Exception = ex;
                log.Write();

                throw ex;
            }
            finally
            {
                iConn.Close();
            }
        }


    }

}
