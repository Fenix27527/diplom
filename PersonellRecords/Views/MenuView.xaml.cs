using System.Windows;
using System.Windows.Controls;

namespace PersonellRecords.Views
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