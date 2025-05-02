using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Application.Models.Pagination;
using RaiseYourVoice.Application.Models.Requests;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;
using RaiseYourVoice.Infrastructure.Persistence.Repositories;

namespace RaiseYourVoice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignService _campaignService;
        private readonly CampaignRepository _campaignRepository; // Direct repository access for advanced features

        public CampaignsController(ICampaignService campaignService, CampaignRepository campaignRepository)
        {
            _campaignService = campaignService;
            _campaignRepository = campaignRepository;
        }

        /// <summary>
        /// Get all campaigns with pagination and filtering
        /// </summary>
        /// <param name="parameters">Filter and pagination parameters</param>
        [HttpGet]
        public async Task<IActionResult> GetAllCampaigns([FromQuery] CampaignFilterParameters parameters)
        {
            var campaigns = await _campaignRepository.GetCampaignsAsync(parameters);
            return Ok(campaigns);
        }
        
        /// <summary>
        /// Get paginated campaigns near a geographic location
        /// </summary>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="maxDistanceKm">Maximum distance in kilometers</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyCampaigns(
            [FromQuery] double latitude, 
            [FromQuery] double longitude,
            [FromQuery] double maxDistanceKm = 50,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var campaigns = await _campaignRepository.GetNearbyAsync(
                latitude, 
                longitude, 
                maxDistanceKm,
                pageNumber,
                pageSize);
                
            return Ok(campaigns);
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedCampaigns([FromQuery] int limit = 5)
        {
            var campaigns = await _campaignService.GetFeaturedCampaignsAsync(limit);
            return Ok(campaigns);
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetCampaignsByCategory(string category, [FromQuery] PaginationParameters parameters)
        {
            try
            {
                if (!Enum.TryParse<CampaignCategory>(category, true, out var campaignCategory))
                {
                    return BadRequest($"Invalid category: {category}");
                }
                
                var filterParameters = new CampaignFilterParameters
                {
                    Category = campaignCategory,
                    PageNumber = parameters.PageNumber,
                    PageSize = parameters.PageSize,
                    SortBy = parameters.SortBy,
                    Ascending = parameters.Ascending
                };
                
                var campaigns = await _campaignRepository.GetCampaignsAsync(filterParameters);
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

            // Increment view count asynchronously
            _ = _campaignRepository.IncrementViewCountAsync(id);

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

        /// <summary>
        /// Search campaigns with full-text search and filtering
        /// </summary>
        /// <param name="parameters">Campaign filter parameters containing search text and other filters</param>
        [HttpGet("search")]
        public async Task<IActionResult> SearchCampaigns([FromQuery] CampaignFilterParameters parameters)
        {
            if (string.IsNullOrEmpty(parameters.SearchText))
            {
                return BadRequest("Search query is required");
            }

            var campaigns = await _campaignRepository.GetCampaignsAsync(parameters);
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
        
        /// <summary>
        /// Get global campaign statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetGlobalStatistics()
        {
            var statistics = await _campaignRepository.GetCampaignStatisticsAsync();
            return Ok(statistics);
        }
    }
}