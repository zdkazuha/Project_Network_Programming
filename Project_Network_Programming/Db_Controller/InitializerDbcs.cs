using Db_Controller.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Db_Controller
{
    public static class InitializerDbcs
    {
        public static void SeedUser(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(new User[]
            {
                new User
                {
                    Id = 1,
                    Username = "Oleg",
                    Password = "PasswordOleg",
                    GroupId = 1
                },
                new User
                {
                    Id = 2,
                    Username = "Vova",
                    Password = "PasswordVova",
                    GroupId = 1
                },
                new User
                {
                    Id = 3,
                    Username = "Vanya",
                    Password = "PasswordVanya",
                    GroupId = 2
                },
                new User
                {
                    Id = 4,
                    Username = "Sasha",
                    Password = "PasswordSasha",
                    GroupId = 2
                },
            });
        }
        public static void SeedGroup(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Group>().HasData(new Group[]
            {
                new Group
                {
                    Id = 1,
                    Name = "Group1"
                },
                new Group
                {
                    Id = 2,
                    Name = "Group2"
                },
            });
        }
        public static void SeedContact(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contact>().HasData(new Contact[]
            {
                new Contact
                {
                    Id = 1,
                    UserId = 4,
                    ContactUserId = 1,
                    Name = "Oleg",
                    CustomName = "Oleg123"
                },
                new Contact
                {
                    Id = 2,
                    UserId = 3,
                    ContactUserId = 3,
                    Name = "Vova",
                    CustomName = "Vova123"
                },
                new Contact
                {
                    Id = 3,
                    UserId = 2,
                    ContactUserId = 2,
                    Name = "Vanya",
                    CustomName = "Vanya123"
                },
                new Contact
                {
                    Id = 4,
                    UserId = 1,
                    ContactUserId = 4,
                    Name = "Sasha",
                    CustomName = "Sasha123"
                },
            });
        }
    }
}
