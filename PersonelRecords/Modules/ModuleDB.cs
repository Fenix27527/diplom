using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace PersonelRecords.Modules
{
    internal class ModuleDB
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["CONNECTION"].ConnectionString;
        public static string GetConnectionString() => connectionString;


        public static DataTable ExecuteSelect(string sql)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                adapter.Fill(dt);
            }
            return dt;
        }

        public static int ExecuteNonQuery(string sql)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static (DataTable Table, SqlDataAdapter Adapter) FillWithAdapter(string sql)
        {
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(connectionString);
            SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(dt);
            return (dt, adapter);
        }
    }
}