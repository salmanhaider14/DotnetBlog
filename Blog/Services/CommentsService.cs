using Blog.Data;
using Blog.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services;

public class CommentsService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        return await _context.Comments
                             .Where(c => c.PostId == postId)
                             .Include(c => c.Author)
                             .ToListAsync();
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
        var comment = await _context.Comments.FindAsync(commentId);

        if (comment is null)
        {
            throw new KeyNotFoundException($"Comment with ID {commentId} was not found.");
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }


    public async Task<Comment?> GetCommentByIdAsync(int commentId)
    {
        return await _context.Comments
                             .Include(c => c.Author)  // Include the author if needed
                             .FirstOrDefaultAsync(c => c.Id == commentId);
    }
}
