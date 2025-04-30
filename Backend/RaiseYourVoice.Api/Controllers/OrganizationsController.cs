using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaiseYourVoice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IGenericRepository<Organization> _organizationRepository;

        public OrganizationsController(IGenericRepository<Organization> organizationRepository)
        {
            _organizationRepository = organizationRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizations()
        {
            var organizations = await _organizationRepository.GetAllAsync();
            return Ok(organizations);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Organization>> GetOrganization(string id)
        {
            var organization = await _organizationRepository.GetByIdAsync(id);
            if (organization == null)
            {
                return NotFound();
            }
            return Ok(organization);
        }

        [HttpGet("verified")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Organization>>> GetVerifiedOrganizations()
        {
            var organizations = await _organizationRepository.GetAllAsync();
            return Ok(organizations.Where(o => o.VerificationStatus == VerificationStatus.Verified));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Organization>> CreateOrganization(Organization organization)
        {
            // Set initial state
            organization.CreatedAt = DateTime.UtcNow;
            organization.VerificationStatus = VerificationStatus.Pending;
            
            await _organizationRepository.AddAsync(organization);
            return CreatedAtAction(nameof(GetOrganization), new { id = organization.Id }, organization);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateOrganization(string id, Organization organization)
        {
            if (id != organization.Id)
            {
                return BadRequest();
            }
            
            // Get existing data for verification checks
            var existingOrg = await _organizationRepository.GetByIdAsync(id);
            if (existingOrg == null)
            {
                return NotFound();
            }
            
            // Only allow moderators and admins to change verification status
            if (existingOrg.VerificationStatus != organization.VerificationStatus && 
                !User.IsInRole("Admin") && 
                !User.IsInRole("Moderator"))
            {
                organization.VerificationStatus = existingOrg.VerificationStatus;
            }
            
            // Record verification details if status changed to verified
            if (existingOrg.VerificationStatus != VerificationStatus.Verified && 
                organization.VerificationStatus == VerificationStatus.Verified)
            {
                organization.VerifiedBy = User.Identity.Name;
                organization.VerificationDate = DateTime.UtcNow;
            }
            
            organization.UpdatedAt = DateTime.UtcNow;
            var success = await _organizationRepository.UpdateAsync(organization);
            if (!success)
            {
                return NotFound();
            }
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrganization(string id)
        {
            var success = await _organizationRepository.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }
            
            return NoContent();
        }

        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> VerifyOrganization(string id)
        {
            var organization = await _organizationRepository.GetByIdAsync(id);
            if (organization == null)
            {
                return NotFound();
            }
            
            organization.VerificationStatus = VerificationStatus.Verified;
            organization.VerifiedBy = User.Identity.Name;
            organization.VerificationDate = DateTime.UtcNow;
            organization.UpdatedAt = DateTime.UtcNow;
            
            var success = await _organizationRepository.UpdateAsync(organization);
            if (!success)
            {
                return NotFound();
            }
            
            return NoContent();
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> RejectOrganization(string id)
        {
            var organization = await _organizationRepository.GetByIdAsync(id);
            if (organization == null)
            {
                return NotFound();
            }
            
            organization.VerificationStatus = VerificationStatus.Rejected;
            organization.UpdatedAt = DateTime.UtcNow;
            
            var success = await _organizationRepository.UpdateAsync(organization);
            if (!success)
            {
                return NotFound();
            }
            
            return NoContent();
        }
    }
}