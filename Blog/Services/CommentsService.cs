using Blog.Data;
using Blog.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services;

public class CommentsService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        // Get top-level comments for the specified post
        var topLevelComments = await _context.Comments
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .Include(c => c.Author)
            .ToListAsync();

        // Recursively load child comments
        foreach (var comment in topLevelComments)
        {
            await LoadChildComments(comment);
        }

        return topLevelComments;
    }

    private async Task LoadChildComments(Comment parentComment)
    {
        // Load child comments for the given parent comment
        var childComments = await _context.Comments
            .Where(c => c.ParentCommentId == parentComment.Id)
            .Include(c => c.Author) // Include the author of child comments
            .ToListAsync();

        parentComment.ChildComments = childComments; // Assign loaded child comments

        // Recursively load child comments for each child comment
        foreach (var childComment in childComments)
        {
            await LoadChildComments(childComment);
        }
    }
    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment?> EditCommentAsync(Comment comment)
    {
        var existingComment = await _context.Comments.FindAsync(comment.Id);

        if (existingComment == null)
        {
            throw new KeyNotFoundException($"Comment with ID {comment.Id} was not found.");
        }

        existingComment.Content = comment.Content;
        existingComment.UpdatedAt = DateTime.UtcNow;

        _context.Comments.Update(existingComment);
        await _context.SaveChangesAsync();

        return existingComment;
    }

    public async Task DeleteCommentAsync(int commentId)
    {
        var comment = await _context.Comments
            .Include(c => c.ChildComments) // Load child comments
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment is null)
        {
            throw new KeyNotFoundException($"Comment with ID {commentId} was not found.");
        }

        // Recursively delete all child comments
        await DeleteChildCommentsAsync(comment);

        // Delete the parent comment
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }

    private async Task DeleteChildCommentsAsync(Comment comment)
    {
        // Load all child comments
        var childComments = await _context.Comments
            .Where(c => c.ParentCommentId == comment.Id)
            .ToListAsync();

        // Recursively delete each child comment
        foreach (var childComment in childComments)
        {
            await DeleteChildCommentsAsync(childComment); // Delete child comments recursively
            _context.Comments.Remove(childComment); // Remove the child comment from the context
        }
    }
}
