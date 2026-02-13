using System.ComponentModel.DataAnnotations;
using System;

public class User
{
    public User()
    {
        Groups = new List<Group>();
    }

    [Key]
    public int ID { get; set; }

    [MaxLength(50), Required]
    public string UserName { get; set; }

    [MaxLength(50), Required]
    public string Password { get; set; }

    [MaxLength(50), Required]
    public string Email { get; set; }

    [MaxLength(50), Required]
    public string Phone { get; set; }

    // Navigation property

    public ICollection<Group> Groups { get; set; }

}