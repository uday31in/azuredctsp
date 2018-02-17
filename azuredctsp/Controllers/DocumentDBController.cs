using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights.DataContracts;


namespace azuredctsp.Controllers
{

    public class DocumentDBController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly DocumentDBConfig _config;
        //private IHubContext<Chat> _hub;

        private const int MinThreadPoolSize = 100;
        
        static AzureDocumentDBHelper dbhelper;

        private TelemetryClient telemetry = new TelemetryClient();

        


        //public DocumentDBController(IOptions<DocumentDBConfig> config, IHostingEnvironment hostingEnvironment, IHubContext<Chat> hub)
        public DocumentDBController(IOptions<DocumentDBConfig> config, IHostingEnvironment hostingEnvironment)
        {
            this._config = config.Value;
            this._hostingEnvironment = hostingEnvironment;
            //this._hub = hub;

            dbhelper = new AzureDocumentDBHelper(config, hostingEnvironment, "uday");

            ThreadPool.SetMinThreads(MinThreadPoolSize, MinThreadPoolSize);
            
        }

        // GET: DocumentDB
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            var response = new Dictionary<string, string>() { { "Endpoint URL", _config.EndPointUrl }  };

           
            var model = new DocumentDBControllerModel
            {
                Guid = Guid.NewGuid().ToString(),
                
            };
            
            return View(model);
            
        }

        [HttpGet]
        public async Task<ActionResult> StartAsync(string id, CancellationToken cancellationToken)
        {   
            await dbhelper.RunAsync(id, cancellationToken);

            return new JsonStringResult(JsonConvert.SerializeObject("[]", Formatting.Indented));
            //var response = new Dictionary<string, string>() { { "FromGetData:", id.ToString() } };

            //var text = JsonConvert.SerializeObject(response, Formatting.Indented);
            //return text;
        }

        [HttpGet]
        public async Task<ActionResult> GetTimeSeries(string id, CancellationToken cancellationToken)
        {
            
            List<DocumentDBStatus> timeseries = new List<DocumentDBStatus>();
            timeseries =  dbhelper.GetTimeSeries(id);
            
            if (timeseries!=null)
            {
                var metric = new MetricTelemetry();
                metric.Name = "RUs";
                metric.Sum = timeseries[0].rus;
                telemetry.TrackMetric(metric);
                
                return new JsonStringResult(JsonConvert.SerializeObject(timeseries, Formatting.Indented));
            }
            else
            {
                return new JsonStringResult("[]");
            }
           
        }
    }   
}