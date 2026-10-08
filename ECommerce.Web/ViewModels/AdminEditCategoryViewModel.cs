using ECommerce.Web.DTOs;

namespace ECommerce.Web.ViewModels;

public class AdminEditCategoryViewModel
{
    public int Id { get; set; }

    public AdminUpdateCategoryDto Category { get; set; } = new();
}