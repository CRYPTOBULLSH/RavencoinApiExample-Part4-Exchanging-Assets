using log4net;
using Newtonsoft.Json.Linq;
using Ravencoin.ApplicationCore.Models;
using Ravencoin.ApplicationCore.RpcConnections;
using System.Threading.Tasks;

namespace Ravencoin.ApplicationCore.BusinessLogic
{
     public class Transactions
    {
        //Inject Logger
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Transactions));

        //Create a ravencore instance to call against
        private readonly RavenCore ravencore = new RavenCore();

        /// <summary>
        /// Return the raw transaction data.
        /// NOTE: By default this function only works for mempool transactions. If the -txindex option isenabled, it also works for blockchain transactions.
        /// DEPRECATED: for now, it also works for transactions with unspent outputs.
        /// 
        /// If verbose is 'true', returns an Object with information about 'txid'.
        /// If verbose is 'false' or omitted, returns a string that is serialized, hex-encoded data for 'txid'.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="txid">(string, required) The transaction id</param>
        /// <param name="verbose">(bool, optional, default=false) If false, return a string, otherwise return a json object</param>
        /// <returns>
        /// Result (if verbose is not set or set to false):
        ///"data"      (string) The serialized, hex-encoded data for 'txid'
        ///
        ///Result(if verbose is set to true) :
        ///{
        ///            "hex" : "data",       (string)The serialized, hex - encoded data for 'txid'
        ///           
        ///             "txid" : "id", (string)The transaction id(same as provided)
        ///           
        ///             "hash" : "id", (string)The transaction hash(differs from txid for witness transactions)
        ///                                                              "size" : n,             (numeric)The serialized transaction size
        ///  "vsize" : n,            (numeric)The virtual transaction size(differs from size for witness transactions)
        ///  "version" : n,          (numeric) The version
        ///  "locktime" : ttt,       (numeric) The lock time
        ///  "vin" : [               (array of json objects)
        ///     {
        ///       "txid": "id",    (string) The transaction id
        ///       "vout": n,         (numeric) 
        ///       "scriptSig": {     (json object) The script
        ///         "asm": "asm",  (string) asm
        ///         "hex": "hex"   (string) hex
        ///       },
        ///       "sequence": n(numeric) The script sequence number
        ///       "txinwitness": ["hex", ...] (array of string) hex-encoded witness data(if any)
        ///     }
        ///     ,...
        ///  ],
        ///  "vout" : [              (array of json objects)
        ///     {
        ///       "value" : x.xxx,            (numeric) The value in RVN
        ///       "n" : n,                    (numeric) index
        ///       "scriptPubKey" : {          (json object)
        ///         "asm" : "asm",          (string) the asm
        ///         "hex" : "hex",          (string) the hex
        ///         "reqSigs" : n,            (numeric) The required sigs
        ///         "type" : "pubkeyhash",  (string) The type, eg 'pubkeyhash'
        ///         "addresses" : [           (json array of string)
        ///           "address"        (string) raven address
        ///           ,...
        ///         ]
        ///       }
        ///     }
        ///     ,...
        ///  ],
        ///  "blockhash" : "hash",   (string)the block hash
        ///  "confirmations" : n,      (numeric)The confirmations
        ///  "time" : ttt,             (numeric)The transaction time in seconds since epoch (Jan 1 1970 GMT)
        ///  "blocktime" : ttt(numeric) The block time in seconds since epoch (Jan 1 1970 GMT)
        ///}
        /// </returns>
        public async Task<ServerResponse> GetRawTransaction(ServerConnection connection, string txid, bool verbose = false)
        {
            _logger?.Info($"Beginning GetRawTransaction Function. TXID: {txid}");

            //Set up parameters to get the hex string of the transaction
            JObject commandParams = new JObject();
            commandParams.Add("txid", txid);
            commandParams.Add("verbose", verbose);

            //Set up the Ravcencoin Object
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "getrawtransaction",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Get the hex string of the transaction back from getrawtransaction, and then parse it to get just the raw hex string from result
            ServerResponse response = await ravencore.Connect(request, connection);

            //Parse the result for the hexstring
            JObject result = JObject.Parse(response.responseContent);
            JToken hexstring = result["result"];

            response.responseContent = hexstring.ToString();

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
        /// Return a JSON object representing the serialized, hex-encoded transaction.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="hexstring">(string, required) The transaction hex string</param>
        /// <returns>
        /// Result:
        ///{
        ///  "txid" : "id",        (string) The transaction id
        ///  "hash" : "id",        (string) The transaction hash(differs from txid for witness transactions)
        ///  "size" : n,             (numeric) The transaction size
        ///  "vsize" : n,            (numeric) The virtual transaction size(differs from size for witness transactions)
        ///  "version" : n,          (numeric) The version
        ///  "locktime" : ttt,       (numeric) The lock time
        ///  "vin" : [               (array of json objects)
        ///     {
        ///       "txid": "id",    (string) The transaction id
        ///       "vout": n,         (numeric) The output number
        ///       "scriptSig": {     (json object) The script
        ///         "asm": "asm",  (string) asm
        ///         "hex": "hex"   (string) hex
        ///       },
        ///       "txinwitness": ["hex", ...] (array of string) hex-encoded witness data(if any)
        ///       "sequence": n(numeric) The script sequence number
        ///     }
        ///     ,...
        ///  ],
        ///  "vout" : [             (array of json objects)
        ///     {
        ///       "value" : x.xxx,            (numeric) The value in RVN
        ///       "n" : n,                    (numeric) index
        ///       "scriptPubKey" : {          (json object)
        ///         "asm" : "asm",          (string) the asm
        ///         "hex" : "hex",          (string) the hex
        ///         "reqSigs" : n,            (numeric) The required sigs
        ///         "type" : "pubkeyhash",  (string) The type, eg 'pubkeyhash'
        ///         "asset" : {               (json object) optional
        ///           "name" : "name",      (string) the asset name
        ///           "amount" : n,           (numeric) the amount of asset that was sent
        ///           "message" : "message", (string optional) the message if one was sent
        ///           "expire_time" : n,      (numeric optional) the message epoch expiration time if one was set
        ///         "addresses" : [           (json array of string)
        ///           "12tvKAXCxZjSmdNbao16dKXC8tRWfcF5oc"   (string) raven address
        ///           ,...
        ///         ]
        ///       }
        ///     }
        ///     ,...
        ///  ],
        ///}
        /// </returns>
        public async Task<ServerResponse> DecodeRawTransaction(ServerConnection connection, string hexstring)
        {
            _logger?.Info($"Beginning DecodeRawTransaction Function. Hexstring: {hexstring}");

            //Set up parameters to get the hex string of the transaction
            JObject commandParams = new JObject();
            commandParams.Add("hexstring", hexstring);

            //Set up the Ravcencoin Object
            ServerCommand request = new ServerCommand(){
                commandId = "0",
                commandMethod = "decoderawtransaction",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Get the hex string of the transaction back from getrawtransaction, and then parse it to get just the raw hex string from result
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
        /// Add inputs to a transaction until it has enough in value to meet its out value.
        /// This will not modify existing inputs, and will add at most one change output to the outputs.
        /// No existing outputs will be modified unless "subtractFeeFromOutputs" is specified.
        /// Note that inputs which were signed may need to be resigned after completion since in/outputs have been added.
        /// The inputs added will not be signed, use signrawtransaction for that.
        /// Note that all existing inputs must have their previous output transaction be in the wallet.
        /// Note that all inputs selected must be of standard form and P2SH scripts must be
        /// in the wallet using importaddress or addmultisigaddress(to calculate fees).
        /// You can see whether this is the case by checking the "solvable" field in the listunspent output.
        /// Only pay-to-pubkey, multisig, and P2SH versions thereof are currently supported for watch-only
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="hexstring">(string, required) The hex string of the raw transaction</param>
        /// <returns>
        /// Result:
        ///{
        ///  "hex":       "value", (string) The resulting raw transaction(hex-encoded string)
        ///  "fee":       n,         (numeric) Fee in RVN the resulting transaction pays
        ///  "changepos": n(numeric) The position of the added change output, or -1
        ///}
        /// </returns>
        public async Task<ServerResponse> FundRawTransaction(ServerConnection connection, string hexstring) {
            _logger?.Info($"Beginning FundRawTransaction Function. Hexstring: {hexstring}");

            //Set up parameters to get the hex string of the transaction
            JObject commandParams = new JObject();
            commandParams.Add("hexstring", hexstring);

            //Set up the Ravcencoin Object
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "fundrawtransaction",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Get the hex string of the transaction back from getrawtransaction, and then parse it to get just the raw hex string from result
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
        /// Submits raw transaction (serialized, hex-encoded) to local node and network.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="hexstring">(string, required) The hex string of the raw transaction)</param>
        /// <param name="allowhighfees">(boolean, optional, default=false) Allow high fees</param>
        /// <returns>
        /// Result:
        ///"hex"             (string) The transaction hash in hex
        /// </returns>
        public async Task<ServerResponse> SendRawTransaction(ServerConnection connection, string hexstring, bool allowhighfees = false) {
            _logger?.Info($"Beginning SendRawTransaction Function. Hexstring: {hexstring}, AllowHighFees: {allowhighfees}");

            //Set up parameters to get the hex string of the transaction
            JObject commandParams = new JObject();
            commandParams.Add("hexstring", hexstring);
            commandParams.Add("allowhighfees", allowhighfees);

            //Set up the Ravcencoin Object
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "sendrawtransaction",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Get the hex string of the transaction back from getrawtransaction, and then parse it to get just the raw hex string from result
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
        /// Decode a hex-encoded script.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="hexstring">(string) the hex encoded script</param>
        /// <returns>
        /// Result:
        ///{
        ///  "asm":"asm",   (string) Script public key
        ///  "hex":"hex",   (string) hex encoded public key
        ///  "type":"type", (string) The output type
        ///  "asset" : {               (json object) optional
        ///     "name" : "name",      (string) the asset name
        ///     "amount" : n,           (numeric) the amount of asset that was sent
        ///     "message" : "message", (string optional) the message if one was sent
        ///     "expire_time" : n,      (numeric optional ) the message epoch expiration time if one was set
        ///  "reqSigs": n,    (numeric) The required signatures
        ///  "addresses": [   (json array of string)
        ///     "address"     (string) raven address
        ///     ,...
        ///  ],
        ///  "p2sh":"address",       (string) address of P2SH script wrapping this redeem script(not returned if the script is already a P2SH).
        ///  "(The following only appears if the script is an asset script)
        ///  "asset_name":"name",      (string) Name of the asset.
        ///  "amount":"x.xx",          (numeric) The amount of assets interacted with.
        ///  "units": n,                (numeric) The units of the asset. (Only appears in the type (new_asset))
        ///  "reissuable": true|false, (boolean) If this asset is reissuable. (Only appears in type(new_asset|reissue_asset))
        ///  "hasIPFS": true|false,    (boolean) If this asset has an IPFS hash. (Only appears in type(new_asset if hasIPFS is true))
        ///  "ipfs_hash": "hash",      (string) The ipfs hash for the new asset. (Only appears in type(new_asset))
        ///  "new_ipfs_hash":"hash",    (string) If new ipfs hash(Only appears in type. (reissue_asset))
        ///}
        /// </returns>
        public async Task<ServerResponse> DecodeScript(ServerConnection connection, string hexstring) {
            _logger?.Info($"Beginning DecodeScript Function. Hexstring: {hexstring}");

            //Set up parameters to get the hex string of the transaction
            JObject commandParams = new JObject();
            commandParams.Add("hexstring", hexstring);

            //Set up the Ravcencoin Object
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "decodescript",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Get the hex string of the transaction back from getrawtransaction, and then parse it to get just the raw hex string from result
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
        /// Returns if raw transaction (serialized, hex-encoded) would be accepted by mempool.
        /// This checks if the transaction violates the consensus or policy rules.
        /// See sendrawtransaction call.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="rawtxs">(array, required) An array of hex strings of raw transactions. Length must be one for now.</param>
        /// <param name="allowhighfees">(boolean, optional, default=false) Allow high fees</param>
        /// <returns>
        /// Result:
        ///        [                   (array) The result of the mempool acceptance test for each raw transaction in the input array.
        ///                                    Length is exactly one for now.
        ///         {
        ///  "txid"           (string) The transaction hash in hex
        ///  "allowed"        (boolean) If the mempool allows this tx to be inserted
        ///  "reject-reason"  (string) Rejection string (only present when 'allowed' is false)
        /// }
        ///]
        /// </returns>
        public async Task<ServerResponse> TestMempoolAccept(ServerConnection connection, string[] rawtxs, bool allowhighfees = false) {
            _logger?.Info($"Beginning TestMempoolAccept Function. Raw Transactions: {rawtxs.ToString()}");

            //Set up parameters to get the hex string of the transaction
            JObject commandParams = new JObject();
            commandParams.Add("rawtxs", JArray.FromObject(rawtxs));
            commandParams.Add("allowhighfees", allowhighfees);

            //Set up the Ravcencoin Object
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "testmempoolaccept",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Get the hex string of the transaction back from getrawtransaction, and then parse it to get just the raw hex string from result
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
        /// Combine multiple partially signed transactions into one transaction.
        /// The combined transaction may be another partially signed transaction or a
        /// fully signed transaction.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="txs">(array) A json array of hex strings of partially signed transactions. ["hexstring"     (string) A transaction hash] </param>
        /// <returns>
        /// Result:
        ///"hex"            (string) The hex-encoded raw transaction with signature(s)
        /// </returns>
        public async Task<ServerResponse> CombineRawTransaction(ServerConnection connection, string[] txs)  {
            _logger?.Info($"Beginning CombineRawTransaction Function. Raw Transactions: {txs.ToString()}");

            //Set up parameters to get the hex string of the transaction
            JObject commandParams = new JObject();
            commandParams.Add("txs", JArray.FromObject(txs));

            //Set up the Ravcencoin Object
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "combinerawtransaction",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Get the hex string of the transaction back from getrawtransaction, and then parse it to get just the raw hex string from result
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
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="inputParams"></param>
        /// <param name="outputParams"></param>
        /// <returns></returns>
        public async Task<ServerResponse> CreateRawTransaction(ServerConnection connection, JArray inputParams, JObject outputParams) {
            _logger?.Info($"Beginning CreateRawTransaction Function. Raw Transactions:");

            

            //JObject outputParams = new JObject();
            //outputParams.Add(toAddress, amount);

            //Set up parameters to get the hex string of the transaction
            JObject commandParams = new JObject();
            commandParams.Add("inputs", JArray.FromObject(inputParams));
            commandParams.Add("outputs", outputParams);

            //Set up the Ravcencoin Object
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "createrawtransaction",
                commandParams = commandParams,
                commandJsonRpc = "2.0"
            };

            //Get the hex string of the transaction back from getrawtransaction, and then parse it to get just the raw hex string from result
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
