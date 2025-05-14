using System.ComponentModel.DataAnnotations;

namespace W3_test.Data.Entities
{
	public class BookEntity
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Title { get; set; }

		[Required]
		[StringLength(50)]
		public string Author { get; set; }

		[Required]
		public decimal Price { get; set; }

		[Required]
		public int Stock { get; set; }
		[Required]
		public string Category { get; set; }
		public string Description { get; set; }
		public string ImageUrl { get; set; }
		public ICollection<CartItemEntity> CartItems { get; set; } = new List<CartItemEntity>();
		public ICollection<OrderItemEntity> OrderItems { get; set; } = new List<OrderItemEntity>();
		
	}
}
