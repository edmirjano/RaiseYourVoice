using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RaiseYourVoice.Application.Models.Requests;
using RaiseYourVoice.Application.Models.Responses;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.IntegrationTests.API;

public class AuthControllerTests : TestBase
{
    [Fact]
    public async Task Register_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Name = "New Test User",
            Email = "newuser@test.com",
            Password = "Password123!",
            PreferredLanguage = "en"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(registerRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/auth/register", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseString, _jsonOptions);
        
        authResponse.Should().NotBeNull();
        authResponse!.UserId.Should().NotBeNullOrEmpty();
        authResponse.Name.Should().Be(registerRequest.Name);
        authResponse.Email.Should().Be(registerRequest.Email);
        authResponse.Role.Should().Be(UserRole.Activist);
        authResponse.Token.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Name = "Duplicate User",
            Email = "admin@test.com", // This email already exists in the test database
            Password = "Password123!",
            PreferredLanguage = "en"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(registerRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/auth/register", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkResult()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@test.com",
            Password = "Admin123!"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/auth/login", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseString, _jsonOptions);
        
        authResponse.Should().NotBeNull();
        authResponse!.UserId.Should().Be("user1");
        authResponse.Name.Should().Be("Admin User");
        authResponse.Email.Should().Be(loginRequest.Email);
        authResponse.Role.Should().Be(UserRole.Admin);
        authResponse.Token.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@test.com",
            Password = "WrongPassword"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/auth/login", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewToken()
    {
        // Arrange - First login to get a token and refresh token
        var loginRequest = new LoginRequest
        {
            Email = "admin@test.com",
            Password = "Admin123!"
        };
        
        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
        loginResponse.EnsureSuccessStatusCode();
        
        var loginResponseString = await loginResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(loginResponseString, _jsonOptions);
        
        // Now use the token and refresh token to get a new token
        var refreshRequest = new RefreshTokenRequest
        {
            Token = authResponse!.Token,
            RefreshToken = authResponse.RefreshToken
        };
        
        var refreshContent = new StringContent(
            JsonSerializer.Serialize(refreshRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var refreshResponse = await _client.PostAsync("/api/auth/refresh-token", refreshContent);
        
        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var refreshResponseString = await refreshResponse.Content.ReadAsStringAsync();
        var newAuthResponse = JsonSerializer.Deserialize<AuthResponse>(refreshResponseString, _jsonOptions);
        
        newAuthResponse.Should().NotBeNull();
        newAuthResponse!.UserId.Should().Be("user1");
        newAuthResponse.Token.Should().NotBeNullOrEmpty();
        newAuthResponse.RefreshToken.Should().NotBeNullOrEmpty();
        
        // The new token should be different from the old token
        newAuthResponse.Token.Should().NotBe(authResponse.Token);
        newAuthResponse.RefreshToken.Should().NotBe(authResponse.RefreshToken);
    }
    
    [Fact]
    public async Task Logout_WithValidToken_ReturnsOk()
    {
        // Arrange - First login to get a token and refresh token
        var loginRequest = new LoginRequest
        {
            Email = "admin@test.com",
            Password = "Admin123!"
        };
        
        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
        loginResponse.EnsureSuccessStatusCode();
        
        var loginResponseString = await loginResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(loginResponseString, _jsonOptions);
        
        // Now logout
        var logoutRequest = new LogoutRequest
        {
            RefreshToken = authResponse!.RefreshToken
        };
        
        var logoutContent = new StringContent(
            JsonSerializer.Serialize(logoutRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        var client = GetAuthenticatedClient(authResponse.Token);
        
        // Act
        var logoutResponse = await client.PostAsync("/api/auth/logout", logoutContent);
        
        // Assert
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}