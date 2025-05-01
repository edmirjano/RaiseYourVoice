using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Application.Models.Requests;
using RaiseYourVoice.Domain.Entities;

namespace RaiseYourVoice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignService _campaignService;

        public CampaignsController(ICampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCampaigns()
        {
            var campaigns = await _campaignService.GetAllCampaignsAsync();
            return Ok(campaigns);
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedCampaigns()
        {
            var campaigns = await _campaignService.GetFeaturedCampaignsAsync();
            return Ok(campaigns);
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetCampaignsByCategory(string category)
        {
            try
            {
                var campaigns = await _campaignService.GetCampaignsByCategoryAsync(category);
                return Ok(campaigns);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCampaignById(string id)
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound();
            }

            return Ok(campaign);
        }

        [HttpPost]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> CreateCampaign([FromBody] Campaign campaign)
        {
            try
            {
                // Check if the authenticated user belongs to the organization
                string userId = User.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");
                
                // In a real implementation, verify that the user belongs to the organization
                // and has permission to create campaigns for it
                
                var createdCampaign = await _campaignService.CreateCampaignAsync(campaign);
                return CreatedAtAction(nameof(GetCampaignById), new { id = createdCampaign.Id }, createdCampaign);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> UpdateCampaign(string id, [FromBody] Campaign campaign)
        {
            try
            {
                if (id != campaign.Id)
                {
                    return BadRequest("Campaign ID mismatch");
                }

                // Verify ownership/permissions in a real implementation
                
                var updatedCampaign = await _campaignService.UpdateCampaignAsync(campaign);
                return Ok(updatedCampaign);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> DeleteCampaign(string id)
        {
            // Verify ownership/permissions in a real implementation
            
            var result = await _campaignService.DeleteCampaignAsync(id);
            if (result)
            {
                return NoContent();
            }

            return NotFound();
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> ApproveCampaign(string id)
        {
            var result = await _campaignService.ApproveCampaignAsync(id);
            if (result)
            {
                return Ok();
            }

            return NotFound();
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> RejectCampaign(string id, [FromBody] RejectionReason rejection)
        {
            var result = await _campaignService.RejectCampaignAsync(id, rejection.Reason);
            if (result)
            {
                return Ok();
            }

            return NotFound();
        }

        [HttpPost("{id}/updates")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> AddCampaignUpdate(string id, [FromBody] CampaignUpdate update)
        {
            try
            {
                // Verify ownership/permissions in a real implementation
                
                var result = await _campaignService.AddCampaignUpdateAsync(id, update);
                if (result)
                {
                    return Ok(update);
                }

                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/milestones")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> AddCampaignMilestone(string id, [FromBody] CampaignMilestone milestone)
        {
            try
            {
                // Verify ownership/permissions in a real implementation
                
                var result = await _campaignService.AddCampaignMilestoneAsync(id, milestone);
                if (result)
                {
                    return Ok(milestone);
                }

                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/transparency-report")]
        [Authorize(Roles = "Organization,Admin")]
        public async Task<IActionResult> UpdateTransparencyReport(string id, [FromBody] TransparencyReport report)
        {
            try
            {
                // Verify ownership/permissions in a real implementation
                
                var result = await _campaignService.UpdateTransparencyReportAsync(id, report);
                if (result)
                {
                    return Ok(report);
                }

                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/feature")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FeatureCampaign(string id, [FromBody] FeatureRequest request)
        {
            var result = await _campaignService.FeatureCampaignAsync(id, request.Featured);
            if (result)
            {
                return Ok();
            }

            return NotFound();
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchCampaigns([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Search query is required");
            }

            var campaigns = await _campaignService.SearchCampaignsAsync(query);
            return Ok(campaigns);
        }

        [HttpGet("{id}/statistics")]
        public async Task<IActionResult> GetCampaignStatistics(string id)
        {
            try
            {
                var statistics = await _campaignService.GetCampaignStatisticsAsync(id);
                return Ok(statistics);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}