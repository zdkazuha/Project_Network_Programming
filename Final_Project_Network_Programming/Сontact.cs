using System;

public class Contact
{
	public Contact()
	{
		Users = new List<User>();
    }

	[Key]
	public int ID { get; set; }

    [MaxLength(50), Required]
    public string ContactName { get; set; }

    [MaxLength(50), Required]
    public string ContactEmail { get; set; }

    [MaxLength(50), Required]
    public string ContactPhone { get; set; }


	public ICollection<User> Users { get; set; }
}
