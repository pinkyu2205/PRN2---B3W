namespace Application.DTOs.Pagination
{
    public class PaginationFilter
    {
        private int _pageNumber;
        private int _pageSize;

        public int PageNumber
        {
            get => _pageNumber <= 0 ? PaginationDefaults.DefaultPageNumber : _pageNumber;
            set => _pageNumber = value <= 0 ? PaginationDefaults.DefaultPageNumber : value;
        }

        public int PageSize
        {
            get => _pageSize <= 0 ? PaginationDefaults.DefaultPageSize : _pageSize;
            set
            {
                if (value <= 0) _pageSize = PaginationDefaults.DefaultPageSize;
                else _pageSize = Math.Min(value, PaginationDefaults.MaxPageSize);
            }
        }

        public string? Search { get; set; }
        public string? Category { get; set; }
    }
}

