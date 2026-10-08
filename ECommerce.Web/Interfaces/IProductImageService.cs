using ECommerce.Web.ViewModels;

namespace ECommerce.Web.Interfaces;

public interface IProductImageService
{
    Task<List<ProductImageViewModel>> GetImagesAsync(int productId);

    Task AddImageAsync(int productId, CreateProductImageViewModel model);

    Task DeleteImageAsync(int imageId);

    Task SetPrimaryAsync(int imageId);
}