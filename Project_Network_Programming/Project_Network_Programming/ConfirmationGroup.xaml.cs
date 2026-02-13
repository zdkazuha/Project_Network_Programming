using DbController;
using Db_Controller.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
    /// Interaction logic for Confirmation.xaml
    /// </summary>
    public partial class ConfirmationGroup : Window
    {
        Db_functional context = new Db_functional();
        public User user;

        private string ChatName;

        public ConfirmationGroup(string SenderUser, User User, string GroupName, int GroupId)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            GroupNameLabel.Content = GroupName;
            UserNameLabel.Content = SenderUser;

            user = User;
            ChatName = GroupName;

            ResultUser = null;
        }

        private void YesBtn(object sender, RoutedEventArgs e)
        {
            try
            {
                var chat = context.GetPrivateChat(ChatName);
                var dbUser = context.Users.FirstOrDefault(u => u.Id == user.Id);

                if (dbUser == null)
                {
                    MessageBox.Show("Користувача не знайдено.");
                    return;
                }

                dbUser = context.RenameUserGroup(dbUser, chat);
                context.SaveChanges();

                user = dbUser;
                context.SaveChanges();

                ResultUser = user;

                MessageBox.Show($"Вітаємо! Ви приєдналися до групи {ChatName}.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Виникла помилка: {ex.Message}");
            }
        }

        private void NoBtn(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ви відмовились від запрошення.");
            this.Close();
        }

        public User ResultUser { get; private set; }
    }
}
