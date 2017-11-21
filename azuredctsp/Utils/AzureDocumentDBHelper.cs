using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace azuredctsp
{
    public class AzureDocumentDBHelper
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly DocumentDBConfig _config;
        //private IHubContext<Chat> _hub;

        private int pendingTaskCount;
        private long documentsInserted;
        private ConcurrentDictionary<int, double> requestUnitsConsumed = new ConcurrentDictionary<int, double>();
        
        private DocumentClient client;
        private static Dictionary<string, List<DocumentDBStatus>> performacnemetrics = new Dictionary<string, List<DocumentDBStatus>>();

        public string test;

        private static readonly ConnectionPolicy ConnectionPolicy = new ConnectionPolicy
        {
            ConnectionMode = ConnectionMode.Gateway,
            ConnectionProtocol = Protocol.Tcp,
            RequestTimeout = new TimeSpan(1, 0, 0),
            MaxConnectionLimit = 1000,
            RetryOptions = new RetryOptions
            {
                MaxRetryAttemptsOnThrottledRequests = 10,
                MaxRetryWaitTimeInSeconds = 60
            }
        };

        //public AzureDocumentDBHelper(IOptions<DocumentDBConfig> config, IHostingEnvironment hostingEnvironment, IHubContext<Chat> hub)
        public AzureDocumentDBHelper(IOptions<DocumentDBConfig> config, IHostingEnvironment hostingEnvironment, string test)
        {
            this._config = config.Value;
            this._hostingEnvironment = hostingEnvironment;
            //this._hub = hub;
            this.test = test;
            
           
            client = new DocumentClient(new Uri(_config.EndPointUrl), _config.AuthorizationKey, ConnectionPolicy);
            
        }

        public List<DocumentDBStatus> GetTimeSeries(string guid)
        {
            List<DocumentDBStatus> toReturn = new List<DocumentDBStatus>();

            if (performacnemetrics.ContainsKey(guid))
            {
                lock (performacnemetrics)
                {
                    return performacnemetrics[guid];

                }
            }
            else
                return null;
            
        }

        public async Task RunAsync(string guid, CancellationToken cancellationToken)
        {
            
            DocumentCollection dataCollection = GetCollectionIfExists(_config.DatabaseName, _config.CollectionName);
            int currentCollectionThroughput = 0;

            performacnemetrics.Add(guid, new List<DocumentDBStatus>());

            if (_config.ShouldCleanupOnStart || dataCollection == null)
            {
                Database database = GetDatabaseIfExists(_config.DatabaseName);
                if (database != null)
                {
                    await client.DeleteDatabaseAsync(database.SelfLink);
                }

                Console.WriteLine("Creating database {0}", _config.DatabaseName);
                database = await client.CreateDatabaseAsync(new Database { Id = _config.DatabaseName });

                Console.WriteLine("Creating collection {0} with {1} RU/s", _config.CollectionName, _config.CollectionThroughput);
                dataCollection = await this.CreatePartitionedCollectionAsync(_config.DatabaseName, _config.CollectionName);

                currentCollectionThroughput = _config.CollectionThroughput;
            }
            else
            {
                OfferV2 offer = (OfferV2)client.CreateOfferQuery().Where(o => o.ResourceLink == dataCollection.SelfLink).AsEnumerable().FirstOrDefault();
                currentCollectionThroughput = offer.Content.OfferThroughput;

                Console.WriteLine("Found collection {0} with {1} RU/s", _config.CollectionName, currentCollectionThroughput);
            }

            int taskCount;
            int degreeOfParallelism = _config.DegreeOfParallelism;

            if (degreeOfParallelism == -1)
            {
                // set TaskCount = 10 for each 10k RUs, minimum 1, maximum 250
                taskCount = Math.Max(currentCollectionThroughput / 1000, 1);
                taskCount = Math.Min(taskCount, 250);
            }
            else
            {
                taskCount = degreeOfParallelism;
            }

            Console.WriteLine("Starting Inserts with {0} tasks", taskCount);
            string sampleDocument = System.IO.File.ReadAllText(_config.DocumentTemplateFile);

            pendingTaskCount = taskCount;
            var tasks = new List<Task>();
            tasks.Add(this.LogOutputStats(guid,cancellationToken));

            long numberOfDocumentsToInsert = long.Parse(_config.NumberOfDocumentsToInsert) / taskCount;
            for (var i = 0; i < taskCount; i++)
            {
                tasks.Add(this.InsertDocument(i, client, dataCollection, sampleDocument, numberOfDocumentsToInsert, cancellationToken));
            }

            await Task.WhenAll(tasks);

            if ((_config.ShouldCleanupOnFinish))
            {
                Console.WriteLine("Deleting Database {0}", _config.DatabaseName);
                await client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(_config.DatabaseName));
            }
            var val = new List<DocumentDBStatus>();
            performacnemetrics.Remove(guid, out val);
        }

        private async Task InsertDocument(int taskId, DocumentClient client, DocumentCollection collection, string sampleJson, long numberOfDocumentsToInsert, CancellationToken cancellationToken)
        {
            requestUnitsConsumed[taskId] = 0;
            string partitionKeyProperty = collection.PartitionKey.Paths[0].Replace("/", "");
            Dictionary<string, object> newDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(sampleJson);

            for (var i = 0; i < numberOfDocumentsToInsert; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                newDictionary["id"] = Guid.NewGuid().ToString();
                newDictionary[partitionKeyProperty] = Guid.NewGuid().ToString();

                try
                {
                    ResourceResponse<Document> response = await client.CreateDocumentAsync(
                            UriFactory.CreateDocumentCollectionUri(_config.DatabaseName, _config.CollectionName),
                            newDictionary,
                            new RequestOptions() { });

                    string partition = response.SessionToken.Split(':')[0];
                    requestUnitsConsumed[taskId] += response.RequestCharge;
                    Interlocked.Increment(ref this.documentsInserted);
                }
                catch (Exception e)
                {
                    if (e is DocumentClientException)
                    {
                        DocumentClientException de = (DocumentClientException)e;
                        if (de.StatusCode != HttpStatusCode.Forbidden)
                        {
                            Trace.TraceError("Failed to write {0}. Exception was {1}", JsonConvert.SerializeObject(newDictionary), e);
                        }
                        else
                        {
                            Interlocked.Increment(ref this.documentsInserted);
                        }
                    }
                }
            }

            Interlocked.Decrement(ref this.pendingTaskCount);
        }

        public async Task LogOutputStats(string guid,CancellationToken cancellationToken)
        {
            long lastCount = 0;
            double lastRequestUnits = 0;
            double lastSeconds = 0;
            double requestUnits = 0;
            double ruPerSecond = 0;
            double ruPerMonth = 0;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            StringBuilder sb = new StringBuilder();

            while (this.pendingTaskCount > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                double seconds = watch.Elapsed.TotalSeconds;

                requestUnits = 0;
                foreach (int taskId in requestUnitsConsumed.Keys)
                {
                    requestUnits += requestUnitsConsumed[taskId];
                }

                long currentCount = this.documentsInserted;
                ruPerSecond = (requestUnits / seconds);
                ruPerMonth = ruPerSecond * 86400 * 30;


                DocumentDBStatus datapoint = (new DocumentDBStatus
                {
                    Time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    docs = currentCount,
                    writes = Math.Round(this.documentsInserted / seconds),
                    rus = Math.Round(ruPerSecond),
                    reads = Math.Round(ruPerMonth / (1000 * 1000 * 1000))
                });
                var serializedObject = JsonConvert.SerializeObject(datapoint);
                
                
                if(performacnemetrics.ContainsKey(guid))
                {
                    lock (performacnemetrics)
                    {
                        //performacnemetrics.FirstOrDefault(x => x.Value.Contains(datapoint));
                        performacnemetrics[guid].Insert(0, datapoint);
                        //performacnemetrics.(guid, newseries, series);
                    }
                }

                

                Console.WriteLine("Inserted {0} docs @ {1} writes/s, {2} RU/s ({3}B max monthly 1KB reads  for {4})",
                    currentCount,
                    Math.Round(this.documentsInserted / seconds),
                    Math.Round(ruPerSecond),
                    Math.Round(ruPerMonth / (1000 * 1000 * 1000)),
                    guid.ToString()
                    );

                lastCount = documentsInserted;
                lastSeconds = seconds;
                lastRequestUnits = requestUnits;
            }

            double totalSeconds = watch.Elapsed.TotalSeconds;
            ruPerSecond = (requestUnits / totalSeconds);
            ruPerMonth = ruPerSecond * 86400 * 30;

            Console.WriteLine();
            Console.WriteLine("Summary:");
            Console.WriteLine("--------------------------------------------------------------------- ");
            Console.WriteLine("Inserted {0} docs @ {1} writes/s, {2} RU/s ({3}B max monthly 1KB reads)",
                lastCount,
                Math.Round(this.documentsInserted / watch.Elapsed.TotalSeconds),
                Math.Round(ruPerSecond),
                Math.Round(ruPerMonth / (1000 * 1000 * 1000)));
            Console.WriteLine("--------------------------------------------------------------------- ");
        }

        /// <summary>
        /// Create a partitioned collection.
        /// </summary>
        /// <returns>The created collection.</returns>
        private async Task<DocumentCollection> CreatePartitionedCollectionAsync(string databaseName, string collectionName)
        {
            DocumentCollection existingCollection = GetCollectionIfExists(databaseName, collectionName);

            DocumentCollection collection = new DocumentCollection();
            collection.Id = collectionName;
            collection.PartitionKey.Paths.Add(_config.CollectionPartitionKey);

            // Show user cost of running this test
            double estimatedCostPerMonth = 0.06 * _config.CollectionThroughput;
            double estimatedCostPerHour = estimatedCostPerMonth / (24 * 30);
            Console.WriteLine("The collection will cost an estimated ${0} per hour (${1} per month)", Math.Round(estimatedCostPerHour, 2), Math.Round(estimatedCostPerMonth, 2));

            return await client.CreateDocumentCollectionAsync(
                    UriFactory.CreateDatabaseUri(databaseName),
                    collection,
                    new RequestOptions { OfferThroughput = _config.CollectionThroughput });
        }

        /// <summary>
        /// Get the database if it exists, null if it doesn't
        /// </summary>
        /// <returns>The requested database</returns>
        private Database GetDatabaseIfExists(string databaseName)
        {
            return client.CreateDatabaseQuery().Where(d => d.Id == databaseName).AsEnumerable().FirstOrDefault();
        }

        /// <summary>
        /// Get the collection if it exists, null if it doesn't
        /// </summary>
        /// <returns>The requested collection</returns>
        private DocumentCollection GetCollectionIfExists(string databaseName, string collectionName)
        {
            if (GetDatabaseIfExists(databaseName) == null)
            {
                return null;
            }

            return client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(databaseName))
                .Where(c => c.Id == collectionName).AsEnumerable().FirstOrDefault();
        }

    }
}
