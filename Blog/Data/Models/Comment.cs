namespace Blog.Data.Models;

public class Comment
{
    public int Id { get; set; }
    public required string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public required string AuthorId { get; set; }
    public int PostId { get; set; }
    public int? ParentCommentId { get; set; }
    public Comment ParentComment { get; set; }
    public List<Comment> ChildComments { get; set; } = new List<Comment>();
    // Navigation Properties
    public ApplicationUser Author { get; set; }
    public Post Post { get; set; }
}

