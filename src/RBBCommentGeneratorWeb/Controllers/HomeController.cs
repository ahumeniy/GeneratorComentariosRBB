using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using RBBCommentGeneratorWeb.Util;

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
            ViewBag.Frase = ObtenerFrase();
            return View();
        }

        private string ObtenerFrase()
        {
            var path = System.IO.Path.Combine(_hostEnvironment.WebRootPath, "data", "frases.json");
            var file = System.IO.File.ReadAllText(path);
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.FrasesViewModel>(file);

            string frase = GetRandomValue(data.frases);

            frase = ReplaceFrase(frase, data);

            return frase;
        }

        [HttpGet]
        [NoCache]
        public IActionResult Obtener()
        {
            return Json(ObtenerFrase());
        }

        private string ReplaceFrase(string frase, Models.FrasesViewModel frases)
        {
            var regx = new System.Text.RegularExpressions.Regex("%[1-5]");
            var match = regx.Match(frase);
            if (match.Success)
            {
                frase = frase.Remove(match.Index, 2);
                string toReplace = "";
                if (match.Value == "%1") toReplace = GetRandomValue(frases.arg1);
                if (match.Value == "%2") toReplace = GetRandomValue(frases.arg2);
                if (match.Value == "%3") toReplace = GetRandomValue(frases.arg3);
                if (match.Value == "%4") toReplace = GetRandomValue(frases.arg4);
                if (match.Value == "%5") toReplace = GetRandomValue(frases.arg5);
                frase = frase.Insert(match.Index, toReplace);
                return ReplaceFrase(frase, frases);
            }
            else
            {
                return frase;
            }
        }

        private string GetRandomValue(string[] items)
        {
            return items[ThreadLocalRandom.Next(0, items.Length)];
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
