using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Db_Controller.Entities
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ContactUserId { get; set; }
        public string Name { get; set; }
        public string CustomName { get; set; }

        // Navigation properties
        public User User { get; set; }
        public User ContactUser { get; set; }
    }

}
