using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs
{
    public class AddToCartDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "ProductId is required.")]
        public int ProductId { get; set; }

        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000.")]
        public int Quantity { get; set; } = 1;
    }
}