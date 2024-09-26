namespace Blog.Data.Models;

public class Comment
{
    public int Id { get; set; }
    public required string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public required string AuthorId { get; set; }
    public int PostId { get; set; }

    // Navigation Properties
    public ApplicationUser Author { get; set; }
    public Post Post { get; set; }
}

