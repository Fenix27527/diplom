using PersonellRecords.Modules;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace PersonellRecords.Views
{
    public partial class StateView : UserControl
    {
        private MainWindow _mainWindow;
        private DataTable _stateTable;

        private readonly string[] _trackedFields = new[]
        {
            "Division", "Post", "NumberOfWorkers", "NumberOfHours", "Salary"
        };

        public StateView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        public void LoadData()
        {
            try
            {
                var (table, adapter) = ModuleDB.FillWithAdapter("SELECT * FROM State");
                _stateTable = table;
                TABLEState.ItemsSource = _stateTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButUpd_Click(object sender, RoutedEventArgs e)
        {
            ModuleHistory.SaveTableChanges(
                table: _stateTable,
                tableName: "State",
                historyTableName: "HISTORYState",
                fieldsToTrack: _trackedFields,
                currentUser: _mainWindow.CurrentUserFIO
            );
        }

        private void ButDel_Click(object sender, RoutedEventArgs e)
        {
            ModuleHistory.DeleteSelectedRows(
                grid: TABLEState,
                table: _stateTable,
                tableName: "State",
                historyTableName: "HISTORYState",
                fieldsToTrack: _trackedFields,
                currentUser: _mainWindow.CurrentUserFIO
            );
        }

        private void ButHistory_Click(object sender, RoutedEventArgs e)
        {
            var hw = new HistoryWindow("HISTORYState");
            hw.Owner = Window.GetWindow(this);
            hw.ShowDialog();
        }

        private void ButSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ModuleSearch.Filter(_stateTable, TbSearch.Text,
                    "Division", "Post", "NumberOfWorkers", "NumberOfHours", "Salary");

                TABLEState.ItemsSource = null;
                TABLEState.ItemsSource = _stateTable.DefaultView;
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
                    FileName = $"Подразделения_{DateTime.Now:yyyy-MM-dd}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Подразделения");

                        worksheet.Cell(1, 1).InsertTable(_stateTable, createTable: false);

                        var headerRow = worksheet.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRow.Style.Font.FontSize = 12;

                        worksheet.Column(4).Style.NumberFormat.Format = "0";
                        worksheet.Column(5).Style.NumberFormat.Format = "0.00";
                        worksheet.Column(6).Style.NumberFormat.Format = "#,##0.00";

                        worksheet.Columns().AdjustToContents();

                        worksheet.Column(1).Width = 5;
                        worksheet.Column(2).Width = 20;
                        worksheet.Column(3).Width = 25;
                        worksheet.Column(4).Width = 15;
                        worksheet.Column(5).Width = 15;
                        worksheet.Column(6).Width = 15;

                        workbook.SaveAs(saveDialog.FileName);
                    }

                    MessageBox.Show("Данные экспортированы!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButBack_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowMenu();
        }
    }
}