namespace Blog.Data.Models;

public class Post
{
    public int Id { get; set; }
    public required string Title { get; set; } = string.Empty;
    public required string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? ImageUrl { get; set; }


    public int CategoryId { get; set; }
    public required string AuthorId { get; set; }

    // Navigation Properties
    public Category Category { get; set; }
    public ApplicationUser Author { get; set; }
}

