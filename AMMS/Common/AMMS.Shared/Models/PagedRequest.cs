namespace AMMS.Shared.Models
{
    public class PagedRequest
    {
        private int _page = PagingDefaults.DefaultPage;
        private int _pageSize = PagingDefaults.DefaultPageSize;

        public int Page
        {
            get => _page;
            set => _page = PagingDefaults.NormalizePage(value);
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = PagingDefaults.NormalizePageSize(value);
        }
    }
}
