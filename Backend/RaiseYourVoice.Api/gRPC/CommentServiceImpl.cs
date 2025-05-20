using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RaiseYourVoice.Api.Protos;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Application.Models.Pagination;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Api.gRPC
{
    public class CommentServiceImpl : CommentService.CommentServiceBase
    {
        private readonly ILogger<CommentServiceImpl> _logger;
        private readonly IGenericRepository<Comment> _commentRepository;
        private readonly IGenericRepository<Post> _postRepository;
        private readonly IGenericRepository<User> _userRepository;

        public CommentServiceImpl(
            ILogger<CommentServiceImpl> logger,
            IGenericRepository<Comment> commentRepository,
            IGenericRepository<Post> postRepository,
            IGenericRepository<User> userRepository)
        {
            _logger = logger;
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        public override async Task<GetCommentsResponse> GetCommentsByPost(GetCommentsByPostRequest request, ServerCallContext context)
        {
            try
            {
                // Verify post exists
                var post = await _postRepository.GetByIdAsync(request.PostId);
                if (post == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Post not found"));
                }

                var paginationParams = new PaginationParameters
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    SortBy = request.SortBy,
                    Ascending = request.Ascending
                };

                // Get comments for the post
                var filterExpression = request.IncludeReplies
                    ? c => c.PostId == request.PostId
                    : c => c.PostId == request.PostId && c.ParentCommentId == null;

                var result = await _commentRepository.GetPagedAsync(paginationParams, filterExpression);
                
                var response = new GetCommentsResponse
                {
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasPreviousPage = result.HasPreviousPage,
                    HasNextPage = result.HasNextPage
                };
                
                foreach (var comment in result.Items)
                {
                    // Get author information
                    var author = await _userRepository.GetByIdAsync(comment.AuthorId);
                    
                    response.Comments.Add(await MapToCommentModelAsync(comment, author));
                }
                
                return response;
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for post {PostId}", request.PostId);
                throw new RpcException(new Status(StatusCode.Internal, "Error retrieving comments"));
            }
        }

        public override async Task<CommentResponse> GetComment(GetCommentRequest request, ServerCallContext context)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(request.Id);
                
                if (comment == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Comment with ID {request.Id} not found"));
                }
                
                // Get author information
                var author = await _userRepository.GetByIdAsync(comment.AuthorId);
                
                return new CommentResponse
                {
                    Comment = await MapToCommentModelAsync(comment, author)
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comment {CommentId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Error retrieving comment"));
            }
        }

        public override async Task<GetCommentsResponse> GetReplies(GetRepliesRequest request, ServerCallContext context)
        {
            try
            {
                // Verify parent comment exists
                var parentComment = await _commentRepository.GetByIdAsync(request.CommentId);
                if (parentComment == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Parent comment not found"));
                }

                var paginationParams = new PaginationParameters
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                // Get replies for the comment
                var result = await _commentRepository.GetPagedAsync(
                    paginationParams, 
                    c => c.ParentCommentId == request.CommentId
                );
                
                var response = new GetCommentsResponse
                {
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasPreviousPage = result.HasPreviousPage,
                    HasNextPage = result.HasNextPage
                };
                
                foreach (var comment in result.Items)
                {
                    // Get author information
                    var author = await _userRepository.GetByIdAsync(comment.AuthorId);
                    
                    response.Comments.Add(await MapToCommentModelAsync(comment, author));
                }
                
                return response;
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting replies for comment {CommentId}", request.CommentId);
                throw new RpcException(new Status(StatusCode.Internal, "Error retrieving replies"));
            }
        }

        public override async Task<CommentResponse> AddComment(AddCommentRequest request, ServerCallContext context)
        {
            try
            {
                // Get user ID from auth context
                var userId = context.GetHttpContext().User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "User not authenticated"));
                }
                
                // Verify post exists
                var post = await _postRepository.GetByIdAsync(request.PostId);
                if (post == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Post not found"));
                }
                
                // Create comment
                var comment = new Comment
                {
                    PostId = request.PostId,
                    AuthorId = userId,
                    Content = request.Content,
                    CreatedAt = DateTime.UtcNow
                };
                
                var createdComment = await _commentRepository.AddAsync(comment);
                
                // Update comment count on post
                post.CommentCount++;
                await _postRepository.UpdateAsync(post);
                
                // Get author information
                var author = await _userRepository.GetByIdAsync(userId);
                
                return new CommentResponse
                {
                    Comment = await MapToCommentModelAsync(createdComment, author)
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to post {PostId}", request.PostId);
                throw new RpcException(new Status(StatusCode.Internal, "Error adding comment"));
            }
        }

        public override async Task<CommentResponse> AddReply(AddReplyRequest request, ServerCallContext context)
        {
            try
            {
                // Get user ID from auth context
                var userId = context.GetHttpContext().User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "User not authenticated"));
                }
                
                // Verify parent comment exists
                var parentComment = await _commentRepository.GetByIdAsync(request.ParentCommentId);
                if (parentComment == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Parent comment not found"));
                }
                
                // Create reply
                var reply = new Comment
                {
                    PostId = parentComment.PostId,
                    AuthorId = userId,
                    Content = request.Content,
                    ParentCommentId = request.ParentCommentId,
                    CreatedAt = DateTime.UtcNow
                };
                
                var createdReply = await _commentRepository.AddAsync(reply);
                
                // Get author information
                var author = await _userRepository.GetByIdAsync(userId);
                
                return new CommentResponse
                {
                    Comment = await MapToCommentModelAsync(createdReply, author)
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reply to comment {CommentId}", request.ParentCommentId);
                throw new RpcException(new Status(StatusCode.Internal, "Error adding reply"));
            }
        }

        public override async Task<CommentResponse> UpdateComment(UpdateCommentRequest request, ServerCallContext context)
        {
            try
            {
                // Get user ID from auth context
                var userId = context.GetHttpContext().User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "User not authenticated"));
                }
                
                // Get existing comment
                var comment = await _commentRepository.GetByIdAsync(request.Id);
                if (comment == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Comment with ID {request.Id} not found"));
                }
                
                // Check if user is author or admin/moderator
                if (comment.AuthorId != userId && 
                    !context.GetHttpContext().User.IsInRole("Admin") && 
                    !context.GetHttpContext().User.IsInRole("Moderator"))
                {
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "You don't have permission to update this comment"));
                }
                
                // Update comment
                comment.Content = request.Content;
                comment.UpdatedAt = DateTime.UtcNow;
                
                await _commentRepository.UpdateAsync(comment);
                
                // Get author information
                var author = await _userRepository.GetByIdAsync(comment.AuthorId);
                
                return new CommentResponse
                {
                    Comment = await MapToCommentModelAsync(comment, author)
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Error updating comment"));
            }
        }

        public override async Task<DeleteCommentResponse> DeleteComment(DeleteCommentRequest request, ServerCallContext context)
        {
            try
            {
                // Get user ID from auth context
                var userId = context.GetHttpContext().User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "User not authenticated"));
                }
                
                // Get existing comment
                var comment = await _commentRepository.GetByIdAsync(request.Id);
                if (comment == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Comment with ID {request.Id} not found"));
                }
                
                // Check if user is author or admin/moderator
                if (comment.AuthorId != userId && 
                    !context.GetHttpContext().User.IsInRole("Admin") && 
                    !context.GetHttpContext().User.IsInRole("Moderator"))
                {
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "You don't have permission to delete this comment"));
                }
                
                // Delete comment
                var success = await _commentRepository.DeleteAsync(request.Id);
                
                // Update comment count on post if this is a root comment
                if (success && comment.ParentCommentId == null)
                {
                    var post = await _postRepository.GetByIdAsync(comment.PostId);
                    if (post != null)
                    {
                        post.CommentCount = Math.Max(0, post.CommentCount - 1);
                        await _postRepository.UpdateAsync(post);
                    }
                }
                
                return new DeleteCommentResponse
                {
                    Success = success
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Error deleting comment"));
            }
        }

        public override async Task<LikeCommentResponse> LikeComment(LikeCommentRequest request, ServerCallContext context)
        {
            try
            {
                // Get user ID from auth context
                var userId = context.GetHttpContext().User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "User not authenticated"));
                }
                
                // Get existing comment
                var comment = await _commentRepository.GetByIdAsync(request.CommentId);
                if (comment == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Comment with ID {request.CommentId} not found"));
                }
                
                // In a real implementation, we would check if the user has already liked the comment
                // and either add or remove the like. For simplicity, we'll just increment the like count.
                comment.LikeCount++;
                await _commentRepository.UpdateAsync(comment);
                
                return new LikeCommentResponse
                {
                    Success = true,
                    LikeCount = comment.LikeCount
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking comment {CommentId}", request.CommentId);
                throw new RpcException(new Status(StatusCode.Internal, "Error liking comment"));
            }
        }

        // Helper method to map domain Comment entity to gRPC CommentModel
        private async Task<CommentModel> MapToCommentModelAsync(Comment comment, User? author = null)
        {
            // If author is not provided, try to get it
            if (author == null)
            {
                author = await _userRepository.GetByIdAsync(comment.AuthorId);
            }
            
            var model = new CommentModel
            {
                Id = comment.Id,
                PostId = comment.PostId,
                AuthorId = comment.AuthorId,
                AuthorName = author?.Name ?? "Unknown User",
                Content = comment.Content,
                CreatedAt = comment.CreatedAt.ToString("o"),
                LikeCount = comment.LikeCount
            };
            
            // Add optional fields if they exist
            if (author?.ProfilePicture != null)
            {
                model.AuthorProfilePicUrl = author.ProfilePicture;
            }
            
            if (comment.UpdatedAt.HasValue)
            {
                model.UpdatedAt = comment.UpdatedAt.Value.ToString("o");
            }
            
            if (comment.ParentCommentId != null)
            {
                model.ParentCommentId = comment.ParentCommentId;
            }
            
            // Get child comment count if this is a parent comment
            if (comment.ParentCommentId == null)
            {
                var replies = await _commentRepository.FindAsync(c => c.ParentCommentId == comment.Id);
                model.ChildCommentCount = replies.Count();
            }
            
            return model;
        }
    }
}