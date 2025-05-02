using System.Collections.Generic;

namespace RaiseYourVoice.Application.Models.Pagination
{
    /// <summary>
    /// Generic class for paginated results
    /// </summary>
    /// <typeparam name="T">Type of items in the result</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Items on the current page
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();
        
        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public long TotalItems { get; set; }
        
        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        
        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
        
        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Create a paged result with empty items
        /// </summary>
        /// <param name="pageNumber">Current page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Empty paged result</returns>
        public static PagedResult<T> Empty(int pageNumber, int pageSize)
        {
            return new PagedResult<T>
            {
                Items = new List<T>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = 0
            };
        }

        /// <summary>
        /// Create a paged result from a list of items and total count
        /// </summary>
        /// <param name="items">Items for current page</param>
        /// <param name="totalItems">Total item count</param>
        /// <param name="pageNumber">Current page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paged result</returns>
        public static PagedResult<T> Create(IEnumerable<T> items, long totalItems, int pageNumber, int pageSize)
        {
            return new PagedResult<T>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}