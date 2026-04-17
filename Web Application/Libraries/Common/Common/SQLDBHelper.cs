using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Globalization;

namespace UBPC.Web.Common
{
    public class SQLDBHelper
    {
        public SQLDBHelper() { }

        #region sql timeout setting
        private const int defaultSqlTimeOut = 30;
        private int _sqlTimeOut = defaultSqlTimeOut;

        public int sqlTimeOut
        {
            get { return _sqlTimeOut; }
            set { _sqlTimeOut = value; }
        }
        #endregion

        #region Transaction
        public IDbTransaction beginTransaction(IDbConnection conn)
        {
            SqlConnection sqlConn = (SqlConnection)conn;
            return sqlConn.BeginTransaction();
        }

        public void commit(IDbTransaction transaction)
        {
            SqlTransaction sqlTrans = (SqlTransaction)transaction;
            sqlTrans.Commit();
        }

        public void rollback(IDbTransaction transaction)
        {
            SqlTransaction sqlTrans = (SqlTransaction)transaction;
            sqlTrans.Rollback();
        }
        #endregion

        #region prepare command and add parameter
        private void AddParameters(IDbCommand command, IDbDataParameter[] commandParameters)
        {
            foreach (SqlParameter p in commandParameters)
            {
                //check for derived output value with no value assigned
                if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                {
                    p.Value = DBNull.Value;
                }

                command.Parameters.Add(p);
            }
        }

        private void AddParameterWithValues(IDbDataParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                //do nothing if we get no data
                return;
            }

            // we must have the same number of values as we pave parameters to put them in
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            //iterate through the SqlParameters, assigning the values from the corresponding position in the 
            //value array
            for (int i = 0, j = commandParameters.Length; i < j; i++)
            {
                commandParameters[i].Value = parameterValues[i];
            }
        }

