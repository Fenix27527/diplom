using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonelRecords.Modules
{
    internal class ModuleRegister
    {
        public static bool Register(string login,string fio, string password, string email)
        {
            // 1. Проверяем, есть ли уже такой логин или email
            string checkSql = $"SELECT COUNT(*) FROM Logins WHERE Login = '{login}' OR Email = '{email}' OR FIO = '{fio}'";
            DataTable dt = ModuleDB.ExecuteSelect(checkSql);
            int count = Convert.ToInt32(dt.Rows[0][0]);
            if (count > 0)
                return false;  

            // 2. Вставляем нового пользователя
            string insertSql = $"INSERT INTO Logins (Login, FIO, Password, Email) VALUES ('{login}', '{fio}', '{password}', '{email}')";
            ModuleDB.ExecuteNonQuery(insertSql);
            return true;
        }
    }
}
