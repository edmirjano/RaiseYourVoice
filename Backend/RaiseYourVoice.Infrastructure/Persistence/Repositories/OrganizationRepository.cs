using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RaiseYourVoice.Domain.Entities;
using RaiseYourVoice.Domain.Enums;

namespace RaiseYourVoice.Infrastructure.Persistence.Repositories
{
    public class OrganizationRepository : MongoRepository<Organization>
    {
        public OrganizationRepository(MongoDbContext context, ILogger<OrganizationRepository> logger) 
            : base(context, "Organizations", logger)
        {
        }

        public async Task<IEnumerable<Organization>> GetByVerificationStatusAsync(VerificationStatus status)
        {
            var filter = Builders<Organization>.Filter.Eq(o => o.VerificationStatus, status);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Organization>> GetByRegionAsync(string region)
        {
            var filter = Builders<Organization>.Filter.AnyEq(o => o.OperatingRegions, region);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Organization>> GetByOrganizationTypeAsync(OrganizationType organizationType)
        {
            var filter = Builders<Organization>.Filter.Eq(o => o.OrganizationType, organizationType);
            return await _collection.Find(filter).ToListAsync();
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

        public async Task<bool> UpdateTeamMemberAsync(string organizationId, TeamMember updatedMember)
        {
            var filter = Builders<Organization>.Filter.And(
                Builders<Organization>.Filter.Eq(o => o.Id, organizationId),
                Builders<Organization>.Filter.ElemMatch(o => o.TeamMembers, tm => tm.Id == updatedMember.Id)
            );

            var update = Builders<Organization>.Update
                .Set(o => o.TeamMembers[-1], updatedMember) // [-1] updates the matched element
                .Set(o => o.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> AddProjectAsync(string organizationId, Project project)
        {
            var filter = Builders<Organization>.Filter.Eq(o => o.Id, organizationId);
            var update = Builders<Organization>.Update
                .Push(o => o.PastProjects, project)
                .Set(o => o.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateImpactMetricsAsync(string organizationId, ImpactMetrics metrics)
        {
            var filter = Builders<Organization>.Filter.Eq(o => o.Id, organizationId);
            var update = Builders<Organization>.Update
                .Set(o => o.ImpactMetrics, metrics)
                .Set(o => o.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateBankingInformationAsync(string organizationId, BankingInformation bankingInfo)
        {
            var filter = Builders<Organization>.Filter.Eq(o => o.Id, organizationId);
            var update = Builders<Organization>.Update
                .Set(o => o.BankingInformation, bankingInfo)
                .Set(o => o.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> AddDocumentAsync(string organizationId, Document document, bool isVerificationDocument)
        {
            var filter = Builders<Organization>.Filter.Eq(o => o.Id, organizationId);
            UpdateDefinition<Organization> update;

            if (isVerificationDocument)
            {
                update = Builders<Organization>.Update
                    .Push(o => o.VerificationDocuments, document)
                    .Set(o => o.UpdatedAt, DateTime.UtcNow);
            }
            else
            {
                update = Builders<Organization>.Update
                    .Push(o => o.LegalDocuments, document)
                    .Set(o => o.UpdatedAt, DateTime.UtcNow);
            }

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateContactInfoAsync(string organizationId, ContactInfo contactInfo)
        {
            var filter = Builders<Organization>.Filter.Eq(o => o.Id, organizationId);
            var update = Builders<Organization>.Update
                .Set(o => o.ContactInfo, contactInfo)
                .Set(o => o.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<Organization>> GetPaginatedOrganizationsAsync(
            int pageNumber, 
            int pageSize,
            VerificationStatus? verificationStatus = null,
            OrganizationType? organizationType = null,
            string? region = null,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true)
        {
            var filterBuilder = Builders<Organization>.Filter;
            var filters = new List<FilterDefinition<Organization>>();

            // Apply filters
            if (verificationStatus.HasValue)
            {
                filters.Add(filterBuilder.Eq(o => o.VerificationStatus, verificationStatus.Value));
            }

            if (organizationType.HasValue)
            {
                filters.Add(filterBuilder.Eq(o => o.OrganizationType, organizationType.Value));
            }

            if (!string.IsNullOrEmpty(region))
            {
                filters.Add(filterBuilder.AnyEq(o => o.OperatingRegions, region));
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Regex(o => o.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    filterBuilder.Regex(o => o.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    filterBuilder.Regex(o => o.MissionStatement, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
                );
                filters.Add(searchFilter);
            }

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            // Create sort definition
            SortDefinition<Organization> sortDefinition;

            if (!string.IsNullOrEmpty(sortBy))
            {
                // Map sortBy string to property expression
                switch (sortBy.ToLower())
                {
                    case "name":
                        sortDefinition = ascending
                            ? Builders<Organization>.Sort.Ascending(o => o.Name)
                            : Builders<Organization>.Sort.Descending(o => o.Name);
                        break;
                    case "foundingdate":
                        sortDefinition = ascending
                            ? Builders<Organization>.Sort.Ascending(o => o.FoundingDate)
                            : Builders<Organization>.Sort.Descending(o => o.FoundingDate);
                        break;
                    case "verificationdate":
                        sortDefinition = ascending
                            ? Builders<Organization>.Sort.Ascending(o => o.VerificationDate)
                            : Builders<Organization>.Sort.Descending(o => o.VerificationDate);
                        break;
                    default:
                        sortDefinition = Builders<Organization>.Sort.Ascending(o => o.Name);
                        break;
                }
            }
            else
            {
                // Default sort by name ascending
                sortDefinition = Builders<Organization>.Sort.Ascending(o => o.Name);
            }

            return await _collection
                .Find(combinedFilter)
                .Sort(sortDefinition)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<long> CountOrganizationsAsync(
            VerificationStatus? verificationStatus = null,
            OrganizationType? organizationType = null,
            string? region = null,
            string? searchTerm = null)
        {
            var filterBuilder = Builders<Organization>.Filter;
            var filters = new List<FilterDefinition<Organization>>();

            // Apply filters
            if (verificationStatus.HasValue)
            {
                filters.Add(filterBuilder.Eq(o => o.VerificationStatus, verificationStatus.Value));
            }

            if (organizationType.HasValue)
            {
                filters.Add(filterBuilder.Eq(o => o.OrganizationType, organizationType.Value));
            }

            if (!string.IsNullOrEmpty(region))
            {
                filters.Add(filterBuilder.AnyEq(o => o.OperatingRegions, region));
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Regex(o => o.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    filterBuilder.Regex(o => o.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    filterBuilder.Regex(o => o.MissionStatement, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
                );
                filters.Add(searchFilter);
            }

            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            return await _collection.CountDocumentsAsync(combinedFilter);
        }
    }
}