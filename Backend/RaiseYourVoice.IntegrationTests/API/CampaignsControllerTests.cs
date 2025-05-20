using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.IntegrationTests.API;

public class CampaignsControllerTests : TestBase
{
    [Fact]
    public async Task GetAllCampaigns_ReturnsOkResult()
    {
        // Arrange
        
        // Act
        var response = await _client.GetAsync("/api/campaigns");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var campaigns = JsonSerializer.Deserialize<List<Campaign>>(responseString, _jsonOptions);
        
        campaigns.Should().NotBeNull();
        campaigns!.Should().NotBeEmpty();
        campaigns.Should().Contain(c => c.Id == "campaign1");
    }
    
    [Fact]
    public async Task GetCampaignById_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var campaignId = "campaign1";
        
        // Act
        var response = await _client.GetAsync($"/api/campaigns/{campaignId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var campaign = JsonSerializer.Deserialize<Campaign>(responseString, _jsonOptions);
        
        campaign.Should().NotBeNull();
        campaign!.Id.Should().Be(campaignId);
        campaign.Title.Should().Be("Test Campaign");
    }
    
    [Fact]
    public async Task GetCampaignById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var campaignId = "nonexistent";
        
        // Act
        var response = await _client.GetAsync($"/api/campaigns/{campaignId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task CreateCampaign_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var token = await GetAuthTokenAsync("org@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var campaign = new Campaign
        {
            Title = "New Test Campaign",
            Description = "A new test campaign created during integration tests",
            OrganizationId = "org1",
            Goal = 5000,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(30),
            Category = CampaignCategory.Environment
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(campaign, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PostAsync("/api/campaigns", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var createdCampaign = JsonSerializer.Deserialize<Campaign>(responseString, _jsonOptions);
        
        createdCampaign.Should().NotBeNull();
        createdCampaign!.Id.Should().NotBeNullOrEmpty();
        createdCampaign.Title.Should().Be(campaign.Title);
        createdCampaign.Description.Should().Be(campaign.Description);
        createdCampaign.OrganizationId.Should().Be(campaign.OrganizationId);
        createdCampaign.Goal.Should().Be(campaign.Goal);
        createdCampaign.Status.Should().Be(CampaignStatus.PendingApproval);
    }
    
    [Fact]
    public async Task CreateCampaign_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var campaign = new Campaign
        {
            Title = "New Test Campaign",
            Description = "A new test campaign created during integration tests",
            OrganizationId = "org1",
            Goal = 5000,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(30),
            Category = CampaignCategory.Environment
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(campaign, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/campaigns", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task UpdateCampaign_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var token = await GetAuthTokenAsync("org@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var campaignId = "campaign1";
        
        // First, get the existing campaign
        var getResponse = await client.GetAsync($"/api/campaigns/{campaignId}");
        getResponse.EnsureSuccessStatusCode();
        
        var getResponseString = await getResponse.Content.ReadAsStringAsync();
        var existingCampaign = JsonSerializer.Deserialize<Campaign>(getResponseString, _jsonOptions);
        
        // Update the campaign
        existingCampaign!.Title = "Updated Test Campaign";
        existingCampaign.Description = "This campaign has been updated during integration tests";
        
        var content = new StringContent(
            JsonSerializer.Serialize(existingCampaign, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PutAsync($"/api/campaigns/{campaignId}", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify the campaign was updated
        var verifyResponse = await client.GetAsync($"/api/campaigns/{campaignId}");
        verifyResponse.EnsureSuccessStatusCode();
        
        var verifyResponseString = await verifyResponse.Content.ReadAsStringAsync();
        var updatedCampaign = JsonSerializer.Deserialize<Campaign>(verifyResponseString, _jsonOptions);
        
        updatedCampaign.Should().NotBeNull();
        updatedCampaign!.Id.Should().Be(campaignId);
        updatedCampaign.Title.Should().Be("Updated Test Campaign");
        updatedCampaign.Description.Should().Be("This campaign has been updated during integration tests");
    }
    
    [Fact]
    public async Task ApproveCampaign_AsAdmin_ReturnsOkResult()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var campaignId = "campaign1";
        
        // Act
        var response = await client.PostAsync($"/api/campaigns/{campaignId}/approve", null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify the campaign was approved
        var verifyResponse = await client.GetAsync($"/api/campaigns/{campaignId}");
        verifyResponse.EnsureSuccessStatusCode();
        
        var verifyResponseString = await verifyResponse.Content.ReadAsStringAsync();
        var updatedCampaign = JsonSerializer.Deserialize<Campaign>(verifyResponseString, _jsonOptions);
        
        updatedCampaign.Should().NotBeNull();
        updatedCampaign!.Status.Should().Be(CampaignStatus.Active);
    }
    
    [Fact]
    public async Task RejectCampaign_AsAdmin_ReturnsOkResult()
    {
        // Arrange
        var token = await GetAuthTokenAsync("admin@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var campaignId = "campaign1";
        
        var rejectionReason = new
        {
            Reason = "This campaign does not meet our guidelines"
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(rejectionReason, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PostAsync($"/api/campaigns/{campaignId}/reject", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify the campaign was rejected
        var verifyResponse = await client.GetAsync($"/api/campaigns/{campaignId}");
        verifyResponse.EnsureSuccessStatusCode();
        
        var verifyResponseString = await verifyResponse.Content.ReadAsStringAsync();
        var updatedCampaign = JsonSerializer.Deserialize<Campaign>(verifyResponseString, _jsonOptions);
        
        updatedCampaign.Should().NotBeNull();
        updatedCampaign!.Status.Should().Be(CampaignStatus.Rejected);
    }
    
    [Fact]
    public async Task GetFeaturedCampaigns_ReturnsOkResult()
    {
        // Arrange
        
        // Act
        var response = await _client.GetAsync("/api/campaigns/featured");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var campaigns = JsonSerializer.Deserialize<List<Campaign>>(responseString, _jsonOptions);
        
        campaigns.Should().NotBeNull();
        campaigns!.Should().NotBeEmpty();
        campaigns.Should().OnlyContain(c => c.IsFeatured);
    }
}