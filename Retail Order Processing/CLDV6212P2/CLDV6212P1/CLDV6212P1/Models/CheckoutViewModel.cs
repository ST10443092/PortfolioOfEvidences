using System.ComponentModel.DataAnnotations;

namespace CLDV6212P1.Models
{
    public class CheckoutViewModel
    {
        [Required, EmailAddress]
        public string CustomerEmail { get; set; }

        [Required]
        public string DeliveryAddress { get; set; }
    }
}
