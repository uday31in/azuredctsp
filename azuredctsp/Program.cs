using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore;
using System;
using System.Linq;

namespace azuredctsp
{

    public class ClientConfig
    {
        public string apiserverurl { get; set; }
    }

    public class DocumentDBConfig
    {
        public string EndPointUrl { get; set; }
        public string AuthorizationKey { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public int CollectionThroughput { get; set; }
        public bool ShouldCleanupOnStart { get; set; }
        public bool ShouldCleanupOnFinish { get; set; }
        public int DegreeOfParallelism { get; set; }
        public string NumberOfDocumentsToInsert { get; set; }
        public string CollectionPartitionKey { get; set; }
        public string DocumentTemplateFile { get; set; }
    }


    public class Program
    {

        public static void Main(string[] args)
        {
            // https://github.com/aspnet/MetaPackages/issues/221

            var builder = new ConfigurationBuilder()
               .AddCommandLine(args)
               .AddEnvironmentVariables()
               .Build();

            WebHost.CreateDefaultBuilder(args)
                 .UseConfiguration(builder)
                 .UseStartup<Startup>()                                
                 .Build()
                 .Run();
        }
    }
}
