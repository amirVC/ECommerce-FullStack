namespace ECommerceAPI.Models
{
    public class Category
    {
        public int Id { get; set; }

        public Guid PublicId { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}