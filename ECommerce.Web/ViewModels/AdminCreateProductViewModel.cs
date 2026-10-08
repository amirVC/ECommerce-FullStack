using ECommerce.Web.DTOs;

namespace ECommerce.Web.ViewModels;

public class AdminCreateProductViewModel
{
    public AdminCreateProductDto Product { get; set; }
        = new();

    public IFormFile? ImageFile { get; set; }

    public List<CategoryLookupDto> Categories { get; set; }
        = new();

    public List<IFormFile>? GalleryImages { get; set; }

    public List<string>? GalleryAltTexts { get; set; }
}