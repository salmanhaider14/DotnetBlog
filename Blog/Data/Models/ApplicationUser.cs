using Microsoft.AspNetCore.Identity;

namespace Blog.Data.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
