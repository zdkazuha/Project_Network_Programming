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
    /// Interaction logic for Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        Db_functional context = new Db_functional();
        public Registration()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void RegistoryBtn(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(usernameBox.Text) || string.IsNullOrWhiteSpace(passwordOneBox.Text) || string.IsNullOrWhiteSpace(passwordTwoBox.Text))
            {
                MessageBox.Show("Будь ласка ведіть і'мя або пароль");
                return;
            }

            if (passwordOneBox.Text != passwordTwoBox.Text)
            {
                MessageBox.Show("Паролі не співпадають");
                return;
            }

            string username = usernameBox.Text;
            string password = passwordOneBox.Text;

            bool Isuser = context.IsUserExist(username);
            if (Isuser == false)
            {
                User User = new User
                {
                    Username = username,
                    Password = password,
                    GroupId = 1
                };

                context.AddUser(User);
                MessageBox.Show("Вітаємо, ви успішно зареєстровані!");

                MainWindow mainWindow = new MainWindow(User);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Вже є користувач с таким іменем!!!");
                return;
            }
        }
    }
}
