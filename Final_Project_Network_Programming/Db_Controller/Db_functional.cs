using Db_Controller.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DbController
{
    public class Db_functional
    {
        private readonly Db_Controller context;
        public DbSet<User> Users => context.Users;
        public DbSet<Group> Group => context.Groups;

        public Db_functional() 
        {
            context = new Db_Controller();
        }

        public void AddUser(string username, string password)
        {
            var user = new User
            {
                Username = username,
                Password = password,
                GroupId = 0
            };

            context.Users.Add(user);
            context.SaveChanges();
        }
        public void AddUser(User user)
        {
            context.Users.Add(user);
            context.SaveChanges();
        }
        public void AddGroup(string name)
        {
            var group = new Group
            {
                Name = name
            };
            context.Groups.Add(group);
            context.SaveChanges();
        }
        public void AddGroup(Group group)
        {
            context.Groups.Add(group);
            context.SaveChanges();
        }
        public void AddContact(int userId, int contactUserId)
        {
            var contact = new Contact
            {
                UserId = userId,
                ContactUserId = contactUserId,
                CustomName = null
            };
            context.Contacts.Add(contact);
            context.SaveChanges();
        }
        public User GetUser(string username, string password)
        {
            return context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }
        public User GetUser(string username)
        {
            return context.Users
                .Include(u => u.Group)
                .AsNoTracking()
                .FirstOrDefault(u => u.Username == username);
        }
        public User GetUser(int Id)
        {
            return context.Users
                .Include(u => u.Group)
                .AsNoTracking()
                .FirstOrDefault(u => u.Id == Id);
        }
        public Group GetGroup(string groupName)
        {
            return context.Groups
                .AsNoTracking()
                .FirstOrDefault(g => g.Name == groupName);
        }
        public bool IsUserExist(string username)
        {
            return context.Users.Any(u => u.Username == username);
        }
        public void AddUserToGroup(User user, string groupName)
        {
            var group = context.Groups.FirstOrDefault(g => g.Name == groupName);
            if (group == null)
            {
                group = new Group { Name = groupName };
                context.Groups.Add(group);
            }

            if (user.GroupId == group.Id) 
                return; 

            user.GroupId = group.Id;
            user.Group = group;

            context.SaveChanges();
        }
        public User GetUserByTcpClient(TcpClient client)
        {
            return context.Users.FirstOrDefault(u => u.Username == client.Client.RemoteEndPoint.ToString());
        }
        public string GetNameGroup(int groupId)
        {
            var group = context.Groups.FirstOrDefault(g => g.Id == groupId);
            return group?.Name;
        }
        public User RenameUserGroup(User user, Group newGroup)
        {
            var dbUser = context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (dbUser == null)
            {
                throw new InvalidOperationException("Користувача не знайдено в базі.");
            }

            dbUser.GroupId = newGroup.Id;
            dbUser.Group = newGroup;

            context.Users.Update(dbUser);
            context.SaveChanges();

            return dbUser;
        }
        public Group GetPrivateChat(string name)
        {
            return new Group ()
            {
                Name = name
            };
        }
        public void AddContact(User User, User user,string customName)
        {
            context.Contacts.Add(new Contact
            {
                UserId = User.Id,
                ContactUserId = user.Id,
                Name = user.Username,
                CustomName = customName
            });
            context.SaveChanges();
        }
        public Contact GetContact(int userId, int contactUserId)
        {
            Contact contact = context.Contacts
                .Include(u => u.User)
                .Include(cu => cu.ContactUser)
                .AsNoTracking()
                .FirstOrDefault(n => n.UserId == userId && n.ContactUserId == contactUserId);

            return contact;
        }
        public List<Contact> GetContact(int ID)
        {
            List<Contact> contacts = context.Contacts
                .Include(u => u.User)
                .Include(cu => cu.ContactUser)
                .AsNoTracking()
                .Where(n => n.UserId == ID)
                .ToList();

            return contacts;
        }
        public void DeleteContact(Contact contact)
        {
            context.Attach(contact);
            context.Contacts.Remove(contact);
            context.SaveChanges();
        }
        public void UpdateContactCustomName(int userId, int contactUserId, string newCustomName)
        {
            var contact = context.Contacts
                .FirstOrDefault(c => c.UserId == userId && c.ContactUserId == contactUserId);

            if (contact == null)
            {
                throw new InvalidOperationException("Контакт не знайдений");
            }

            contact.CustomName = newCustomName;

            context.Contacts.Update(contact);
            context.SaveChanges();
        }

        public void SaveChanges()
        {
            context.SaveChanges();
        }
    }
}
