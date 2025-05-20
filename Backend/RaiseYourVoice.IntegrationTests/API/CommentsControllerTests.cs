using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.IntegrationTests.API;

public class CommentsControllerTests : TestBase
{
    [Fact]
    public async Task GetCommentsByPost_WithValidPostId_ReturnsOkResult()
    {
        // Arrange
        var postId = "post1";
        
        // Act
        var response = await _client.GetAsync($"/api/comments/post/{postId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var comments = JsonSerializer.Deserialize<List<Comment>>(responseString, _jsonOptions);
        
        comments.Should().NotBeNull();
        comments!.Should().NotBeEmpty();
        comments.Should().OnlyContain(c => c.PostId == postId);
    }
    
    [Fact]
    public async Task GetComment_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var commentId = "comment1";
        
        // Act
        var response = await _client.GetAsync($"/api/comments/{commentId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var comment = JsonSerializer.Deserialize<Comment>(responseString, _jsonOptions);
        
        comment.Should().NotBeNull();
        comment!.Id.Should().Be(commentId);
        comment.Content.Should().Be("This is a test comment");
    }
    
    [Fact]
    public async Task GetComment_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var commentId = "nonexistent";
        
        // Act
        var response = await _client.GetAsync($"/api/comments/{commentId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task CreateComment_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var comment = new Comment
        {
            PostId = "post1",
            Content = "This is a new test comment created during integration tests"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(comment, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PostAsync("/api/comments", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var createdComment = JsonSerializer.Deserialize<Comment>(responseString, _jsonOptions);
        
        createdComment.Should().NotBeNull();
        createdComment!.Id.Should().NotBeNullOrEmpty();
        createdComment.PostId.Should().Be(comment.PostId);
        createdComment.Content.Should().Be(comment.Content);
        createdComment.AuthorId.Should().Be("user2"); // The ID of the user@test.com user
    }
    
    [Fact]
    public async Task CreateComment_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var comment = new Comment
        {
            PostId = "post1",
            Content = "This is a new test comment created during integration tests"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(comment, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/comments", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task UpdateComment_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var commentId = "comment1";
        
        // First, get the existing comment
        var getResponse = await client.GetAsync($"/api/comments/{commentId}");
        getResponse.EnsureSuccessStatusCode();
        
        var getResponseString = await getResponse.Content.ReadAsStringAsync();
        var existingComment = JsonSerializer.Deserialize<Comment>(getResponseString, _jsonOptions);
        
        // Update the comment
        existingComment!.Content = "This comment has been updated during integration tests";
        
        var content = new StringContent(
            JsonSerializer.Serialize(existingComment, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PutAsync($"/api/comments/{commentId}", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the comment was updated
        var verifyResponse = await client.GetAsync($"/api/comments/{commentId}");
        verifyResponse.EnsureSuccessStatusCode();
        
        var verifyResponseString = await verifyResponse.Content.ReadAsStringAsync();
        var updatedComment = JsonSerializer.Deserialize<Comment>(verifyResponseString, _jsonOptions);
        
        updatedComment.Should().NotBeNull();
        updatedComment!.Id.Should().Be(commentId);
        updatedComment.Content.Should().Be("This comment has been updated during integration tests");
    }
    
    [Fact]
    public async Task DeleteComment_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var commentId = "comment1";
        
        // Act
        var response = await client.DeleteAsync($"/api/comments/{commentId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the comment was deleted
        var verifyResponse = await client.GetAsync($"/api/comments/{commentId}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}