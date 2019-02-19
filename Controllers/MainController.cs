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
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "About me page description";

            return View();
        }

		public IActionResult Resume()
		{
			ViewData["Message"] = "Resume page description";

			return View();
		}

		public IActionResult Chatbot()
		{
			ViewData["Message"] = "Chatbot page description";

			return View();
		}

		public IActionResult Contact()
        {
            ViewData["Message"] = "Contact page description";

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
