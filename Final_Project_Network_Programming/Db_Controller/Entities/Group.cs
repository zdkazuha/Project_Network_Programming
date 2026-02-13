using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Db_Controller.Entities
{
    public class Group
    {
        public Group()
        {
            Users = new List<User>();
        }
        [Key]
        public int Id { get; set; }
        public string Name { get ; set; }

        // Navigation properties
        public ICollection<User> Users { get; set; } 
    }
}
