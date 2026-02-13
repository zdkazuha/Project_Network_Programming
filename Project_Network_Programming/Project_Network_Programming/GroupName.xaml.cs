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
    /// Interaction logic for GroupName.xaml
    /// </summary>
    public partial class GroupName : Window
    {
        Db_functional context = new Db_functional();
        public User User;

        public GroupName(User user)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            User = user;

            ResultUser = null;
        }

        private void JointheGroupBtn(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(groupNameBox.Text))
            {
                MessageBox.Show("Будь ласка введіть назву групи");
                return;
            }
            string groupName = groupNameBox.Text;

            var group = context.GetGroup(groupName);
            if (group == null)
            {
                MessageBox.Show("Група не знайдена");
                return;
            }

            var UserRename = context.RenameUserGroup(User, group);
            context.SaveChanges();

            try
            {
                User = UserRename;
                context.SaveChanges();
                
                ResultUser = User;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public User ResultUser { get; private set; }
    }
}
