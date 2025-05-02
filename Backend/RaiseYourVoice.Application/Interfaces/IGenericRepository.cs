using System.Linq.Expressions;
using RaiseYourVoice.Application.Models.Pagination;
using RaiseYourVoice.Domain.Common;

namespace RaiseYourVoice.Application.Interfaces
{
    /// <summary>
    /// Generic repository interface for all entities
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IGenericRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Get all entities
        /// </summary>
        /// <returns>All entities</returns>
        Task<IEnumerable<T>> GetAllAsync();
        
        /// <summary>
        /// Find entities by expression
        /// </summary>
        /// <param name="expression">Filter expression</param>
        /// <returns>Matching entities</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
        
        /// <summary>
        /// Get entity by ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>Entity or null if not found</returns>
        Task<T?> GetByIdAsync(string id);
        
        /// <summary>
        /// Add a new entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <returns>Added entity</returns>
        Task<T> AddAsync(T entity);
        
        /// <summary>
        /// Update an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <returns>True if update succeeded</returns>
        Task<bool> UpdateAsync(T entity);
        
        /// <summary>
        /// Delete an entity by ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>True if delete succeeded</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Get paginated entities
        /// </summary>
        /// <param name="parameters">Pagination parameters</param>
        /// <returns>Paged result of entities</returns>
        Task<PagedResult<T>> GetPagedAsync(PaginationParameters parameters);

        /// <summary>
        /// Get paginated entities with filtering
        /// </summary>
        /// <param name="parameters">Pagination parameters</param>
        /// <param name="filterExpression">Filter expression</param>
        /// <returns>Paged and filtered result of entities</returns>
        Task<PagedResult<T>> GetPagedAsync(PaginationParameters parameters, Expression<Func<T, bool>> filterExpression);
    }
}