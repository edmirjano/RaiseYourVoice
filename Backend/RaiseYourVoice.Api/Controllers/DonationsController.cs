using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonationsController : ControllerBase
    {
        private readonly IDonationService _donationService;
        private readonly ICampaignService _campaignService;
        private readonly IPaymentGateway _paymentGateway;

        public DonationsController(
            IDonationService donationService, 
            ICampaignService campaignService,
            IPaymentGateway paymentGateway)
        {
            _donationService = donationService;
            _campaignService = campaignService;
            _paymentGateway = paymentGateway;
        }

        [HttpGet("campaign/{campaignId}")]
        public async Task<IActionResult> GetDonationsByCampaign(string campaignId, [FromQuery] bool includeAnonymous = false)
        {
            // Only return non-anonymous donations for public viewing unless specifically requested by admin
            var donations = await _donationService.GetDonationsByCampaignAsync(campaignId);
            
            if (!includeAnonymous && !User.IsInRole("Admin") && !User.IsInRole("Moderator"))
            {
                var publicDonations = new List<Donation>();
                foreach (var donation in donations)
                {
                    if (!donation.IsAnonymous)
                    {
                        publicDonations.Add(donation);
                    }
                }
                return Ok(publicDonations);
            }
            
            return Ok(donations);
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserDonations()
        {
            string userId = User.Identity.Name;
            var donations = await _donationService.GetDonationsByUserAsync(userId);
            return Ok(donations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDonationById(string id)
        {
            var donation = await _donationService.GetDonationByIdAsync(id);
            if (donation == null)
            {
                return NotFound();
            }

            // Check permissions for anonymous donations
            if (donation.IsAnonymous && 
                !User.IsInRole("Admin") && 
                !User.IsInRole("Moderator") && 
                User.Identity.Name != donation.UserId)
            {
                return Forbid();
            }

            return Ok(donation);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDonation([FromBody] PaymentRequest paymentRequest)
        {
            try
            {
                // Validate campaign exists
                var campaign = await _campaignService.GetCampaignByIdAsync(paymentRequest.CampaignId);
                if (campaign == null)
                {
                    return BadRequest("Campaign not found");
                }

                // Process payment
                var paymentResult = await _paymentGateway.ProcessPaymentAsync(paymentRequest);
                if (!paymentResult.Success)
                {
                    return BadRequest($"Payment failed: {paymentResult.ErrorMessage}");
                }

                // Create donation record
                string userId = User.Identity?.Name;
                
                var donation = new Donation
                {
                    CampaignId = paymentRequest.CampaignId,
                    UserId = userId, // Can be null for anonymous users
                    Amount = paymentRequest.Amount,
                    IsAnonymous = paymentRequest.CustomerInfo?.FullName == null || string.IsNullOrEmpty(userId),
                    Message = paymentRequest.Description,
                    PaymentStatus = paymentResult.Status,
                    TransactionId = paymentResult.TransactionId,
                    PaymentMethod = paymentRequest.PaymentMethod.Type,
                    Currency = paymentRequest.Currency,
                    ReceiptUrl = paymentResult.ReceiptUrl,
                    DonorInformation = paymentRequest.CustomerInfo
                };

                var createdDonation = await _donationService.CreateDonationAsync(donation);
                return CreatedAtAction(nameof(GetDonationById), new { id = createdDonation.Id }, createdDonation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/refund")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> RefundDonation(string id, [FromBody] RefundRequest request)
        {
            try
            {
                var result = await _donationService.RefundDonationAsync(id, request.Reason);
                if (result)
                {
                    return Ok();
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("campaign/{campaignId}/statistics")]
        public async Task<IActionResult> GetDonationStatistics(string campaignId)
        {
            try
            {
                var statistics = await _donationService.GetDonationStatisticsAsync(campaignId);
                return Ok(statistics);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("insights")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDonationInsights([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var insights = await _donationService.GetDonationInsightsAsync(startDate, endDate);
            return Ok(insights);
        }

        [HttpGet("{id}/receipt")]
        [Authorize]
        public async Task<IActionResult> GenerateDonationReceipt(string id)
        {
            try
            {
                var donation = await _donationService.GetDonationByIdAsync(id);
                if (donation == null)
                {
                    return NotFound();
                }

                // Verify the user is authorized to access this receipt
                string userId = User.Identity.Name;
                if (donation.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Moderator"))
                {
                    return Forbid();
                }

                string receiptUrl = await _donationService.GenerateDonationReceiptAsync(id);
                return Ok(new { ReceiptUrl = receiptUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("subscription")]
        [Authorize]
        public async Task<IActionResult> CreateSubscriptionDonation([FromBody] SubscriptionRequest request)
        {
            try
            {
                string userId = User.Identity.Name;
                
                var result = await _donationService.CreateSubscriptionDonationAsync(
                    userId, 
                    request.CampaignId, 
                    request.Amount, 
                    request.PaymentMethodId);
                
                if (result)
                {
                    return Ok();
                }
                
                return BadRequest("Failed to create subscription donation");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("subscription/{subscriptionId}")]
        [Authorize]
        public async Task<IActionResult> CancelSubscriptionDonation(string subscriptionId)
        {
            try
            {
                // In a real implementation, verify the user owns this subscription
                
                var result = await _donationService.CancelSubscriptionDonationAsync(subscriptionId);
                if (result)
                {
                    return Ok();
                }
                
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class RefundRequest
    {
        public string Reason { get; set; }
    }

    public class SubscriptionRequest
    {
        public string CampaignId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethodId { get; set; }
    }
}