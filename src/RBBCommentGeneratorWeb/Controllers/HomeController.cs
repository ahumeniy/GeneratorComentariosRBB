using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace RBBCommentGeneratorWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostEnvironment;

        public HomeController(IHostingEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Obtener()
        {
            var path = System.IO.Path.Combine(_hostEnvironment.WebRootPath, "data", "frases.json");
            var file = System.IO.File.ReadAllText(path);
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.FrasesViewModel>(file);

            string frase = GetRandomValue(data.frases)
            .Replace("%1", GetRandomValue(data.arg1))
            .Replace("%2", GetRandomValue(data.arg2))
            .Replace("%3", GetRandomValue(data.arg3))
            .Replace("%4", GetRandomValue(data.arg4))
            .Replace("%5", GetRandomValue(data.arg5));

            return Json(frase);
        }

        private string GetRandomValue(string[] items)
        {
            var rnd = new Random();
            return items[rnd.Next(0, items.Length)];
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
