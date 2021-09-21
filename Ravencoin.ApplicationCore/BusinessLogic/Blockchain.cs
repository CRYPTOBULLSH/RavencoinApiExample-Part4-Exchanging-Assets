using Ravencoin.ApplicationCore.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ravencoin.ApplicationCore.RpcConnections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using log4net;

namespace Ravencoin.ApplicationCore.BusinessLogic
{
    public class Blockchain {
        //Inject Logger
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Blockchain));
        
        //Create a ravencore instance to call against
        private readonly RavenCore ravencore = new RavenCore();

        /// <summary>
        /// Removes all transaction from the mempool
        /// </summary>
        /// <param name="connection">ServerConnection (required)</param>
        /// <returns>
        /// void
        /// </returns>
        public async Task<ServerResponse> ClearMempool(ServerConnection connection) {
            _logger?.Info($"Beginning ClearMempool Function.");

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "clearmempool",
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
        /// Decodes a block
        /// </summary>
        /// <param name="connection">ServerConnection (required)</param>
        /// <param name="blockhex">(string, required) The block hex</param>
        /// <returns>
        /// void
        /// </returns>
        public async Task<ServerResponse> DecodeBlock(ServerConnection connection, string blockhex) {
            _logger?.Info($"Beginning DecodeBlock Function. Blockhex: {blockhex}");

            JObject commandParams = new JObject();
            commandParams.Add("blockhex", blockhex);

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "decodeblock",
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
        /// Returns the hash of the best (tip) block in the longest blockchain.
        /// </summary>
        /// <param name="connection">ServerConnection (required)</param>
        /// <returns>
        /// Result:
        /// "hex"      (string) the block hash hex encoded
        /// </returns>
        public async Task<ServerResponse> GetBestBlockhash(ServerConnection connection) {
            _logger?.Info($"Beginning GetBestBlockhash Function.");
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "getbestblockhash",
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
        /// Gets a block
        /// If verbosity is 0, returns a string that is serialized, hex-encoded data for block 'hash'.
        /// If verbosity is 1, returns an Object with information about block<hash>.
        /// If verbosity is 2, returns an Object with information about block<hash> and information about each transaction. 
        /// </summary>
        /// <param name="connection">ServerConnection (required)</param>
        /// <param name="blockhash">(string, required) The block hash</param>
        /// <param name="verbosity">(numeric, optional, default=1) 0 for hex encoded data, 1 for a json object, and 2 for json object with transaction data</param>
        /// <returns>
        /// Result (for verbosity = 0):
        ///"data"             (string) A string that is serialized, hex-encoded data for block 'hash'.
        ///Result(for verbosity = 1) :
        ///{
        ///            "hash" : "hash",     (string)the block hash(same as provided)
        ///  "confirmations" : n,   (numeric)The number of confirmations, or - 1 if the block is not on the main chain
        ///  "size" : n,            (numeric)The block size
        ///  "strippedsize" : n,    (numeric)The block size excluding witness data
        ///  "weight" : n(numeric) The block weight as defined in BIP 141
        ///  "height" : n,          (numeric)The block height or index
        ///  "version" : n,         (numeric)The block version
        ///  "versionHex" : "00000000", (string)The block version formatted in hexadecimal
        ///  "merkleroot" : "xxxx", (string)The merkle root
        ///  "tx" : [               (array of string) The transaction ids
        ///     "transactionid"(string) The transaction id
        ///     ,...
        ///  ],
        ///  "time" : ttt,          (numeric)The block time in seconds since epoch(Jan 1 1970 GMT)
        ///  "mediantime" : ttt,    (numeric)The median block time in seconds since epoch(Jan 1 1970 GMT)
        ///  "nonce" : n,           (numeric)The nonce
        ///  "bits" : "1d00ffff", (string)The bits
        ///  "difficulty" : x.xxx,  (numeric)The difficulty
        ///  "chainwork" : "xxxx",  (string)Expected number of hashes required to produce the chain up to this block(in hex)
        ///  "previousblockhash" : "hash",  (string)The hash of the previous block
        ///  "nextblockhash" : "hash"(string) The hash of the next block
        ///}
        ///        Result(for verbosity = 2) :
        ///        {
        ///            ...,                     Same output as verbosity = 1.
        ///          "tx" : [               (array of Objects) The transactions in the format of the getrawtransaction RPC.Different from verbosity = 1 "tx" result.
        ///                 ,...
        ///  ],
        ///  ,...                     Same output as verbosity = 1.
        ///        }
        /// </returns>
        public async Task<ServerResponse> GetBlock(ServerConnection connection, string blockhash, int verbosity = 1) {
            _logger?.Info($"Beginning GetBlock Function. Blockhash: {blockhash}, Verbosity: {verbosity}");

            JObject commandParams = new JObject();
            commandParams.Add("blockhash", blockhash);
            commandParams.Add("verbosity", verbosity);

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "getblock",
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
        /// Returns hash of block in best-block-chain at height provided.
        /// </summary>
        /// <param name="connection">ServerConnection (required)</param>
        /// <param name="height">(numeric, required) The height index</param>
        /// <returns>
        /// Result:
        /// "hash"         (string) The block hash
        /// </returns>
        public async Task<ServerResponse> GetBlockhash(ServerConnection connection, int height) {
            _logger?.Info($"Beginning GetBlockhash Function.");

            JObject commandParams = new JObject();
            commandParams.Add("height", height);

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "getblockhash",
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

        // GetBlockHashes currently does not support named parameters. Therefore, it's commented out.

        /// <summary>
        /// Returns array of hashes of blocks within the timestamp range provided.
        /// </summary>
        /// <param name="connection">ServerConnection (required)</param>
        /// <param name="high">(numeric, required) The newer block timestamp</param>
        /// <param name="low">(numeric, required) The older block timestamp</param>
        /// <param name="noOrphans">(boolean) will only include blocks on the main chain</param>
        /// <param name="logicalTime">(boolean) will only include blocks on the main chain</param>
        /// <returns>
        /// Result:
        /// [
        ///  "hash"         (string) The block hash
        ///]
        ///[
        ///  {
        ///    "blockhash": (string) The block hash
        ///    "logicalts": (numeric) The logical timestamp
        ///  }
        ///]
        /// </returns>
        //public async Task<ServerResponse> GetBlockhashes(ServerConnection connection, int high, int low, bool noOrphans = true, bool logicalTime = true) {

        //    //JObject payload = JObject.FromObject(new { high = high, low = low });
        //    //var result = ravencorestring.DoRPC<string>("getblockhashes", payload).Result;
        //    //return result;

        //    //Come back to this, it doesnt use named json objects, just expects a simple array in the parameter. Not sure how to do that easily.
        //    //_logger?.Info($"Beginning GetBlockHashes Function.");

        //    JObject options =  new JObject();
        //    options.Add("noOrphans", noOrphans);
        //    options.Add("logicalTime", logicalTime);

        //    JObject commandParams = new JObject();
        //    commandParams.Add("high", high);
        //    commandParams.Add("low", low);
        //    //commandParams.Add("options", options);

        //    ServerCommand request = new ServerCommand() {
        //        commandId = "0",
        //        commandMethod = "getblockhashes",
        //        commandParams = commandParams,
        //        commandJsonRpc = "2.0"
        //    };

        //    string str = JsonConvert.SerializeObject(request);
        //    ServerResponse response = await ravencore.Connect(request, connection);

        //    //Check to see if this response is ok or not.
        //    if (response.statusCode == System.Net.HttpStatusCode.OK) {
        //        _logger?.Info($"Recieved Response: {response.responseContent}");
        //        return response;
        //    } else {
        //        _logger?.Error($"Received Error code: {response.statusCode}. Error: {response.errorEx}.");
        //        return response;
        //    }
        //}

        /// <summary>
        /// If verbose is false, returns a string that is serialized, hex-encoded data for blockheader 'hash'.
        /// If verbose is true, returns an Object with information about blockheader<hash>.
        /// </summary>
        /// <param name="connection">ServerConnection (required)</param>
        /// <param name="hash">(string, required) The block hash</param>
        /// <param name="verbose">(boolean, optional, default=true) true for a json object, false for the hex encoded data</param>
        /// <returns>
        /// Result:(for verbose=false):
        /// "data"             (string) A string that is serialized, hex-encoded data for block 'hash'.
        /// </returns>
        public async Task<ServerResponse> GetBlockHeader(ServerConnection connection, string blockhash, bool verbose = true) {
            _logger?.Info($"Beginning GetBlockHeader Function. Hash: {blockhash}, Verbose: {verbose}");

            JObject commandParams = new JObject();
            commandParams.Add("blockhash", blockhash);
            commandParams.Add("verbose", verbose);

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "getblockheader",
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
        /// If txid is in the mempool, returns all in-mempool ancestors.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="txid">"txid"                 (string, required) The transaction id (must be in mempool)</param>
        /// <param name="verbose">verbose                  (boolean, optional, default=false) True for a json object, false for array of transaction ids</param>
        /// <returns>
        /// Result (for verbose=false):
        ///        [                       (json array of strings)
        ///  "transactionid"           (string) The transaction id of an in-mempool ancestor transaction
        ///  ,...
        ///]
        ///
        ///Result(for verbose= true) :
        ///{
        ///            (json object)
        ///  "transactionid" : {
        ///                (json object)
        ///    "size" : n,             (numeric) virtual transaction size as defined in BIP 141. This is different from actual serialized size for witness transactions as witness data is discounted.
        ///    "fee" : n,              (numeric) transaction fee in RVN
        ///    "modifiedfee" : n,      (numeric) transaction fee with fee deltas used for mining priority
        ///    "time" : n,             (numeric) local time transaction entered pool in seconds since 1 Jan 1970 GMT
        ///    "height" : n,           (numeric) block height when transaction entered pool
        ///    "descendantcount" : n,  (numeric) number of in-mempool descendant transactions(including this one)
        ///    "descendantsize" : n,   (numeric) virtual transaction size of in-mempool descendants(including this one)
        ///    "descendantfees" : n,   (numeric) modified fees(see above) of in-mempool descendants(including this one)
        ///    "ancestorcount" : n,    (numeric) number of in-mempool ancestor transactions(including this one)
        ///    "ancestorsize" : n,     (numeric) virtual transaction size of in-mempool ancestors(including this one)
        ///    "ancestorfees" : n,     (numeric) modified fees(see above) of in-mempool ancestors(including this one)
        ///    "wtxid" : hash,         (string) hash of serialized transaction, including witness data
        ///    "depends" : [           (array) unconfirmed transactions used as inputs for this transaction
        ///        "transactionid",    (string) parent transaction id
        ///       ... ]
        ///    }, ...
        ///}
        /// </returns>
        public async Task<ServerResponse> GetMempoolAncestors(ServerConnection connection, string txid, bool verbose = false) {
            _logger?.Info($"Beginning GetMempoolAncestors Function. TXID: {txid}, Verbose: {verbose}");

            JObject commandParams = new JObject();
            commandParams.Add("txid", txid);
            commandParams.Add("verbose", verbose);

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "getmempoolancestors",
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
        /// Return information about all known tips in the block tree, including the main chain as well as orphaned branches.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>
        /// Result:
        ///        [
        ///          {
        ///    "height": xxxx,         (numeric) height of the chain tip
        ///    "hash": "xxxx",         (string) block hash of the tip
        ///    "branchlen": 0          (numeric) zero for main chain
        ///    "status": "active"      (string) "active" for the main chain
        ///    },
        ///  {
        ///    "height": xxxx,
        ///    "hash": "xxxx",
        ///    "branchlen": 1          (numeric) length of branch connecting the tip to the main chain
        ///    "status": "xxxx"        (string) status of the chain(active, valid-fork, valid-headers, headers-only, invalid)
        ///  }
        /// ]
        /// Possible values for status:
        /// 1.  "invalid"               This branch contains at least one invalid block
        /// 2.  "headers-only"          Not all blocks for this branch are available, but the headers are valid
        /// 3.  "valid-headers"         All blocks are available for this branch, but they were never fully validated
        /// 4.  "valid-fork"            This branch is not part of the active chain, but is fully validated
        /// 5.  "active"                This is the tip of the active main chain, which is certainly valid
        ///  </returns>
        public async Task<ServerResponse> GetChainTips(ServerConnection connection) {
            _logger?.Info($"Beginning GetChainTips Function.");

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "getchaintips",
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
        /// Compute statistics about the total number and rate of transactions in the chain.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="nblocks">nblocks      (numeric, optional) Size of the window in number of blocks (default: one month).</param>
        /// <param name="blockhash">"blockhash"  (string, optional) The hash of the block that ends the window</param>
        /// <returns>
        /// Result:
        ///{
        ///  "time": xxxxx,                (numeric) The timestamp for the final block in the window in UNIX format.
        ///  "txcount": xxxxx, (numeric) The total number of transactions in the chain up to that point.
        ///  "window_block_count": xxxxx, (numeric) Size of the window in number of blocks.
        ///  "window_tx_count": xxxxx, (numeric) The number of transactions in the window. Only returned if "window_block_count" is > 0.
        ///  "window_interval": xxxxx, (numeric) The elapsed time in the window in seconds.Only returned if "window_block_count" is > 0.
        ///  "txrate": x.xx, (numeric) The average rate of transactions per second in the window. Only returned if "window_interval" is > 0.
        ///}
        /// </returns>
        public async Task<ServerResponse> GetChainTxStats(ServerConnection connection, int nblocks = 30, string blockhash = "") {
            _logger?.Info($"Beginning GetChainTxStats Function. NBlocks: {nblocks}, Blockhash: {blockhash}");

            JObject commandParams = new JObject();
            commandParams.Add("nblocks", nblocks);
            if (!string.IsNullOrEmpty(blockhash)){ commandParams.Add("blockhash", blockhash); };

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "getchaintxstats",
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
        /// Returns the proof-of-work difficulty as a multiple of the minimum difficulty.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>
        /// Result:
        /// n.nnn(numeric) the proof-of-work difficulty as a multiple of the minimum difficulty.
        /// </returns>
        public async Task<ServerResponse> GetDifficulty(ServerConnection connection) {
            _logger?.Info($"Beginning GetDifficulty Function.");

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "getdifficulty",
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
        /// If txid is in the mempool, returns all in-mempool descendants.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="txid">"txid"                 (string, required) The transaction id (must be in mempool)</param>
        /// <param name="verbose">verbose                  (boolean, optional, default=false) True for a json object, false for array of transaction ids</param>
        /// <returns>
        /// Result (for verbose=false):
        ///        [                       (json array of strings)
        ///  "transactionid"           (string) The transaction id of an in-mempool descendant transaction
        ///  ,...
        ///]
        ///
        ///Result(for verbose= true) :
        ///{
        ///            (json object)
        ///  "transactionid" : {
        ///                (json object)
        ///    "size" : n,             (numeric) virtual transaction size as defined in BIP 141. This is different from actual serialized size for witness transactions as witness data is discounted.
        ///    "fee" : n,              (numeric) transaction fee in RVN
        ///    "modifiedfee" : n,      (numeric) transaction fee with fee deltas used for mining priority
        ///    "time" : n,             (numeric) local time transaction entered pool in seconds since 1 Jan 1970 GMT
        ///    "height" : n,           (numeric) block height when transaction entered pool
        ///    "descendantcount" : n,  (numeric) number of in-mempool descendant transactions(including this one)
        ///    "descendantsize" : n,   (numeric) virtual transaction size of in-mempool descendants(including this one)
        ///    "descendantfees" : n,   (numeric) modified fees(see above) of in-mempool descendants(including this one)
        ///    "ancestorcount" : n,    (numeric) number of in-mempool ancestor transactions(including this one)
        ///    "ancestorsize" : n,     (numeric) virtual transaction size of in-mempool ancestors(including this one)
        ///    "ancestorfees" : n,     (numeric) modified fees(see above) of in-mempool ancestors(including this one)
        ///    "wtxid" : hash,         (string) hash of serialized transaction, including witness data
        ///    "depends" : [           (array) unconfirmed transactions used as inputs for this transaction
        ///        "transactionid",    (string) parent transaction id
        ///       ... ]
        ///    }, ...
        ///}
        /// </returns>
        public async Task<ServerResponse> GetMempoolDescendants(ServerConnection connection, string txid, bool verbose = false) {
            _logger?.Info($"Beginning GetMempoolDescendants Function. TXID: {txid}, Verbose: {verbose}");

            JObject commandParams = new JObject();
            commandParams.Add("txid", txid);
            commandParams.Add("verbose", verbose);

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "getmempooldescendants",
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
        /// Returns mempool data for given transaction
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="txid">"txid"                   (string, required) The transaction id (must be in mempool)</param>
        /// <returns>
        /// Result:
        ///{                           (json object)
        ///    "size" : n,             (numeric) virtual transaction size as defined in BIP 141. This is different from actual serialized size for witness transactions as witness data is discounted.
        ///    "fee" : n,              (numeric) transaction fee in RVN
        ///    "modifiedfee" : n,      (numeric) transaction fee with fee deltas used for mining priority
        ///    "time" : n,             (numeric) local time transaction entered pool in seconds since 1 Jan 1970 GMT
        ///    "height" : n,           (numeric) block height when transaction entered pool
        ///    "descendantcount" : n,  (numeric) number of in-mempool descendant transactions(including this one)
        ///    "descendantsize" : n,   (numeric) virtual transaction size of in-mempool descendants(including this one)
        ///    "descendantfees" : n,   (numeric) modified fees(see above) of in-mempool descendants(including this one)
        ///    "ancestorcount" : n,    (numeric) number of in-mempool ancestor transactions(including this one)
        ///    "ancestorsize" : n,     (numeric) virtual transaction size of in-mempool ancestors(including this one)
        ///    "ancestorfees" : n,     (numeric) modified fees(see above) of in-mempool ancestors(including this one)
        ///    "wtxid" : hash,         (string) hash of serialized transaction, including witness data
        ///    "depends" : [           (array) unconfirmed transactions used as inputs for this transaction
        ///        "transactionid",    (string) parent transaction id
        ///       ... ]
        ///    }
        /// </returns>
        public async Task<ServerResponse> GetMempoolEntry(ServerConnection connection, string txid) {
            _logger?.Info($"Beginning GetMempoolEntry Function. TXID: {txid}");

            JObject commandParams = new JObject();
            commandParams.Add("txid", txid);


            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "getmempoolentry",
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
        /// Returns details on the active state of the TX memory pool.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>
        /// Result:
        ///{
        ///  "size": xxxxx,               (numeric) Current tx count
        ///  "bytes": xxxxx,              (numeric) Sum of all virtual transaction sizes as defined in BIP 141. Differs from actual serialized size because witness data is discounted
        ///  "usage": xxxxx,              (numeric) Total memory usage for the mempool
        ///  "maxmempool": xxxxx,         (numeric) Maximum memory usage for the mempool
        ///  "mempoolminfee": xxxxx(numeric) Minimum fee rate in RVN/kB for tx to be accepted
        ///}
        /// </returns>
        public async Task<ServerResponse> GetMemPoolInfo(ServerConnection connection) {
            _logger?.Info($"Beginning GetMemPoolInfo Function.");

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "getmempoolinfo",
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
        /// Returns all transaction ids in memory pool as a json array of string transaction ids.
        /// Hint: use getmempoolentry to fetch a specific transaction from the mempool.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="verbose">verbose (boolean, optional, default=false) True for a json object, false for array of transaction ids</param>
        /// <returns>
        /// Result: (for verbose = false):
        ///        [                     (json array of string)
        ///  "transactionid"     (string) The transaction id
        ///  ,...
        ///]
        ///
        ///Result: (for verbose = true):
        ///{                           (json object)
        ///  "transactionid" : {       (json object)
        ///    "size" : n,             (numeric) virtual transaction size as defined in BIP 141. This is different from actual serialized size for witness transactions as witness data is discounted.
        ///    "fee" : n,              (numeric) transaction fee in RVN
        ///    "modifiedfee" : n,      (numeric) transaction fee with fee deltas used for mining priority
        ///    "time" : n,             (numeric) local time transaction entered pool in seconds since 1 Jan 1970 GMT
        ///    "height" : n,           (numeric) block height when transaction entered pool
        ///    "descendantcount" : n,  (numeric) number of in-mempool descendant transactions(including this one)
        ///    "descendantsize" : n,   (numeric) virtual transaction size of in-mempool descendants(including this one)
        ///    "descendantfees" : n,   (numeric) modified fees(see above) of in-mempool descendants(including this one)
        ///    "ancestorcount" : n,    (numeric) number of in-mempool ancestor transactions(including this one)
        ///    "ancestorsize" : n,     (numeric) virtual transaction size of in-mempool ancestors(including this one)
        ///    "ancestorfees" : n,     (numeric) modified fees(see above) of in-mempool ancestors(including this one)
        ///    "wtxid" : hash,         (string) hash of serialized transaction, including witness data
        ///    "depends" : [           (array) unconfirmed transactions used as inputs for this transaction
        ///        "transactionid",    (string) parent transaction id
        ///       ... ]
        ///    }, ...
        ///}
        /// </returns>
        public async Task<ServerResponse> GetRawMempool(ServerConnection connection, bool verbose = false) {
            _logger?.Info($"Beginning GetRawMempool Function. Verbose: {verbose}");

            JObject commandParams = new JObject();
            commandParams.Add("verbose", verbose);


            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "getrawmempool",
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
        /// Treats a block as if it were received before others with the same work.
        /// A later preciousblock call can override the effect of an earlier one.
        /// The effects of preciousblock are not retained across restarts
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="blockhash">"blockhash"   (string, required) the hash of the block to mark as precious</param>
        /// <returns></returns>
        public async Task<ServerResponse> PreciousBlock(ServerConnection connection, string blockhash) {
            _logger?.Info($"Beginning PreciousBlock Function. Blockhash: {blockhash}");

            JObject commandParams = new JObject();
            commandParams.Add("blockhash", blockhash);


            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "preciousblock",
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
        /// Prunes the blockchain
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="height">"height"       (numeric, required) The block height to prune up to. May be set to a discrete height, or a unix timestamp to prune blocks whose block time is at least 2 hours older than the provided timestamp.</param>
        /// <returns>
        /// Result:
        /// n(numeric) Height of the last block pruned.
        /// </returns>
        public async Task<ServerResponse> PruneBlockchain(ServerConnection connection, int height) {
            _logger?.Info($"Beginning PruneBlockchain Function. Height: {height}");

            JObject commandParams = new JObject();
            commandParams.Add("height", height);


            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "pruneblockchain",
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
        /// Returns details on the active state of the TX memory pool.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>
        /// Result:
        ///{
        ///  "size": xxxxx,               (numeric) Current tx count
        ///  "bytes": xxxxx,              (numeric) Sum of all virtual transaction sizes as defined in BIP 141. Differs from actual serialized size because witness data is discounted
        ///  "usage": xxxxx,              (numeric) Total memory usage for the mempool
        ///  "maxmempool": xxxxx,         (numeric) Maximum memory usage for the mempool
        ///  "mempoolminfee": xxxxx(numeric) Minimum fee rate in RVN/kB for tx to be accepted
        ///}
        /// </returns>
        public async Task<ServerResponse> SaveMempool(ServerConnection connection) {
            _logger?.Info($"Beginning SaveMempool Function.");

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "savemempool",
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
        /// Verifies blockchain database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="checklevel">checklevel   (numeric, optional, 0-4, default=3) How thorough the block verification is.</param>
        /// <param name="nblocks">nblocks      (numeric, optional, default=6, 0=all) The number of blocks to check.</param>
        /// <returns>
        /// Result:
        ///true|false       (boolean) Verified or not
        /// </returns>
        public async Task<ServerResponse> VerifyChain(ServerConnection connection, int checklevel = 3, int nblocks = 6) {
            _logger?.Info($"Beginning VerifyChain Function. Check Level: {checklevel}, Number of Blocks: {nblocks}");

            JObject commandParams = new JObject();
            commandParams.Add("checklevel", checklevel);
            commandParams.Add("nblocks", nblocks);

            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "verifychain",
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
        /// Returns an object containing various state info regarding blockchain processing.
        /// </summary>
        /// <param name="connection">ServerConnection (required)</param>
        /// <returns>
        /// Result:
        ///{
        ///  "chain": "xxxx",        (string) current network name as defined in BIP70(main, test, regtest)
        ///  "blocks": xxxxxx,         (numeric) the current number of blocks processed in the server
        ///  "headers": xxxxxx,        (numeric) the current number of headers we have validated
        ///  "bestblockhash": "...", (string) the hash of the currently best block
        ///  "difficulty": xxxxxx,     (numeric) the current difficulty
        ///  "mediantime": xxxxxx,     (numeric) median time for the current best block
        ///  "verificationprogress": xxxx, (numeric) estimate of verification progress[0..1]
        ///  "chainwork": "xxxx"     (string) total amount of work in active chain, in hexadecimal
        ///  "size_on_disk": xxxxxx,   (numeric) the estimated size of the block and undo files on disk
        ///  "pruned": xx,             (boolean) if the blocks are subject to pruning
        ///  "pruneheight": xxxxxx,    (numeric) lowest-height complete block stored(only present if pruning is enabled)
        ///  "automatic_pruning": xx,  (boolean) whether automatic pruning is enabled(only present if pruning is enabled)
        ///  "prune_target_size": xxxxxx,  (numeric) the target size used by pruning(only present if automatic pruning is enabled)
        ///  "softforks": [            (array) status of softforks in progress
        ///     {
        ///        "id": "xxxx",        (string) name of softfork
        ///        "version": xx,         (numeric) block version
        ///        "reject": {            (object) progress toward rejecting pre-softfork blocks
        ///           "status": xx,       (boolean) true if threshold reached
        ///        },
        ///     }, ...
        ///  ],
        ///  "bip9_softforks": {
        ///    (object)status of BIP9 softforks in progress
        ///     "xxxx" : {
        ///        (string)name of the softfork
        ///        "status": "xxxx",    (string)one of "defined", "started", "locked_in", "active", "failed"
        ///        "bit": xx,             (numeric)the bit(0 - 28) in the block version field used to signal this softfork(only for "started" status)
        ///            "startTime": xx,       (numeric)the minimum median time past of a block at which the bit gains its meaning
        ///        "timeout": xx,         (numeric)the median time past of a block at which the deployment is considered failed if not yet locked in
        ///        "since": xx,           (numeric)height of the first block to which the status applies
        ///        "statistics": {
        ///            (object)numeric statistics about BIP9 signalling for a softfork (only for "started" status)
        ///                    "period": xx,       (numeric)the length in blocks of the BIP9 signalling period
        ///           "threshold": xx,    (numeric)the number of blocks with the version bit set required to activate the feature
        ///           "elapsed": xx,      (numeric)the number of blocks elapsed since the beginning of the current period
        ///           "count": xx,        (numeric)the number of blocks with the version bit set in the current period
        ///           "possible": xx(boolean) returns false if there are not enough blocks left in this period to pass activation threshold
        ///        }
        ///    }
        ///}
        ///"warnings" : "...",         (string)any network and blockchain warnings.
        ///}
        /// </returns>
        public async Task<ServerResponse> GetBlockchainInfo(ServerConnection connection)
        {
            _logger?.Info($"Beginning GetBlockchainInfo Function.");

            ServerCommand request = new ServerCommand(){
                commandId = "0",
                commandMethod = "getblockchaininfo",
                commandJsonRpc = "2.0"
            };

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
        /// Returns the number of blocks in the longest blockchain.
        /// </summary>
        /// <param name="connection">ServerConnection (required)</param>
        /// <returns>
        /// Result:
        /// n(numeric) The current block count
        /// </returns>
        public  async Task<ServerResponse> GetBlockCount(ServerConnection connection)
        {
            _logger?.Info($"Beginning GetBlockCount Function.");
            ServerCommand request = new ServerCommand(){
                commandId = "0",
                commandMethod = "getblockcount",
                commandJsonRpc = "2.0"
            };

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
        /// Returns details about an unspent transaction output.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="txid">"txid"             (string, required) The transaction id</param>
        /// <param name="n">"n"                (numeric, required) vout number</param>
        /// <param name="include_mempool">"include_mempool"  (boolean, optional) Whether to include the mempool. Default: true.     Note that an unspent output that is spent in the mempool won't appear.</param>
        /// <returns>
        /// Result:
        ///{
        ///  "bestblock" : "hash",    (string) the block hash
        ///  "confirmations" : n,       (numeric) The number of confirmations
        ///  "value" : x.xxx,           (numeric) The transaction value in RVN
        ///  "scriptPubKey" : {         (json object)
        ///     "asm" : "code",       (string) 
        ///     "hex" : "hex",        (string) 
        ///     "reqSigs" : n,          (numeric) Number of required signatures
        ///     "type" : "pubkeyhash", (string) The type, eg pubkeyhash
        ///     "addresses" : [          (array of string) array of raven addresses
        ///        "address"     (string) raven address
        ///        ,...
        ///     ]
        ///  },
        ///  "coinbase" : true|false   (boolean) Coinbase or not
        ///}
        /// </returns>
        public async Task<ServerResponse> GetTxOut(ServerConnection connection, string txid, int n, bool include_mempool = true) {

            _logger?.Info($"Beginning VerifyChain Function. TXID: {txid}, N (VOut Number): {n}, Include Mempool: {include_mempool}");

            //Set up parameters to get the hex string of the transaction
            JObject commandParams = new JObject();
            commandParams.Add("txid", txid);
            commandParams.Add("n", n);
            commandParams.Add("include_mempool", include_mempool);

            //Set up the Ravcencoin Object
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "gettxout",
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
