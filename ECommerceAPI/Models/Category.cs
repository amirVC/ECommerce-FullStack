namespace ECommerceAPI.Models
{
    public class Category
    {
        public int Id { get; set; }

        // Public API identifier.
        public Guid PublicId { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}