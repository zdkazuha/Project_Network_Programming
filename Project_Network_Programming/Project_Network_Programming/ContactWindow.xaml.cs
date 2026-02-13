using DbController;
using Db_Controller.Entities;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Network_Programming
{
    public partial class ContactWindow : Window
    {
        private readonly Db_functional context = new Db_functional();
        public User User { get; set; }

        public ObservableCollection<ListContact> ListContact { get; set; } = new ObservableCollection<ListContact>();

        public ContactWindow(User user)
        {
            InitializeComponent();
            DataContext = this;

            User = user;

            FillListContacts();
        }

        private void FillListContacts()
        {
            ListContact.Clear();

            var contacts = context.GetContact(User.Id); 

            if (contacts == null || contacts.Count == 0)
            {
                ListContact.Add(new ListContact
                {
                    Name = "—",
                    CustomName = "У вас немає контактів"
                });
                return;
            }

            foreach (var contact in contacts)
            {
                ListContact.Add(new ListContact
                {
                    Name = contact.Name,
                    CustomName = contact.CustomName
                });
            }
        }
        private void ContactListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContactListBox.SelectedItem is ListContact selectedContact)
            {
                ContactInfoListBox.Items.Clear();
                ContactInfoListBox.Items.Add($"Username: {selectedContact.Name}");
                ContactInfoListBox.Items.Add($"Custom Name: {selectedContact.CustomName}");

                CustomNameTextBox.Text = selectedContact.CustomName ?? "";
            }
        }
        private void AddContactBtn(object sender, RoutedEventArgs e)
        {
            AddContactWindow addContactWindow = new AddContactWindow(User);
            addContactWindow.ShowDialog();
            FillListContacts();
        }
        private void DeleteContactBtn(object sender, RoutedEventArgs e)
        {
            if (ContactListBox.SelectedItem is ListContact selectedContact)
            {
                var contactUser = context.Users
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Username == selectedContact.Name);

                if (contactUser == null)
                {
                    MessageBox.Show("Контакт не знайдено");
                    return;
                }

                var contactToDelete = context.GetContact(User.Id, contactUser.Id);
                if (contactToDelete != null)
                {
                    context.DeleteContact(contactToDelete);
                    context.SaveChanges();

                    FillListContacts();
                }
                else
                {
                    MessageBox.Show("Контакт не знайдено");
                }
            }
            else
            {
                MessageBox.Show("Виберіть контакт для видалення");
            }
        }
        private void RenameContactBtn(object sender, RoutedEventArgs e)
        {
            if (ContactListBox.SelectedItem is ListContact selectedContact)
            {
                var contactUser = context.Users
                    .FirstOrDefault(u => u.Username == selectedContact.Name);

                if (contactUser == null)
                {
                    MessageBox.Show("Контакт не знайдено");
                    return;
                }

                string newName = CustomNameTextBox.Text?.Trim();

                if (!string.IsNullOrWhiteSpace(newName))
                {
                    try
                    {
                        context.UpdateContactCustomName(User.Id, contactUser.Id, newName);

                        FillListContacts();  
                        CustomNameTextBox.Clear();
                    }
                    catch (InvalidOperationException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Введіть нове ім'я контакту");
                }
            }
            else
            {
                MessageBox.Show("Виберіть контакт для перейменування");
            }
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class ListContact
    {
        public string Name { get; set; }
        public string CustomName { get; set; }

        public override string ToString()
        {
            return $"{CustomName} ({Name})";
        }
    }
}
