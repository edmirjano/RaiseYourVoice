using System;
using System.Collections.Generic;
using RaiseYourVoice.Domain.Common;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Domain.Entities
{
    public class Campaign : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public decimal Goal { get; set; }
        public decimal AmountRaised { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public CampaignStatus Status { get; set; }
        public string CoverImageUrl { get; set; }
        public List<string> AdditionalImagesUrls { get; set; } = new List<string>();
        public string VideoUrl { get; set; }
        public List<CampaignUpdate> Updates { get; set; } = new List<CampaignUpdate>();
        public List<Donation> Donations { get; set; } = new List<Donation>();
        public List<CampaignTag> Tags { get; set; } = new List<CampaignTag>();
        public CampaignCategory Category { get; set; }
        public Location Location { get; set; }
        public bool IsFeatured { get; set; }
        public int ViewCount { get; set; }
        public List<string> Documents { get; set; } = new List<string>(); // URLs to documents about the campaign
        public string BudgetBreakdownUrl { get; set; } // URL to budget breakdown document
        public decimal MinimumDonationAmount { get; set; } = 1.00m;
        public bool AllowAnonymousDonations { get; set; } = true;
        public string ThankYouMessage { get; set; }
        public List<CampaignMilestone> Milestones { get; set; } = new List<CampaignMilestone>();
        public TransparencyReport TransparencyReport { get; set; }
    }

    public class CampaignUpdate
    {
        public string Id { get; set; }
        public string CampaignId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PostedAt { get; set; }
        public List<string> MediaUrls { get; set; } = new List<string>();
    }

    public class CampaignTag
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class CampaignMilestone
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime? ReachedAt { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class TransparencyReport
    {
        public string Id { get; set; }
        public string CampaignId { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<ExpenseItem> Expenses { get; set; } = new List<ExpenseItem>();
        public List<string> ReceiptUrls { get; set; } = new List<string>();
        public string AuditDocumentUrl { get; set; }
    }

    public class ExpenseItem
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public string ReceiptUrl { get; set; }
    }
}