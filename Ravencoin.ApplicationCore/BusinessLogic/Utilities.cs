using Ravencoin.ApplicationCore.Models;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ravencoin.ApplicationCore.RpcConnections;
using log4net;

namespace Ravencoin.ApplicationCore.BusinessLogic
{
    public class Utilities
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Utilities));

        RavenCore ravencore = new RavenCore();
        public async Task<ServerResponse> ValidateAddress(string address, ServerConnection connection)
        {
            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("address", address);

            //Set up the ServerCommand
            ServerCommand request = new ServerCommand(){
                commandId = "0",
                commandMethod = "validateaddress",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Send the ServerCommand to RavenCore. See comments for Response Value
            ServerResponse response = await ravencore.Connect(request, connection);

            return response;
        }
    }
}
