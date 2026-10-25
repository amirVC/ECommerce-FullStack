using ECommerce.Web.Common;
using ECommerce.Web.ViewModels;

namespace ECommerce.Web.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductViewModel>> GetProductsAsync(
        int page = 1,
        string? search = null,
        int? categoryId = null,
        string? sortBy = null);

    Task<ProductViewModel?> GetProductByIdAsync(int id);

    Task<ProductViewModel?> GetProductBySlugAsync(string slug);

    Task<List<ProductViewModel>> GetRelatedProductsAsync(int productId);
}