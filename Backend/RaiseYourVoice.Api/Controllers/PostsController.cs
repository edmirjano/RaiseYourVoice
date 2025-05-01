using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IGenericRepository<Post> _postRepository;

        public PostsController(IGenericRepository<Post> postRepository)
        {
            _postRepository = postRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts([FromQuery] PostType? postType = null)
        {
            var posts = await _postRepository.GetAllAsync();
            
            if (postType.HasValue)
            {
                posts = posts.Where(p => p.PostType == postType.Value);
            }
            
            return Ok(posts.OrderByDescending(p => p.CreatedAt));
        }

        [HttpGet("feed")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Post>>> GetSocialFeed()
        {
            var posts = await _postRepository.GetAllAsync();
            return Ok(posts.Where(p => p.PostType == PostType.Activism && p.Status == PostStatus.Published)
                          .OrderByDescending(p => p.CreatedAt));
        }

        [HttpGet("opportunities")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Post>>> GetOpportunities()
        {
            var posts = await _postRepository.GetAllAsync();
            return Ok(posts.Where(p => p.PostType == PostType.Opportunity && p.Status == PostStatus.Published)
                          .OrderByDescending(p => p.CreatedAt));
        }

        [HttpGet("success-stories")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Post>>> GetSuccessStories()
        {
            var posts = await _postRepository.GetAllAsync();
            return Ok(posts.Where(p => p.PostType == PostType.SuccessStory && p.Status == PostStatus.Published)
                          .OrderByDescending(p => p.CreatedAt));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Post>> GetPost(string id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return Ok(post);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator,Activist,Organization")]
        public async Task<ActionResult<Post>> CreatePost(Post post)
        {
            // Set author ID from the authenticated user
            post.AuthorId = User.Identity?.Name ?? string.Empty;
            post.CreatedAt = DateTime.UtcNow;
            post.Status = PostStatus.Published;
            
            await _postRepository.AddAsync(post);
            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePost(string id, Post post)
        {
            if (id != post.Id)
            {
                return BadRequest();
            }
            
            // Check if user is author or admin/moderator
            var existingPost = await _postRepository.GetByIdAsync(id);
            if (existingPost == null)
            {
                return NotFound();
            }
            
            if (existingPost.AuthorId != User.Identity?.Name && 
                !User.IsInRole("Admin") && 
                !User.IsInRole("Moderator"))
            {
                return Forbid();
            }
            
            post.UpdatedAt = DateTime.UtcNow;
            var success = await _postRepository.UpdateAsync(post);
            if (!success)
            {
                return NotFound();
            }
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(string id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            
            // Check if user is author or admin/moderator
            if (post.AuthorId != User.Identity?.Name && 
                !User.IsInRole("Admin") && 
                !User.IsInRole("Moderator"))
            {
                return Forbid();
            }
            
            var success = await _postRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }
            
            return NoContent();
        }
    }
}