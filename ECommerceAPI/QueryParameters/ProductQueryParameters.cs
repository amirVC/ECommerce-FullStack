namespace ECommerceAPI.Models.QueryParameters;

public class ProductQueryParameters
{
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public string? SortBy { get; set; }

    public int Page { get; set; } = 1;

    private int _pageSize = 9;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, 50);
    }
}