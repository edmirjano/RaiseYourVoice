using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.GeoJsonObjectModel;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Persistence.Seeding
{
    public class OrganizationSeeder : BaseDataSeeder<Organization>
    {
        public OrganizationSeeder(MongoDbContext dbContext, ILogger<OrganizationSeeder> logger) 
            : base(dbContext, logger, "Organizations")
        {
        }

        protected override async Task SeedDataAsync()
        {
            var organizations = new List<Organization>
            {
                new Organization
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Earth Advocates Albania",
                    Description = "A non-profit organization dedicated to environmental protection and sustainability in Albania.",
                    LogoUrl = "https://storage.raiseyourvoice.al/organizations/earth-advocates-logo.webp",
                    Website = "https://earthadvocates.al",
                    SocialMediaLinks = new List<SocialMediaLink>
                    {
                        new SocialMediaLink { Platform = "Facebook", Url = "https://facebook.com/earthadvocatesalbania" },
                        new SocialMediaLink { Platform = "Instagram", Url = "https://instagram.com/earthadvocatesalbania" }
                    },
                    ContactInfo = new ContactInfo
                    {
                        Email = "info@earthadvocates.al",
                        Phone = "+355 42 123 456",
                        Address = "Rr. Myslym Shyri, Nr. 10, Tirana, Albania"
                    },
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedBy = "System",
                    VerificationDate = DateTime.UtcNow.AddDays(-30),
                    FoundingDate = new DateTime(2018, 4, 22), // Earth Day
                    MissionStatement = "To protect and restore the natural environment of Albania through advocacy, education, and direct action.",
                    VisionStatement = "A sustainable Albania where people live in harmony with nature.",
                    HeadquartersLocation = new GeoLocation { 
                        Type = "Point",
                        Coordinates = new double[] { 19.8187, 41.3275 } // Tirana coordinates
                    },
                    OperatingRegions = new List<string> { "Tirana", "Durres", "Vlore", "Sarande" },
                    OrganizationType = OrganizationType.NonProfit,
                    RegistrationNumber = "L71305451H",
                    TaxIdentificationNumber = "L71305451H",
                    LegalDocuments = new List<Document>(),
                    VerificationDocuments = new List<Document>(),
                    TeamMembers = new List<TeamMember>
                    {
                        new TeamMember
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = "Arben Maloku",
                            Title = "Executive Director",
                            Bio = "Environmental activist with 15 years of experience in non-profit management.",
                            ProfilePicture = "https://storage.raiseyourvoice.al/organizations/team/arben-maloku.webp"
                        },
                        new TeamMember
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = "Elira Dervishi",
                            Title = "Program Director",
                            Bio = "Marine biologist specializing in Adriatic Sea conservation.",
                            ProfilePicture = "https://storage.raiseyourvoice.al/organizations/team/elira-dervishi.webp"
                        }
                    },
                    PastProjects = new List<Project>
                    {
                        new Project
                        {
                            Id = Guid.NewGuid().ToString(),
                            Title = "Coastal Cleanup Initiative",
                            Description = "Organized volunteer cleanup efforts along the Albanian coastline.",
                            StartDate = DateTime.UtcNow.AddMonths(-6),
                            EndDate = DateTime.UtcNow.AddMonths(-1),
                            ImpactDescription = "Removed over 5 tons of plastic waste from beaches.",
                            ImageUrls = new List<string> { "https://storage.raiseyourvoice.al/organizations/projects/coastal-cleanup.webp" }
                        }
                    },
                    ImpactMetrics = new ImpactMetrics
                    {
                        PeopleHelped = 5000,
                        AreasCovered = 12,
                        VolunteerHours = 2500,
                        TreesPlanted = 1500,
                        CustomMetrics = new Dictionary<string, string>
                        {
                            { "Plastic Waste Collected (kg)", "5000" },
                            { "Educational Workshops Held", "35" }
                        }
                    },
                    CreatedAt = DateTime.UtcNow.AddMonths(-6),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                
                new Organization
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Youth Empowerment Network",
                    Description = "Empowering youth through education, skills training, and civic engagement opportunities.",
                    LogoUrl = "https://storage.raiseyourvoice.al/organizations/yen-logo.webp",
                    Website = "https://youthempowerment.al",
                    SocialMediaLinks = new List<SocialMediaLink>
                    {
                        new SocialMediaLink { Platform = "Facebook", Url = "https://facebook.com/youthempowermentnetwork" },
                        new SocialMediaLink { Platform = "Instagram", Url = "https://instagram.com/youthempowermentnetwork" }
                    },
                    ContactInfo = new ContactInfo
                    {
                        Email = "contact@youthempowerment.al",
                        Phone = "+355 42 234 567",
                        Address = "Rr. Ismail Qemali, Nr. 15, Tirana, Albania"
                    },
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedBy = "System",
                    VerificationDate = DateTime.UtcNow.AddDays(-45),
                    FoundingDate = new DateTime(2016, 8, 12), // International Youth Day
                    MissionStatement = "To create opportunities for young people to develop skills, engage in their communities, and become leaders of positive change.",
                    VisionStatement = "A society where all young people have the support, skills, and opportunities to reach their full potential.",
                    HeadquartersLocation = new GeoLocation { 
                        Type = "Point",
                        Coordinates = new double[] { 19.8215, 41.3266 } // Tirana coordinates
                    },
                    OperatingRegions = new List<string> { "Tirana", "Korce", "Shkoder", "Elbasan" },
                    OrganizationType = OrganizationType.NonProfit,
                    RegistrationNumber = "L82405762K",
                    TaxIdentificationNumber = "L82405762K",
                    LegalDocuments = new List<Document>(),
                    VerificationDocuments = new List<Document>(),
                    TeamMembers = new List<TeamMember>
                    {
                        new TeamMember
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = "Vjosa Berisha",
                            Title = "Executive Director",
                            Bio = "Youth development specialist with experience in international NGOs.",
                            ProfilePicture = "https://storage.raiseyourvoice.al/organizations/team/vjosa-berisha.webp"
                        },
                        new TeamMember
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = "Dritan Hoxha",
                            Title = "Program Coordinator",
                            Bio = "Education expert focused on skills development and employability.",
                            ProfilePicture = "https://storage.raiseyourvoice.al/organizations/team/dritan-hoxha.webp"
                        }
                    },
                    PastProjects = new List<Project>
                    {
                        new Project
                        {
                            Id = Guid.NewGuid().ToString(),
                            Title = "Digital Skills Bootcamp",
                            Description = "Three-month intensive training program teaching coding and digital marketing to unemployed youth.",
                            StartDate = DateTime.UtcNow.AddMonths(-8),
                            EndDate = DateTime.UtcNow.AddMonths(-5),
                            ImpactDescription = "Trained 100 young people with 70% finding employment within 3 months.",
                            ImageUrls = new List<string> { "https://storage.raiseyourvoice.al/organizations/projects/digital-bootcamp.webp" }
                        }
                    },
                    ImpactMetrics = new ImpactMetrics
                    {
                        PeopleHelped = 2500,
                        AreasCovered = 4,
                        VolunteerHours = 1200,
                        CustomMetrics = new Dictionary<string, string>
                        {
                            { "Youth Employed After Programs", "350" },
                            { "Training Hours Delivered", "1500" }
                        }
                    },
                    CreatedAt = DateTime.UtcNow.AddMonths(-8),
                    UpdatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            await _collection.InsertManyAsync(organizations);
            _logger.LogInformation("Inserted {Count} sample organizations", organizations.Count);
        }
    }
}