namespace ECommerce.Web.DTOs;
public class AdminLowStockProductDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Stock { get; set; }

    public string CategoryName { get; set; } = string.Empty;
}