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
        private SqlDataAdapter _adapter;
        public StateView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            Loaded += StateView_Loaded;
        }
        private void StateView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var (table, adapter) = ModuleDB.FillWithAdapter("SELECT * FROM State");
                _stateTable = table;
                _adapter = adapter;

                // Настраиваем автоматическое создание команд (INSERT, UPDATE, DELETE)
                SqlCommandBuilder builder = new SqlCommandBuilder(_adapter);
                _adapter.InsertCommand = builder.GetInsertCommand();
                _adapter.UpdateCommand = builder.GetUpdateCommand();
                _adapter.DeleteCommand = builder.GetDeleteCommand();

                TABLEState.ItemsSource = _stateTable.DefaultView;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка");
            }
        }

        private void UpdateDB()
        {
            try
            {
                _adapter.Update(_stateTable);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Ошибка");
            }
        }
        private void ButUpd_Click(object sender, RoutedEventArgs e)
        {
            UpdateDB();
            MessageBox.Show("Данные обновлены", "Успех!");
        }

        private void ButDel_Click(object sender, RoutedEventArgs e)
        {
            if (TABLEState.SelectedItems == null || TABLEState.SelectedItems.Count == 0)
                return;

            var rowsDel = new List<DataRow>();
            foreach (DataRowView rowView in TABLEState.SelectedItems)
            {
                rowsDel.Add(rowView.Row);
            }

            foreach (var row in rowsDel)
            {
                row.Delete();
            }
        }
        private void ButSearch_Click(object sender, RoutedEventArgs e)
        {
            ModuleSearch.Filter(_stateTable, TbSearch.Text, "Division", "Post", "NumberOfWorkers", "NumberOfHours", "Salary");
        }
        private void ButBack_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowMenu();
        }
    }
}
