using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace XLE_Task_MichałPiotrowski.Controllers {
    public class DataExportController : Controller {
        readonly string fakeJsonUrl = "https://jsonplaceholder.typicode.com/todos/1";

        public async Task<FileResult> Index() {
            byte[] res;
            using(WebClient client = new()) {
                res = await client.DownloadDataTaskAsync(fakeJsonUrl);
            }
            System.Diagnostics.Debug.WriteLine(Directory.GetCurrentDirectory());

            var date = DateTime.Now.ToString();
            date = date.Trim();
            string path = $"WeatherData{date}";

            return File(res, "application/json", path);
        }
    }
}
