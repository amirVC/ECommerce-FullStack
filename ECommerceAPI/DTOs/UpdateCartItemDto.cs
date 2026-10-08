using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs
{
    public class UpdateCartItemDto
    {
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000.")]
        public int Quantity { get; set; }
    }
}