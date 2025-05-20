using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RaiseYourVoice.Application.Models.Requests;

namespace RaiseYourVoice.IntegrationTests.API;

public class LocalizationsControllerTests : TestBase
{
    [Fact]
    public async Task GetAllTranslations_ReturnsOkResult()
    {
        // Arrange
        // Set the Accept-Language header to English
        _client.DefaultRequestHeaders.Add("Accept-Language", "en");
        
        // Act
        var response = await _client.GetAsync("/api/localizations");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(responseString, _jsonOptions);
        
        translations.Should().NotBeNull();
    }
    
    [Fact]
    public async Task GetTranslationsByCategory_WithValidCategory_ReturnsOkResult()
    {
        // Arrange
        // Set the Accept-Language header to English
        _client.DefaultRequestHeaders.Add("Accept-Language", "en");
        
        // First, create a test translation
        var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var translationRequest = new TranslationRequest
        {
            Key = "test.key",
            Language = "en",
            Value = "Test Value",
            Category = "test"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(translationRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        await client.PostAsync("/api/localizations", content);
        
        // Act
        var response = await _client.GetAsync("/api/localizations/category/test");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(responseString, _jsonOptions);
        
        translations.Should().NotBeNull();
        translations!.Should().ContainKey("test.key");
        translations["test.key"].Should().Be("Test Value");
    }
    
    [Fact]
    public async Task GetTranslation_WithValidKey_ReturnsOkResult()
    {
        // Arrange
        // Set the Accept-Language header to English
        _client.DefaultRequestHeaders.Add("Accept-Language", "en");
        
        // First, create a test translation
        var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var translationRequest = new TranslationRequest
        {
            Key = "test.specific.key",
            Language = "en",
            Value = "Specific Test Value",
            Category = "test"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(translationRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        await client.PostAsync("/api/localizations", content);
        
        // Act
        var response = await _client.GetAsync("/api/localizations/test.specific.key");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var translation = JsonSerializer.Deserialize<Dictionary<string, string>>(responseString, _jsonOptions);
        
        translation.Should().NotBeNull();
        translation!.Should().ContainKey("value");
        translation["value"].Should().Be("Specific Test Value");
    }
    
    [Fact]
    public async Task SetTranslation_AsAdmin_ReturnsOkResult()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var translationRequest = new TranslationRequest
        {
            Key = "test.new.key",
            Language = "en",
            Value = "New Test Value",
            Category = "test",
            Description = "A test translation key"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(translationRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PostAsync("/api/localizations", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify the translation was created
        client.DefaultRequestHeaders.Add("Accept-Language", "en");
        var verifyResponse = await client.GetAsync("/api/localizations/test.new.key");
        verifyResponse.EnsureSuccessStatusCode();
        
        var verifyResponseString = await verifyResponse.Content.ReadAsStringAsync();
        var translation = JsonSerializer.Deserialize<Dictionary<string, string>>(verifyResponseString, _jsonOptions);
        
        translation.Should().NotBeNull();
        translation!.Should().ContainKey("value");
        translation["value"].Should().Be("New Test Value");
    }
    
    [Fact]
    public async Task SetTranslation_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var translationRequest = new TranslationRequest
        {
            Key = "test.forbidden.key",
            Language = "en",
            Value = "Forbidden Test Value",
            Category = "test"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(translationRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PostAsync("/api/localizations", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}