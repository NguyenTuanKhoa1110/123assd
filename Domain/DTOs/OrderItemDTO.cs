namespace W3_test.Domain.DTOs;

public class OrderItemDTO
{
	public Guid Id { get; set; }
	public Guid OrderId { get; set; }
	public Guid BookId { get; set; }
	public BookDTO Book { get; set; }
	public int Quantity { get; set; }
	public decimal Price { get; set; }
}