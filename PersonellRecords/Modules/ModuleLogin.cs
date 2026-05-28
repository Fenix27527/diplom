using System;
using System.Data;
using System.Data.SqlClient;

namespace PersonellRecords.Modules
{
    internal static class ModuleLogin
    {
        public static bool TryLogin(string login, string password, out string fio)
        {
            fio = null;
            string sql = "SELECT FIO FROM Logins WHERE Login = @login AND Password = @password";

            using (var conn = new SqlConnection(ModuleDB.GetConnectionString()))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);

                conn.Open();
                object result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    fio = result.ToString();
                    return true;
                }
            }
            return false;
        }
    }
}