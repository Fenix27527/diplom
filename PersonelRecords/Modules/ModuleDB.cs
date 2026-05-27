using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace PersonelRecords.Modules
{
    internal static class ModuleDB
    {
        // Строка подключения — одна на весь класс
        private static readonly string _connStr =
            ConfigurationManager.ConnectionStrings["CONNECTION"]?.ConnectionString
            ?? throw new InvalidOperationException("Строка подключения 'CONNECTION' не найдена в App.config!");

        public static string GetConnectionString() => _connStr;

        // Вспомогательный метод: создаёт соединение (чтобы не дублировать код)
        private static SqlConnection NewConn() => new SqlConnection(_connStr);

        public static DataTable ExecuteSelect(string sql)
        {
            var dt = new DataTable();
            using (var conn = NewConn())
            {
                using (var adapter = new SqlDataAdapter(sql, conn))
                {
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        public static int ExecuteNonQuery(string sql)
        {
            using (var conn = NewConn())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static (DataTable Table, SqlDataAdapter Adapter) FillWithAdapter(string sql)
        {
            var dt = new DataTable();
            var conn = NewConn();
            var adapter = new SqlDataAdapter(sql, conn);

            try
            {
                adapter.Fill(dt);
                return (dt, adapter);
            }
            catch
            {
                adapter.Dispose();
                conn.Dispose();
                throw;
            }
        }
        // Загружает уникальные комбинации Подразделение-Должность из таблицы State
        public static DataTable GetDivisionPostList()
        {
            return ExecuteSelect("SELECT DISTINCT Division, Post FROM State ORDER BY Division, Post");
        }
    }
}