using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PersonellRecords.Modules
{
    public static class ModuleHistory
    {
        public static void SaveTableChanges(
            DataTable table,
            string tableName,
            string historyTableName,
            string[] fieldsToTrack,
            string currentUser = null)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table), "Таблица не загружена");

            try
            {
                SaveHistory(table, tableName, historyTableName, fieldsToTrack, currentUser);
                ApplyChangesToDatabase(table, tableName, fieldsToTrack);
                table.AcceptChanges();

                MessageBox.Show("Изменения сохранены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public static void DeleteSelectedRows(
            DataGrid grid,
            DataTable table,
            string tableName,
            string historyTableName,
            string[] fieldsToTrack,
            string currentUser = null)
        {
            if (grid.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите строки для удаления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Удалить выбранные записи ({grid.SelectedItems.Count})?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var idsToDelete = new List<int>();

                foreach (DataRowView rowView in grid.SelectedItems)
                {
                    DataRow row = rowView.Row;
                    int id = Convert.ToInt32(row["Id"]);
                    idsToDelete.Add(id);

                    WriteHistoryEntry(tableName, historyTableName, "DELETE", row, null, fieldsToTrack, currentUser);
                }

                using (var conn = new SqlConnection(ModuleDB.GetConnectionString()))
                {
                    conn.Open();
                    foreach (int id in idsToDelete)
                    {
                        using (var cmd = new SqlCommand($"DELETE FROM {tableName} WHERE Id = @Id", conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                var rowsToDelete = new List<DataRow>();
                foreach (DataRowView rowView in grid.SelectedItems)
                {
                    rowsToDelete.Add(rowView.Row);
                }

                foreach (var row in rowsToDelete)
                {
                    row.Delete();
                }

                table.AcceptChanges();
                MessageBox.Show("Записи удалены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void SaveHistory(DataTable table, string tableName, string historyTableName, string[] fields, string currentUser)
        {
            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Unchanged) continue;

                DataRow oldRow = null;

                if (row.RowState == DataRowState.Modified)
                {
                    oldRow = table.NewRow();
                    foreach (DataColumn col in table.Columns)
                    {
                        oldRow[col] = row[col, DataRowVersion.Original];
                    }
                }
                else if (row.RowState == DataRowState.Added)
                {
                }
                else if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                WriteHistoryEntry(tableName, historyTableName,
                    row.RowState == DataRowState.Added ? "INSERT" : "UPDATE",
                    oldRow, row, fields, currentUser);
            }
        }

        private static void WriteHistoryEntry(
            string tableName,
            string historyTableName,
            string changeType,
            DataRow oldRow,
            DataRow newRow,
            string[] fields,
            string currentUser)
        {
            int originalId = 0;

            if (newRow != null && newRow["Id"] != DBNull.Value)
            {
                originalId = Convert.ToInt32(newRow["Id"]);
            }
            else if (oldRow != null && oldRow["Id"] != DBNull.Value)
            {
                originalId = Convert.ToInt32(oldRow["Id"]);
            }

            bool hasChangedByUser = CheckIfColumnExists(historyTableName, "ChangedByUser");

            string columns = "OriginalId, ChangeType, ChangeDate";
            if (hasChangedByUser)
                columns += ", ChangedByUser";

            string values = "@id, @type, GETDATE()";
            if (hasChangedByUser)
                values += ", @changedBy";

            var parameters = new List<(string name, object value)>
            {
                ("@id", originalId),
                ("@type", changeType)
            };

            if (hasChangedByUser)
            {
                parameters.Add(("@changedBy", currentUser ?? (object)DBNull.Value));
            }

            foreach (var field in fields)
            {
                string oldField = $"Old_{field}";
                string newField = $"New_{field}";

                columns += $", {oldField}, {newField}";
                values += $", @old{field}, @new{field}";

                parameters.Add(($"@old{field}", oldRow?[field] ?? DBNull.Value));
                parameters.Add(($"@new{field}", newRow?[field] ?? DBNull.Value));
            }

            string sql = $@"
                INSERT INTO {historyTableName} ({columns})
                VALUES ({values})";

            using (var conn = new SqlConnection(ModuleDB.GetConnectionString()))
            using (var cmd = new SqlCommand(sql, conn))
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.name, param.value ?? DBNull.Value);
                }

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static bool CheckIfColumnExists(string tableName, string columnName)
        {
            string sql = $@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = @tableName AND COLUMN_NAME = @columnName";

            using (var conn = new SqlConnection(ModuleDB.GetConnectionString()))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@tableName", tableName);
                cmd.Parameters.AddWithValue("@columnName", columnName);

                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private static void ApplyChangesToDatabase(DataTable table, string tableName, string[] fields)
        {
            using (var conn = new SqlConnection(ModuleDB.GetConnectionString()))
            {
                conn.Open();

                foreach (DataRow row in table.Rows)
                {
                    if (row.RowState == DataRowState.Unchanged) continue;
                    if (row.RowState == DataRowState.Deleted) continue;

                    if (row.RowState == DataRowState.Added)
                    {
                        InsertRow(conn, tableName, row, fields);
                    }
                    else if (row.RowState == DataRowState.Modified)
                    {
                        UpdateRow(conn, tableName, row, fields);
                    }
                }
            }
        }

        private static void InsertRow(SqlConnection conn, string tableName, DataRow row, string[] fields)
        {
            string columns = string.Join(", ", fields);
            string parameters = string.Join(", ", fields.Select(f => "@" + f));

            string sql = $@"
                INSERT INTO {tableName} ({columns})
                VALUES ({parameters});
                SELECT CAST(SCOPE_IDENTITY() AS int);";

            using (var cmd = new SqlCommand(sql, conn))
            {
                AddParameters(cmd, row, fields, includeId: false);
                int newId = Convert.ToInt32(cmd.ExecuteScalar());
                row["Id"] = newId;
            }
        }

        private static void UpdateRow(SqlConnection conn, string tableName, DataRow row, string[] fields)
        {
            string setClause = string.Join(", ", fields.Select(f => $"{f} = @{f}"));

            string sql = $@"
                UPDATE {tableName}
                SET {setClause}
                WHERE Id = @Id";

            using (var cmd = new SqlCommand(sql, conn))
            {
                AddParameters(cmd, row, fields, includeId: true);
                cmd.ExecuteNonQuery();
            }
        }

        private static void AddParameters(SqlCommand cmd, DataRow row, string[] fields, bool includeId)
        {
            foreach (var field in fields)
            {
                object value = row[field];
                if (value == null || value == DBNull.Value)
                    cmd.Parameters.AddWithValue("@" + field, DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@" + field, value);
            }

            if (includeId)
            {
                cmd.Parameters.AddWithValue("@Id", row["Id"]);
            }
        }
    }
}