using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static azuredctsp.Program;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Options;

namespace azuredctsp.Controllers
{

    public class JsonStringResult : ContentResult
    {
        public JsonStringResult(string json)
        {
            Content = json;
            ContentType = "application/json";
        }
    }
    public class DataCentre
    {
        public string displayName { get; set; }
        public string id { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string name { get; set; }
        public string subscriptionId { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ClientConfig _config;

        public HomeController(IOptions<ClientConfig> config, IHostingEnvironment hostingEnvironment)
        {
            _config = config.Value;
            _hostingEnvironment = hostingEnvironment;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult Locations()
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string contentRootPath = _hostingEnvironment.ContentRootPath;


            string allText = System.IO.File.ReadAllText(Path.Combine(webRootPath, "azurelocations.json"));

            
            object jsonObject = JsonConvert.DeserializeObject(allText);
            return Json(jsonObject);

        }

        public IActionResult Error()
        {
            return View();
            
        }

        [HttpPost]
        public async Task<ActionResult> Calculate([FromBody] IList<DataCentre> value)
        {
            if (value == null)
            {
                return BadRequest();
            }
            string _url = _config.apiserverurl;

            var request = JsonConvert.SerializeObject(value);

            var response = await GetData(_url, request);


            return new JsonStringResult(response);
        }


        public static async Task<string> GetData(string url, string data)
        {
            HttpClient client = new HttpClient();
            StringContent queryString = new StringContent(data);

            var content = new StringContent(data, Encoding.UTF8, "application/json");
            //var result = client.PostAsync(url, content).Result;

            HttpResponseMessage response = await client.PostAsync(new Uri(url), content);

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }
    }
}
