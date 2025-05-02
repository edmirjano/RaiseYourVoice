using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RaiseYourVoice.Infrastructure.Persistence.Seeding
{
    /// <summary>
    /// Coordinates the execution of all data seeders in the correct order
    /// </summary>
    public class DataSeederCoordinator
    {
        private readonly ILogger<DataSeederCoordinator> _logger;
        private readonly UserSeeder _userSeeder;
        private readonly OrganizationSeeder _organizationSeeder;
        private readonly CampaignSeeder _campaignSeeder;
        private readonly PostSeeder _postSeeder;
        private readonly DonationSeeder _donationSeeder;
        private readonly CommentSeeder _commentSeeder;
        private readonly IEnumerable<IDataSeeder> _additionalSeeders;

        public DataSeederCoordinator(
            ILogger<DataSeederCoordinator> logger, 
            UserSeeder userSeeder,
            OrganizationSeeder organizationSeeder,
            CampaignSeeder campaignSeeder,
            PostSeeder postSeeder,
            DonationSeeder donationSeeder,
            CommentSeeder commentSeeder,
            IEnumerable<IDataSeeder> additionalSeeders = null)
        {
            _logger = logger;
            _userSeeder = userSeeder;
            _organizationSeeder = organizationSeeder;
            _campaignSeeder = campaignSeeder;
            _postSeeder = postSeeder;
            _donationSeeder = donationSeeder;
            _commentSeeder = commentSeeder;
            _additionalSeeders = additionalSeeders ?? new List<IDataSeeder>();
        }

        /// <summary>
        /// Seeds all data in the correct order, only if collections are empty
        /// </summary>
        public async Task SeedAllAsync()
        {
            _logger.LogInformation("Starting data seeding process");
            
            // Seed users first as they are referenced by other entities
            await _userSeeder.SeedAsync();
            
            // Seed organizations next
            await _organizationSeeder.SeedAsync();
            
            // Seed campaigns which depend on organizations
            await _campaignSeeder.SeedAsync();
            
            // Seed posts which depend on users
            await _postSeeder.SeedAsync();
            
            // Seed donations which depend on campaigns and users
            await _donationSeeder.SeedAsync();
            
            // Seed comments which depend on posts and users
            await _commentSeeder.SeedAsync();
            
            // Seed any additional data
            foreach (var seeder in _additionalSeeders)
            {
                await seeder.SeedAsync();
            }
            
            _logger.LogInformation("Data seeding process completed successfully");
        }

        /// <summary>
        /// Reseeds all data, removing existing data first
        /// </summary>
        public async Task ReseedAllAsync()
        {
            _logger.LogInformation("Starting data reseeding process");
            
            // Same order as SeedAllAsync but using ReseedAsync instead
            await _userSeeder.ReseedAsync();
            await _organizationSeeder.ReseedAsync();
            await _campaignSeeder.ReseedAsync();
            await _postSeeder.ReseedAsync();
            await _donationSeeder.ReseedAsync();
            await _commentSeeder.ReseedAsync();
            
            foreach (var seeder in _additionalSeeders)
            {
                await seeder.ReseedAsync();
            }
            
            _logger.LogInformation("Data reseeding process completed successfully");
        }
    }
}