using Ravencoin.ApplicationCore.Models;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ravencoin.ApplicationCore.RpcConnections;
using Newtonsoft.Json;
using log4net;

namespace Ravencoin.ApplicationCore.BusinessLogic
{
    public class Assets
    {
        // Inject Logger
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Assets));

        //Create a ravencore instance to call against
        private readonly RavenCore ravencore = new RavenCore();

        /// <summary>
        /// Returns assets metadata if that asset exists
        /// </summary>
        /// <param name="asset_name"> (string, required) the name of the asset</param>
        /// <param name="connection"> ServerConnection (required) </param>
        /// <returns>
        /// Result:
        ///{
        ///  name: (string),
        ///  amount: (number),
        ///  units: (number),
        ///  reissuable: (number),
        ///  has_ipfs: (number),
        ///  ipfs_hash: (hash), (only if has_ipfs = 1 and that data is a ipfs hash)
        ///  txid_hash: (hash), (only if has_ipfs = 1 and that data is a txid hash)
        ///  verifier_string: (string)
        /// }
        /// </returns>
        /// 
        public async Task<ServerResponse> GetAssetData(ServerConnection connection,string asset_name)
        {
            _logger?.Info($"Beginning GetAssetData Function for Asset:  {asset_name}.");

            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("asset_name", asset_name);

            //Set up the ServerCommand
            ServerCommand request = new ServerCommand(){
                commandId = "0",
                commandMethod = "getassetdata",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            string str = JsonConvert.SerializeObject(request);

            //Send the ServerCommand to RavenCore. See comments for Response Value
            ServerResponse response = await ravencore.Connect(request, connection);
            
            //Check to see if this response is ok or not.
            if (response.statusCode == System.Net.HttpStatusCode.OK){
                _logger?.Info($"Recieved Response: {response.responseContent}");
                return response;
            }
            else{
                _logger?.Error($"Received Error code: {response.statusCode}. Error: {response.errorEx}.");
                return response;
            }
        }

        /// <summary>
        /// Gets cached info 
        /// </summary>
        /// <param name="connection"> ServerConnection (required) </param>
        /// <returns>
        /// Result:
        /// [
        /// uxto cache size:
        /// asset total(exclude dirty):
        /// asset address map:
        /// asset address balance:
        /// my unspent asset:
        /// reissue data:
        /// asset metadata map:
        /// asset metadata list(est):
        ///  dirty cache(est):
        /// ]
        /// </returns>
        /// 
        public async Task<ServerResponse> GetCacheInfo(ServerConnection connection)
        {
            _logger?.Info($"Beginning GetCacheInfo Function..");

            //Wrap properties in a JObject
            JObject commandParams = new JObject();

            //Set up the ServerCommand
            ServerCommand request = new ServerCommand()
            {
                commandId = "0",
                commandMethod = "getcacheinfo",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Send the ServerCommand to RavenCore. See comments for Response Value
            ServerResponse response = await ravencore.Connect(request, connection);

            _logger?.Info($"Recieved Response: {response.responseContent}");
            return response;
        }

        /// <summary>
        /// Returns details for the asset snapshot, at the specified height
        /// </summary>
        /// <param name="asset_name"> asset_name (required) </param>
        /// <param name="connection"> ServerConnection (required) </param>
        /// <returns>
        ///Result:
        ///{
        ///  name: (string),
        ///  height: (number),
        ///  owners: [
        ///    {
        ///      address: (string),
        ///      amount_owned: (number),
        ///    }
        ///}
        /// </returns>
        /// 
        public async Task<ServerResponse> GetSnapshot(ServerConnection connection, string asset_name)
        {
            _logger?.Info($"Beginning GetSnapshot Function for Asset: {asset_name}");

            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("asset_name", asset_name);

            //Set up the ServerCommand
            ServerCommand request = new ServerCommand(){
                commandId = "0",
                commandMethod = "getsnapshot",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Send the ServerCommand to RavenCore. See comments for Response Value
            ServerResponse response = await ravencore.Connect(request, connection);

            //Check to see if this response is ok or not.
            if (response.statusCode == System.Net.HttpStatusCode.OK)
            {
                _logger?.Info($"Recieved Response: {response.responseContent}");
                return response;
            }
            else
            {
                _logger?.Error($"Received Error code: {response.statusCode}. Error: {response.errorEx}.");
                return response;
            }
        }

        /// <summary>
        /// Issue an asset, subasset or unique asset.
        /// Asset name must not conflict with any existing asset.
        /// Unit as the number of decimals precision for the asset (0 for whole units ("1"), 8 for max precision("1.00000000")
        /// Reissuable is true/false for whether additional units can be issued by the original issuer.
        /// If issuing a unique asset these values are required (and will be defaulted to): qty=1, units=0, reissuable=false.
        /// </summary>
        /// <param name="asset_name"> a unique name (required) </param>
        /// <param name="qty"> default=1, the number of units to be issued (optional) </param>
        /// <param name="to_address"> address asset will be sent to, if it is empty, address will be generated for you (optional) </param>
        /// <param name="change_address"> address the the rvn change will be sent to, if it is empty, change address will be generated for you (optional) </param>
        /// <param name="units"> default=0, min=0, max=8, the number of decimals precision for the asset (0 for whole units ("1"), 8 for max precision ("1.00000000") (optional) </param>
        /// <param name="reissuable"> default=true (false for unique assets)), whether future reissuance is allowed (optional) </param>
        /// <param name="has_ipfs"> default=false, whether ipfs hash is going to be added to the asset (optional) </param>
        /// <param name="ipfs_hash">  optional but required if has_ipfs = 1), an ipfs hash or a txid hash once RIP5 is activated (optional) </param>
        /// <param name="connection"> ServerConnection (required) </param>
        /// <returns>
        /// Result:
        /// "txid"   (string) The transaction id 
        public async Task<ServerResponse> Issue(ServerConnection connection, string asset_name, double qty = 1, string to_address = "", string change_address = "", int units = 0, bool reissuable = true, bool? has_ipfs = false, string ipfs_hash = "") {
            _logger?.Info($"Beginning Issue Function for Asset: {asset_name}. Quantity: {qty}, To_Address: {to_address}, Change_Address: {change_address}, Units: {units}, Resissuable: {reissuable}, Has_IPFS: {has_ipfs}, IPFS_Hash: {ipfs_hash}");

            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("asset_name", asset_name);
            commandParams.Add("qty", qty);
            if (!string.IsNullOrEmpty(to_address)) { commandParams.Add("to_address", to_address); };
            if (!string.IsNullOrEmpty(change_address)) { commandParams.Add("change_address", change_address); };
            commandParams.Add("units", units);
            commandParams.Add("reissuable", reissuable);
            commandParams.Add("has_ipfs", has_ipfs);
            if (!string.IsNullOrEmpty(ipfs_hash)) { commandParams.Add("ipfs_hash", ipfs_hash); };

            //Set up the ServerCommand
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "issue",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Send the ServerCommand to RavenCore. See comments for Response Value
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
        /// Issue unique asset(s).
        /// root_name must be an asset you own.
        /// An asset will be created for each element of asset_tags.
        /// If provided ipfs_hashes must be the same length as asset_tags.
        /// Five (5) RVN will be burned for each asset created.
        /// </summary>
        /// <param name="root_name"> (string, required) name of the asset the unique asset(s) are being issued under </param>
        /// <param name="asset_tags"> (array, required) the unique tag for each asset which is to be issued </param>
        /// <param name="connection"> ServerConnection (required) </param>
        /// <param name="ipfs_hashes"> (array, optional) ipfs hashes or txid hashes corresponding to each supplied tag (should be same size as "asset_tags") </param>
        /// <param name="to_address"> (string, optional, default=""), address assets will be sent to, if it is empty, address will be generated for you </param>
        /// <param name="change_address"> (string, optional, default=""), address the the rvn change will be sent to, if it is empty, change address will be generated for you </param>
        /// <returns>
        /// Result:
        /// "txid"   (string) The transaction id 
        public async Task<ServerResponse> IssueUnique(ServerConnection connection, string root_name, string[] asset_tags, string[] ipfs_hashes = null, string to_address = "", string change_address = "") {
            _logger?.Info($"Beginning Issue Function for Asset: {root_name}. Asset Tags: {asset_tags.ToString()}, IPFS Hashes: {ipfs_hashes.ToString()}, To_Address: {to_address}, Change_Address: {change_address}.");

            //Handle an empty string array since this is an optional parameter
            ipfs_hashes ??= new string[0];

            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("root_name", root_name);
            commandParams.Add("asset_tags", JArray.FromObject(asset_tags));
            commandParams.Add("ipfs_hashes",JArray.FromObject(ipfs_hashes));
            if (!string.IsNullOrEmpty(to_address)) { commandParams.Add("to_address", to_address); };
            if (!string.IsNullOrEmpty(change_address)) { commandParams.Add("change_address", change_address); } ;


            //Set up the ServerCommand
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "issueunique",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Send the ServerCommand to RavenCore. See comments for Response Value
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
        /// Returns a list of all assets
        /// This could be a slow/expensive operation as it reads from the database
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="asset">(string, optional, default="*") filters results -- must be an asset name or a partial asset name followed by '*' ('*' matches all trailing characters)</param>
        /// <param name="verbose">(boolean, optional, default=false) when false result is just a list of asset names -- when true results are asset name mapped to metadata </param>
        /// <param name="asset">(integer, optional, default=ALL) truncates results to include only the first _count_ assets found </param>
        /// <param name="asset">(integer, optional, default=0) results skip over the first _start_ assets found (if negative it skips back from the end) </param>
        /// 
        /// <returns>
        /// Result (verbose=false):
        /// [
        ///   asset_name,        
        ///   ...
        /// ]
        /// </returns>
        public async Task<ServerResponse> ListAssets(ServerConnection connection, string asset_name = "*", bool verbose = false, int count = int.MaxValue, int start = 0) {
            _logger?.Info($"Beginning ListAssets Function for Asset: {asset_name}. Verbose: {verbose}, Count: {count}, Start: {start}");

            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("asset_name", asset_name);
            commandParams.Add("verbose", verbose);
            commandParams.Add("count", count);
            commandParams.Add("start", start);
            

            //Set up the ServerCommand
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "listassets",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Send the ServerCommand to RavenCore. See comments for Response Value
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
        /// Returns a list of all asset that are owned by this wallet
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="asset_name">(string, optional, default="*") filters results -- must be an asset name or a partial asset name followed by '*' ('*' matches all trailing characters)</param>
        /// <param name="verbose">(boolean, optional, default=false) when false result is just a list of asset names -- when true results are asset name mapped to metadata </param>
        /// <param name="count">(integer, optional, default=ALL) truncates results to include only the first _count_ assets found </param>
        /// <param name="start">(integer, optional, default=0) results skip over the first _start_ assets found (if negative it skips back from the end) </param>
        /// <param name="confs">(integer, optional, default=0) results are skipped if they don't have this number of confirmations </param>
        /// <returns>
        /// Result (verbose=false):
        ///{
        ///  (asset_name): balance,
        ///  ...
        ///}
        ///
        ///    Result(verbose= true) :
        ///    {
        ///        (asset_name):
        ///    {
        ///            "balance": balance,
        ///      "outpoints":
        ///        [
        ///          {
        ///                "txid": txid,
        ///            "vout": vout,
        ///            "amount": amount
        ///          }
        ///            { ...}, { ...}
        ///        ]
        ///    }
        ///    }
        ///{...}, { ...}
        /// </returns>
        public async Task<ServerResponse> ListMyAssets(ServerConnection connection, string asset_name = "*", bool verbose = false, int count = int.MaxValue, int start = 0, int confs = 0 ) {
            _logger?.Info($"Beginning ListMyAssets Function for Asset: {asset_name}. Verbose: {verbose}, Count: {count}, Start: {start}, Confs: {confs}");

            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("asset", asset_name);
            commandParams.Add("verbose", verbose);
            commandParams.Add("count", count);
            commandParams.Add("start", start);
            commandParams.Add("confs", confs);

            //Set up the ServerCommand
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "listmyassets",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Send the ServerCommand to RavenCore. See comments for Response Value
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
        /// Removes details for the asset snapshot, at the specified height
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="asset_name">(string, required) the name of the asset</param>
        /// <param name="block_height">(int, required) the block height of the snapshot</param>
        /// <returns>
        /// Result:
        /// {
        ///  name: (string),
        ///  height: (number),
        /// }
        /// </returns>
        public async Task<ServerResponse> PurgeSnapshot(ServerConnection connection, string asset_name, int block_height) {
            _logger?.Info($"Beginning ListMyAssets Function for Asset: {asset_name}. Block Height: {block_height}");

            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("asset_name", asset_name);
            commandParams.Add("block_height", block_height);

            //Set up the ServerCommand
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "purgesnapshot",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Send the ServerCommand to RavenCore. See comments for Response Value
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
        /// Reissues a quantity of an asset to an owned address if you own the Owner Token
        /// Can change the reissuable flag during reissuance
        /// Can change the ipfs hash during reissuance
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="asset_name">(string, required) the name of the asset that is being reissued</param>
        /// <param name="qty">(numeric, required) number of assets to reissue</param>
        /// <param name="to_address">(string, required) address to send the asset to</param>
        /// <param name="change_address">(string, optional) address that the change of the transaction will be sent to</param>
        /// <param name="reissuable">(boolean, optional, default=true), whether future reissuance is allowed</param>
        /// <param name="new_units">(numeric, optional, default=-1), the new units that will be associated with the asset</param>
        /// <param name="new_ipfs">(string, optional, default=""), whether to update the current ipfs hash or txid once RIP5 is active</param>
        /// <returns>
        ///  Result:
        /// "txid"                     (string) The transaction id
        /// </returns>
        public async Task<ServerResponse> Reissue(ServerConnection connection, string asset_name, double qty, string to_address, string change_address = "", bool reissuable = true, double new_units = -1, string new_ipfs="") {
            _logger?.Info($"Beginning Issue Function for Asset: {asset_name}. Quantity: {qty}, To_Address: {to_address}, Change_Address: {change_address}, Reissueable: {reissuable}, New Units: {new_units}, New_IPFS: {new_ipfs}");;

            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("asset_name", asset_name);
            commandParams.Add("qty", qty);
            commandParams.Add("to_address", to_address);
            if (!string.IsNullOrEmpty(change_address)) { commandParams.Add("change_address", change_address); };
            commandParams.Add("reissuable", reissuable);
            commandParams.Add("new_units", new_units);
            if (!string.IsNullOrEmpty(new_ipfs)) { commandParams.Add("new_ipfs", new_ipfs); };

            //Set up the ServerCommand
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "reissue",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Send the ServerCommand to RavenCore. See comments for Response Value
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
        /// Transfers a quantity of an owned asset to a given address
        /// </summary>
        /// <param name="connection">ServerConnection (required)</param>
        /// <param name="asset_name">asset_name (string, required)</param>
        /// <param name="qty">(numeric, required) number of assets you want to send to the address</param>
        /// <param name="to_address">(string, required) address to send the asset to</param>
        /// <param name="message">(string, optional) Once RIP5 is voted in ipfs hash or txid hash to send along with the transfer</param>
        /// <param name="expire_time">(numeric, optional) UTC timestamp of when the message expires</param>
        /// <param name="change_address">(string, optional, default = "") the transactions RVN change will be sent to this address</param>
        /// <param name="asset_change_address">(string, optional, default = "") the transactions Asset change will be sent to this address</param>
        /// <returns>
        /// Result:
        /// txid[txid]
        /// </returns>
        public async Task<ServerResponse> Transfer(ServerConnection connection, string asset_name, int qty, string to_address, string message = "", double expire_time = 0, string change_address = "", string asset_change_address = "")
        {
            _logger?.Info($"Beginning TransferAsset Function for Asset:  {asset_name}, quantity: {qty}, To Address: {to_address}, Message: {message}, Expire Time: {expire_time}, Change Address: {change_address}, Asset Change Address: {asset_change_address}.");

            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("asset_name", asset_name);
            commandParams.Add("qty", qty);
            commandParams.Add("to_address", to_address);
            if (!string.IsNullOrEmpty(message)) { commandParams.Add("message", message); };
            if (expire_time != 0) { commandParams.Add("expire_time", expire_time); };
            if (!string.IsNullOrEmpty(change_address)) { commandParams.Add("change_address", change_address); };
            if (!string.IsNullOrEmpty(asset_change_address)) { commandParams.Add("asset_change_address", asset_change_address); };

            //Set up the ServerCommand
            ServerCommand request = new ServerCommand(){
                commandId = "0",
                commandMethod = "transfer",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Send the ServerCommand to RavenCore. See comments for Response Value
            ServerResponse response = await ravencore.Connect(request, connection);

            //Check to see if this response is ok or not.
            if (response.statusCode == System.Net.HttpStatusCode.OK)
            {
                _logger?.Info($"Recieved Response: {response.responseContent}");
                return response;
            }
            else
            {
                _logger?.Error($"Received Error code: {response.statusCode}. Error: {response.errorEx}.");
                return response;
            }
        }

        /// <summary>
        /// Transfer a quantity of an owned asset in a specific address to a given address
        /// </summary>
        /// <param name="connection">ServerConnection (required)</param>
        /// <param name="asset_name">asset_name (string, required)</param>
        /// <param name="from_address">(string, required) address that the asset will be transferred from</param>
        /// <param name="qty">(numeric, required) number of assets you want to send to the address</param>
        /// <param name="to_address">(string, required) address to send the asset to</param>
        /// <param name="message">(string, optional) Once RIP5 is voted in ipfs hash or txid hash to send along with the transfer</param>
        /// <param name="expire_time">(numeric, optional) UTC timestamp of when the message expires</param>
        /// <param name="change_address">(string, optional, default = "") the transactions RVN change will be sent to this address</param>
        /// <param name="asset_change_address">(string, optional, default = "") the transactions Asset change will be sent to this address</param>
        /// <returns>
        /// Result:
        /// txid[txid]
        /// </returns>
        public async Task<ServerResponse> TransferFromAddress(ServerConnection connection, string asset_name, string from_address, int qty, string to_address, string message = "", double expire_time = 0, string change_address = "", string asset_change_address = "") {
            _logger?.Info($"Beginning TransferAsset Function for Asset:  {asset_name}, From Address: {from_address}, Quantity: {qty}, To Address: {to_address}, Message: {message}, Expire Time: {expire_time}, Change Address: {change_address}, Asset Change Address: {asset_change_address}.");

            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("asset_name", asset_name);
            commandParams.Add("from_address", from_address);
            commandParams.Add("qty", qty);
            commandParams.Add("to_address", to_address);
            if (!string.IsNullOrEmpty(message)) { commandParams.Add("message", message); } ;
            commandParams.Add("expire_time", expire_time);
            if (!string.IsNullOrEmpty(change_address)) { commandParams.Add("change_address", change_address); };
            if (!string.IsNullOrEmpty(asset_change_address)) { commandParams.Add("asset_change_address", asset_change_address); };

            //Set up the ServerCommand
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "transferfromaddress",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Send the ServerCommand to RavenCore. See comments for Response Value
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
        /// Transfer a quantity of an owned asset in specific address(es) to a given address
        /// </summary>
        /// <param name="connection">ServerConnection (required)</param>
        /// <param name="asset_name">asset_name (string, required)</param>
        /// <param name="from_address">(array, required) list of from addresses to send from</param>
        /// <param name="qty">(numeric, required) number of assets you want to send to the address</param>
        /// <param name="to_address">(string, required) address to send the asset to</param>
        /// <param name="message">(string, optional) Once RIP5 is voted in ipfs hash or txid hash to send along with the transfer</param>
        /// <param name="expire_time">(numeric, optional) UTC timestamp of when the message expires</param>
        /// <param name="change_address">(string, optional, default = "") the transactions RVN change will be sent to this address</param>
        /// <param name="asset_change_address">(string, optional, default = "") the transactions Asset change will be sent to this address</param>
        /// <returns>
        /// Result:
        /// txid[txid]
        /// </returns>
        public async Task<ServerResponse> TransferFromAddresses(ServerConnection connection, string asset_name, string[] from_addresses, int qty, string to_address, string message = "", double expire_time = 0, string change_address = "", string asset_change_address = "") {
            _logger?.Info($"Beginning TransferAsset Function for Asset:  {asset_name}, From Address: {from_addresses}, Quantity: {qty}, To Address: {to_address}, Message: {message}, Expire Time: {expire_time}, Change Address: {change_address}, Asset Change Address: {asset_change_address}.");

            //Wrap properties in a JObject
            JObject commandParams = new JObject();
            commandParams.Add("asset_name", asset_name);
            commandParams.Add("from_addresses", JArray.FromObject(from_addresses));
            commandParams.Add("qty", qty);
            commandParams.Add("to_address", to_address);
            if (!string.IsNullOrEmpty(message)) { commandParams.Add("message", message); };
            commandParams.Add("expire_time", expire_time);
            if (!string.IsNullOrEmpty(change_address)) { commandParams.Add("change_address", change_address); };
            if (!string.IsNullOrEmpty(asset_change_address)) { commandParams.Add("asset_change_address", asset_change_address); };


            //Set up the ServerCommand
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "transferfromaddresses",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Send the ServerCommand to RavenCore. See comments for Response Value
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
