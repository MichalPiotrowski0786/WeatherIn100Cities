using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using XLE_Task_MichałPiotrowski.Models;

namespace XLE_Task_MichałPiotrowski.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;
        IList<FinalModel> viewModel;

        public HomeController(ILogger<HomeController> logger) {
            _logger = logger;
        }

        public async Task<string> GetDataFromCountriesAPI() {
            using(WebClient client = new()) { // use WebClient to get response from countries api
                string link = new("https://countriesnow.space/api/v0.1/countries");
                try {
                    var res = await client.DownloadStringTaskAsync(link);
                    return res;
                } catch(Exception e) {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    return null;
                }
            }
        }

        public string[] DeserializedCountriesResponse(string body) {
            string stringToSearch = "data"; // search "data" string in response string to locate and prepare JSON 
            int stringToSearchStartIndex = body.IndexOf(stringToSearch) + stringToSearch.Length + 2;
            body = body.Remove(0, stringToSearchStartIndex);
            body = body.Remove(body.Length - 1);

            // deserialize response to ICollection of CountryCitiesModel
            var viewModel = JsonConvert.DeserializeObject<ICollection<CountryCitiesModel>>(body);
            if(viewModel is not null && viewModel.Count > 0) {
                List<string> cityList = new();
                foreach(CountryCitiesModel model in viewModel) {
                    if(model.cities is not null && model.cities.Length > 0) {
                        foreach(string city in model.cities) {
                            if(cityList is not null) {
                                cityList.Add(city);
                            }
                        }
                    }
                }
                if(cityList.Count == 0) {
                    return null;
                } else {
                    return cityList.ToArray();
                }
            } else {
                return null;
            }
        }

        public async Task<FinalModel> GetDataFromWeatherAPI(string city) {
            string body = "";
            string link = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid=75631f06853d699bf264f477854dd2a9&units=metric";
            using(WebClient client = new()) {
                try {
                    body = await client.DownloadStringTaskAsync(link);
                } catch(Exception e) {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    return null;
                }
            }
            string badResponse = "city not found"; // fix it later to "cod":"404"
            if(body.Length == 0 || body.Contains(badResponse)) return null;
            JObject dynamicRes = (JObject)JsonConvert.DeserializeObject(body); // decided to use JObject instead of Model becuase of amount of information that weather api json provides

            float lat = (float)((JObject)dynamicRes.GetValue("coord")).GetValue("lat");
            float lon = (float)((JObject)dynamicRes.GetValue("coord")).GetValue("lon");
            string description = (string)((JObject)dynamicRes.GetValue("weather")[0]).GetValue("description"); // ?? why is this in array
            float temperature = (float)((JObject)dynamicRes.GetValue("main")).GetValue("temp");
            float pressure = (float)((JObject)dynamicRes.GetValue("main")).GetValue("pressure");
            float humidity = (float)((JObject)dynamicRes.GetValue("main")).GetValue("humidity");
            float wind = (float)((JObject)dynamicRes.GetValue("wind")).GetValue("speed");
            string countryCode = (string)((JObject)dynamicRes.GetValue("sys")).GetValue("country");

            return new FinalModel(city, countryCode,lat, lon, description, temperature, pressure, humidity, wind);
        }

        public async Task<IList<FinalModel>> GetFinalModelsIList(string[] cities) {
            //int finalListSize = 100;
            List<FinalModel> finalModelsList = new();
            if(cities is null || cities.Length == 0) return null;

            Random rand = new();
            int counter = 0;
            while(finalModelsList.Count < 100) {
                int randomIndex = rand.Next(0, cities.Length - 1);
                string city = cities[randomIndex];

                var weatherApiResponse = await GetDataFromWeatherAPI(city);

                if(weatherApiResponse is not null) {
                    finalModelsList.Add(weatherApiResponse);
                    counter++;
                }
            }
            return finalModelsList;
        }

        //public ActionResult ExcelExport() {
        //    System.Diagnostics.Debug.WriteLine("EXCEL");
        //    using(var package = new ExcelPackage()) {
        //        var worksheet = package.Workbook.Worksheets.Add("Data");

        //        worksheet.Cells[1, 1].Value = "ID";
        //        worksheet.Cells[1, 2].Value = "Product";
        //        worksheet.Cells[1, 3].Value = "Quantity";
        //        worksheet.Cells[1, 4].Value = "Price";
        //        worksheet.Cells[1, 5].Value = "Value";


        //    }
        //}

        public IActionResult Index() {
            MiniProfiler profiler = MiniProfiler.StartNew();
            viewModel = GetFinalModelsIList(DeserializedCountriesResponse(GetDataFromCountriesAPI().Result)).Result;
            profiler.Stop();

            if(viewModel is not null && viewModel.Count > 0) {
                return View(viewModel);
            } else {
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
