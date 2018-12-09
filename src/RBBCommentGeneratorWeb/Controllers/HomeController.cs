using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using RBBCommentGeneratorWeb.Util;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace RBBCommentGeneratorWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostEnvironment;
        private readonly IMemoryCache _memoryCache;

        public HomeController(IHostingEnvironment hostEnvironment, IMemoryCache memoryCache)
        {
            _hostEnvironment = hostEnvironment;
            _memoryCache = memoryCache;
        }

        public async Task<IActionResult> Index(string Id = null)
        {
            string frase = null;

            if (!string.IsNullOrWhiteSpace(Id))
            {
                byte[] fraseZip = null;
                var mcKey = $"surl_{Id}";
                if (_memoryCache.TryGetValue(mcKey, out fraseZip))
                {
                    frase = TextUtilities.Unzip(fraseZip);

                    await StorageClient.SaveShorten(Id, frase);

                    _memoryCache.Remove(mcKey);
                }
                else
                {
                    frase = await StorageClient.GetForShorten(Id);
                }
            }

            if (frase == null)
                frase = ObtenerFrase();
            else
            {
                ViewBag.Surl = Id;
            }

            ViewBag.FraseShort = System.Convert.ToBase64String(TextUtilities.Zip(frase));

            ViewBag.Frase = frase;

            return View();
        }

        private string ObtenerFrase()
        {
            var path = System.IO.Path.Combine(_hostEnvironment.WebRootPath, "data", "frases.json");
            var file = System.IO.File.ReadAllText(path);
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.FrasesViewModel>(file);

            string frase = GetRandomValue(data.frases);

            frase = ReplaceFrase(frase, data);

            frase = DeterminarGenero(frase);

            return frase;
        }

        private string DeterminarGenero(string frase)
        {
            var neutral = new Regex("un@");
            var loc = neutral.Match(frase);
            if (!loc.Success)
                return frase;

            var siguiente = new Regex(@"[\s,\.]");
            var match = siguiente.Match(frase, loc.Index + 3);

            if (!match.Success)
                return neutral.Replace(frase, "un");

            var rgxFem = new Regex(@".*[a].?$");
            var esFemenino = rgxFem.Match(frase.Substring(loc.Index + 3, match.Index - loc.Index + 3));

            if (esFemenino.Success)
                frase = neutral.Replace(frase, "una");
            else
                frase = neutral.Replace(frase, "un");

            return DeterminarGenero(frase);
        }

        [HttpGet]
        [NoCache]
        public IActionResult Obtener()
        {
            var response = new FraseResponse(ObtenerFrase());
            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> Surl(string fraseB64)
        {
            var b64 = System.Convert.FromBase64String(fraseB64);
            var frase = Util.TextUtilities.Unzip(b64);

            var existente = await StorageClient.GetShortFor(frase);

            if (existente != null)
                return Json(existente);

            var dtNow = DateTime.UtcNow;
            var dtOrigin = new DateTime(1986, 8, 1);
            var diff = dtNow - dtOrigin;
            var diffmsec = BitConverter.GetBytes((uint)Math.Round(diff.TotalMilliseconds)).Reverse().ToArray();

            var sversion = System.Convert.ToBase64String(diffmsec);

            _memoryCache.Set($"surl_{sversion}", b64, TimeSpan.FromHours(1));

            return Json(sversion);
        }

        private string ReplaceFrase(string frase, Models.FrasesViewModel frases)
        {
            var regx = new Regex("%[a-z]*%");
            var match = regx.Match(frase);
            if (match.Success)
            {
                string toReplace = "";
                if (match.Value == "%insulto%") toReplace = GetRandomValue(frases.insulto);
                if (match.Value == "%consejo%") toReplace = GetRandomValue(frases.consejo);
                if (match.Value == "%personaje%") toReplace = GetRandomValue(frases.personaje);
                if (match.Value == "%accion%") toReplace = GetRandomValue(frases.accion);
                if (match.Value == "%cierre%") toReplace = GetRandomValue(frases.cierre);
                frase = regx.Replace(frase, toReplace, 1);
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
