using System;
using System.Data;
using System.Data.SqlClient;

namespace PersonellRecords.Modules
{
    internal static class ModuleRegister
    {
        public static bool Register(string login, string fio, string password, string email)
        {
            string checkSql = "SELECT COUNT(*) FROM Logins WHERE Login = @login OR Email = @email OR FIO = @fio";

            using (var conn = new SqlConnection(ModuleDB.GetConnectionString()))
            using (var cmd = new SqlCommand(checkSql, conn))
            {
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@fio", fio);

                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0) return false;
            }

            string insertSql = "INSERT INTO Logins (Login, FIO, Password, Email) VALUES (@login, @fio, @password, @email)";

            using (var conn = new SqlConnection(ModuleDB.GetConnectionString()))
            using (var cmd = new SqlCommand(insertSql, conn))
            {
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@fio", fio);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@email", email);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }
    }
}