using ECommerce.Web.DTOs;

namespace ECommerce.Web.ViewModels;

public class AdminEditProductViewModel
{
    public int Id { get; set; }

    public string OriginalSlug { get; set; } = string.Empty;

    public AdminUpdateProductDto Product { get; set; } = new();
    public IFormFile? ImageFile { get; set; }
    public List<CategoryLookupDto> Categories { get; set; } = new();
    public List<ProductImageViewModel> Images { get; set; } = new();
}