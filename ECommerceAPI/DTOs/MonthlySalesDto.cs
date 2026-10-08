namespace ECommerceAPI.DTOs;

public class MonthlySalesDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public int OrdersCount { get; set; }

    public decimal Revenue { get; set; }
}