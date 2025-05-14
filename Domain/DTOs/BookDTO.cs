using System.Collections.Generic;
using W3_test.Data.Entities;
using W3_test.Domain.Models;
namespace W3_test.Domain.DTOs
{
	public class BookDTO
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Author { get; set; }
		public decimal Price { get; set; }
		public int Stock { get; set; }
		public string Category { get; set; }
		public string Description { get; set; }
		public string ImageUrl { get; set; }


	}
}
