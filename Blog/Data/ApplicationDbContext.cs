using Blog.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>().HasData(
    // Main categories
    new Category { Id = 1, Name = "Technology" },
    new Category { Id = 2, Name = "Lifestyle" },
    new Category { Id = 3, Name = "Travel" },
    new Category { Id = 4, Name = "Education" },
    new Category { Id = 5, Name = "Health" },

    // Technology subcategories
    new Category { Id = 6, Name = "Programming" },
    new Category { Id = 7, Name = "Gaming" },
    new Category { Id = 8, Name = "Software" },
    new Category { Id = 9, Name = "Hardware" },
    new Category { Id = 10, Name = "Artificial Intelligence" },

    // Lifestyle subcategories
    new Category { Id = 11, Name = "Fashion" },
    new Category { Id = 12, Name = "Food" },
    new Category { Id = 13, Name = "Wellness" },
    new Category { Id = 14, Name = "Relationships" },
    new Category { Id = 15, Name = "Personal Development" },

    // Travel subcategories
    new Category { Id = 16, Name = "Adventure" },
    new Category { Id = 17, Name = "Cultural" },
    new Category { Id = 18, Name = "Foodie" },
    new Category { Id = 19, Name = "Luxury" },
    new Category { Id = 20, Name = "Budget" },

    // Education subcategories
    new Category { Id = 21, Name = "Academic" },
    new Category { Id = 22, Name = "Professional Development" },
    new Category { Id = 23, Name = "Online Courses" },
    new Category { Id = 24, Name = "Language Learning" },
    new Category { Id = 25, Name = "Career Advice" },

    // Health subcategories
    new Category { Id = 26, Name = "Fitness" },
    new Category { Id = 27, Name = "Nutrition" },
    new Category { Id = 28, Name = "Mental Health" },
    new Category { Id = 29, Name = "Self-Care" },
    new Category { Id = 30, Name = "Disease Prevention" },

    // Additional categories
    new Category { Id = 31, Name = "Business" },
    new Category { Id = 32, Name = "Finance" },
    new Category { Id = 33, Name = "Sports" },
    new Category { Id = 34, Name = "Entertainment" },
    new Category { Id = 35, Name = "Politics" }
);

    }
}
