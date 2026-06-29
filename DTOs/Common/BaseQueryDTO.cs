namespace ShopManagementAPI.DTOs.Common
{
    public class BaseQueryDTO
    {
        // Pagination
        public int Page { get; set; } = 1;

        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > 100 ? 100 : value;
        }

        // Search
        public string? Keyword { get; set; }

        // Sort
        public string SortBy { get; set; } = "Id";

        public string SortDirection { get; set; } = "asc";
    }
}
