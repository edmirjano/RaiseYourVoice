using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;
using RaiseYourVoice.Infrastructure.Persistence;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class OrganizationRepository : MongoGenericRepository<Organization>
    {
        public OrganizationRepository(MongoDbContext context) : base(context, "Organizations")
        {
        }

        public async Task<IEnumerable<Organization>> GetByVerificationStatusAsync(VerificationStatus status)
        {
            var filter = Builders<Organization>.Filter.Eq(o => o.VerificationStatus, status);
            return await _collection.Find(filter)
                .SortByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Organization>> GetByRegionAsync(string region)
        {
            var filter = Builders<Organization>.Filter.AnyEq(o => o.OperatingRegions, region);
            return await _collection.Find(filter)
                .SortByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Organization>> GetByOrganizationTypeAsync(OrganizationType organizationType)
        {
            var filter = Builders<Organization>.Filter.Eq(o => o.OrganizationType, organizationType);
            return await _collection.Find(filter)
                .SortByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateVerificationStatusAsync(string organizationId, VerificationStatus status, string verifiedById, DateTime verificationDate)
        {
            var filter = Builders<Organization>.Filter.Eq(o => o.Id, organizationId);
            var update = Builders<Organization>.Update
                .Set(o => o.VerificationStatus, status)
                .Set(o => o.VerifiedBy, verifiedById)
                .Set(o => o.VerificationDate, verificationDate)
                .Set(o => o.UpdatedAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> AddTeamMemberAsync(string organizationId, TeamMember member)
        {
            var filter = Builders<Organization>.Filter.Eq(o => o.Id, organizationId);
            var update = Builders<Organization>.Update
                .Push(o => o.TeamMembers, member)
                .Set(o => o.UpdatedAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> RemoveTeamMemberAsync(string organizationId, string teamMemberId)
        {
            var filter = Builders<Organization>.Filter.Eq(o => o.Id, organizationId);
            var update = Builders<Organization>.Update
                .PullFilter(o => o.TeamMembers, tm => tm.Id == teamMemberId)
                .Set(o => o.UpdatedAt, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        
        public async Task<IEnumerable<Organization>> SearchOrganizationsAsync(string searchTerm)
        {
            var filter = Builders<Organization>.Filter.Or(
                Builders<Organization>.Filter.Regex(o => o.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Organization>.Filter.Regex(o => o.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Organization>.Filter.Regex(o => o.MissionStatement, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );
            
            return await _collection.Find(filter)
                .SortByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Organization>> GetByLocationAsync(double latitude, double longitude, double maxDistanceInKm = 10)
        {
            // Convert km to radians (Earth's radius is approximately 6371 km)
            double maxDistanceInRadians = maxDistanceInKm / 6371.0;
            
            // Create a geospatial query to find organizations near the given coordinates
            var filter = Builders<Organization>.Filter.NearSphere(
                o => o.HeadquartersLocation, 
                longitude, 
                latitude, 
                maxDistanceInRadians
            );
            
            return await _collection.Find(filter)
                .SortByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}