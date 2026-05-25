using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace PersonelRecords.Modules
{

    public static class TableHelper
    {
        // Загрузка таблицы
        public static (DataTable Table, SqlDataAdapter Adapter) LoadTable(string tableName)
        {
            return ModuleDB.FillWithAdapter($"SELECT * FROM {tableName}");
        }

        // Настройка адаптера с автокомандами
        public static void ConfigureAdapter(SqlDataAdapter adapter)
        {
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            adapter.InsertCommand = builder.GetInsertCommand();
            adapter.UpdateCommand = builder.GetUpdateCommand();
            adapter.DeleteCommand = builder.GetDeleteCommand();
        }

        // Обновление базы
        public static void UpdateTable(DataTable table, SqlDataAdapter adapter, bool reload = false)
        {
            adapter.Update(table);
            if (reload)
            {
                table.Clear();
                adapter.Fill(table);
            }
            else
                table.AcceptChanges();
        }

        // Получение оригинальной строки
        public static DataRow GetOriginalRow(DataRow row, DataTable templateTable)
        {
            DataRow original = templateTable.NewRow();
            foreach (DataColumn col in templateTable.Columns)
            {
                original[col] = row.HasVersion(DataRowVersion.Original)
                    ? row[col, DataRowVersion.Original]
                    : DBNull.Value;
            }
            return original;
        }

        // Безопасное получение Id
        public static bool TryGetId(DataRow row, string keyColumn, out int id)
        {
            object val = row[keyColumn];
            if (val == null || val == DBNull.Value)
            {
                id = 0;
                return false;
            }
            id = Convert.ToInt32(val);
            return true;
        }

        // Удаление выбранных строк с сохранением истории
        public static void DeleteSelectedRows(DataGrid grid, DataTable table, SqlDataAdapter adapter,
            string keyColumn, Action<int, DataRow> saveHistoryForDelete)
        {
            if (grid.SelectedItems == null || grid.SelectedItems.Count == 0) return;

            var rowsToDelete = new List<DataRow>();
            foreach (DataRowView rv in grid.SelectedItems)
                rowsToDelete.Add(rv.Row);

            foreach (var row in rowsToDelete)
            {
                if (TryGetId(row, keyColumn, out int id))
                    saveHistoryForDelete(id, row);
            }

            foreach (var row in rowsToDelete)
                row.Delete();

            UpdateTable(table, adapter);
        }

        // Сохранение изменений с историей
        public static void SaveChanges(DataTable table, SqlDataAdapter adapter, string keyColumn,
            Action<int, DataRow, DataRow> saveHistoryForUpdate)
        {
            var changes = table.GetChanges();
            if (changes != null)
            {
                foreach (DataRow row in changes.Rows)
                {
                    if (row.RowState == DataRowState.Modified)
                    {
                        if (TryGetId(row, keyColumn, out int id))
                        {
                            DataRow oldRow = GetOriginalRow(row, table);
                            saveHistoryForUpdate(id, oldRow, row);
                        }
                    }
                    // Можно обработать Added и Deleted отдельно
                }
            }
            UpdateTable(table, adapter);
        }
    }
}
