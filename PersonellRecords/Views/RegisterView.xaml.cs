using PersonellRecords.Modules;
using System.Windows;
using System.Windows.Controls;

namespace PersonellRecords.Views
{
    public partial class RegisterView : UserControl
    {
        private MainWindow _mainWindow;

        public RegisterView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void ButReg_Click(object sender, RoutedEventArgs e)
        {
            string login = TBLogin.Text;
            string fio = TBfio.Text;
            string password = TBPassword.Text;
            string email = TBEmail.Text;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(fio) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка");
                return;
            }

            if (!email.Contains("@"))
            {
                MessageBox.Show("Введите корректный email (с символом @).", "Ошибка");
                return;
            }

            if (ModuleRegister.Register(login, fio, password, email))
            {
                MessageBox.Show("Регистрация успешна! Теперь войдите.", "Успех!");
                _mainWindow.ShowLogin();
            }
            else
            {
                MessageBox.Show("Данные логин или почта уже зарегистрированы.", "Ошибка");
            }
        }

        private void ButBack_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowLogin();
        }
    }
}