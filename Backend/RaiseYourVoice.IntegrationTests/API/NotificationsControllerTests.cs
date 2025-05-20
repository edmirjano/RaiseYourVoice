using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.IntegrationTests.API;

public class NotificationsControllerTests : TestBase
{
    [Fact]
    public async Task GetUserNotifications_WithAuthentication_ReturnsOkResult()
    {
        // Arrange
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        // First, create a test notification
        await CreateTestNotification(client);
        
        // Act
        var response = await client.GetAsync("/api/notifications");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var notifications = JsonSerializer.Deserialize<List<Notification>>(responseString, _jsonOptions);
        
        notifications.Should().NotBeNull();
        notifications!.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task GetUserNotifications_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        
        // Act
        var response = await _client.GetAsync("/api/notifications");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task CreateNotification_AsAdmin_ReturnsCreatedResult()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var notification = new Notification
        {
            Title = "Test Notification",
            Content = "This is a test notification created during integration tests",
            Type = NotificationType.SystemAnnouncement,
            TargetAudience = new TargetAudience
            {
                Type = TargetType.AllUsers
            }
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(notification, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PostAsync("/api/notifications", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var createdNotification = JsonSerializer.Deserialize<Notification>(responseString, _jsonOptions);
        
        createdNotification.Should().NotBeNull();
        createdNotification!.Id.Should().NotBeNullOrEmpty();
        createdNotification.Title.Should().Be(notification.Title);
        createdNotification.Content.Should().Be(notification.Content);
        createdNotification.Type.Should().Be(notification.Type);
        createdNotification.SentBy.Should().Be("user1"); // The ID of the admin@test.com user
    }
    
    [Fact]
    public async Task CreateNotification_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var notification = new Notification
        {
            Title = "Test Notification",
            Content = "This is a test notification created during integration tests",
            Type = NotificationType.SystemAnnouncement,
            TargetAudience = new TargetAudience
            {
                Type = TargetType.AllUsers
            }
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(notification, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PostAsync("/api/notifications", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task MarkAsRead_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        // First, create a test notification
        var notification = await CreateTestNotification(client);
        
        // Act
        var response = await client.PutAsync($"/api/notifications/{notification.Id}/read", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    public async Task DismissNotification_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        // First, create a test notification
        var notification = await CreateTestNotification(client);
        
        // Act
        var response = await client.PutAsync($"/api/notifications/{notification.Id}/dismiss", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    public async Task BroadcastNotification_AsAdmin_ReturnsCreatedResult()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var notification = new Notification
        {
            Title = "Broadcast Test Notification",
            Content = "This is a broadcast test notification created during integration tests",
            Type = NotificationType.SystemAnnouncement,
            TargetAudience = new TargetAudience
            {
                Type = TargetType.AllUsers
            }
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(notification, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PostAsync("/api/notifications/broadcast", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var createdNotification = JsonSerializer.Deserialize<Notification>(responseString, _jsonOptions);
        
        createdNotification.Should().NotBeNull();
        createdNotification!.Id.Should().NotBeNullOrEmpty();
        createdNotification.Title.Should().Be(notification.Title);
        createdNotification.Content.Should().Be(notification.Content);
        createdNotification.Type.Should().Be(notification.Type);
        createdNotification.SentBy.Should().Be("user1"); // The ID of the admin@test.com user
    }
    
    private async Task<Notification> CreateTestNotification(HttpClient client)
    {
        // Get admin token for creating a notification
        var adminToken = await GetAuthTokenAsync("admin@test.com", "Admin123!");
        var adminClient = GetAuthenticatedClient(adminToken);
        
        var notification = new Notification
        {
            Title = "Test Notification for User",
            Content = "This is a test notification created during integration tests",
            Type = NotificationType.SystemAnnouncement,
            TargetAudience = new TargetAudience
            {
                Type = TargetType.SpecificUsers,
                UserIds = new[] { "user2" } // The ID of the user@test.com user
            }
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(notification, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        var response = await adminClient.PostAsync("/api/notifications", content);
        response.EnsureSuccessStatusCode();
        
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Notification>(responseString, _jsonOptions)!;
    }
}