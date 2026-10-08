namespace ECommerceAPI.DTOs;

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages =>
        (int)Math.Ceiling((double)TotalCount / PageSize);
}