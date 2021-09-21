using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Ravencoin.ApplicationCore.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using log4net;

namespace Ravencoin.ApplicationCore.RpcConnections
{
    public class RavenCore
    {
        //Inject Logger
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RavenCore));

        public async Task<ServerResponse> Connect(ServerCommand command, ServerConnection serverConnection)
        {
            _logger?.Info($"Beginning RavenCore RPC Connection. Command: {command.commandMethod}");

            HttpClient client = new HttpClient();
            Uri baseUri = new Uri($"http://{serverConnection.host}:{serverConnection.port}");
            client.BaseAddress = baseUri;
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.ConnectionClose = true;
            //client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //Set up authentication
            var authenticationString = $"{serverConnection.username}:{serverConnection.password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

            //Set up the body of the message.
            //This adds the authorization header and encoding the RavencoinRequest object into json.
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            //Set the content of the body
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(command), Encoding.UTF8, "application/json");

            // Try to send the message, if we get an error response as such with details
            try{
                //make the request
                HttpResponseMessage httpresponse = await client.SendAsync(requestMessage);

                //ensure we get a good response
                if (httpresponse.StatusCode != System.Net.HttpStatusCode.OK) {
                    return new ServerResponse { statusCode = httpresponse.StatusCode, errorEx = httpresponse.Content.ReadAsStringAsync().Result };
                }

                ServerResponse response = new ServerResponse{
                    statusCode = System.Net.HttpStatusCode.OK,
                    responseContent = await httpresponse.Content.ReadAsStringAsync()
                };
                return response;
            }
            catch (Exception httpEx){
                ServerResponse exresponse = new ServerResponse{
                    statusCode = System.Net.HttpStatusCode.BadRequest,
                    errorEx = httpEx.Message
                };
                return exresponse;
            }
        }
    }
}
