using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PersonelRecords.Modules
{
    public partial class HistoryWindow : Window
    {
        private DataTable _historyTable;

        public HistoryWindow()
        {
            InitializeComponent();
            Loaded += HistoryWindow_Loaded;
        }

        private void HistoryWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string sql = "SELECT * FROM HISTORYWorkers ORDER BY ChangeDate DESC";
            DataTable dt = ModuleDB.ExecuteSelect(sql);

            ListHistory.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                string date = Convert.ToDateTime(row["ChangeDate"]).ToString("dd.MM.yyyy HH:mm");
                string type = row["ChangeType"].ToString();
                string fio = (row["New_FIO"] as string) ?? (row["Old_FIO"] as string) ?? "(нет ФИО)";
                int originalId = Convert.ToInt32(row["OriginalId"]);

                ListBoxItem item = new ListBoxItem();
                item.Content = $"{date} - {type} : {fio} (ID {originalId})";
                item.Tag = row;
                ListHistory.Items.Add(item);
            }
        }

        private void ListHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListHistory.SelectedItem is ListBoxItem selected && selected.Tag is DataRow row)
            {
                TBType.Text = row["ChangeType"].ToString();

                // Словарь перевода названий полей (без префиксов)
                var rusNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "FIO", "ФИО" },
                    { "DateOfBirth", "Дата рождения" },
                    { "Division", "Подразделение" },
                    { "Post", "Должность" },
                    { "INN", "ИНН" },
                    { "Address", "Адрес" },
                    { "DateOfReception", "Дата приёма" },
                    { "Family", "Семейное положение" },
                    { "Education", "Образование" },
                    { "Awards", "Награды" }
                };

                string oldDetails = "";
                string newDetails = "";

                foreach (DataColumn col in row.Table.Columns)
                {
                    string colName = col.ColumnName;
                    // Пропускаем служебные колонки (без учёта регистра)
                    if (colName.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                        colName.Equals("OriginalId", StringComparison.OrdinalIgnoreCase) ||
                        colName.Equals("ChangeType", StringComparison.OrdinalIgnoreCase) ||
                        colName.Equals("ChangeDate", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (colName.StartsWith("Old_", StringComparison.OrdinalIgnoreCase))
                    {
                        string field = colName.Substring(4);
                        object val = row[colName];
                        if (val != null && val != DBNull.Value)
                        {
                            string display = rusNames.ContainsKey(field) ? rusNames[field] : field;
                            oldDetails += $"{display}: {val}\n";
                        }
                    }
                    else if (colName.StartsWith("New_", StringComparison.OrdinalIgnoreCase))
                    {
                        string field = colName.Substring(4);
                        object val = row[colName];
                        if (val != null && val != DBNull.Value)
                        {
                            string display = rusNames.ContainsKey(field) ? rusNames[field] : field;
                            newDetails += $"{display}: {val}\n";
                        }
                    }
                }

                OldValuesTextBlock.Text = string.IsNullOrEmpty(oldDetails) ? "(нет данных)" : oldDetails;
                NewValuesTextBlock.Text = string.IsNullOrEmpty(newDetails) ? "(нет данных)" : newDetails;
            }
        }
    }
}