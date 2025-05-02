using System.Threading.Tasks;

namespace RaiseYourVoice.Infrastructure.Persistence.Seeding
{
    /// <summary>
    /// Interface for data seeders that populate initial data in the database
    /// </summary>
    public interface IDataSeeder
    {
        /// <summary>
        /// Seeds data if the target collection is empty
        /// </summary>
        Task SeedAsync();
        
        /// <summary>
        /// Force reseeds data even if the collection is not empty
        /// </summary>
        Task ReseedAsync();
        
        /// <summary>
        /// Checks if seeding is required (if collection is empty)
        /// </summary>
        Task<bool> SeedingRequiredAsync();
    }
}