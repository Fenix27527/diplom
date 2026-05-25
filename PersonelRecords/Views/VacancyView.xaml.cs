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
    public partial class VacancyView : UserControl
    {
        private MainWindow _mainWindow;
        private DataTable _vacancyTable;
        private SqlDataAdapter _vacancyAdapter;
        private DataTable _resumesTable;
        private SqlDataAdapter _resumesAdapter;

        public VacancyView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            Loaded += VacancyView_Loaded;
        }
        private void VacancyView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var (vTable, vAdapter) = ModuleDB.FillWithAdapter("SELECT * FROM Vacancy");
                _vacancyTable = vTable;
                _vacancyAdapter = vAdapter;
                var vBuilder = new SqlCommandBuilder(_vacancyAdapter);
                _vacancyAdapter.InsertCommand = vBuilder.GetInsertCommand();
                _vacancyAdapter.UpdateCommand = vBuilder.GetUpdateCommand();
                _vacancyAdapter.DeleteCommand = vBuilder.GetDeleteCommand();
                TABLEVacancy.ItemsSource = _vacancyTable.DefaultView;

                var (rTable, rAdapter) = ModuleDB.FillWithAdapter("SELECT * FROM Resumes");
                _resumesTable = rTable;
                _resumesAdapter = rAdapter;
                var rBuilder = new SqlCommandBuilder(_resumesAdapter);
                _resumesAdapter.InsertCommand = rBuilder.GetInsertCommand();
                _resumesAdapter.UpdateCommand = rBuilder.GetUpdateCommand();
                _resumesAdapter.DeleteCommand = rBuilder.GetDeleteCommand();
                TABLEResume.ItemsSource = _resumesTable.DefaultView;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка");
            }
        }

        private void UpdateVacancyDB()
        {
            try 
            {
                _vacancyAdapter.Update(_vacancyTable); 
            }
            catch (System.Exception ex) 
            {
                MessageBox.Show("Ошибка сохранения вакансий: " + ex.Message, "Ошибка"); 
            }
        }

        private void UpdateResumesDB()
        {
            try 
            { 
                _resumesAdapter.Update(_resumesTable); 
            }
            catch (System.Exception ex) 
            { 
                MessageBox.Show("Ошибка сохранения резюме: " + ex.Message, "Ошибка"); 
            }
        }
        private void ButUpdVac_Click(object sender, RoutedEventArgs e)
        {
            UpdateVacancyDB();
            MessageBox.Show("Данные обновлены", "Успех!");
        }
        private void ButDelVac_Click(object sender, RoutedEventArgs e)
        {
            if (TABLEVacancy.SelectedItems == null || TABLEVacancy.SelectedItems.Count == 0)
                return;

            var rowsDel = new List<DataRow>();
            foreach (DataRowView rowView in TABLEVacancy.SelectedItems)
            {
                rowsDel.Add(rowView.Row);
            }

            foreach (var row in rowsDel)
            {
                row.Delete();
            }
        }

        private void ButUpdRes_Click(object sender, RoutedEventArgs e)
        {
            UpdateResumesDB();
            MessageBox.Show("Данные обновлены", "Успех!");
        }

        private void ButDelRes_Click(object sender, RoutedEventArgs e)
        {
            if (TABLEResume.SelectedItems == null || TABLEResume.SelectedItems.Count == 0)
                return;

            var rowsDel = new List<DataRow>();
            foreach (DataRowView rowView in TABLEResume.SelectedItems)
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
            ModuleSearch.Filter(_vacancyTable, TbSearch.Text, "Post", "Conditions");
            ModuleSearch.Filter(_resumesTable, TbSearch.Text, "FIO", "Post", "Link");
        }
        private void ButBack_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowMenu();
        }
    }
}
