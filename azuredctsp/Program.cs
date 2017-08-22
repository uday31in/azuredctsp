using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace azuredctsp
{
    public class Program
    {
        public static class ClientConfig
        {
            public static string apiserverurl;
            public static string port;

        }


        public static void Main(string[] args)
        {
            Dictionary<string, string> switchMappings = new Dictionary<string, string>
            {
                {"-apiserver", "apiserver" },
                {"-port", "port"}
           
            };


            var configuration = new ConfigurationBuilder()
                                     .AddJsonFile("appsettings.json", optional: true)
                                     .AddEnvironmentVariables("azuredctsp_")
                                     .AddCommandLine(args, switchMappings)
                                     .Build();

            ClientConfig.apiserverurl = configuration["apiserver"];
            ClientConfig.port = (configuration["port"] != null) ? (configuration["port"]) : "5000";



            Console.WriteLine("Using Server Listen URL http://*:{0}", ClientConfig.port);
            Console.WriteLine("using api server url: {0}", ClientConfig.apiserverurl);


            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .UseUrls(string.Format("http://*:{0}", ClientConfig.port))
                .Build();

            host.Run();
        }
    }
}
