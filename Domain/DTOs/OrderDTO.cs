namespace W3_test.Domain.DTOs;

public class OrderDTO
{
	public Guid Id { get; set; }
	
	public AppUserDTO User { get; set; }
	public DateTime OrderDate { get; set; }
	public decimal TotalAmount { get; set; }
	public string Status { get; set; }
	public string ShippingAddress { get; set; }
	public string PaymentMethod { get; set; }
	public List<OrderItemDTO> OrderItems { get; set; }
}