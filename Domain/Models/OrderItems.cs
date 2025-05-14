namespace W3_test.Domain.Models
{
	public class OrderItems
	{
		public Guid Id { get; set; }
		public Guid OrderId { get; set; }
		public int BookId { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }
	}
}
