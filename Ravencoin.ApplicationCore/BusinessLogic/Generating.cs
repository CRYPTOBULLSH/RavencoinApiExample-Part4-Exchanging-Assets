using log4net;
using Newtonsoft.Json.Linq;
using Ravencoin.ApplicationCore.Models;
using Ravencoin.ApplicationCore.RpcConnections;
using System.Threading.Tasks;

namespace Ravencoin.ApplicationCore.BusinessLogic {
    public class Generating {
        //Inject Logger
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Transactions));

        //Create a ravencore instance to call against
        private readonly RavenCore ravencore = new RavenCore();

        /// <summary>
        /// Mine up to nblocks blocks immediately (before the RPC call returns) to an address in the wallet.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="nblocks">(numeric, required) How many blocks are generated immediately.</param>
        /// <param name="maxtries">(numeric, optional) How many iterations to try (default = 1000000).</param>
        /// <returns>
        /// Result:
        /// [blockhashes] (array) hashes of blocks generated
        /// </returns>
        public async Task<ServerResponse> Generate(ServerConnection connection, int nblocks, int maxtries = 1000000) {
            _logger?.Info($"Beginning Generate Function. NBlocks: {nblocks}, Max Tries: {maxtries}");

            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("nblocks", nblocks);
            commandParams.Add("maxtries", maxtries);

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "generate",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            ServerResponse response = await ravencore.Connect(request, connection);

            //Check to see if this response is ok or not.
            if (response.statusCode == System.Net.HttpStatusCode.OK) {
                _logger?.Info($"Recieved Response: {response.responseContent}");
                return response;
            } else {
                _logger?.Error($"Received Error code: {response.statusCode}. Error: {response.errorEx}.");
                return response;
            }
        }

        /// <summary>
        /// Mine blocks immediately to a specified address (before the RPC call returns)
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="nblocks">(numeric, required) How many blocks are generated immediately.</param>
        /// <param name="address">(string, required) The address to send the newly generated raven to.</param>
        /// <param name="maxtries">(numeric, optional) How many iterations to try (default = 1000000).</param>
        /// <returns>
        /// Result:
        /// [blockhashes] (array) hashes of blocks generated
        /// </returns>
        public async Task<ServerResponse> GenerateToAddress(ServerConnection connection, int nblocks, string address, int maxtries = 1000000) {
            _logger?.Info($"Beginning GenerateToAddress Function. NBlocks: {nblocks}, Address: {address}, Max Tries: {maxtries}");

            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("nblocks", nblocks);
            commandParams.Add("address", address);
            commandParams.Add("maxtries", maxtries);

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "generatetoaddress",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            ServerResponse response = await ravencore.Connect(request, connection);

            //Check to see if this response is ok or not.
            if (response.statusCode == System.Net.HttpStatusCode.OK) {
                _logger?.Info($"Recieved Response: {response.responseContent}");
                return response;
            } else {
                _logger?.Error($"Received Error code: {response.statusCode}. Error: {response.errorEx}.");
                return response;
            }
        }

        /// <summary>
        /// Return if the server is set to generate coins or not. The default is false.
        /// It is set with the command line argument -gen(or raven.conf setting gen)
        /// It can also be set with the setgenerate call.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>
        /// Result
        /// true|false
        /// </returns>
        public async Task<ServerResponse> GetGenerate(ServerConnection connection) {
            _logger?.Info($"Beginning GetGenerate Function.");

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "getgenerate",
                commandJsonRpc = "2.0"
            };

            ServerResponse response = await ravencore.Connect(request, connection);

            //Check to see if this response is ok or not.
            if (response.statusCode == System.Net.HttpStatusCode.OK) {
                _logger?.Info($"Recieved Response: {response.responseContent}");
                return response;
            } else {
                _logger?.Error($"Received Error code: {response.statusCode}. Error: {response.errorEx}.");
                return response;
            }
        }

        /// <summary>
        /// Set 'generate' true or false to turn generation on or off.
        /// Generation is limited to 'genproclimit' processors, -1 is unlimited.
        /// See the getgenerate call for the current setting.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="generate">(boolean, required) Set to true to turn on generation, false to turn off.</param>
        /// <param name="genproclimit">(numeric, optional) Set the processor limit for when generation is on. Can be -1 for unlimited.</param>
        /// <returns></returns>
        public async Task<ServerResponse> SetGenerate(ServerConnection connection, bool generate, int genproclimit = -1) {
            _logger?.Info($"Beginning SetGenerate Function. Generate: {generate}, GenProcLimit: {genproclimit}");

            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("generate", generate);
            commandParams.Add("genproclimit", genproclimit);

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "setgenerate",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            ServerResponse response = await ravencore.Connect(request, connection);

            //Check to see if this response is ok or not.
            if (response.statusCode == System.Net.HttpStatusCode.OK) {
                _logger?.Info($"Recieved Response: {response.responseContent}");
                return response;
            } else {
                _logger?.Error($"Received Error code: {response.statusCode}. Error: {response.errorEx}.");
                return response;
            }
        }
    }
}
