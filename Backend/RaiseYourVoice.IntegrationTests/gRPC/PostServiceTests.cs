using System.Text;
using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using RaiseYourVoice.Api.Protos;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.IntegrationTests.gRPC;

public class PostServiceTests : TestBase, IAsyncLifetime
{
    private GrpcChannel? _channel;
    private PostService.PostServiceClient? _client;
    private Metadata? _headers;
    
    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        // Create a gRPC channel
        _channel = GrpcChannel.ForAddress(_factory.Server.BaseAddress, new GrpcChannelOptions
        {
            HttpClient = _factory.CreateClient()
        });
        
        _client = new PostService.PostServiceClient(_channel);
        
        // Get an auth token for the headers
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        _headers = new Metadata
        {
            { "Authorization", $"Bearer {token}" }
        };
    }
    
    public new async Task DisposeAsync()
    {
        await _channel!.ShutdownAsync();
        await base.DisposeAsync();
    }
    
    [Fact]
    public async Task GetPosts_ReturnsPostsList()
    {
        // Arrange
        var request = new GetPostsRequest
        {
            PageNumber = 1,
            PageSize = 10
        };
        
        // Act
        var response = await _client!.GetPostsAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Posts.Should().NotBeEmpty();
        response.TotalItems.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public async Task GetPost_WithValidId_ReturnsPost()
    {
        // Arrange
        var request = new GetPostRequest
        {
            Id = "post1"
        };
        
        // Act
        var response = await _client!.GetPostAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Post.Should().NotBeNull();
        response.Post.Id.Should().Be("post1");
        response.Post.Title.Should().Be("Test Post");
    }
    
    [Fact]
    public async Task CreatePost_WithValidData_ReturnsCreatedPost()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            Title = "New gRPC Test Post",
            Content = "This is a test post created via gRPC",
            PostType = PostType.Activism.ToString()
        };
        
        // Act
        var response = await _client!.CreatePostAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Post.Should().NotBeNull();
        response.Post.Id.Should().NotBeNullOrEmpty();
        response.Post.Title.Should().Be(request.Title);
        response.Post.Content.Should().Be(request.Content);
        response.Post.PostType.Should().Be(request.PostType);
        response.Post.AuthorId.Should().Be("user2"); // The ID of the user@test.com user
    }
    
    [Fact]
    public async Task UpdatePost_WithValidData_ReturnsUpdatedPost()
    {
        // Arrange
        var postId = "post1";
        
        var request = new UpdatePostRequest
        {
            Id = postId,
            Title = "Updated gRPC Test Post",
            Content = "This post has been updated via gRPC",
            PostType = PostType.Activism.ToString()
        };
        
        // Act
        var response = await _client!.UpdatePostAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Post.Should().NotBeNull();
        response.Post.Id.Should().Be(postId);
        response.Post.Title.Should().Be(request.Title);
        response.Post.Content.Should().Be(request.Content);
    }
    
    [Fact]
    public async Task DeletePost_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var postId = "post1";
        
        var request = new DeletePostRequest
        {
            Id = postId
        };
        
        // Act
        var response = await _client!.DeletePostAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        
        // Verify the post was deleted by trying to get it
        var getRequest = new GetPostRequest
        {
            Id = postId
        };
        
        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(async () => 
            await _client!.GetPostAsync(getRequest, _headers));
    }
    
    [Fact]
    public async Task GetPostsByType_ReturnsFilteredPosts()
    {
        // Arrange
        var request = new GetPostsByTypeRequest
        {
            PostType = PostType.Activism.ToString(),
            PageNumber = 1,
            PageSize = 10
        };
        
        // Act
        var response = await _client!.GetPostsByTypeAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Posts.Should().NotBeEmpty();
        response.Posts.Should().OnlyContain(p => p.PostType == PostType.Activism.ToString());
    }
    
    [Fact]
    public async Task SearchPosts_WithValidQuery_ReturnsMatchingPosts()
    {
        // Arrange
        var request = new SearchPostsRequest
        {
            Query = "test",
            PageNumber = 1,
            PageSize = 10
        };
        
        // Act
        var response = await _client!.SearchPostsAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Posts.Should().NotBeEmpty();
        response.Posts.Should().Contain(p => p.Title.Contains("Test", StringComparison.OrdinalIgnoreCase) || 
                                           p.Content.Contains("test", StringComparison.OrdinalIgnoreCase));
    }
}