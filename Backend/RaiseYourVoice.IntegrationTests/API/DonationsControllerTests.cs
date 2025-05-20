using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RaiseYourVoice.Application.Models.Requests;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.IntegrationTests.API;

public class DonationsControllerTests : TestBase
{
    [Fact]
    public async Task GetDonationsByCampaign_WithValidCampaignId_ReturnsOkResult()
    {
        // Arrange
        var campaignId = "campaign1";
        
        // First, create a test donation
        await CreateTestDonation(campaignId);
        
        // Act
        var response = await _client.GetAsync($"/api/donations/campaign/{campaignId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var donations = JsonSerializer.Deserialize<List<Donation>>(responseString, _jsonOptions);
        
        donations.Should().NotBeNull();
        donations!.Should().NotBeEmpty();
        donations.Should().OnlyContain(d => d.CampaignId == campaignId);
    }
    
    [Fact]
    public async Task GetDonationById_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var campaignId = "campaign1";
        var donation = await CreateTestDonation(campaignId);
        
        // Act
        var response = await _client.GetAsync($"/api/donations/{donation.Id}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var retrievedDonation = JsonSerializer.Deserialize<Donation>(responseString, _jsonOptions);
        
        retrievedDonation.Should().NotBeNull();
        retrievedDonation!.Id.Should().Be(donation.Id);
        retrievedDonation.CampaignId.Should().Be(campaignId);
        retrievedDonation.Amount.Should().Be(100);
    }
    
    [Fact]
    public async Task GetDonationById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var donationId = "nonexistent";
        
        // Act
        var response = await _client.GetAsync($"/api/donations/{donationId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task CreateDonation_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var paymentRequest = new PaymentRequest
        {
            CampaignId = "campaign1",
            Amount = 50,
            Currency = "USD",
            Description = "Test donation",
            PaymentMethod = new PaymentMethodInfo
            {
                Type = "card",
                CardNumber = "4242424242424242",
                ExpiryMonth = "12",
                ExpiryYear = "2025",
                Cvc = "123",
                CardholderName = "Test User"
            },
            CustomerInfo = new DonorInformation
            {
                Email = "donor@test.com",
                FullName = "Test Donor",
                Address = "123 Test St",
                City = "Test City",
                State = "Test State",
                Country = "US",
                PostalCode = "12345",
                Phone = "123-456-7890"
            }
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(paymentRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await client.PostAsync("/api/donations", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var createdDonation = JsonSerializer.Deserialize<Donation>(responseString, _jsonOptions);
        
        createdDonation.Should().NotBeNull();
        createdDonation!.Id.Should().NotBeNullOrEmpty();
        createdDonation.CampaignId.Should().Be(paymentRequest.CampaignId);
        createdDonation.Amount.Should().Be(paymentRequest.Amount);
        createdDonation.UserId.Should().Be("user2"); // The ID of the user@test.com user
    }
    
    [Fact]
    public async Task GetDonationStatistics_WithValidCampaignId_ReturnsOkResult()
    {
        // Arrange
        var campaignId = "campaign1";
        
        // Create a few test donations
        await CreateTestDonation(campaignId);
        await CreateTestDonation(campaignId);
        await CreateTestDonation(campaignId);
        
        // Act
        var response = await _client.GetAsync($"/api/donations/campaign/{campaignId}/statistics");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseString = await response.Content.ReadAsStringAsync();
        var statistics = JsonSerializer.Deserialize<Dictionary<string, decimal>>(responseString, _jsonOptions);
        
        statistics.Should().NotBeNull();
        statistics!.Should().ContainKey("totalAmount");
        statistics.Should().ContainKey("averageDonation");
        statistics.Should().ContainKey("count");
    }
    
    private async Task<Donation> CreateTestDonation(string campaignId)
    {
        var token = await GetAuthTokenAsync("user@test.com", "Admin123!");
        var client = GetAuthenticatedClient(token);
        
        var paymentRequest = new PaymentRequest
        {
            CampaignId = campaignId,
            Amount = 100,
            Currency = "USD",
            Description = "Test donation",
            PaymentMethod = new PaymentMethodInfo
            {
                Type = "card",
                CardNumber = "4242424242424242",
                ExpiryMonth = "12",
                ExpiryYear = "2025",
                Cvc = "123",
                CardholderName = "Test User"
            },
            CustomerInfo = new DonorInformation
            {
                Email = "donor@test.com",
                FullName = "Test Donor",
                Address = "123 Test St",
                City = "Test City",
                State = "Test State",
                Country = "US",
                PostalCode = "12345",
                Phone = "123-456-7890"
            }
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(paymentRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");
        
        var response = await client.PostAsync("/api/donations", content);
        response.EnsureSuccessStatusCode();
        
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Donation>(responseString, _jsonOptions)!;
    }
}