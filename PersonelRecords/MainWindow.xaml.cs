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
            Content = new WorkersView(this);
        }

        public void ShowState()
        {
            Content = new StateView(this);
        }

        public void ShowVacancy()
        {
            Content = new VacancyView(this);
        }
    }
}
