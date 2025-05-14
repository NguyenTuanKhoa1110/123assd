using System.ComponentModel.DataAnnotations;

namespace W3_test.Data.Entities
{
	public class CartEntity
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid UserId { get; set; } 

		public ICollection<CartItemEntity> Items { get; set; } = new List<CartItemEntity>();
		
	}
}
