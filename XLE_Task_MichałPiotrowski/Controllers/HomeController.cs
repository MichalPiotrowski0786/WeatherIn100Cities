using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using XLE_Task_MichałPiotrowski.Models;

namespace XLE_Task_MichałPiotrowski.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) {
            _logger = logger;
        }

        //public IActionResult Index() {
        //    return View();
        //}

        public IEnumerable<ToDo> GetDataFromAPI() {
            string response = "";
            using(WebClient client = new()) {
                response = client.DownloadString("https://jsonplaceholder.typicode.com/todos");
            }
            var viewModel = JsonConvert.DeserializeObject<IEnumerable<ToDo>>(response);

            return viewModel;
        }

        public IActionResult Index() {
            var vm = GetDataFromAPI();
            return View(vm);
        }

        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