        private void AddParameterWithValues(object[] parameterValues, IDbDataParameter[] commandParameters)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                //do nothing if we get no data
                return;
            }

            // we must have the same number of values as we pave parameters to put them in
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            //iterate through the SqlParameters, assigning the values from the corresponding position in the 
            //value array
            for (int i = 0, j = commandParameters.Length; i < j; i++)
            {
                parameterValues[i] = commandParameters[i].Value;
            }
        }

        private void PrepareCommand(IDbCommand command, IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, IDbDataParameter[] commandParameters)
        {
            //if the provided connection is not open, open it
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            //set the command timeout
            command.CommandTimeout = _sqlTimeOut;

            //associate the connection with the command
            command.Connection = connection;

            //set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText;

            //assign transaction if existed
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            //set the command type
            command.CommandType = commandType;

            //attach the command parameters if they are provided
            if (commandParameters != null)
            {
                AddParameters(command, commandParameters);
            }

            return;
        }
        #endregion

        #region Handle Connection
        public void openConnection(string connectionString)
        {
            using (IDbConnection cn = new SqlConnection(connectionString))
            {
                cn.Open();
            }
        }

        public IDbConnection initConnection(string connectionString)
        {
            IDbConnection cn = new SqlConnection(connectionString);
            cn.Open();
            return cn;
        }
        #endregion

        #region executeNonQuery
        public int executeNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return executeNonQuery(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        public int executeNonQuery(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                cn.Open();

                //call the overload methods
                return executeNonQuery(cn, commandType, commandText, commandParameters);
            }
        }

        public int executeNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //Get existing SP parameter set names
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(connectionString, spName);

                //assign the provided values to these parameters based on parameter order
                AddParameterWithValues(commandParameters, parameterValues);

                //call the overload methods that takes an array of SqlParameters
                int returnValue = executeNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);

                //assign the returned values to these parameters based on parameter order
                AddParameterWithValues(parameterValues, commandParameters);


                return returnValue;
            }
            //call here without params
            else
            {
                return executeNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        public int executeNonQuery(IDbConnection connection, CommandType commandType, string commandText)
        {
            return executeNonQuery(connection, commandType, commandText, (SqlParameter[])null);
        }

        public int executeNonQuery(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            try
            {
                //create a command and prepare it for execution
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters);

                    //finally, execute the command.
                    int retval = cmd.ExecuteNonQuery();

                    // detach the SqlParameters from the command object, so they can be used again.
                    cmd.Parameters.Clear();
                    return retval;
                }

            }
            catch (Exception ex)
            {
                string errMsg = string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString());
                throw (new Exception(errMsg, ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }

        public int executeNonQuery(IDbConnection connection, string spName, params object[] parameterValues)
        {
            //if we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(connection, spName);

                //assign the provided values to these parameters based on parameter order
                AddParameterWithValues(commandParameters, parameterValues);

                //call the overload that takes an array of SqlParameters
                int retval = executeNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);

                //assign the returned values to these parameters based on parameter order
                AddParameterWithValues(commandParameters, parameterValues);

                return retval;
            }
            //otherwise we can just call the SP without params
            else
            {
                return executeNonQuery(connection, CommandType.StoredProcedure, spName);
            }
        }

        public int executeNonQuery(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of SqlParameters
            return executeNonQuery(transaction, commandType, commandText, (SqlParameter[])null);
        }

        public int executeNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            try
            {
                //create a command and prepare it for execution
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

                    //finally, execute the command.
                    int retval = cmd.ExecuteNonQuery();

                    // detach the SqlParameters from the command object, so they can be used again.
                    cmd.Parameters.Clear();
                    return retval;
                }

            }
            catch (Exception ex)
            {
                string errMsg = string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString());
                throw (new Exception(errMsg, ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }

        public int executeNonQuery(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            //if we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(transaction, spName);

                //assign the provided values to these parameters based on parameter order
                AddParameterWithValues(commandParameters, parameterValues);

                //call the overload that takes an array of SqlParameters
                int retval = executeNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);

                //assign the returned values to these parameters based on parameter order
                AddParameterWithValues(parameterValues, commandParameters);

                return retval;
            }
            //otherwise we can just call the SP without params
            else
            {
                return executeNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion executeNonQuery

        #region executeNonQuery with SqlCommand out parameter
        public int executeNonQuery(IDbConnection connection, CommandType commandType, string commandText, out SqlCommand sqlCmd)
        {
            return executeNonQuery(connection, commandType, commandText, out  sqlCmd, (SqlParameter[])null);
        }

        public int executeNonQuery(IDbConnection connection, CommandType commandType, string commandText, out SqlCommand sqlCmd, params IDbDataParameter[] commandParameters)
        {
            try
            {
                //create a command and prepare it for execution
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters);

                    //finally, execute the command.
                    int retval = cmd.ExecuteNonQuery();

                    sqlCmd = cmd;
                    return retval;
                }

            }
            catch (Exception ex)
            {
                string errMsg = string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString());
                throw (new Exception(errMsg, ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }

        public int executeNonQuery(IDbConnection connection, string spName, out SqlCommand sqlCmd, params object[] parameterValues)
        {
            //if we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(connection, spName);

                //assign the provided values to these parameters based on parameter order
                AddParameterWithValues(commandParameters, parameterValues);

                //call the overload that takes an array of SqlParameters
                int retval = executeNonQuery(connection, CommandType.StoredProcedure, spName, out sqlCmd, commandParameters);

                //assign the returned values to these parameters based on parameter order
                AddParameterWithValues(commandParameters, parameterValues);

                return retval;
            }
            //otherwise we can just call the SP without params
            else
            {
                return executeNonQuery(connection, CommandType.StoredProcedure, spName, out  sqlCmd);
            }
        }

        public int executeNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, out SqlCommand sqlCmd)
        {
            //pass through the call providing null for the set of SqlParameters
            return executeNonQuery(transaction, commandType, commandText, out  sqlCmd, (SqlParameter[])null);
        }

        public int executeNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, out SqlCommand sqlCmd, params IDbDataParameter[] commandParameters)
        {
            try
            {
                //create a command and prepare it for execution
                SqlCommand cmd = new SqlCommand();

                PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

                //finally, execute the command.
                int retval = cmd.ExecuteNonQuery();

                sqlCmd = cmd;
                return retval;


            }
            catch (Exception ex)
            {
                string errMsg = string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString());
                throw (new Exception(errMsg, ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }

        public int executeNonQuery(IDbTransaction transaction, string spName, out SqlCommand sqlCmd, params object[] parameterValues)
        {
            //if we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(transaction, spName);

                //assign the provided values to these parameters based on parameter order
                AddParameterWithValues(commandParameters, parameterValues);

                //call the overload that takes an array of SqlParameters
                int retval = executeNonQuery(transaction, CommandType.StoredProcedure, spName, out sqlCmd, commandParameters);

                //assign the returned values to these parameters based on parameter order
                AddParameterWithValues(parameterValues, commandParameters);

                return retval;
            }
            //otherwise we can just call the SP without params
            else
            {
                return executeNonQuery(transaction, CommandType.StoredProcedure, spName, out sqlCmd);
            }
        }
        #endregion

        #region ExecuteDataSet
        public DataSet executeDataset(string connectionString, CommandType commandType, string commandText)
        {
            return executeDataset(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        public DataSet executeDataset(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                cn.Open();

                //call the overload method
                return executeDataset(cn, commandType, commandText, commandParameters);
            }
        }

        public DataSet executeDataset(string connectionString, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(connectionString, spName);
                AddParameterWithValues(commandParameters, parameterValues);
                return executeDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return executeDataset(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        public DataSet executeDataset(IDbConnection connection, CommandType commandType, string commandText)
        {
            return executeDataset(connection, commandType, commandText, (SqlParameter[])null);
        }

        public DataSet executeDataset(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandTimeout = sqlTimeOut;
                    PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        //detach parameter
                        cmd.Parameters.Clear();

                        return ds;
                    }

                }

            }
            catch (Exception ex)
            {
                throw (new Exception(string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString()), ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }

        }

        public DataSet executeDataset(IDbConnection connection, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(connection, spName);
                AddParameterWithValues(commandParameters, parameterValues);

                return executeDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return executeDataset(connection, CommandType.StoredProcedure, spName);
            }
        }

        public DataSet executeDataset(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return executeDataset(transaction, commandType, commandText, (SqlParameter[])null);
        }

        public DataSet executeDataset(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        cmd.Parameters.Clear();

                        return ds;
                    }

                }

            }
            catch (Exception ex)
            {
                throw (new Exception(string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString()), ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }

        public DataSet executeDataset(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(transaction, spName);
                AddParameterWithValues(commandParameters, parameterValues);
                return executeDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return executeDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }
        #endregion ExecuteDataSet

        #region ExecuteDataTable
        public DataTable executeDataTable(string connectionString, CommandType commandType, string commandText)
        {
            return executeDataTable(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        public DataTable executeDataTable(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                cn.Open();
                return executeDataTable(cn, commandType, commandText, commandParameters);
            }
        }

        public DataTable executeDataTable(string connectionString, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(connectionString, spName);
                AddParameterWithValues(commandParameters, parameterValues);
                return executeDataTable(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return executeDataTable(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        public DataTable executeDataTable(IDbConnection connection, CommandType commandType, string commandText)
        {
            return executeDataTable(connection, commandType, commandText, (SqlParameter[])null);
        }

        public DataTable executeDataTable(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandTimeout = sqlTimeOut;
                    PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        cmd.Parameters.Clear();
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (new Exception(string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString()), ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }

        }

        public DataTable executeDataTable(IDbConnection connection, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(connection, spName);
                AddParameterWithValues(commandParameters, parameterValues);
                return executeDataTable(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return executeDataTable(connection, CommandType.StoredProcedure, spName);
            }
        }

        public DataTable executeDataTable(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return executeDataTable(transaction, commandType, commandText, (SqlParameter[])null);
        }

        public DataTable executeDataTable(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        cmd.Parameters.Clear();
                        return dt;
                    }
                }

            }
            catch (Exception ex)
            {
                throw (new Exception(string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString()), ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }

        public DataTable executeDataTable(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(transaction, spName);
                AddParameterWithValues(commandParameters, parameterValues);
                return executeDataTable(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return executeDataTable(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteDataTable

        #region ExecuteDataAdapter with DataTable out parameter
        public SqlDataAdapter executeDataAdapter(IDbConnection connection, CommandType commandType, string commandText, out DataTable dataTable, params IDbDataParameter[] commandParameters)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    SqlCommandBuilder builder = new SqlCommandBuilder(da);
                    da.UpdateCommand = builder.GetUpdateCommand();
                    DataTable dt = new DataTable();

                    da.Fill(dt);
                    cmd.Parameters.Clear();
                    dataTable = dt;
                    return da;

                }

            }
            catch (Exception ex)
            {
                throw (new Exception(string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString()), ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }

        }

        public SqlDataAdapter executeDataAdapter(IDbConnection connection, CommandType commandType, string commandText, out DataTable dataTable)
        {
            return executeDataAdapter(connection, commandType, commandText, out dataTable, (SqlParameter[])null);
        }

        public SqlDataAdapter executeDataAdapter(IDbTransaction transaction, CommandType commandType, string commandText, out DataTable dataTable, params IDbDataParameter[] commandParameters)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    SqlCommandBuilder builder = new SqlCommandBuilder(da);
                    da.UpdateCommand = builder.GetUpdateCommand();
                    DataTable dt = new DataTable();

                    da.Fill(dt);
                    cmd.Parameters.Clear();
                    dataTable = dt;
                    return da;

                }

            }
            catch (Exception ex)
            {
                throw (new Exception(string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString()), ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }

        public SqlDataAdapter executeDataAdapter(IDbTransaction transaction, CommandType commandType, string commandText, out DataTable dataTable)
        {
            return executeDataAdapter(transaction, commandType, commandText, out dataTable, (SqlParameter[])null);
        }
        #endregion

        #region ExecuteDataAdapter without DataTable out parameter
        public SqlDataAdapter executeDataAdapter(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    return da;
                }

            }
            catch (Exception ex)
            {
                throw (new Exception(string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString()), ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }

        }

        public SqlDataAdapter executeDataAdapter(IDbConnection connection, CommandType commandType, string commandText)
        {
            return executeDataAdapter(connection, commandType, commandText, (SqlParameter[])null);
        }

        public SqlDataAdapter executeDataAdapter(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    return da;
                }

            }
            catch (Exception ex)
            {
                throw (new Exception(string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString()), ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }

        public SqlDataAdapter executeDataAdapter(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return executeDataAdapter(transaction, commandType, commandText, (SqlParameter[])null);
        }
        #endregion

        #region executeFillDT
        //The Data Table pass in will contain the filled record
        //DataTable is a reference type.
        public void executeFillDT(IDbConnection connection, CommandType commandType, string commandText, DataTable dataTable, params IDbDataParameter[] commandParameters)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        using (SqlCommandBuilder builder = new SqlCommandBuilder(da))
                        {
                            da.Fill(dataTable);
                            cmd.Parameters.Clear();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw (new Exception(string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString()), ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }

        }
        //The Data Table pass in will contain the filled record
        //DataTable is a reference type.
        public void executeFillDT(IDbConnection connection, CommandType commandType, string commandText, DataTable dataTable)
        {
            executeFillDT(connection, commandType, commandText, dataTable, (SqlParameter[])null);
        }
        //The Data Table pass in will contain the filled record
        //DataTable is a reference type.
        public void executeFillDT(IDbTransaction transaction, CommandType commandType, string commandText, DataTable dataTable, params IDbDataParameter[] commandParameters)
        {

            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        using (SqlCommandBuilder builder = new SqlCommandBuilder(da))
                        {
                            da.Fill(dataTable);
                            cmd.Parameters.Clear();
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                throw (new Exception(string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString()), ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }
        //The Data Table pass in will contain the filled record
        //DataTable is a reference type.
        public void executeFillDT(IDbTransaction transaction, CommandType commandType, string commandText, DataTable dataTable)
        {
            executeFillDT(transaction, commandType, commandText, dataTable, (SqlParameter[])null);
        }

        #endregion

        #region executeReader

        private enum SqlConnectionType
        {
            /// <summary>Connection is owned and managed by SqlHelper</summary>
            Internal,
            /// <summary>Connection is owned and managed by the caller</summary>
            External
        }


        private SqlDataReader executeReader(IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, IDbDataParameter[] commandParameters, SqlConnectionType connectionOwnership)
        {
            try
            {
                //create a command and prepare it for execution
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters);

                    //create a reader
                    SqlDataReader dr;

                    // call executeReader with the appropriate CommandBehavior
                    if (connectionOwnership == SqlConnectionType.External)
                    {
                        dr = cmd.ExecuteReader();
                    }
                    else
                    {
                        dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    }

                    // detach the SqlParameters from the command object, so they can be used again.
                    cmd.Parameters.Clear();

                    return dr;
                }

            }
            catch (Exception ex)
            {
                throw (new Exception(string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString()), ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }

        public SqlDataReader executeReader(string connectionString, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of SqlParameters
            return executeReader(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        public SqlDataReader executeReader(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            SqlConnection cn = new SqlConnection(connectionString);
            cn.Open();

            try
            {
                return executeReader(cn, null, commandType, commandText, commandParameters, SqlConnectionType.Internal);
            }
            catch
            {
                cn.Close();
                throw;
            }
        }

        public SqlDataReader executeReader(string connectionString, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(connectionString, spName);
                AddParameterWithValues(commandParameters, parameterValues);
                return executeReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return executeReader(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        public SqlDataReader executeReader(IDbConnection connection, CommandType commandType, string commandText)
        {
            return executeReader(connection, commandType, commandText, (SqlParameter[])null);
        }

        public SqlDataReader executeReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return executeReader(connection, (SqlTransaction)null, commandType, commandText, commandParameters, SqlConnectionType.External);
        }

        public SqlDataReader executeReader(IDbConnection connection, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(connection, spName);

                AddParameterWithValues(commandParameters, parameterValues);

                return executeReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return executeReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        public SqlDataReader executeReader(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return executeReader(transaction, commandType, commandText, (SqlParameter[])null);
        }

        public SqlDataReader executeReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return executeReader(transaction.Connection, transaction, commandType, commandText, commandParameters, SqlConnectionType.External);
        }

        public SqlDataReader executeReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(transaction, spName);
                AddParameterWithValues(commandParameters, parameterValues);

                return executeReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return executeReader(transaction, CommandType.StoredProcedure, spName);
            }
        }
        #endregion executeReader

        #region executeScalar

        public object executeScalar(string connectionString, CommandType commandType, string commandText)
        {
            return executeScalar(connectionString, commandType, commandText, (SqlParameter[])null);
        }


        public object executeScalar(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                cn.Open();

                return executeScalar(cn, commandType, commandText, commandParameters);
            }
        }

        public object executeScalar(string connectionString, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(connectionString, spName);
                AddParameterWithValues(commandParameters, parameterValues);
                return executeScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return executeScalar(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        public object executeScalar(IDbConnection connection, CommandType commandType, string commandText)
        {
            return executeScalar(connection, commandType, commandText, (SqlParameter[])null);
        }

        public object executeScalar(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters);
                object retval = cmd.ExecuteScalar();

                cmd.Parameters.Clear();
                return retval;
            }
            catch (Exception ex)
            {
                throw (new Exception(string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString()), ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }

        public object executeScalar(IDbConnection connection, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(connection, spName);
                AddParameterWithValues(commandParameters, parameterValues);

                return executeScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return executeScalar(connection, CommandType.StoredProcedure, spName);
            }
        }

        public object executeScalar(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return executeScalar(transaction, commandType, commandText, (SqlParameter[])null);
        }

        public object executeScalar(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

                object retval = cmd.ExecuteScalar();

                cmd.Parameters.Clear();
                return retval;
            }
            catch (Exception ex)
            {
                throw (new Exception(string.Concat(ex.Message.ToString(), "Executed SQL Statement:", commandText.ToString()), ex));
            }
            finally
            {
                //reset connection timeout to default
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }

        public object executeScalar(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.getSpParameterSet(transaction, spName);
                AddParameterWithValues(commandParameters, parameterValues);
                return executeScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return executeScalar(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion executeScalar

        #region sql bulk insert
        public void bulkInsertDataTabe(IDbConnection connection, int BatchSize, SqlBulkCopyOptions options, string destinationTableName, DataTable inputDT)
        {
            try
            {
                // Run the Bulk Copy operation to import the data
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(((SqlConnection)connection).ConnectionString, options))
                {
                    // Setup the bulk copy
                    bulkCopy.BatchSize = BatchSize;
                    bulkCopy.BulkCopyTimeout = _sqlTimeOut;
                    bulkCopy.DestinationTableName = destinationTableName;

                    // Write the DataTable to the server
                    bulkCopy.WriteToServer(inputDT);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }

        public void bulkInsertDataTabe(string connectionString, int BatchSize, SqlBulkCopyOptions options, string destinationTableName, DataTable inputDT)
        {
            try
            {
                // Run the Bulk Copy operation to import the data
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString, options))
                {
                    // Setup the bulk copy
                    bulkCopy.BatchSize = BatchSize;
                    bulkCopy.BulkCopyTimeout = _sqlTimeOut;
                    bulkCopy.DestinationTableName = destinationTableName;

                    // Write the DataTable to the server
                    bulkCopy.WriteToServer(inputDT);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }

        public void bulkInsertDataTable(IDbTransaction transaction, int BatchSize, SqlBulkCopyOptions options, string destinationTableName, DataTable inputDT)
        {
            try
            {
                // Run the Bulk Copy operation to import the data
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(((SqlTransaction)transaction).Connection, options, (SqlTransaction)transaction))
                {
                    // Setup the bulk copy
                    bulkCopy.BatchSize = BatchSize;
                    bulkCopy.BulkCopyTimeout = _sqlTimeOut;
                    bulkCopy.DestinationTableName = destinationTableName;

                    // Write the DataTable to the server
                    bulkCopy.WriteToServer(inputDT);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _sqlTimeOut = defaultSqlTimeOut;
            }
        }
        #endregion

        #region Create SQL Parameter
        public IDbDataParameter CreateParameter(DbType parameterType, int size, string name, ParameterDirection direction, object value)
        {
            if (size > 0)
            {
                return new SqlParameter
                {
                    DbType = parameterType,
                    Size = size,
                    ParameterName = name,
                    Direction = direction,
                    Value = value
                };
            }
            else
            {
                return new SqlParameter
                {
                    DbType = parameterType,
                    ParameterName = name,
                    Direction = direction,
                    Value = value
                };
            }


        }
        #endregion

    }

    public sealed class SqlHelperParameterCache
    {
        #region private methods, variables, and constructors

        private SqlHelperParameterCache() { }

        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        internal static string mConnStr = string.Empty;

        private static SqlParameter[] discoverSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                cn.Open();
                return discoverSpParameterSet(cn, spName, includeReturnValueParameter);
            }
        }

        private static SqlParameter[] discoverSpParameterSet(IDbConnection cn, string spName, bool includeReturnValueParameter)
        {
            if (cn == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            SqlCommand cmd = new SqlCommand(spName, (SqlConnection)cn);
            cmd.CommandType = CommandType.StoredProcedure;

            string schemaName = "dbo";
            int firstDot = spName.IndexOf('.');
            if (firstDot > 0)
            {
                schemaName = spName.Substring(0, firstDot);
                spName = spName.Substring(firstDot + 1);
            }

            try
            {
                SqlCommandBuilder.DeriveParameters(cmd);
            }
            catch
            {
                SqlCommand getParams = new SqlCommand("sp_procedure_params_rowset", (SqlConnection)cn);
                getParams.CommandType = CommandType.StoredProcedure;
                getParams.Parameters.AddWithValue("@procedure_name", spName);
                getParams.Parameters.AddWithValue("@procedure_schema", schemaName);

                SqlDataReader sdr = getParams.ExecuteReader();

                if (sdr.HasRows)
                {
                    using (sdr)
                    {
                        // Read the parameter information
                        int ParamNameCol = sdr.GetOrdinal("PARAMETER_NAME");
                        int ParamSizeCol = sdr.GetOrdinal("CHARACTER_MAXIMUM_LENGTH");
                        int ParamTypeCol = sdr.GetOrdinal("TYPE_NAME");
                        int ParamNullCol = sdr.GetOrdinal("IS_NULLABLE");
                        int ParamPrecCol = sdr.GetOrdinal("NUMERIC_PRECISION");
                        int ParamDirCol = sdr.GetOrdinal("PARAMETER_TYPE");
                        int ParamScaleCol = sdr.GetOrdinal("NUMERIC_SCALE");

                        while (sdr.Read())
                        {
                            string name = sdr.GetString(ParamNameCol);
                            string datatype = sdr.GetString(ParamTypeCol);
                            // Is this xml?
                            // ADO.NET 1.1 does not support XML, replace with text
                            switch (datatype.ToLower())
                            {
                                case "xml":
                                    datatype = "Text";
                                    break;
                                case "numeric":
                                    datatype = "Decimal";
                                    break;
                            }

                            object parsedType = Enum.Parse(typeof(SqlDbType), datatype, true);
                            SqlDbType type = (SqlDbType)parsedType;
                            bool Nullable = sdr.GetBoolean(ParamNullCol);
                            SqlParameter param = new SqlParameter(name, type);
                            // Determine parameter direction
                            int dir = sdr.GetInt16(ParamDirCol);
                            switch (dir)
                            {
                                case 1:
                                    param.Direction = ParameterDirection.Input;
                                    break;
                                case 2:
                                    param.Direction = ParameterDirection.Output;
                                    break;
                                case 3:
                                    param.Direction = ParameterDirection.InputOutput;
                                    break;
                                case 4:
                                    param.Direction = ParameterDirection.ReturnValue;
                                    break;
                            }
                            param.IsNullable = Nullable;
                            if (!sdr.IsDBNull(ParamPrecCol))
                            {
                                param.Precision = (Byte)sdr.GetInt16(ParamPrecCol);
                            }
                            if (!sdr.IsDBNull(ParamSizeCol))
                            {
                                param.Size = sdr.GetInt32(ParamSizeCol);
                            }
                            if (!sdr.IsDBNull(ParamScaleCol))
                            {
                                param.Scale = (Byte)sdr.GetInt16(ParamScaleCol);
                            }
                            cmd.Parameters.Add(param);
                        }
                    }
                }
            }

            if (!includeReturnValueParameter)
            {
                cmd.Parameters.RemoveAt(0);
            }

            SqlParameter[] discoveredParameters = new SqlParameter[cmd.Parameters.Count]; ;

            cmd.Parameters.CopyTo(discoveredParameters, 0);

            // WORKAROUND begin
            foreach (SqlParameter sqlParam in discoveredParameters)
            {
                if ((sqlParam.SqlDbType == SqlDbType.VarChar) &&
                    (sqlParam.Size == Int32.MaxValue))
                {
                    sqlParam.SqlDbType = SqlDbType.Text;
                }
            }
            // WORKAROUND end

            // Init the parameters with a DBNull value
            foreach (SqlParameter discoveredParameter in discoveredParameters)
            {
                discoveredParameter.Value = DBNull.Value;
            }
            return discoveredParameters;
            //			}
        }

        private static SqlParameter[] discoverSpParameterSet(IDbTransaction tx, string spName, bool includeReturnValueParameter)
        {
            using (SqlCommand cmd = new SqlCommand(spName, (SqlConnection)tx.Connection, (SqlTransaction)tx))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                SqlCommandBuilder.DeriveParameters(cmd);

                if (!includeReturnValueParameter)
                {
                    cmd.Parameters.RemoveAt(0);
                }

                SqlParameter[] discoveredParameters = new SqlParameter[cmd.Parameters.Count]; ;

                cmd.Parameters.CopyTo(discoveredParameters, 0);

                return discoveredParameters;
            }
        }

        //deep copy of cached SqlParameter array
        private static SqlParameter[] cloneParameters(SqlParameter[] originalParameters)
        {
            SqlParameter[] clonedParameters = new SqlParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (SqlParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        #endregion private methods, variables, and constructors

        #region caching functions

        public static void cacheParameterSet(string connectionString, string commandText, params SqlParameter[] commandParameters)
        {
            string hashKey = connectionString + ":" + commandText;

            paramCache[hashKey] = commandParameters;
        }

        public static SqlParameter[] getCachedParameterSet(string connectionString, string commandText)
        {
            string hashKey = connectionString + ":" + commandText;

            SqlParameter[] cachedParameters = (SqlParameter[])paramCache[hashKey];

            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return cloneParameters(cachedParameters);
            }
        }

        #endregion caching functions

        #region Parameter Discovery Functions

        public static SqlParameter[] getSpParameterSet(string connectionString, string spName)
        {
            return getSpParameterSet(connectionString, spName, false);
        }

        public static SqlParameter[] getSpParameterSet(IDbConnection cn, string spName)
        {
            return getSpParameterSet(cn, spName, false);
        }

        public static SqlParameter[] getSpParameterSet(IDbTransaction tx, string spName)
        {
            return getSpParameterSet(tx, spName, false);
        }

        public static SqlParameter[] getSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            string hashKey = connectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

            SqlParameter[] cachedParameters;

            cachedParameters = (SqlParameter[])paramCache[hashKey];

            if (cachedParameters == null)
            {
                cachedParameters = (SqlParameter[])(paramCache[hashKey] = discoverSpParameterSet(connectionString, spName, includeReturnValueParameter)); //work around
                //cachedParameters = (SqlParameter[])(paramCache[hashKey] = discoverSpParameterSet(mConnStr, spName, includeReturnValueParameter));
            }

            return cloneParameters(cachedParameters);
        }

        public static SqlParameter[] getSpParameterSet(IDbConnection cn, string spName, bool includeReturnValueParameter)
        {
            string hashKey = cn.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

            SqlParameter[] cachedParameters;

            cachedParameters = (SqlParameter[])paramCache[hashKey];

            if (cachedParameters == null)
            {
                //cachedParameters = (SqlParameter[])(paramCache[hashKey] = discoverSpParameterSet(cn, spName, includeReturnValueParameter)); // work around
                cachedParameters = (SqlParameter[])(paramCache[hashKey] = discoverSpParameterSet(mConnStr, spName, includeReturnValueParameter));
            }

            return cloneParameters(cachedParameters);
        }

        public static SqlParameter[] getSpParameterSet(IDbTransaction tx, string spName, bool includeReturnValueParameter)
        {
            string hashKey = tx.Connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

            SqlParameter[] cachedParameters;

            cachedParameters = (SqlParameter[])paramCache[hashKey];

            if (cachedParameters == null)
            {
                //cachedParameters = (SqlParameter[])(paramCache[hashKey] = discoverSpParameterSet(tx, spName, includeReturnValueParameter)); //work around
                cachedParameters = (SqlParameter[])(paramCache[hashKey] = discoverSpParameterSet(mConnStr, spName, includeReturnValueParameter));
            }

            return cloneParameters(cachedParameters);
        }

        #endregion Parameter Discovery Functions

    }

}
