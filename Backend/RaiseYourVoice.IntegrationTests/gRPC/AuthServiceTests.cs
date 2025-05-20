using System.Text;
using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using RaiseYourVoice.Api.Protos;

namespace RaiseYourVoice.IntegrationTests.gRPC;

public class AuthServiceTests : TestBase, IAsyncLifetime
{
    private GrpcChannel? _channel;
    private AuthService.AuthServiceClient? _client;
    
    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        // Create a gRPC channel
        _channel = GrpcChannel.ForAddress(_factory.Server.BaseAddress, new GrpcChannelOptions
        {
            HttpClient = _factory.CreateClient()
        });
        
        _client = new AuthService.AuthServiceClient(_channel);
    }
    
    public new async Task DisposeAsync()
    {
        await _channel!.ShutdownAsync();
        await base.DisposeAsync();
    }
    
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "admin@test.com",
            Password = "Admin123!"
        };
        
        // Act
        var response = await _client!.LoginAsync(request);
        
        // Assert
        response.Should().NotBeNull();
        response.UserId.Should().Be("user1");
        response.Name.Should().Be("Admin User");
        response.Email.Should().Be(request.Email);
        response.Role.Should().Be("Admin");
        response.Token.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Login_WithInvalidCredentials_ThrowsRpcException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "admin@test.com",
            Password = "WrongPassword"
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(async () => 
            await _client!.LoginAsync(request));
        
        exception.Status.StatusCode.Should().Be(StatusCode.Unauthenticated);
    }
    
    [Fact]
    public async Task Register_WithValidData_ReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "New gRPC User",
            Email = "grpc-user@test.com",
            Password = "Password123!",
            PreferredLanguage = "en"
        };
        
        // Act
        var response = await _client!.RegisterAsync(request);
        
        // Assert
        response.Should().NotBeNull();
        response.UserId.Should().NotBeNullOrEmpty();
        response.Name.Should().Be(request.Name);
        response.Email.Should().Be(request.Email);
        response.Role.Should().Be("Activist");
        response.Token.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Register_WithExistingEmail_ThrowsRpcException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "Duplicate User",
            Email = "admin@test.com", // This email already exists
            Password = "Password123!",
            PreferredLanguage = "en"
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(async () => 
            await _client!.RegisterAsync(request));
        
        exception.Status.StatusCode.Should().Be(StatusCode.AlreadyExists);
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
        
        var loginResponse = await _client!.LoginAsync(loginRequest);
        
        var refreshRequest = new RefreshTokenRequest
        {
            Token = loginResponse.Token,
            RefreshToken = loginResponse.RefreshToken
        };
        
        // Act
        var response = await _client!.RefreshTokenAsync(refreshRequest);
        
        // Assert
        response.Should().NotBeNull();
        response.UserId.Should().Be(loginResponse.UserId);
        response.Token.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();
        
        // The new token should be different from the old token
        response.Token.Should().NotBe(loginResponse.Token);
        response.RefreshToken.Should().NotBe(loginResponse.RefreshToken);
    }
    
    [Fact]
    public async Task Logout_WithValidToken_ReturnsSuccess()
    {
        // Arrange - First login to get a token and refresh token
        var loginRequest = new LoginRequest
        {
            Email = "admin@test.com",
            Password = "Admin123!"
        };
        
        var loginResponse = await _client!.LoginAsync(loginRequest);
        
        var logoutRequest = new LogoutRequest
        {
            RefreshToken = loginResponse.RefreshToken
        };
        
        // Create headers with the token
        var headers = new Metadata
        {
            { "Authorization", $"Bearer {loginResponse.Token}" }
        };
        
        // Act
        var response = await _client!.LogoutAsync(logoutRequest, headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
    }
    
    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsUserResponse()
    {
        // Arrange - First login to get a token
        var loginRequest = new LoginRequest
        {
            Email = "admin@test.com",
            Password = "Admin123!"
        };
        
        var loginResponse = await _client!.LoginAsync(loginRequest);
        
        // Create headers with the token
        var headers = new Metadata
        {
            { "Authorization", $"Bearer {loginResponse.Token}" }
        };
        
        var request = new GetCurrentUserRequest();
        
        // Act
        var response = await _client!.GetCurrentUserAsync(request, headers);
        
        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be("user1");
        response.Name.Should().Be("Admin User");
        response.Email.Should().Be("admin@test.com");
        response.Role.Should().Be("Admin");
    }
    
    [Fact]
    public async Task GetCurrentUser_WithoutToken_ThrowsRpcException()
    {
        // Arrange
        var request = new GetCurrentUserRequest();
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(async () => 
            await _client!.GetCurrentUserAsync(request));
        
        exception.Status.StatusCode.Should().Be(StatusCode.Unauthenticated);
    }
}