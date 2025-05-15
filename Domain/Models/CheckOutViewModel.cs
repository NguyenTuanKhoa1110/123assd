using System.ComponentModel.DataAnnotations;
using W3_test.Domain.Models;
namespace W3_test.Domain.Models
{
    
        public class CheckoutViewModel
        {
           
            public List<OrderItems> Items { get; set; }

            [Display(Name = "Shipping Address")]
            [Required(ErrorMessage = "Shipping address is required.")]
            public string ShippingAddress { get; set; }

            [Display(Name = "Payment Method")]
            [Required(ErrorMessage = "Please select a payment method.")]
            public string PaymentMethod { get; set; }

            public decimal TotalAmount { get; set; }
        }
    }

