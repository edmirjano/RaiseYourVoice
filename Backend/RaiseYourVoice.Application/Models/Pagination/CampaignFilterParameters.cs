using System;
using System.Collections.Generic;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Application.Models.Pagination
{
    /// <summary>
    /// Campaign-specific filter parameters for pagination and filtering
    /// </summary>
    public class CampaignFilterParameters : PaginationParameters
    {
        /// <summary>
        /// Filter by campaign categories
        /// </summary>
        public CampaignCategory? Category { get; set; }
        
        /// <summary>
        /// Filter by campaign status
        /// </summary>
        public CampaignStatus? Status { get; set; }
        
        /// <summary>
        /// Filter by organization ID
        /// </summary>
        public string? OrganizationId { get; set; }
        
        /// <summary>
        /// Filter by featured status
        /// </summary>
        public bool? IsFeatured { get; set; }
        
        /// <summary>
        /// Filter by minimum goal amount
        /// </summary>
        public decimal? MinGoal { get; set; }
        
        /// <summary>
        /// Filter by maximum goal amount
        /// </summary>
        public decimal? MaxGoal { get; set; }
        
        /// <summary>
        /// Filter by minimum funding progress percentage
        /// </summary>
        public int? MinProgressPercentage { get; set; }
        
        /// <summary>
        /// Filter by maximum funding progress percentage
        /// </summary>
        public int? MaxProgressPercentage { get; set; }
        
        /// <summary>
        /// Filter by start date range (earliest)
        /// </summary>
        public DateTime? StartDateFrom { get; set; }
        
        /// <summary>
        /// Filter by start date range (latest)
        /// </summary>
        public DateTime? StartDateTo { get; set; }
        
        /// <summary>
        /// Filter by end date range (earliest)
        /// </summary>
        public DateTime? EndDateFrom { get; set; }
        
        /// <summary>
        /// Filter by end date range (latest)
        /// </summary>
        public DateTime? EndDateTo { get; set; }
        
        /// <summary>
        /// Filter by whether the campaign is active (started and not ended)
        /// </summary>
        public bool? IsActive { get; set; }
        
        /// <summary>
        /// Filter by keyword in title or description
        /// </summary>
        public string? SearchText { get; set; }
        
        /// <summary>
        /// Filter by location (requires both Latitude and Longitude)
        /// </summary>
        public double? Latitude { get; set; }
        
        /// <summary>
        /// Filter by location (requires both Latitude and Longitude)
        /// </summary>
        public double? Longitude { get; set; }
        
        /// <summary>
        /// Maximum distance in kilometers for location-based search
        /// </summary>
        public double? MaxDistanceKm { get; set; } = 50;
        
        /// <summary>
        /// Filter by specific set of tags
        /// </summary>
        public IEnumerable<string>? Tags { get; set; }
    }
}