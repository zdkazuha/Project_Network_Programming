using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Db_Controller.Entities
{
    public class User
    {
        public User()
        {
            Contacts = new List<Contact>();
        }
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int GroupId { get; set; }

        // Navigation properties
        public Group Group { get; set; }
        public ICollection<Contact> Contacts { get; set; }
    }
}
