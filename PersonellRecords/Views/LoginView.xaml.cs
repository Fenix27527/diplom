using PersonellRecords.Modules;
using System.Windows;
using System.Windows.Controls;

namespace PersonellRecords.Views
{
    public partial class LoginView : UserControl
    {
        private MainWindow _mainWindow;

        public LoginView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void ButInput_Click(object sender, RoutedEventArgs e)
        {
            string login = TBAuthor.Text;
            string password = PBPassword.Password;

            if (ModuleLogin.TryLogin(login, password, out string userFio))
            {
                _mainWindow.CurrentUserFIO = userFio;
                _mainWindow.ShowMenu();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка входа",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButRegister_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowRegister();
        }

        private void BtnCloseProg_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}