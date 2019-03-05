using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SandulasWebApp.Models;

namespace SandulasWebApp.Controllers
{
	public class MainController : Controller
	{
		public IActionResult About()
		{
			return View();
		}

		public IActionResult Resume()
		{
			ViewData["Message"] = "Under construction";

			return View();
		}

		public IActionResult Chatbot()
		{
			ViewData["Message"] = "Under construction";

			return View();
		}

		public IActionResult Contact()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
