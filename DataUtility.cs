using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUFramework
{
    public static class DataUtility
    {
        private static string connstring = "";

        public static string SetConnectionString(string servername, string databasename, string username, string password)
        {
            connstring = "Server=tcp:" + servername +
                ";Initial Catalog=" + databasename
                + ";Persist Security Info=False;User ID=" + username
                + ";Password=" + password + ";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
            }
            return connstring;
        }
        public static string SetSQLExpressConnectionString(string databasename)
        {
            connstring = "Server=.\\SQLExpress;Initial Catalog=" + databasename + ";Trusted_Connection=yes;";
            using (SqlConnection conn = new SqlConnection(connstring))
            {
                conn.Open();
            }
            return connstring;
        }

        public static string ConnectionString { get => connstring; set => connstring = value; }
    }
}
