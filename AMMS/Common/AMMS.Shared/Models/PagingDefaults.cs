namespace AMMS.Shared.Models;

public static class PagingDefaults
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public static int NormalizePage(int page) => page < 1 ? DefaultPage : page;

    public static int NormalizePageSize(int pageSize) =>
        pageSize < 1 ? DefaultPageSize : pageSize > MaxPageSize ? MaxPageSize : pageSize;

    public static (int Page, int PageSize) Normalize(int page, int pageSize) =>
        (NormalizePage(page), NormalizePageSize(pageSize));
}
