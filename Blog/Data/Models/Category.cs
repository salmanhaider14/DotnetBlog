namespace Blog.Data.Models;

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; } = string.Empty;
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}

