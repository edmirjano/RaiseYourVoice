using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RaiseYourVoice.Api.Protos;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Application.Models.Pagination;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Api.gRPC
{
    public class PostServiceImpl : PostService.PostServiceBase
    {
        private readonly ILogger<PostServiceImpl> _logger;
        private readonly IGenericRepository<Post> _postRepository;
        
        public PostServiceImpl(
            ILogger<PostServiceImpl> logger,
            IGenericRepository<Post> postRepository)
        {
            _logger = logger;
            _postRepository = postRepository;
        }

        public override async Task<GetPostsResponse> GetPosts(GetPostsRequest request, ServerCallContext context)
        {
            try
            {
                var paginationParams = new PaginationParameters
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    SortBy = request.SortBy,
                    Ascending = request.Ascending
                };

                var result = await _postRepository.GetPagedAsync(paginationParams);
                
                var response = new GetPostsResponse
                {
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasPreviousPage = result.HasPreviousPage,
                    HasNextPage = result.HasNextPage
                };
                
                foreach (var post in result.Items)
                {
                    response.Posts.Add(MapToPostModel(post));
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts");
                throw new RpcException(new Status(StatusCode.Internal, "Error retrieving posts"));
            }
        }

        public override async Task<PostResponse> GetPost(GetPostRequest request, ServerCallContext context)
        {
            try
            {
                var post = await _postRepository.GetByIdAsync(request.Id);
                
                if (post == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Post with ID {request.Id} not found"));
                }
                
                return new PostResponse
                {
                    Post = MapToPostModel(post)
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post {PostId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Error retrieving post"));
            }
        }

        public override async Task<PostResponse> CreatePost(CreatePostRequest request, ServerCallContext context)
        {
            try
            {
                // Get user ID from auth context
                var userId = context.GetHttpContext().User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "User not authenticated"));
                }
                
                // Parse post type
                if (!Enum.TryParse<PostType>(request.PostType, true, out var postType))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid post type: {request.PostType}"));
                }
                
                // Create post entity
                var post = new Post
                {
                    Title = request.Title,
                    Content = request.Content,
                    MediaUrls = request.MediaUrls.ToList(),
                    PostType = postType,
                    AuthorId = userId,
                    Tags = request.Tags.ToList(),
                    Status = PostStatus.Published,
                    CreatedAt = DateTime.UtcNow
                };
                
                // Add location if provided
                if (request.Location != null)
                {
                    post.Location = new Location
                    {
                        Address = request.Location.Address,
                        City = request.Location.City,
                        Country = request.Location.Country,
                        Latitude = request.Location.Latitude,
                        Longitude = request.Location.Longitude
                    };
                }
                
                // Add event date if provided
                if (!string.IsNullOrEmpty(request.EventDate))
                {
                    if (DateTime.TryParse(request.EventDate, out var eventDate))
                    {
                        post.EventDate = eventDate;
                    }
                }
                
                // Save post
                var createdPost = await _postRepository.AddAsync(post);
                
                return new PostResponse
                {
                    Post = MapToPostModel(createdPost)
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                throw new RpcException(new Status(StatusCode.Internal, "Error creating post"));
            }
        }

        public override async Task<PostResponse> UpdatePost(UpdatePostRequest request, ServerCallContext context)
        {
            try
            {
                // Get user ID from auth context
                var userId = context.GetHttpContext().User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "User not authenticated"));
                }
                
                // Get existing post
                var post = await _postRepository.GetByIdAsync(request.Id);
                if (post == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Post with ID {request.Id} not found"));
                }
                
                // Check if user is author or admin/moderator
                if (post.AuthorId != userId && 
                    !context.GetHttpContext().User.IsInRole("Admin") && 
                    !context.GetHttpContext().User.IsInRole("Moderator"))
                {
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "You don't have permission to update this post"));
                }
                
                // Parse post type
                if (!Enum.TryParse<PostType>(request.PostType, true, out var postType))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid post type: {request.PostType}"));
                }
                
                // Update post
                post.Title = request.Title;
                post.Content = request.Content;
                post.MediaUrls = request.MediaUrls.ToList();
                post.PostType = postType;
                post.Tags = request.Tags.ToList();
                post.UpdatedAt = DateTime.UtcNow;
                
                // Update location if provided
                if (request.Location != null)
                {
                    post.Location = new Location
                    {
                        Address = request.Location.Address,
                        City = request.Location.City,
                        Country = request.Location.Country,
                        Latitude = request.Location.Latitude,
                        Longitude = request.Location.Longitude
                    };
                }
                else
                {
                    post.Location = null;
                }
                
                // Update event date if provided
                if (!string.IsNullOrEmpty(request.EventDate))
                {
                    if (DateTime.TryParse(request.EventDate, out var eventDate))
                    {
                        post.EventDate = eventDate;
                    }
                }
                else
                {
                    post.EventDate = null;
                }
                
                // Save updated post
                await _postRepository.UpdateAsync(post);
                
                return new PostResponse
                {
                    Post = MapToPostModel(post)
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Error updating post"));
            }
        }

        public override async Task<DeletePostResponse> DeletePost(DeletePostRequest request, ServerCallContext context)
        {
            try
            {
                // Get user ID from auth context
                var userId = context.GetHttpContext().User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "User not authenticated"));
                }
                
                // Get existing post
                var post = await _postRepository.GetByIdAsync(request.Id);
                if (post == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Post with ID {request.Id} not found"));
                }
                
                // Check if user is author or admin/moderator
                if (post.AuthorId != userId && 
                    !context.GetHttpContext().User.IsInRole("Admin") && 
                    !context.GetHttpContext().User.IsInRole("Moderator"))
                {
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "You don't have permission to delete this post"));
                }
                
                // Delete post
                var success = await _postRepository.DeleteAsync(request.Id);
                
                return new DeletePostResponse
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
                _logger.LogError(ex, "Error deleting post {PostId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Error deleting post"));
            }
        }

        public override async Task<LikePostResponse> LikePost(LikePostRequest request, ServerCallContext context)
        {
            try
            {
                // Get user ID from auth context
                var userId = context.GetHttpContext().User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "User not authenticated"));
                }
                
                // Get existing post
                var post = await _postRepository.GetByIdAsync(request.PostId);
                if (post == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Post with ID {request.PostId} not found"));
                }
                
                // In a real implementation, we would check if the user has already liked the post
                // and either add or remove the like. For simplicity, we'll just increment the like count.
                post.LikeCount++;
                await _postRepository.UpdateAsync(post);
                
                return new LikePostResponse
                {
                    Success = true,
                    LikeCount = post.LikeCount
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking post {PostId}", request.PostId);
                throw new RpcException(new Status(StatusCode.Internal, "Error liking post"));
            }
        }

        public override async Task<GetPostsResponse> GetPostsByType(GetPostsByTypeRequest request, ServerCallContext context)
        {
            try
            {
                // Parse post type
                if (!Enum.TryParse<PostType>(request.PostType, true, out var postType))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid post type: {request.PostType}"));
                }
                
                var paginationParams = new PaginationParameters
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
                
                // Create filter expression for post type
                var result = await _postRepository.GetPagedAsync(
                    paginationParams, 
                    p => p.PostType == postType && p.Status == PostStatus.Published
                );
                
                var response = new GetPostsResponse
                {
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasPreviousPage = result.HasPreviousPage,
                    HasNextPage = result.HasNextPage
                };
                
                foreach (var post in result.Items)
                {
                    response.Posts.Add(MapToPostModel(post));
                }
                
                return response;
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts by type {PostType}", request.PostType);
                throw new RpcException(new Status(StatusCode.Internal, "Error retrieving posts"));
            }
        }

        public override async Task<GetPostsResponse> GetPostsByUser(GetPostsByUserRequest request, ServerCallContext context)
        {
            try
            {
                var paginationParams = new PaginationParameters
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
                
                // Create filter expression for user ID
                var result = await _postRepository.GetPagedAsync(
                    paginationParams, 
                    p => p.AuthorId == request.UserId
                );
                
                var response = new GetPostsResponse
                {
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasPreviousPage = result.HasPreviousPage,
                    HasNextPage = result.HasNextPage
                };
                
                foreach (var post in result.Items)
                {
                    response.Posts.Add(MapToPostModel(post));
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts by user {UserId}", request.UserId);
                throw new RpcException(new Status(StatusCode.Internal, "Error retrieving posts"));
            }
        }

        public override async Task<GetPostsResponse> SearchPosts(SearchPostsRequest request, ServerCallContext context)
        {
            try
            {
                var paginationParams = new PaginationParameters
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
                
                // In a real implementation, we would use a more sophisticated search mechanism
                // For simplicity, we'll just search in title and content
                var result = await _postRepository.GetPagedAsync(
                    paginationParams, 
                    p => (p.Title.Contains(request.Query) || p.Content.Contains(request.Query)) && 
                         p.Status == PostStatus.Published
                );
                
                var response = new GetPostsResponse
                {
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasPreviousPage = result.HasPreviousPage,
                    HasNextPage = result.HasNextPage
                };
                
                foreach (var post in result.Items)
                {
                    response.Posts.Add(MapToPostModel(post));
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching posts with query {Query}", request.Query);
                throw new RpcException(new Status(StatusCode.Internal, "Error searching posts"));
            }
        }

        // Helper method to map domain Post entity to gRPC PostModel
        private PostModel MapToPostModel(Post post)
        {
            var model = new PostModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                PostType = post.PostType.ToString(),
                AuthorId = post.AuthorId,
                CreatedAt = post.CreatedAt.ToString("o"),
                LikeCount = post.LikeCount,
                CommentCount = post.CommentCount,
                Status = post.Status.ToString()
            };
            
            // Add optional fields if they exist
            if (post.UpdatedAt.HasValue)
            {
                model.UpdatedAt = post.UpdatedAt.Value.ToString("o");
            }
            
            if (post.MediaUrls != null)
            {
                model.MediaUrls.AddRange(post.MediaUrls);
            }
            
            if (post.Tags != null)
            {
                model.Tags.AddRange(post.Tags);
            }
            
            if (post.Location != null)
            {
                model.Location = new LocationModel
                {
                    Address = post.Location.Address,
                    City = post.Location.City,
                    Country = post.Location.Country
                };
                
                if (post.Location.Latitude.HasValue)
                {
                    model.Location.Latitude = post.Location.Latitude.Value;
                }
                
                if (post.Location.Longitude.HasValue)
                {
                    model.Location.Longitude = post.Location.Longitude.Value;
                }
            }
            
            if (post.EventDate.HasValue)
            {
                model.EventDate = post.EventDate.Value.ToString("o");
            }
            
            return model;
        }
    }
}