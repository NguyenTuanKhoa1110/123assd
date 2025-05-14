using System.ComponentModel.DataAnnotations;

namespace W3_test.Data.Entities
{
	public class OrderItemEntity
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid OrderId { get; set; }

		[Required]
		public Guid BookId { get; set; }

		[Required]
		public int Quantity { get; set; }

		[Required]
		public decimal Price { get; set; }

		public OrderEntity Order { get; set; }
		public BookEntity Book { get; set; }
		
	}
}

