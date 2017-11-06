using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Logging;

namespace azuredctsp
{

    public class ClientConfig
    {
        public  string apiserverurl { get; set; }
    }

    public class Program
    {

        public static void Main(string[] args)
        {
            // https://github.com/aspnet/MetaPackages/issues/221

            var commandConfig = new ConfigurationBuilder()
               .AddCommandLine(args)
               .Build();

            WebHost.CreateDefaultBuilder(args)
                 .UseConfiguration(commandConfig)
                 .UseStartup<Startup>()                 
                 .Build()
                 .Run();
        }
    }
}
