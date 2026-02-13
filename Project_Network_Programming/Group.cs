using System;

public class Group
{
	public Group()
	{
        Users = new List<User>();
    }

	[Key]
	public int ID { get; set; }

    [MaxLength(50), Required]
    public string GroupName { get; set; }

    [MaxLength(50), Required]
    public string GroupAddresse { get; set; }

    [MaxLength(50), Required]
    public string GroupPort { get; set; }

    // Navigation property

	public ICollection<User> Users { get; set; }

}
