using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using XLE_Task_MichałPiotrowski.Models;

namespace XLE_Task_MichałPiotrowski.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;
        const string countryApiLink = "https://countriesnow.space/api/v0.1/countries";
        protected const string WEATHER_API_KEY = "75631f06853d699bf264f477854dd2a9";

        public HomeController(ILogger<HomeController> logger) {
            _logger = logger;
        }

        public IActionResult Index() {
            var MainViewModelList = GetFinalModelsIList(DeserializedCountriesResponse(GetDataFromCountriesAPI().Result)).Result;

            if(MainViewModelList is not null && MainViewModelList.Count > 0) {
                try {
                    // return list of FinalModels to TempData, to use it later in downloading etc.
                    TempData["list"] = JsonConvert.SerializeObject(MainViewModelList.ToArray());
                } catch(Exception e) {
                    Debug.WriteLine(e.Message);
                }
                return View(MainViewModelList);
            } else {
                // if list of FinalModels is non-existent or empty, return error page
                return RedirectToAction(nameof(Error));
            }
        }

        public async Task<string> GetDataFromCountriesAPI() {
            using(WebClient client = new()) { 
                // use WebClient to get response from countries api
                try {
                    var res = await client.DownloadStringTaskAsync(countryApiLink);
                    return res;
                } catch(Exception e) {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    return null;
                }
            }
        }

        public string[] DeserializedCountriesResponse(string body) {
            string stringToSearch = "data"; // search "data" string in response string to locate and prepare JSON for deserialization
            int stringToSearchStartIndex = body.IndexOf(stringToSearch) + stringToSearch.Length + 2;
            body = body.Remove(0, stringToSearchStartIndex);
            body = body.Remove(body.Length - 1);

            // deserialize response to ICollection of CountryCitiesModel
            var countryModelList = JsonConvert.DeserializeObject<ICollection<CountryCitiesModel>>(body);
            if(countryModelList is not null && countryModelList.Count > 0) {
                List<string> cityList = new();
                foreach(CountryCitiesModel model in countryModelList) {
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
            string weatherApiLink = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={WEATHER_API_KEY}&units=metric";
            using(WebClient client = new()) {
                try {
                    body = await client.DownloadStringTaskAsync(weatherApiLink);
                } catch(Exception e) {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    return null;
                }
            }
            string badResponse = "\"cod\":\"404\"";
            if(body.Length == 0 || body.Contains(badResponse)) return null;
            // decided to use JObject instead of Model becuase of amount
            // of information that weather api json provides
            JObject dynamicRes = (JObject)JsonConvert.DeserializeObject(body);

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

        public async Task<List<FinalModel>> GetFinalModelsIList(string[] cities) {
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

        public async Task<FileResult> Download() {
            FinalModel[] list;
            if(TempData["list"] is not null) {
                // deserialize json object and make it array again
                list = JsonConvert.DeserializeObject<FinalModel[]>(TempData["list"].ToString());
                Debug.WriteLine(TempData["list"]);
            } else return null;

            if(list is not null && list.Length > 0) {
                DataTable Dt = new();
                Dt.Columns.Add("City", typeof(string));
                Dt.Columns.Add("Country", typeof(string));
                Dt.Columns.Add("Description", typeof(string));
                Dt.Columns.Add("Temperature", typeof(float));
                Dt.Columns.Add("Pressure", typeof(float));
                Dt.Columns.Add("Humidity", typeof(float));
                Dt.Columns.Add("Wind", typeof(float));

                foreach(var data in list) {
                    DataRow row = Dt.NewRow();
                    row[0] = data.City;
                    row[1] = data.CountryCode;
                    row[2] = data.Description;
                    row[3] = data.Temperature;
                    row[4] = data.Pressure;
                    row[5] = data.Humidity;
                    row[6] = data.Wind;
                    Dt.Rows.Add(row);
                }

                byte[] res;
                using(ExcelPackage package = new()) {
                    var sheet = package.Workbook.Worksheets.Add("Sheet0");

                    sheet.Cells.LoadFromDataTable(Dt,true);
                    sheet.Row(1).Style.Font.Bold = true;

                    res = await package.GetAsByteArrayAsync();
                }

                // prepare date to use it in file name
                var date = DateTime.Now.ToString();
                date = date.Replace(".", string.Empty);
                date = date.Replace(":", string.Empty);
                date = date.Replace(" ", "_");

                // System.Diagnostics.Debug.WriteLine(date);
                string mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string format = "xlsx";
                string name = $"WeatherData{date}.{format}";

                return File(res, mime, name);
            } else return null;
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
