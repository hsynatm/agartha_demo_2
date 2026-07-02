namespace AMMS.Shared.Models
{

    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = [];

        public int Page { get; init; }

        public int PageSize { get; init; }

        public int TotalCount { get; init; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasNextPage => Page < TotalPages;

        public bool HasPreviousPage => Page > 1;

        public static PagedResult<TDestination> WithMappedItems<TSource, TDestination>(
            PagedResult<TSource> source,
            IReadOnlyList<TDestination> items)
        {
            return new PagedResult<TDestination>
            {
                Items = items,
                Page = source.Page,
                PageSize = source.PageSize,
                TotalCount = source.TotalCount
            };
        }
    }


}
