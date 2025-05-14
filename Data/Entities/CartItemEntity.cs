using System.ComponentModel.DataAnnotations;

namespace W3_test.Data.Entities
{
	
		public class CartItemEntity
		{
			[Key]
			public Guid Id { get; set; }

			[Required]
			public Guid CartId { get; set; }

			[Required]
			public Guid BookId { get; set; }

			[Required]
			public int Quantity { get; set; }

			[Required]
			public decimal Price { get; set; }

			public CartEntity Cart { get; set; }
			public BookEntity Book { get; set; }
			
	}
	}

