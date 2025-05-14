using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
namespace W3_test.Domain.Models
{
	public class EditUserViewModel
	{
		public Guid Id { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }

        public string SelectedRole { get; set; }

        public List<SelectListItem> AllRoles { get; set; } = new();
	}

}
