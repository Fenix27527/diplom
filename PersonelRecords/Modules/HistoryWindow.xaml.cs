using PersonelRecords.Modules;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PersonelRecords.Views
{
    public partial class HistoryWindow : Window
    {
        private DataTable _historyTable; // Исходная таблица из БД
        private DataTable _displayTable; // Таблица для отображения (с русскими названиями)

        public HistoryWindow(string historyTableName)
        {
            InitializeComponent();
            Title = $"История изменений - {GetTableName(historyTableName)}";
            LoadHistory(historyTableName);
        }

        private void LoadHistory(string tableName)
        {
            try
            {
                _historyTable = ModuleDB.ExecuteSelect($"SELECT * FROM {tableName} ORDER BY ChangeDate DESC");

                // Создаём таблицу для отображения с русскими названиями
                _displayTable = new DataTable();
                _displayTable.Columns.Add("ID", typeof(int));
                _displayTable.Columns.Add("№Записи", typeof(int));
                _displayTable.Columns.Add("Тип", typeof(string));
                _displayTable.Columns.Add("Дата/Время", typeof(string));
                _displayTable.Columns.Add("Изменил", typeof(string));

                // Определяем, какие поля есть в таблице (кроме служебных)
                foreach (DataColumn col in _historyTable.Columns)
                {
                    if (col.ColumnName.StartsWith("Old_") &&
                        col.ColumnName != "ChangedByUser")
                    {
                        // Убираем "Old_" и переводим имя
                        string fieldName = col.ColumnName.Substring(4);
                        string russianName = GetColumnName(fieldName);

                        // Добавляем колонки Old и New
                        _displayTable.Columns.Add($"Old_{russianName}", typeof(string));
                        _displayTable.Columns.Add($"New_{russianName}", typeof(string));
                    }
                }

                // Заполняем данными
                foreach (DataRow row in _historyTable.Rows)
                {
                    var newRow = _displayTable.NewRow();
                    newRow["ID"] = row["Id"];
                    newRow["№Записи"] = row["OriginalId"];
                    newRow["Тип"] = GetChangeTypeName(row["ChangeType"].ToString());
                    newRow["Дата/Время"] = Convert.ToDateTime(row["ChangeDate"]).ToString("dd.MM.yyyy HH:mm");
                    newRow["Изменил"] = row["ChangedByUser"] != DBNull.Value ? row["ChangedByUser"].ToString() : "-";

                    // Заполняем Old/New значения
                    foreach (DataColumn col in _historyTable.Columns)
                    {
                        if (col.ColumnName.StartsWith("Old_") && col.ColumnName != "ChangedByUser")
                        {
                            string fieldName = col.ColumnName.Substring(4); // Убираем "Old_"
                            string russianName = GetColumnName(fieldName);
                            string value = row[col] != DBNull.Value ? row[col].ToString() : "-";
                            newRow[$"Old_{russianName}"] = value;
                        }
                        else if (col.ColumnName.StartsWith("New_") && col.ColumnName != "ChangedByUser")
                        {
                            string fieldName = col.ColumnName.Substring(4); // Убираем "New_"
                            string russianName = GetColumnName(fieldName);
                            string value = row[col] != DBNull.Value ? row[col].ToString() : "-";
                            newRow[$"New_{russianName}"] = value;
                        }
                    }

                    _displayTable.Rows.Add(newRow);
                }

                DataGridHistory.ItemsSource = _displayTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataGridHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridHistory.SelectedItem is DataRowView selectedRow)
            {
                // Находим соответствующую строку в исходной таблице по ID
                int id = Convert.ToInt32(selectedRow["ID"]);
                DataRow originalRow = null;

                foreach (DataRow row in _historyTable.Rows)
                {
                    if (Convert.ToInt32(row["Id"]) == id)
                    {
                        originalRow = row;
                        break;
                    }
                }

                if (originalRow != null)
                {
                    // Отображаем старые значения
                    string oldValues = "";
                    string newValues = "";

                    foreach (DataColumn col in _historyTable.Columns)
                    {
                        if (col.ColumnName.StartsWith("Old_") && col.ColumnName != "ChangedByUser")
                        {
                            string fieldName = GetColumnName(col.ColumnName.Substring(4));
                            object value = originalRow[col];
                            if (value != DBNull.Value && !string.IsNullOrEmpty(value.ToString()))
                            {
                                oldValues += $"{fieldName}: {value}\n";
                            }
                        }
                        else if (col.ColumnName.StartsWith("New_") && col.ColumnName != "ChangedByUser")
                        {
                            string fieldName = GetColumnName(col.ColumnName.Substring(4));
                            object value = originalRow[col];
                            if (value != DBNull.Value && !string.IsNullOrEmpty(value.ToString()))
                            {
                                newValues += $"{fieldName}: {value}\n";
                            }
                        }
                    }

                    OldValuesTextBlock.Text = string.IsNullOrEmpty(oldValues) ? "(нет данных)" : oldValues;
                    NewValuesTextBlock.Text = string.IsNullOrEmpty(newValues) ? "(нет данных)" : newValues;
                    TBType.Text = GetChangeTypeName(originalRow["ChangeType"].ToString());
                }
            }
        }

        private string GetColumnName(string englishName)
        {
            switch (englishName)
            {
                case "FIO": return "ФИО";
                case "DateOfBirth": return "Дата рождения";
                case "Division": return "Подразделение";
                case "Post": return "Должность";
                case "INN": return "ИНН";
                case "Address": return "Адрес";
                case "DateOfReception": return "Дата приёма";
                case "Family": return "Семья";
                case "Education": return "Образование";
                case "Awards": return "Награды";
                case "NumberOfWorkers": return "Кол-во сотрудников";
                case "NumberOfHours": return "Кол-во часов";
                case "Salary": return "Ставка";
                case "Conditions": return "Условия";
                case "VacancyId": return "№ Вакансии";
                case "Link": return "Ссылка";
                default: return englishName;
            }
        }

        private string GetChangeTypeName(string changeType)
        {
            switch (changeType?.ToUpper())
            {
                case "INSERT": return "СОЗДАНИЕ";
                case "UPDATE": return "ИЗМЕНЕНИЕ";
                case "DELETE": return "УДАЛЕНИЕ";
                default: return changeType ?? "—";
            }
        }

        private string GetTableName(string tableName)
        {
            switch (tableName)
            {
                case "HISTORYWorkers": return "Работники";
                case "HISTORYState": return "Подразделения";
                case "HISTORYVacancy": return "Вакансии";
                case "HISTORYResumes": return "Резюме";
                default: return tableName;
            }
        }
    }
}