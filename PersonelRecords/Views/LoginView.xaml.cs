using PersonelRecords.Modules;
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

            // Пытаемся войти, получая ФИО в out-параметр
            if (ModuleLogin.TryLogin(login, password, out string userFio))
            {
                // ✅ Сохраняем ФИО текущего пользователя в MainWindow
                _mainWindow.CurrentUserFIO = userFio;

                // Переходим в меню
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