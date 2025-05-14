using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using W3_test.Domain.Models;

namespace W3_test.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Error()
		{
			return View();
		}
	}
}
