using Blog.Data;
using Blog.Data.Models;
using Blog.Data.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services;

public class PostService(ApplicationDbContext _context)
{

    #region Create

    /// <summary>
    /// Adds a new post to the database.
    /// </summary>
    public async Task<Post> CreatePostAsync(CreatePostVM post)
    {
        if (post is null)
            throw new ArgumentNullException(nameof(post));

        post.CreatedAt = DateTime.UtcNow;

        var postToAdd = new Post
        {
            Title = post.Title,
            Content = post.Content,
            AuthorId = post.AuthorId,
            CategoryId = post.CategoryId,
        };

        _context.Posts.Add(postToAdd);
        await _context.SaveChangesAsync();

        return postToAdd;
    }

    #endregion

    #region Read

    /// <summary>
    /// Retrieves all posts with their related Category and Author.
    /// </summary>
    public async Task<List<Post>> GetAllPostsAsync()
    {
        return await _context.Posts
            .OrderByDescending(p => p.CreatedAt)
            .Include(p => p.Category)
            .Include(p => p.Author)
            .ToListAsync();
    }
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .ToListAsync();
    }
    public async Task<Category?> GetCategoryByIdAsync(int Id)
    {
        return _context.Categories
            .Include(c => c.Posts)
            .FirstOrDefault(c => c.Id == Id);

    }

    /// <summary>
    /// Retrieves a post by its Id.
    /// </summary>
    public async Task<Post?> GetPostByIdAsync(int id)
    {
        return await _context.Posts
            .Include(p => p.Category)
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Retrieves posts by CategoryId.
    /// </summary>
    public async Task<List<Post>> GetPostsByCategoryAsync(int categoryId)
    {
        return await _context.Posts
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Category)
            .Include(p => p.Author)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves posts by AuthorId.
    /// </summary>
    public async Task<List<Post>> GetPostsByAuthorAsync(string authorId)
    {
        return await _context.Posts
            .Where(p => p.AuthorId == authorId)
            .OrderByDescending(p => p.CreatedAt)
            .Include(p => p.Category)
            .Include(p => p.Author)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves posts based on a search keyword in the title.
    /// </summary>
    public async Task<List<Post>> SearchPostsAsync(string keyword)
    {
        return await _context.Posts
            .Where(p => p.Title.Contains(keyword))
            .Include(p => p.Category)
            .Include(p => p.Author)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a paginated list of posts.
    /// </summary>
    public async Task<(List<Post> Posts, int TotalPages)> GetPaginatedPostsAsync(int pageNumber, int pageSize)
    {
        var totalPosts = await _context.Posts.CountAsync();
        var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

        var posts = await _context.Posts
            .Include(p => p.Category)
            .Include(p => p.Author)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (posts, totalPages);
    }
    #endregion

    #region Update

    /// <summary>
    /// Updates an existing post.
    /// </summary>
    public async Task<Post?> UpdatePostAsync(UpdatePostVM updatedPost, int Id)
    {
        var existingPost = await _context.Posts.FindAsync(Id);

        if (existingPost == null)
            return null;

        existingPost.Title = updatedPost.Title;
        existingPost.Content = updatedPost.Content;
        existingPost.CategoryId = updatedPost.CategoryId;
        existingPost.ImageUrl = updatedPost.ImageUrl;
        existingPost.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingPost;
    }

    #endregion

    #region Delete

    /// <summary>
    /// Deletes a post by its Id.
    /// </summary>
    public async Task<bool> DeletePostAsync(int postId)
    {
        var post = await _context.Posts.FindAsync(postId);

        if (post == null)
            return false;

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Validation Helpers

    /// <summary>
    /// Checks if a post with the given Id exists.
    /// </summary>
    public async Task<bool> PostExistsAsync(int id)
    {
        return await _context.Posts.AnyAsync(p => p.Id == id);
    }

    /// <summary>
    /// Checks if a post with the given title already exists.
    /// </summary>
    public async Task<bool> PostTitleExistsAsync(string title)
    {
        return await _context.Posts.AnyAsync(p => p.Title == title);
    }

    #endregion
}
