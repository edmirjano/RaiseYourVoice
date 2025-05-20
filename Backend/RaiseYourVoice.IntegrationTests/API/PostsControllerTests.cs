using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.IntegrationTests.API;

public class PostsControllerTests : TestBase
{
    [Fact]
    public async Task GetPosts_ReturnsOkResult()
    {
        // Arrange
        
        // Act
        var response = await _client.GetAsync("/api/posts");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var posts = JsonSerializer.Deserialize<List<Post>>(responseString, _jsonOptions);
        
        posts.Should().NotBeNull();
        posts!.Should().NotBeEmpty();
        posts.Should().Contain(p => p.Id == "post1");
    }
    
    [Fact]
    public async Task GetPostById_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var postId = "post1";
        
        // Act
        var response = await _client.GetAsync($"/api/posts/{postId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var post = JsonSerializer.Deserialize<Post>(responseString, _jsonOptions);
        
        post.Should().NotBeNull();
        post!.Id.Should().Be(postId);
        post.Title.Should().Be("Test Post");
    }
    
    [Fact]
    public async Task GetPostById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var postId = "nonexistent";
        
        // Act
        var response = await _client.GetAsync($"/api/posts/{postId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task CreatePost_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var post = new Post
        {
            Title = "New Test Post",
            Content = "This is a new test post created during integration tests",
            PostType = PostType.Activism,
            Status = PostStatus.Published
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(post, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PostAsync("/api/posts", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var createdPost = JsonSerializer.Deserialize<Post>(responseString, _jsonOptions);
        
        createdPost.Should().NotBeNull();
        createdPost!.Id.Should().NotBeNullOrEmpty();
        createdPost.Title.Should().Be(post.Title);
        createdPost.Content.Should().Be(post.Content);
        createdPost.PostType.Should().Be(post.PostType);
        createdPost.Status.Should().Be(post.Status);
        createdPost.AuthorId.Should().Be("user2"); // The ID of the user@test.com user
    }
    
    [Fact]
    public async Task CreatePost_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var post = new Post
        {
            Title = "New Test Post",
            Content = "This is a new test post created during integration tests",
            PostType = PostType.Activism,
            Status = PostStatus.Published
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(post, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/posts", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task UpdatePost_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var postId = "post1";
        
        // First, get the existing post
        var getResponse = await client.GetAsync($"/api/posts/{postId}");
        getResponse.EnsureSuccessStatusCode();
        
        var getResponseString = await getResponse.Content.ReadAsStringAsync();
        var existingPost = JsonSerializer.Deserialize<Post>(getResponseString, _jsonOptions);
        
        // Update the post
        existingPost!.Title = "Updated Test Post";
        existingPost.Content = "This post has been updated during integration tests";
        
        var content = new StringContent(
            JsonSerializer.Serialize(existingPost, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PutAsync($"/api/posts/{postId}", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the post was updated
        var verifyResponse = await client.GetAsync($"/api/posts/{postId}");
        verifyResponse.EnsureSuccessStatusCode();
        
        var verifyResponseString = await verifyResponse.Content.ReadAsStringAsync();
        var updatedPost = JsonSerializer.Deserialize<Post>(verifyResponseString, _jsonOptions);
        
        updatedPost.Should().NotBeNull();
        updatedPost!.Id.Should().Be(postId);
        updatedPost.Title.Should().Be("Updated Test Post");
        updatedPost.Content.Should().Be("This post has been updated during integration tests");
    }
    
    [Fact]
    public async Task DeletePost_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var postId = "post1";
        
        // Act
        var response = await client.DeleteAsync($"/api/posts/{postId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the post was deleted
        var verifyResponse = await client.GetAsync($"/api/posts/{postId}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task GetPostsByType_ReturnsOkResult()
    {
        // Arrange
        var postType = "Activism";
        
        // Act
        var response = await _client.GetAsync($"/api/posts/type/{postType}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var posts = JsonSerializer.Deserialize<List<Post>>(responseString, _jsonOptions);
        
        posts.Should().NotBeNull();
        posts!.Should().NotBeEmpty();
        posts.Should().OnlyContain(p => p.PostType == PostType.Activism);
    }
}