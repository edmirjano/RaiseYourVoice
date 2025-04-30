using System;
using System.Collections.Generic;
using RaiseYourVoice.Domain.Common;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Domain.Entities
{
    public class Organization : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string Website { get; set; }
        public Dictionary<SocialMediaType, string> SocialMediaLinks { get; set; } = new Dictionary<SocialMediaType, string>();
        public ContactInfo ContactInfo { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? VerificationDate { get; set; }
        public DateTime FoundingDate { get; set; }
        public string MissionStatement { get; set; }
        public string VisionStatement { get; set; }
        public Location HeadquartersLocation { get; set; }
        public List<string> OperatingRegions { get; set; } = new List<string>();
        public OrganizationType OrganizationType { get; set; }
        public string RegistrationNumber { get; set; }
        public string TaxIdentificationNumber { get; set; }
        public List<Document> LegalDocuments { get; set; } = new List<Document>();
        public List<Document> VerificationDocuments { get; set; } = new List<Document>();
        public List<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
        public List<Project> PastProjects { get; set; } = new List<Project>();
        public ImpactMetrics ImpactMetrics { get; set; }
        public BankingInformation BankingInformation { get; set; }
    }

    public class ContactInfo
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonRole { get; set; }
    }

    public class Document
    {
        public string Title { get; set; }
        public string FileUrl { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; }
    }

    public class TeamMember
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string Bio { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class Project
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> Achievements { get; set; } = new List<string>();
        public decimal? Budget { get; set; }
    }

    public class ImpactMetrics
    {
        public int PeopleBenefited { get; set; }
        public int CommunitiesServed { get; set; }
        public Dictionary<string, int> CategoryMetrics { get; set; } = new Dictionary<string, int>();
    }

    public class BankingInformation
    {
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountHolderName { get; set; }
        public string Currency { get; set; }
    }
}