using ClosedXML.Excel;
using Microsoft.Win32;
using PersonellRecords.Modules;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PersonellRecords.Views
{
    public partial class WorkersView : UserControl
    {
        private MainWindow _mainWindow;
        private DataTable _workersTable;
        private DataTable _divisionPostList;

        private readonly string[] _trackedFields = new[]
        {
            "FIO", "DateOfBirth", "Division", "Post", "INN",
            "Address", "DateOfReception", "Family", "Education", "Awards"
        };

        public WorkersView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        public void LoadData()
        {
            try
            {
                var (table, adapter) = ModuleDB.FillWithAdapter("SELECT * FROM Workers");
                _workersTable = table;
                _divisionPostList = ModuleDB.GetDivisionPostList();
                TABLEWorkers.ItemsSource = _workersTable.DefaultView;
                SetupComboBoxColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupComboBoxColumns()
        {
            var divisionColumn = TABLEWorkers.Columns
                .Cast<DataGridColumn>()
                .FirstOrDefault(c => c.Header?.ToString() == "Подразделение") as DataGridTextColumn;

            var postColumn = TABLEWorkers.Columns
                .Cast<DataGridColumn>()
                .FirstOrDefault(c => c.Header?.ToString() == "Должность") as DataGridTextColumn;

            if (divisionColumn != null)
            {
                var comboBoxColumn = new DataGridComboBoxColumn
                {
                    Header = "Подразделение",
                    Width = divisionColumn.Width,
                    ItemsSource = _divisionPostList.DefaultView,
                    DisplayMemberPath = "Division",
                    SelectedValuePath = "Division",
                    SelectedValueBinding = new Binding("Division")
                    {
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    }
                };
                int index = TABLEWorkers.Columns.IndexOf(divisionColumn);
                TABLEWorkers.Columns.RemoveAt(index);
                TABLEWorkers.Columns.Insert(index, comboBoxColumn);
            }

            if (postColumn != null)
            {
                var comboBoxColumn = new DataGridComboBoxColumn
                {
                    Header = "Должность",
                    Width = postColumn.Width,
                    ItemsSource = _divisionPostList.DefaultView,
                    DisplayMemberPath = "Post",
                    SelectedValuePath = "Post",
                    SelectedValueBinding = new Binding("Post")
                    {
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    }
                };

                int index = TABLEWorkers.Columns.IndexOf(postColumn);
                TABLEWorkers.Columns.RemoveAt(index);
                TABLEWorkers.Columns.Insert(index, comboBoxColumn);
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private bool IsTextAllowed(string text)
        {
            return Regex.IsMatch(text, @"^[0-9]+$");
        }

        private void InnColumn_LosingFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text != null)
            {
                string inn = textBox.Text.Trim();

                if (inn.Length < 10 || inn.Length > 12)
                {
                    MessageBox.Show("ИНН должен содержать от 10 до 12 цифр!", "Ошибка ввода",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    textBox.Focus();
                }
            }
        }

        private void ApplyInnValidation()
        {
            var innColumn = TABLEWorkers.Columns
                .Cast<DataGridColumn>()
                .FirstOrDefault(c => c.Header?.ToString() == "ИНН");

            if (innColumn != null)
            {
                TABLEWorkers.PreparingCellForEdit += (s, e) =>
                {
                    if (e.Column == innColumn && e.EditingElement is TextBox textBox)
                    {
                        textBox.PreviewTextInput += TextBox_PreviewTextInput;
                        textBox.LostFocus += InnColumn_LosingFocus;
                    }
                };
            }
        }

        private void ButUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAllInn())
                return;
            ModuleHistory.SaveTableChanges(
                table: _workersTable,
                tableName: "Workers",
                historyTableName: "HISTORYWorkers",
                fieldsToTrack: _trackedFields,
                currentUser: _mainWindow.CurrentUserFIO
            );
        }

        private bool ValidateAllInn()
        {
            foreach (DataRow row in _workersTable.Rows)
            {
                if (row.RowState == DataRowState.Unchanged) continue;

                var innValue = row["INN"];
                if (innValue != null && innValue != DBNull.Value)
                {
                    string inn = innValue.ToString().Trim();
                    if (!Regex.IsMatch(inn, @"^[0-9]{10,12}$"))
                    {
                        MessageBox.Show(
                            $"Неверный формат ИНН в строке {row["Id"]}: должно быть 10-12 цифр",
                            "Ошибка валидации",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return false;
                    }
                }
            }
            return true;
        }

        private void ButDelete_Click(object sender, RoutedEventArgs e)
        {
            ModuleHistory.DeleteSelectedRows(
                 grid: TABLEWorkers,
                 table: _workersTable,
                 tableName: "Workers",
                 historyTableName: "HISTORYWorkers",
                 fieldsToTrack: _trackedFields,
                 currentUser: _mainWindow.CurrentUserFIO
             );
        }

        private void ButHistory_Click(object sender, RoutedEventArgs e)
        {
            var hw = new HistoryWindow("HISTORYWorkers");
            hw.Owner = Window.GetWindow(this);
            hw.ShowDialog();
        }

        private void ButSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ModuleSearch.Filter(_workersTable, TbSearch.Text,
                    "FIO", "Division", "Post", "INN", "Address", "Family", "Education", "Awards");

                TABLEWorkers.ItemsSource = null;
                TABLEWorkers.ItemsSource = _workersTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"Сотрудники_{DateTime.Now:yyyy-MM-dd}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Сотрудники");

                        for (int i = 0; i < _workersTable.Columns.Count; i++)
                        {
                            worksheet.Cell(1, i + 1).Value = _workersTable.Columns[i].ColumnName;
                        }

                        for (int i = 0; i < _workersTable.Rows.Count; i++)
                        {
                            for (int j = 0; j < _workersTable.Columns.Count; j++)
                            {
                                var cell = worksheet.Cell(i + 2, j + 1);
                                var value = _workersTable.Rows[i][j];

                                if (value is DateTime dateTime)
                                {
                                    cell.Value = dateTime.Date;
                                    cell.Style.DateFormat.Format = "dd.MM.yyyy";
                                }
                                else
                                {
                                    cell.Value = value?.ToString() ?? "";
                                }
                            }
                        }

                        var headerRow = worksheet.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRow.Style.Font.FontSize = 12;

                        worksheet.Column(6).Style.NumberFormat.Format = "@";
                        worksheet.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.Column(3).Style.DateFormat.Format = "dd.MM.yyyy";
                        worksheet.Column(8).Style.DateFormat.Format = "dd.MM.yyyy";

                        worksheet.Column(7).Style.Alignment.WrapText = true;
                        worksheet.Column(9).Style.Alignment.WrapText = true;
                        worksheet.Column(10).Style.Alignment.WrapText = true;
                        worksheet.Column(11).Style.Alignment.WrapText = true;

                        worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.Columns().AdjustToContents();

                        worksheet.Column(1).Width = 5;
                        worksheet.Column(2).Width = 25;
                        worksheet.Column(3).Width = 12;
                        worksheet.Column(4).Width = 20;
                        worksheet.Column(5).Width = 20;
                        worksheet.Column(6).Width = 15;
                        worksheet.Column(7).Width = 30;
                        worksheet.Column(8).Width = 12;
                        worksheet.Column(9).Width = 20;
                        worksheet.Column(10).Width = 20;
                        worksheet.Column(11).Width = 20;

                        worksheet.Range(1, 1, 1, _workersTable.Columns.Count).SetAutoFilter();

                        workbook.SaveAs(saveDialog.FileName);
                    }

                    MessageBox.Show($"Данные экспортированы в файл:\n{saveDialog.FileName}", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButBack_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.ShowMenu();
        }
    }
}