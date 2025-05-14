namespace W3_test.Domain.Models
{
	public class CartItems
	{
		public Guid Id { get; set; }
		public Guid CartId { get; set; }
		public int BookId { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }
	}
}
