using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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

        //public ICollection<CountryCitiesModel> GetDataFromCountriesAPI() {
        //    string response = "";
        //    using(WebClient client = new()) {
        //        response = client.DownloadString("https://countriesnow.space/api/v0.1/countries");
        //    }
        //    string stringToSearch = "data";
        //    int stringToSearchStartIndex = response.IndexOf(stringToSearch)+stringToSearch.Length+2;
        //    response = response.Remove(0,stringToSearchStartIndex);
        //    response = response.Remove(response.Length - 1);

        //    var viewModel = JsonConvert.DeserializeObject<ICollection<CountryCitiesModel>>(response);
        //    return viewModel;
        //}

        public string[] GetDataFromCountriesAPI() {
            string response = "";
            using(WebClient client = new()) {
                response = client.DownloadString("https://countriesnow.space/api/v0.1/countries");
            }
            string stringToSearch = "data";
            int stringToSearchStartIndex = response.IndexOf(stringToSearch) + stringToSearch.Length + 2;
            response = response.Remove(0, stringToSearchStartIndex);
            response = response.Remove(response.Length - 1);

            var viewModel = JsonConvert.DeserializeObject<ICollection<CountryCitiesModel>>(response);
            if(viewModel is not null && viewModel.Count > 0) {
                List<string> cityList = new();
                foreach(CountryCitiesModel model in viewModel) {
                    if(model.cities is not null && model.cities.Length > 0) {
                        foreach(string city in model.cities) {
                            if(cityList is not null) cityList.Add(city);
                        }
                    }
                }
                if(cityList.Count > 0) {
                    return cityList.ToArray();
                } else {
                    return null;
                }
            } else {
                return null;
            }
        }

        public FinalModel GetDataFromWeatherAPI(string city) {
            string response = "";
            using(WebClient client = new()) {
                response = client.DownloadString($"https://api.openweathermap.org/data/2.5/weather?q={city}&appid=75631f06853d699bf264f477854dd2a9&units=metric");
            }
            string badResponse = "city not found"; // fix it later to "cod":"404"
            if(response.Length == 0 || response.Contains(badResponse)) return null;
            JObject dynamicRes = (JObject)JsonConvert.DeserializeObject(response); // decided to use JObject instead of Model becuase of amount of information that weather api json provides

            float lat = (float)((JObject)dynamicRes.GetValue("coord")).GetValue("lat");
            float lon = (float)((JObject)dynamicRes.GetValue("coord")).GetValue("lon");
            string description = (string)((JObject)dynamicRes.GetValue("weather")[0]).GetValue("description"); // ?? why is this in array
            float temperature = (float)((JObject)dynamicRes.GetValue("main")).GetValue("temp");
            float pressure = (float)((JObject)dynamicRes.GetValue("main")).GetValue("pressure");
            float humidity = (float)((JObject)dynamicRes.GetValue("main")).GetValue("humidity");
            float wind = (float)((JObject)dynamicRes.GetValue("wind")).GetValue("speed");

            // System.Diagnostics.Debug.WriteLine("!------------------------------: "+dynamicRes.GetValue("coord"));
            return new FinalModel(city,lat, lon, description, temperature, pressure, humidity, wind);
        }

        public IActionResult Index() {
            //var vm = GetDataFromCountriesAPI();
            var vm = GetDataFromWeatherAPI("Gdynia");
            if(vm is not null) {
                return View(vm);
            }else {
                return RedirectToAction(nameof(Error));
            }
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
