using PersonelRecords.Modules;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PersonelRecords.Views
{
    public partial class StateView : UserControl
    {
        private MainWindow _mainWindow;
        private DataTable _stateTable;
        // Поля для отслеживания изменений (кроме Id)
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
                // Загружаем таблицу через ModuleDB
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

                // ✅ Обновляем отображение
                TABLEState.ItemsSource = null;
                TABLEState.ItemsSource = _stateTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ButBack_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowMenu();
        }

    }
}
