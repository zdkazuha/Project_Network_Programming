using DbController;
using Db_Controller.Entities;
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
using System.Windows.Shapes;

namespace Final_Project_Network_Programming
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {

        Db_functional context = new Db_functional();
        public Login()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void LoginBtn(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(usernameBox.Text) || string.IsNullOrWhiteSpace(passwordBox.Text))
            {
                MessageBox.Show("Будь ласка ведіть і'мя або пароль");
                return;
            }

            string username = usernameBox.Text;
            string password = passwordBox.Text;

            var user = context.GetUser(username, password);
            if(user != null)
            {
                MainWindow mainWindow = new MainWindow(user);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Невірне ім'я користувача або пароль \nМожливо ви бажаєту зарееструватись");
            }
        }

        private void RegistoryBtn(object sender, RoutedEventArgs e)
        {
            Registration registration = new Registration();
            registration.Show();

            this.Close();
        }
    }
}
