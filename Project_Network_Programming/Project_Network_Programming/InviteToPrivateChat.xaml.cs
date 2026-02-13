using DbController;
using Db_Controller.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Final_Project_Network_Programming
{
    public partial class InviteToPrivateChat : Window
    {
        private Db_functional context = new Db_functional();
        private StreamWriter sw;
        private User User;

        public InviteToPrivateChat(StreamWriter sw_, User user)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            User = user;
            sw = sw_;

            ResultUser = null;
        }

        private async void InviteBtn(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernameBox.Text) || string.IsNullOrWhiteSpace(chatNameBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть ім’я користувача або назву чату.");
                return;
            }

            string username = usernameBox.Text;
            string chatName = chatNameBox.Text;

            var user_1 = context.GetUser(username);
            if (user_1 != null)
            {
                if (sw == null)
                {
                    MessageBox.Show("Не вдалося відправити повідомлення: немає активного з'єднання.");
                    return;
                }

                if (user_1.Username == User.Username)
                {
                    MessageBox.Show("Ви не можете запросити себе в чат.");
                    return;
                }

                try
                {
                    Group group = context.GetPrivateChat(chatName);
                    if (group == null)
                    {
                        MessageBox.Show("Вказаної групи не існує.");
                        return;
                    }

                    var inviter = context.GetUser(User.Username);
                    if (inviter != null)
                    {
                        inviter = context.RenameUserGroup(inviter, group);
                        context.SaveChanges();
                    }


                    user_1 = inviter;
                    context.SaveChanges();

                    ResultUser = user_1;

                    string message = $"{User.Username}:Invite:{username}:{group.Name}:{group.Id}";
                    await sw.WriteLineAsync(message);
                    await sw.FlushAsync();

                    MessageBox.Show("Запрошення відправлено!");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Сталася помилка при відправці запрошення: {ex.Message}");
                }
            }
            else
                MessageBox.Show("Користувача з таким іменем не існує");
        }
        public User ResultUser { get; private set; }
    }
}
