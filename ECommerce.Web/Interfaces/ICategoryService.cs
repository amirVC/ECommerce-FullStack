using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryLookupDto>> GetAllAsync();
}