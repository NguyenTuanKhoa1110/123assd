using System.ComponentModel.DataAnnotations;
using W3_test.Data.Entities;
namespace W3_test.Data.Entities
{
	public class OrderEntity
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public DateTime OrderDate { get; set; }
		
		public string Status { get; set; }
		public string ShippingAddress { get; set; }
		public string PaymentMethod { get; set; }
		public decimal TotalAmount { get; set; }
		public List<OrderItemEntity> Items { get; set; }
		
	}
}

