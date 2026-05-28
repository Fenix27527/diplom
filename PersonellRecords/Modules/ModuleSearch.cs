using System;
using System.Collections.Generic;
using System.Data;

namespace PersonellRecords.Modules
{
    internal static class ModuleSearch
    {
        public static void Filter(DataTable table, string searchText, params string[] columnNames)
        {
            if (table == null) return;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                table.DefaultView.RowFilter = string.Empty;
                return;
            }

            string safeSearch = searchText.Replace("'", "''");
            var conditions = new List<string>();

            foreach (var colName in columnNames)
            {
                if (!table.Columns.Contains(colName)) continue;

                DataColumn col = table.Columns[colName];

                if (col.DataType == typeof(string))
                {
                    conditions.Add($"[{colName}] LIKE '%{safeSearch}%'");
                }
                else if (col.DataType == typeof(int) || col.DataType == typeof(long))
                {
                    if (int.TryParse(searchText, out int num))
                    {
                        conditions.Add($"[{colName}] = {num}");
                    }
                }
                else if (col.DataType == typeof(DateTime))
                {
                    if (DateTime.TryParse(searchText, out DateTime date))
                    {
                        conditions.Add($"CONVERT([{colName}], 'System.String') LIKE '{date:yyyy-MM-dd}%'");
                    }
                }
                else
                {
                    conditions.Add($"CONVERT([{colName}], 'System.String') LIKE '%{safeSearch}%'");
                }
            }

            table.DefaultView.RowFilter = conditions.Count > 0
                ? string.Join(" OR ", conditions)
                : string.Empty;
        }
    }
}