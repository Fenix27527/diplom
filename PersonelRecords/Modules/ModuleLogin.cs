using System.Data;

namespace PersonelRecords.Modules
{
    internal static class ModuleLogin
    {
        /// Пытается авторизовать пользователя.
        /// Возвращает: 
        ///   - true, если вход успешен, и в out-параметр fio записывает ФИО
        ///   - false, если логин/пароль неверны
        public static bool TryLogin(string login, string password, out string fio)
        {
            fio = null; // По умолчанию — не авторизован

            // Используем параметризованный запрос для защиты от SQL-инъекций
            string sql = "SELECT FIO FROM Logins WHERE Login = @login AND Password = @password";

            using (var conn = new System.Data.SqlClient.SqlConnection(ModuleDB.GetConnectionString()))
            using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);

                conn.Open();
                object result = cmd.ExecuteScalar();

                if (result != null && result != System.DBNull.Value)
                {
                    fio = result.ToString(); // Получили ФИО
                    return true; // Успешный вход
                }
            }

            return false; // Неверный логин или пароль
        }
    }
}