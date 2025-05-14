namespace W3_test.Domain.Models
{
	public class Cart
	{
		
			public Guid Id { get; set; }
			public Guid UserId { get; set; }
			public List<CartItems> Items { get; set; }
			 
		}
	}


