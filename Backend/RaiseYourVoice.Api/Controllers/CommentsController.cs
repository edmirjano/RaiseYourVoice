using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaiseYourVoice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly IGenericRepository<Comment> _commentRepository;
        private readonly IGenericRepository<Post> _postRepository;

        public CommentsController(
            IGenericRepository<Comment> commentRepository,
            IGenericRepository<Post> postRepository)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
        }

        [HttpGet("post/{postId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByPost(string postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            var comments = await _commentRepository.FindAsync(c => c.PostId == postId);
            return Ok(comments.OrderByDescending(c => c.CreatedAt));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Comment>> GetComment(string id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            return Ok(comment);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Comment>> CreateComment(Comment comment)
        {
            var post = await _postRepository.GetByIdAsync(comment.PostId);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            // Set the author ID from the authenticated user
            comment.AuthorId = User.Identity.Name;
            comment.CreatedAt = DateTime.UtcNow;
            
            await _commentRepository.AddAsync(comment);
            
            // Update comment count on the post
            post.CommentCount++;
            await _postRepository.UpdateAsync(post);
            
            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(string id, Comment comment)
        {
            if (id != comment.Id)
            {
                return BadRequest();
            }
            
            // Check if user is the author or an admin/moderator
            var existingComment = await _commentRepository.GetByIdAsync(id);
            if (existingComment == null)
            {
                return NotFound();
            }
            
            if (existingComment.AuthorId != User.Identity.Name && 
                !User.IsInRole("Admin") && 
                !User.IsInRole("Moderator"))
            {
                return Forbid();
            }
            
            comment.UpdatedAt = DateTime.UtcNow;
            var success = await _commentRepository.UpdateAsync(comment);
            if (!success)
            {
                return NotFound();
            }
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(string id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            
            // Check if user is the author or an admin/moderator
            if (comment.AuthorId != User.Identity.Name && 
                !User.IsInRole("Admin") && 
                !User.IsInRole("Moderator"))
            {
                return Forbid();
            }
            
            var success = await _commentRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }
            
            // Update comment count on the post
            var post = await _postRepository.GetByIdAsync(comment.PostId);
            if (post != null)
            {
                post.CommentCount = Math.Max(0, post.CommentCount - 1);
                await _postRepository.UpdateAsync(post);
            }
            
            return NoContent();
        }
    }
}