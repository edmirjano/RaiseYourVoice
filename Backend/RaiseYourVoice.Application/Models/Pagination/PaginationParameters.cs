using System.ComponentModel.DataAnnotations;

namespace RaiseYourVoice.Application.Models.Pagination
{
    /// <summary>
    /// Base class for pagination parameters
    /// </summary>
    public class PaginationParameters
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;
        private int _pageNumber = 1;

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than or equal to 1")]
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = (value < 1) ? 1 : value;
        }

        /// <summary>
        /// Number of items per page (default 10, max 50)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than or equal to 1")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : (value < 1) ? 1 : value;
        }

        /// <summary>
        /// Field to sort by (implementation specific)
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction (true = ascending, false = descending)
        /// </summary>
        public bool Ascending { get; set; } = false;
    }
}