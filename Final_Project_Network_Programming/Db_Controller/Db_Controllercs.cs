using DbController;
using Db_Controller.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Db_Controller;

namespace DbController
{
    public class Db_Controller : DbContext
    {
        public Db_Controller()
        {
            if (!this.Database.CanConnect())
            {
                this.Database.EnsureDeleted();  
                this.Database.EnsureCreated(); 
            }
            else
            {
                this.Database.EnsureCreated();  
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;
                              Initial Catalog=DbChat;
                              Integrated Security=True;
                              Encrypt=False;
                              TrustServerCertificate=False;
                              Application Intent=ReadWrite;
                              Multi Subnet Failover=False;
                              Connect Timeout=60");

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Contact>()
                .HasOne(c => c.User)   
                .WithMany(u => u.Contacts)  
                .HasForeignKey(c => c.UserId);  

            modelBuilder.Entity<Contact>()
                .HasOne(c => c.ContactUser)  
                .WithMany() 
                .HasForeignKey(c => c.ContactUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Group)
                .WithMany(g => g.Users)
                .HasForeignKey(u => u.GroupId);

            modelBuilder.SeedUser();
            modelBuilder.SeedGroup();
            modelBuilder.SeedContact();
        }



        public DbSet<User> Users { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Group> Groups { get; set; }
    }
}
