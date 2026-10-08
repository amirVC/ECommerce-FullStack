using ECommerce.Web.DTOs;

namespace ECommerce.Web.ViewModels;

public class AdminCreateCategoryViewModel
{
    public AdminCreateCategoryDto Category { get; set; } = new();
}