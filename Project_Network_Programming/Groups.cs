using System;

public class Groups
{
    public Groups()
    {
        Groups = new List<Group>();
    }
    [Key]
    public int ID { get; set; }

    public ICollection<Group> Groups { get; set; };
}
