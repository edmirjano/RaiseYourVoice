using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.IntegrationTests.API;

public class OrganizationsControllerTests : TestBase
{
    [Fact]
    public async Task GetOrganizations_ReturnsOkResult()
    {
        // Arrange
        
        // Act
        var response = await _client.GetAsync("/api/organizations");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var organizations = JsonSerializer.Deserialize<List<Organization>>(responseString, _jsonOptions);
        
        organizations.Should().NotBeNull();
        organizations!.Should().NotBeEmpty();
        organizations.Should().Contain(o => o.Id == "org1");
    }
    
    [Fact]
    public async Task GetOrganization_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var organizationId = "org1";
        
        // Act
        var response = await _client.GetAsync($"/api/organizations/{organizationId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var organization = JsonSerializer.Deserialize<Organization>(responseString, _jsonOptions);
        
        organization.Should().NotBeNull();
        organization!.Id.Should().Be(organizationId);
        organization.Name.Should().Be("Test Organization");
    }
    
    [Fact]
    public async Task GetOrganization_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var organizationId = "nonexistent";
        
        // Act
        var response = await _client.GetAsync($"/api/organizations/{organizationId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task GetVerifiedOrganizations_ReturnsOkResult()
    {
        // Arrange
        
        // Act
        var response = await _client.GetAsync("/api/organizations/verified");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var organizations = JsonSerializer.Deserialize<List<Organization>>(responseString, _jsonOptions);
        
        organizations.Should().NotBeNull();
        organizations!.Should().NotBeEmpty();
        organizations.Should().OnlyContain(o => o.VerificationStatus == VerificationStatus.Verified);
    }
    
    [Fact]
    public async Task CreateOrganization_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var organization = new Organization
        {
            Name = "New Test Organization",
            Description = "A new test organization created during integration tests",
            OrganizationType = OrganizationType.NGO,
            FoundingDate = DateTime.UtcNow.AddYears(-1)
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(organization, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PostAsync("/api/organizations", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var createdOrganization = JsonSerializer.Deserialize<Organization>(responseString, _jsonOptions);
        
        createdOrganization.Should().NotBeNull();
        createdOrganization!.Id.Should().NotBeNullOrEmpty();
        createdOrganization.Name.Should().Be(organization.Name);
        createdOrganization.Description.Should().Be(organization.Description);
        createdOrganization.OrganizationType.Should().Be(organization.OrganizationType);
        createdOrganization.VerificationStatus.Should().Be(VerificationStatus.Pending);
    }
    
    [Fact]
    public async Task CreateOrganization_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var organization = new Organization
        {
            Name = "New Test Organization",
            Description = "A new test organization created during integration tests",
            OrganizationType = OrganizationType.NGO,
            FoundingDate = DateTime.UtcNow.AddYears(-1)
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(organization, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/organizations", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task UpdateOrganization_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var organizationId = "org1";
        
        // First, get the existing organization
        var getResponse = await client.GetAsync($"/api/organizations/{organizationId}");
        getResponse.EnsureSuccessStatusCode();
        
        var getResponseString = await getResponse.Content.ReadAsStringAsync();
        var existingOrganization = JsonSerializer.Deserialize<Organization>(getResponseString, _jsonOptions);
        
        // Update the organization
        existingOrganization!.Name = "Updated Test Organization";
        existingOrganization.Description = "This organization has been updated during integration tests";
        
        var content = new StringContent(
            JsonSerializer.Serialize(existingOrganization, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PutAsync($"/api/organizations/{organizationId}", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the organization was updated
        var verifyResponse = await client.GetAsync($"/api/organizations/{organizationId}");
        verifyResponse.EnsureSuccessStatusCode();
        
        var verifyResponseString = await verifyResponse.Content.ReadAsStringAsync();
        var updatedOrganization = JsonSerializer.Deserialize<Organization>(verifyResponseString, _jsonOptions);
        
        updatedOrganization.Should().NotBeNull();
        updatedOrganization!.Id.Should().Be(organizationId);
        updatedOrganization.Name.Should().Be("Updated Test Organization");
        updatedOrganization.Description.Should().Be("This organization has been updated during integration tests");
    }
    
    [Fact]
    public async Task VerifyOrganization_AsAdmin_ReturnsNoContent()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var organizationId = "org1";
        
        // Act
        var response = await client.PostAsync($"/api/organizations/{organizationId}/verify", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the organization was verified
        var verifyResponse = await client.GetAsync($"/api/organizations/{organizationId}");
        verifyResponse.EnsureSuccessStatusCode();
        
        var verifyResponseString = await verifyResponse.Content.ReadAsStringAsync();
        var updatedOrganization = JsonSerializer.Deserialize<Organization>(verifyResponseString, _jsonOptions);
        
        updatedOrganization.Should().NotBeNull();
        updatedOrganization!.VerificationStatus.Should().Be(VerificationStatus.Verified);
        updatedOrganization.VerifiedBy.Should().Be("user1"); // The ID of the admin@test.com user
    }
    
    [Fact]
    public async Task RejectOrganization_AsAdmin_ReturnsNoContent()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var organizationId = "org1";
        
        // Act
        var response = await client.PostAsync($"/api/organizations/{organizationId}/reject", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the organization was rejected
        var verifyResponse = await client.GetAsync($"/api/organizations/{organizationId}");
        verifyResponse.EnsureSuccessStatusCode();
        
        var verifyResponseString = await verifyResponse.Content.ReadAsStringAsync();
        var updatedOrganization = JsonSerializer.Deserialize<Organization>(verifyResponseString, _jsonOptions);
        
        updatedOrganization.Should().NotBeNull();
        updatedOrganization!.VerificationStatus.Should().Be(VerificationStatus.Rejected);
    }
}