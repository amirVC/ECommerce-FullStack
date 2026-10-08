namespace ECommerce.Web.DTOs;

public class AdminReviewDto : ReviewDto
{
    public string Status { get; set; } = "Approved";
    public string ProductName { get; set; } = string.Empty;
}

public class AdminUpdateReviewStatusDto
{
    public string Status { get; set; } = string.Empty;
}