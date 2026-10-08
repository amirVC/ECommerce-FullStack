using ECommerce.Web.Common;

namespace ECommerce.Web.ViewModels;

public class ProductCatalogViewModel
{
    public PagedResult<ProductViewModel> Products { get; set; }
        = new();

    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public string? SortBy { get; set; }

    public HashSet<int> WishlistedProductIds { get; set; } = new();
}