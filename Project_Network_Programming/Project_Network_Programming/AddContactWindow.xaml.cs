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
    /// Interaction logic for AddContactWindow.xaml
    /// </summary>
    public partial class AddContactWindow : Window
    {
        Db_functional context = new Db_functional();
        public User User;
        string UserName;
        string CustomUserName;
        public AddContactWindow(User user)
        {
            InitializeComponent();

            User = user;
        }

        private void AddContactBtn(object sender, RoutedEventArgs e)
        {
            UserName = UserNameBox.Text;
            if (string.IsNullOrWhiteSpace(UserName))
            {
                MessageBox.Show("Введіть ім'я користувача");
                return;
            }

            CustomUserName = CustomUserNameBox.Text;
            if (string.IsNullOrWhiteSpace(CustomUserName))
            {
                MessageBox.Show("Введіть кастомне ім'я користувача");
                return;
            }

            var user_ = context.GetUser(UserName);
            if (user_ == null)
            {
                MessageBox.Show("Користувача не знайдено");
                return;
            }

            if (user_.Id == User.Id)
            {
                MessageBox.Show("Не можна додати себе в контакти");
                return;
            }

            var contact = context.GetContact(User.Id,user_.Id);
            if (contact != null)
            {
                MessageBox.Show("Контакт вже існує");
                return;
            }

            try
            {
                context.AddContact(User, user_, CustomUserName);
                MessageBox.Show("Контакт успішно додано");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при додаванні контакту: " + ex.Message);
            }
        }
    }
}
