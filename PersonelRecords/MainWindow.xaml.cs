using Microsoft.Win32;
using PersonelRecords.Views;
using System;
using System.Collections.Generic;
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

namespace PersonelRecords
{
    public partial class MainWindow : Window
    {
        public string CurrentUserFIO { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            ShowLogin();
        }
        public void ShowLogin()
        {
            Content = new LoginView(this);
        }
        public void ShowRegister()
        {
            Content = new RegisterView(this);
        }

        public void ShowMenu()
        {
            Content = new MenuView(this);
        }

        public void ShowWorkers()
        {
            var workersView = new WorkersView(this);
            workersView.LoadData();
            Content = workersView;
        }

        public void ShowState()
        {
            var stateView = new StateView(this);
            stateView.LoadData();  // ← Важно!
            Content = stateView;
        }

        public void ShowVacancy()
        {
            var vacancyView = new VacancyView(this);
            vacancyView.LoadData();  // ← Важно!
            Content = vacancyView;
        }
    }
}
