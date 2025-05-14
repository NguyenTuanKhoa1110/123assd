

namespace W3_test.Domain.Models
{
	public class Order
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal TotalAmount { get; set; }
		public List<OrderItems> Items { get; set; }
	}
}

