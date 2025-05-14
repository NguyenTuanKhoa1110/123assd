namespace W3_test.Domain.DTOs
{
	public class CartDTO
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public List<CartItemDTO> Items { get; set; }
	}
}
