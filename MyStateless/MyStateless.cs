using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json.Linq;

namespace MyStateless
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class MyStateless : StatelessService
    {
        public MyStateless(StatelessServiceContext context)
            : base(context)
        { }

        private readonly ServicePartitionResolver servicePartitionResolver = ServicePartitionResolver.GetDefault();

        private readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(context => this.CreateInputListener(context)) };
        }

        private ICommunicationListener CreateInputListener(ServiceContext context)
        {
            // Service instance's URL is the node's IP & desired port
            EndpointResourceDescription inputEndpoint =
                context.CodePackageActivationContext.GetEndpoint("WebApiServiceEndpoint");
            string uriPrefix = String.Format("{0}://+:{1}/alphabetpartitions/", inputEndpoint.Protocol, inputEndpoint.Port);
            var uriPublished = uriPrefix.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
            return new HttpCommunicationListener(uriPrefix, uriPublished, this.ProcessInputRequest);
        }

        private async Task ProcessInputRequest(HttpListenerContext context, CancellationToken cancelRequest)
        {
            String output = null;
            try
            {
                string lastname = context.Request.QueryString["lastname"];
                char firstLetterOfLastName = lastname.First();
                ServicePartitionKey partitionKey = new ServicePartitionKey(Char.ToUpper(firstLetterOfLastName) - 'A');

                Uri alphabetServiceUri = new Uri(@"fabric:/MyFabric.Partitions/MyStateful");

                ResolvedServicePartition partition =
                    await this.servicePartitionResolver.ResolveAsync(alphabetServiceUri, partitionKey, cancelRequest);
                ResolvedServiceEndpoint ep = partition.GetEndpoint();

                JObject addresses = JObject.Parse(ep.Address);
                string primaryReplicaAddress = (string) addresses["Endpoints"].First();

                UriBuilder primaryReplicaUriBuilder = new UriBuilder(primaryReplicaAddress);
                primaryReplicaUriBuilder.Query = "lastname=" + lastname;

                string result = await this.httpClient.GetStringAsync(primaryReplicaUriBuilder.Uri);

                output = String.Format(
                    "Result: {0}. <p>Partition key: '{1}' generated from the first letter '{2}' of input value '{3}'. <br>Processing service partition ID: {4}. <br>Processing service replica address: {5}",
                    result,
                    partitionKey,
                    firstLetterOfLastName,
                    lastname,
                    partition.Info.Id,
                    primaryReplicaAddress);

                using (var response = context.Response)
                {
                    if (output != null)
                    {
                        output = output + "added to Partition: " + primaryReplicaAddress;
                        byte[] outBytes = Encoding.UTF8.GetBytes(output);
                        response.OutputStream.Write(outBytes, 0, outBytes.Length);
                    }
                }

            }
            catch (Exception ex)
            {
                output = ex.Message;
            }
        }
    }
}
