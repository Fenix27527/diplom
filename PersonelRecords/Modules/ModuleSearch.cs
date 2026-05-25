using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

namespace PersonelRecords.Modules
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

            // Экранируем одинарные кавычки в поисковом запросе (чтобы не сломать LIKE)
            string safeSearch = searchText.Replace("'", "''");

            // Строим условие: колонка1 LIKE '%text%' OR колонка2 LIKE '%text%' ...
            var conditions = new List<string>();
            foreach (var col in columnNames)
            {
                conditions.Add($"CONVERT([{col}], 'System.String') LIKE '%{safeSearch}%'");
            }
            table.DefaultView.RowFilter = string.Join(" OR ", conditions);
        }
    }
}
