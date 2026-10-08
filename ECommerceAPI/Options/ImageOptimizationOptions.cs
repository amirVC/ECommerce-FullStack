namespace ECommerceAPI.Options;

public class ImageOptimizationOptions
{
    public int MaxWidth { get; set; } = 1600;
    public int MaxHeight { get; set; } = 1600;
    public int Quality { get; set; } = 80;

    public int ThumbnailWidth { get; set; } = 300;
    public int ThumbnailHeight { get; set; } = 300;
    public int ThumbnailQuality { get; set; } = 75;
}