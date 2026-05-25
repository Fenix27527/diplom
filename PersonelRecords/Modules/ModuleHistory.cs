using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonelRecords.Modules
{
    internal class ModuleHistory
    {
        public static void SaveWorkerHistory(int originalId, string changeType, DataRow oldRow, DataRow newRow)
        {
            DataRow rowForOld = (changeType == "INSERT") ? null : oldRow;
            DataRow rowForNew = (changeType == "DELETE") ? null : newRow;
            string sql = @"
                INSERT INTO HISTORYWorkers 
                (OriginalId, ChangeType, ChangeDate,
                 Old_FIO, Old_DateOfBirth, Old_Division, Old_Post, Old_INN, Old_Address, Old_DateOfReception, Old_Family, Old_Education, Old_Awards,
                 New_FIO, New_DateOfBirth, New_Division, New_Post, New_INN, New_Address, New_DateOfReception, New_Family, New_Education, New_Awards)
                VALUES
                (@id, @type, GETDATE(),
                 @oldFIO, @oldDOB, @oldDiv, @oldPost, @oldINN, @oldAddr, @oldDOR, @oldFam, @oldEdu, @oldAwd,
                 @newFIO, @newDOB, @newDiv, @newPost, @newINN, @newAddr, @newDOR, @newFam, @newEdu, @newAwd)";

            using (SqlConnection conn = new SqlConnection(ModuleDB.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                // Общие параметры
                cmd.Parameters.AddWithValue("@id", originalId);
                cmd.Parameters.AddWithValue("@type", changeType);

                // Параметры для старых значений
                AddRowParameters(cmd, rowForOld, "old");
                // Параметры для новых значений
                AddRowParameters(cmd, rowForNew, "new");

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static void AddRowParameters(SqlCommand cmd, DataRow row, string prefix)
        {
            // Функция: получить значение из колонки или DBNull.Value
            object GetValue(string colName)
            {
                if (row == null) return DBNull.Value;
                object val = row[colName];
                return (val == null || val == DBNull.Value) ? DBNull.Value : val;
            }

            cmd.Parameters.AddWithValue($"@{prefix}FIO", GetValue("FIO"));
            cmd.Parameters.AddWithValue($"@{prefix}DOB", GetValue("DateOfBirth"));
            cmd.Parameters.AddWithValue($"@{prefix}Div", GetValue("Division"));
            cmd.Parameters.AddWithValue($"@{prefix}Post", GetValue("Post"));
            cmd.Parameters.AddWithValue($"@{prefix}INN", GetValue("INN"));
            cmd.Parameters.AddWithValue($"@{prefix}Addr", GetValue("Address"));
            cmd.Parameters.AddWithValue($"@{prefix}DOR", GetValue("DateOfReception"));
            cmd.Parameters.AddWithValue($"@{prefix}Fam", GetValue("Family"));
            cmd.Parameters.AddWithValue($"@{prefix}Edu", GetValue("Education"));
            cmd.Parameters.AddWithValue($"@{prefix}Awd", GetValue("Awards"));
        }
    }
}