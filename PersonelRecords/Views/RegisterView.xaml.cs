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

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(fio) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
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
