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

namespace PersonelRecords.Views
{
    public partial class MenuView : UserControl
    {
        private MainWindow _mainWindow;
        public MenuView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void BtnWorkers_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowWorkers();
        }

        private void BtnVacancy_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowVacancy();
        }

        private void BtnState_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowState();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
