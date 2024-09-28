using System.ComponentModel.DataAnnotations;

namespace Blog.Data.ViewModels;

public class CreatePostVM
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public List<string>? ImageUrls { get; set; } = new();

    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; } // Make CategoryId nullable
    public string AuthorId { get; set; }
}

