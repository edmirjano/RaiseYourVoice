using System.Text;
using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using RaiseYourVoice.Api.Protos;

namespace RaiseYourVoice.IntegrationTests.gRPC;

public class CommentServiceTests : TestBase, IAsyncLifetime
{
    private GrpcChannel? _channel;
    private CommentService.CommentServiceClient? _client;
    private Metadata? _headers;
    
    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        // Create a gRPC channel
        _channel = GrpcChannel.ForAddress(_factory.Server.BaseAddress, new GrpcChannelOptions
        {
            HttpClient = _factory.CreateClient()
        });
        
        _client = new CommentService.CommentServiceClient(_channel);
        
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
    public async Task GetCommentsByPost_WithValidPostId_ReturnsCommentsList()
    {
        // Arrange
        var request = new GetCommentsByPostRequest
        {
            PostId = "post1",
            PageNumber = 1,
            PageSize = 10,
            IncludeReplies = true
        };
        
        // Act
        var response = await _client!.GetCommentsByPostAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Comments.Should().NotBeEmpty();
        response.Comments.Should().OnlyContain(c => c.PostId == "post1");
    }
    
    [Fact]
    public async Task GetComment_WithValidId_ReturnsComment()
    {
        // Arrange
        var request = new GetCommentRequest
        {
            Id = "comment1"
        };
        
        // Act
        var response = await _client!.GetCommentAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Comment.Should().NotBeNull();
        response.Comment.Id.Should().Be("comment1");
        response.Comment.Content.Should().Be("This is a test comment");
    }
    
    [Fact]
    public async Task AddComment_WithValidData_ReturnsCreatedComment()
    {
        // Arrange
        var request = new AddCommentRequest
        {
            PostId = "post1",
            Content = "This is a test comment created via gRPC"
        };
        
        // Act
        var response = await _client!.AddCommentAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Comment.Should().NotBeNull();
        response.Comment.Id.Should().NotBeNullOrEmpty();
        response.Comment.PostId.Should().Be(request.PostId);
        response.Comment.Content.Should().Be(request.Content);
        response.Comment.AuthorId.Should().Be("user2"); // The ID of the user@test.com user
    }
    
    [Fact]
    public async Task AddReply_WithValidData_ReturnsCreatedReply()
    {
        // Arrange
        var request = new AddReplyRequest
        {
            ParentCommentId = "comment1",
            Content = "This is a test reply created via gRPC"
        };
        
        // Act
        var response = await _client!.AddReplyAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Comment.Should().NotBeNull();
        response.Comment.Id.Should().NotBeNullOrEmpty();
        response.Comment.ParentCommentId.Should().Be(request.ParentCommentId);
        response.Comment.Content.Should().Be(request.Content);
        response.Comment.AuthorId.Should().Be("user2"); // The ID of the user@test.com user
    }
    
    [Fact]
    public async Task UpdateComment_WithValidData_ReturnsUpdatedComment()
    {
        // Arrange
        var request = new UpdateCommentRequest
        {
            Id = "comment1",
            Content = "This comment has been updated via gRPC"
        };
        
        // Act
        var response = await _client!.UpdateCommentAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Comment.Should().NotBeNull();
        response.Comment.Id.Should().Be(request.Id);
        response.Comment.Content.Should().Be(request.Content);
    }
    
    [Fact]
    public async Task DeleteComment_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var request = new DeleteCommentRequest
        {
            Id = "comment1"
        };
        
        // Act
        var response = await _client!.DeleteCommentAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        
        // Verify the comment was deleted by trying to get it
        var getRequest = new GetCommentRequest
        {
            Id = "comment1"
        };
        
        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(async () => 
            await _client!.GetCommentAsync(getRequest, _headers));
    }
    
    [Fact]
    public async Task LikeComment_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var request = new LikeCommentRequest
        {
            CommentId = "comment1"
        };
        
        // Act
        var response = await _client!.LikeCommentAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.LikeCount.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public async Task GetReplies_WithValidCommentId_ReturnsRepliesList()
    {
        // Arrange
        // First, create a reply to comment1
        var addReplyRequest = new AddReplyRequest
        {
            ParentCommentId = "comment1",
            Content = "This is a test reply for testing GetReplies"
        };
        
        await _client!.AddReplyAsync(addReplyRequest, _headers);
        
        var request = new GetRepliesRequest
        {
            CommentId = "comment1",
            PageNumber = 1,
            PageSize = 10
        };
        
        // Act
        var response = await _client!.GetRepliesAsync(request, _headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Comments.Should().NotBeEmpty();
        response.Comments.Should().OnlyContain(c => c.ParentCommentId == "comment1");
    }
}