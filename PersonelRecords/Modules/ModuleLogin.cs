using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonelRecords.Modules
{
    internal class ModuleLogin
    {
        public static bool Login(string login, string password)
        {
            string sql = $"SELECT COUNT(*) FROM Logins WHERE Login = '{login}' AND Password = '{password}'";
            DataTable dt = ModuleDB.ExecuteSelect(sql);
            int count = Convert.ToInt32(dt.Rows[0][0]);
            return count > 0;
        }
    }
}
