using PersonelRecords.Modules;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PersonelRecords.Views
{
    public partial class WorkersView : UserControl
    {
        private MainWindow _mainWindow;
        private DataTable _workersTable;
        private DataTable _divisionPostList;

        // Поля для отслеживания изменений (кроме Id)
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
                // 1. Загружаем основную таблицу
                var (table, adapter) = ModuleDB.FillWithAdapter("SELECT * FROM Workers");
                _workersTable = table;

                // 2. Загружаем список подразделений и должностей для выпадающих списков
                _divisionPostList = ModuleDB.GetDivisionPostList();

                // 3. Привязываем данные к DataGrid
                TABLEWorkers.ItemsSource = _workersTable.DefaultView;

                // 4. Настраиваем выпадающие списки в колонках
                SetupComboBoxColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// Настраивает ComboBox для колонок Division и Post
        /// Настраивает ComboBox для колонок Division и Post
        private void SetupComboBoxColumns()
        {
            // Находим колонки по заголовкам
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
                    DisplayMemberPath = "Division",      // Что показывать в списке
                    SelectedValuePath = "Division",       // Что записывать в ячейку
                    SelectedValueBinding = new System.Windows.Data.Binding("Division")
                    {
                        UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
                    }
                };

                // Заменяем колонку
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
                    SelectedValueBinding = new System.Windows.Data.Binding("Post")
                    {
                        UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
                    }
                };

                int index = TABLEWorkers.Columns.IndexOf(postColumn);
                TABLEWorkers.Columns.RemoveAt(index);
                TABLEWorkers.Columns.Insert(index, comboBoxColumn);
            }
        }

        /// Валидация ИНН при вводе (только цифры, 10-12 символов)
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            e.Handled = !IsTextAllowed(e.Text);
        }

        private bool IsTextAllowed(string text)
        {
            return Regex.IsMatch(text, @"^[0-9]+$");
        }

        /// Проверка длины ИНН при потере фокуса
        private void InnColumn_LosingFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text != null)
            {
                string inn = textBox.Text.Trim();

                if (inn.Length < 10 || inn.Length > 12)
                {
                    MessageBox.Show("ИНН должен содержать от 10 до 12 цифр!", "Ошибка ввода",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    textBox.Focus(); // Возвращаем фокус
                }
            }
        }

        /// Применяем валидацию к колонке ИНН при загрузке
        private void ApplyInnValidation()
        {
            var innColumn = TABLEWorkers.Columns
                .Cast<DataGridColumn>()
                .FirstOrDefault(c => c.Header?.ToString() == "ИНН");

            if (innColumn != null)
            {
                // Добавляем обработчик для всех ячеек этой колонки
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
        /// Проверяет все строки на корректность ИНН перед сохранением
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
                // Фильтруем таблицу
                ModuleSearch.Filter(_workersTable, TbSearch.Text,
                    "FIO", "Division", "Post", "INN", "Address", "Family", "Education", "Awards");

                // ✅ ОБНОВЛЯЕМ таблицу на экране (критически важно!)
                TABLEWorkers.ItemsSource = null;
                TABLEWorkers.ItemsSource = _workersTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButBack_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.ShowMenu();
        }
    }
}
