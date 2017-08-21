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
        public IActionResult Index()
        {
            return View();
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

        [HttpPost]
        public async Task<ActionResult> Calculate([FromBody] IList<DataCentre> value)
        {
            if (value == null)
            {
                return BadRequest();
            }
            string _url = ClientConfig.apiserverurl;


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
