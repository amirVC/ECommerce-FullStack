using Microsoft.AspNetCore.Http;

namespace ECommerce.Web.ViewModels;

public class AdminCreateProductImageViewModel
{
    public IFormFile ImageFile { get; set; } = null!;

    public string AltText { get; set; } = string.Empty;
}