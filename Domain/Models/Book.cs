using System.ComponentModel.DataAnnotations;

namespace W3_test.Domain.Models
{
	public class Book
	{

		public Guid Id { get; set; }

		public string Title { get; set; }

		public string Author { get; set; }

		public string Description { get; set; }

		public decimal Price { get; set; }

		public string ImageUrl { get; set; }

		public int Stock { get; set; }

		public string Category { get; set; }
		//public byte[] RowVersion { get; set; }
	}
}
