namespace W3_test.Domain.DTOs
{
	public class CartItemDTO
	{
		public Guid Id { get; set; }
		public Guid CartId { get; set; }
		public Guid BookId { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }
		public BookDTO Book { get; set; } 
		public string BookTitle { get; set; }
	}
}
