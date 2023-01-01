using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Dapper;

namespace CPUFramework
{
    public static class SQLUtility
    {
        public enum ExecSQLTypeEnum { NoResultSet, SingleRecord, MultipleRecords }
        public static T ExecuteGetSingleDapper<T>(string sprocname, DynamicParameters dynamparam) where T : new()
        {
            (T tobj, List<T> tlst) = DoExecuteSQLDapper<T>(sprocname, dynamparam, ExecSQLTypeEnum.SingleRecord);
            return tobj;
        }
        public static List<T> ExecuteGetMultipleDapper<T>(string sprocname, DynamicParameters dynamparam) where T : new()
        {
            (T tobj, List<T> tlst) = DoExecuteSQLDapper<T>(sprocname, dynamparam, ExecSQLTypeEnum.MultipleRecords);
            return tlst;
        }
        public static void ExecuteSQLDapper(string sprocname, DynamicParameters dynamparam)
        {
            DoExecuteSQLDapper<object>(sprocname, dynamparam);
        }
        private static (T, List<T>) DoExecuteSQLDapper<T>(string sprocname, DynamicParameters dynamparam, ExecSQLTypeEnum execSQLType = ExecSQLTypeEnum.NoResultSet) where T : new()
        {
            T tobj = new();
            List<T> tlst = new();
            dynamparam.Add("Message", "", direction: ParameterDirection.InputOutput);
            dynamparam.Add("return_value", "", direction: ParameterDirection.ReturnValue);
            using (SqlConnection conn = new(DataUtility.ConnectionString))
            {
                try
                {
                    switch (execSQLType)
                    {
                        case ExecSQLTypeEnum.SingleRecord:
                            tobj = conn.QueryFirstOrDefault<T>(sprocname, dynamparam, commandType: CommandType.StoredProcedure);
                            if (tobj == null)
                            {
                                tobj = new T();
                            }
                            break;
                        case ExecSQLTypeEnum.MultipleRecords:
                            tlst = conn.Query<T>(sprocname, dynamparam, commandType: CommandType.StoredProcedure).ToList();
                            break;

                        default:
                            conn.Execute(sprocname, dynamparam, commandType: CommandType.StoredProcedure);
                            break;
                    }
                }
                catch (SqlException ex) when (IsConstraintError(ex.Message))
                {
                    throw new CPUException(ex.Message);
                }
            }
            int ret = dynamparam.Get<int>("return_value");
            string msg = dynamparam.Get<string>("Message");

            if (ret == 1)
            {
                throw new CPUException(msg);
            }
            return (tobj, tlst);
        }
        private static DataTable ExecuteSQL(SqlCommand cmd, string connstringvalue)
        {
            Debug.Print(GetSQL(cmd));

            using (SqlConnection conn = new(connstringvalue))
            {
                cmd.Connection = conn;
                conn.Open();

                if (cmd.CommandType == CommandType.StoredProcedure && cmd.Parameters.Count == 0)
                {
                    SqlCommandBuilder.DeriveParameters(cmd);
                }

                DataTable dt = new();

                try
                {

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (cmd.Parameters.Contains("@return_value") && cmd.Parameters["@return_value"].Value != null)
                    {
                        int returnval = (int)cmd.Parameters["@return_value"].Value;
                        if (returnval == 1)
                        {
                            string msg = cmd.Parameters["@Message"].Value.ToString();
                            throw new CPUException(msg, cmd.CommandText);
                        }
                    }



                    dt.Load(dr);
                }
                catch (SqlException ex) when (IsConstraintError(ex.Message))
                {
                    throw new CPUException(ex.Message, cmd.CommandText);
                }
                return dt;
            }


        }
        private static bool IsConstraintError(string message)
        {
            bool b = false;

            if (message.ToLower().Contains("f_") || message.ToLower().Contains("ck_") || message.ToLower().Contains("u_"))
            {
                b = true;
            }
            return b;
        }
        public static SqlCommand GetSQLCommand(string connstringvalue, string sprocname)
        {
            using (SqlConnection conn = new SqlConnection(connstringvalue))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(sprocname, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(cmd);
                return cmd;
            }
        }

        public static DataTable GetDataTable(SqlCommand cmd, string connstringval)
        {
            return ExecuteSQL(cmd, connstringval);
        }
        public static DataTable GetDataTable(string connstringvalue, string sqlstatement)
        {
            SqlCommand cmd = new(sqlstatement);
            cmd.CommandType = CommandType.Text;

            return ExecuteSQL(cmd, connstringvalue);
        }
        public static DataTable GetDataTableFromSproc(string connstringvalue, string sprocname)
        {
            SqlCommand cmd = new(sprocname) { CommandType = CommandType.StoredProcedure };

            return ExecuteSQL(cmd, connstringvalue);
        }

        private static string GetSQL(SqlCommand cmd)
        {
            string val = "";

            if (cmd.Connection != null)
            {
                val += "***********\n--" + cmd.Connection.DataSource + "\nuse " + cmd.Connection.Database + "\ngo\n";
            }
            if (cmd.CommandType == CommandType.StoredProcedure)
            {

                val += "exec " + cmd.CommandText;

                foreach (SqlParameter p in cmd.Parameters)
                {
                    if (p.ParameterName != "@RETURN_VALUE")
                    {
                        string paramval = "null";

                        if (p.Value != null)
                        {
                            paramval = p.Value.ToString();
                        }
                        val += "\n" + p.ParameterName + " = " + paramval;
                    }
                }
            }
            else
            {
                val += cmd.CommandText;
            }

            return val;
        }
    }
}
