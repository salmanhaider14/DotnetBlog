﻿using Blog.Data;
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
            ImageUrls = post.ImageUrls
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
            .Take(9)
            .ToListAsync();
    }
    public async Task<(List<Category> Categories, int TotalPages)> GetAllCategoriesAsync(string keyword = null, int? pageNumber = null, int? pageSize = null)
    {
        var query = _context.Categories.AsQueryable();

        // Apply search filter if keyword is provided
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(c => c.Name.Contains(keyword));
        }

        // Count total categories matching the query
        var totalCategories = await query.CountAsync();

        // Set a default pageSize if none is provided
        var pageSizeValue = pageSize ?? totalCategories; // If pageSize is null, show all categories on one page
        var totalPages = (int)Math.Ceiling(totalCategories / (double)pageSizeValue);

        // Apply pagination only if both pageNumber and pageSize are provided
        if (pageNumber.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip((pageNumber.Value - 1) * pageSizeValue)
                .Take(pageSizeValue);
        }

        // Execute the query and return the results
        var categories = await query.ToListAsync();
        return (categories, totalPages);
    }
    public async Task<Category?> GetCategoryByIdAsync(int Id)
    {
        return _context.Categories
            .Include(c => c.Posts)
            .ThenInclude(p => p.Author)
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
    public async Task<(List<Post> Posts, int TotalPages)> GetPostsByAuthorAsync(string authorId, string keyword, int pageNumber, int pageSize, string category = null)
    {

        var query = _context.Posts
            .Where(p => p.AuthorId == authorId)
            .Include(p => p.Category)
            .Include(p => p.Author)
            .AsQueryable();

        // Apply search if a keyword is provided
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p => p.Title.Contains(keyword) || p.Content.Contains(keyword));
        }
        // Apply category filter if selected
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category.Name == category);
        }

        // Calculate total number of posts
        var totalPosts = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

        // Apply pagination
        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (posts, totalPages);
    }

    /// <summary>
    /// Retrieves a paginated list of posts.
    /// </summary>
    public async Task<(List<Post> Posts, int TotalPages)> SearchAndPaginatePostsAsync(string keyword, string category, string author, int pageNumber, int pageSize)
    {
        var query = _context.Posts
            .Include(p => p.Category)
            .Include(p => p.Author)
            .AsQueryable();

        // Apply search if a keyword is provided
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p => p.Title.Contains(keyword) || p.Content.Contains(keyword));
        }
        // Apply category filter if selected
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category.Name == category);
        }

        // Apply author filter if selected
        if (!string.IsNullOrWhiteSpace(author))
        {
            query = query.Where(p => p.Author.FullName == author);
        }

        // Calculate total number of posts
        var totalPosts = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

        // Apply pagination
        var posts = await query
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
        existingPost.ImageUrls = updatedPost.ImageUrls;
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
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var post = await _context.Posts
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                return false;

            // Gather and delete all comments recursively
            var commentsToDelete = post.Comments.ToList();

            foreach (var comment in commentsToDelete)
            {
                await DeleteCommentAndChildrenAsync(comment.Id);
            }

            // Delete the post itself
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception(ex.Message);
        }
    }



    public async Task DeleteCommentAndChildrenAsync(int commentId)
    {
        // Initialize a list to gather all comments to be deleted
        var commentsToDelete = new List<Comment>();

        // Recursively gather all comments and child comments
        await GatherCommentsToDeleteAsync(commentId, commentsToDelete);

        // Remove all comments in one batch
        _context.Comments.RemoveRange(commentsToDelete);
        await _context.SaveChangesAsync();
    }

    private async Task GatherCommentsToDeleteAsync(int commentId, List<Comment> commentsToDelete)
    {
        var comment = await _context.Comments
            .Include(c => c.ChildComments)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null) return;

        // Add the current comment to the list
        commentsToDelete.Add(comment);

        // Recursively gather all child comments
        foreach (var childComment in comment.ChildComments)
        {
            await GatherCommentsToDeleteAsync(childComment.Id, commentsToDelete);
        }
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
