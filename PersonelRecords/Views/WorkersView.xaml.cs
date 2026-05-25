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
    public partial class WorkersView : UserControl
    {
        private MainWindow _mainWindow;
        private DataTable _workersTable;
        private SqlDataAdapter _adapter;
        private string _primaryKeyColumn = "Id";

        public WorkersView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            Loaded += WorkersView_Loaded;
        }

        private void WorkersView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var (table, adapter) = ModuleDB.FillWithAdapter("SELECT * FROM Workers");
                _workersTable = table;
                _adapter = adapter;

                // Определяем имя колонки первичного ключа (без учёта регистра)
                var pkCol = _workersTable.Columns.Cast<DataColumn>()
                    .FirstOrDefault(c => c.ColumnName.Equals("Id", StringComparison.OrdinalIgnoreCase));
                if (pkCol != null)
                    _primaryKeyColumn = pkCol.ColumnName;
                else
                    _primaryKeyColumn = _workersTable.Columns[0].ColumnName; // запасной вариант

                // Убедитесь, что в командах SqlCommandBuilder используется правильное имя
                SqlCommandBuilder builder = new SqlCommandBuilder(_adapter);
                _adapter.InsertCommand = builder.GetInsertCommand();
                _adapter.UpdateCommand = builder.GetUpdateCommand();
                _adapter.DeleteCommand = builder.GetDeleteCommand();

                TABLEWorkers.ItemsSource = _workersTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }



        private void ButUpdate_Click(object sender, RoutedEventArgs e)
        {
            TableHelper.SaveChanges(_workersTable, _adapter, "Id",
        (id, oldRow, newRow) => ModuleHistory.SaveWorkerHistory(id, "UPDATE", oldRow, newRow));
            MessageBox.Show("Сохранено");
        }
        

        private void ButDelete_Click(object sender, RoutedEventArgs e)
        {
            TableHelper.DeleteSelectedRows(TABLEWorkers, _workersTable, _adapter, "Id",
         (id, row) => ModuleHistory.SaveWorkerHistory(id, "DELETE", row, null));
        }

        private void ButHistory_Click(object sender, RoutedEventArgs e)
        {
            HistoryWindow hw = new HistoryWindow();
            hw.Owner = Window.GetWindow(this);
            hw.ShowDialog();
        }

        private void ButSearch_Click(object sender, RoutedEventArgs e)
        {
            ModuleSearch.Filter(_workersTable, TbSearch.Text,
                "FIO", "Division", "Post", "INN", "Address", "Family", "Education", "Awards");
        }

        private void ButBack_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowMenu();
        }
    }
}
