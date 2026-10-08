namespace ECommerceAPI.QueryParameters;

public class AuditLogQueryParameters
{
    public int Page { get; set; } = 1;

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, 100);
    }

    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public int? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}