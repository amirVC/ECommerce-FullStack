using Microsoft.AspNetCore.Http;

namespace ECommerceAPI.DTOs;

public class AdminCreateProductImageDto
{
    public IFormFile ImageFile { get; set; } = null!;

    public string AltText { get; set; } = string.Empty;
}