using Ravencoin.ApplicationCore.Models;
using System.Threading.Tasks;
using Ravencoin.ApplicationCore.RpcConnections;
using Newtonsoft.Json.Linq;
using log4net;

namespace Ravencoin.ApplicationCore.BusinessLogic {
    public class Wallet {

        //Inject Logger
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Wallet));
        
        //Create a ravencore instance to call against
        private readonly RavenCore ravencore = new RavenCore();

        /// <summary>
        /// Gets the transaction details for an in-wallet transaction ID. This will only return data on your own wallets transactions, not external ones. Use GetPublicTransaction for out of wallet transactions
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="txid"></param>
        /// <returns>
        /// {
        ///  "amount" : x.xxx,        (numeric) The transaction amount in RVN
        ///  "fee": x.xxx,            (numeric) The amount of the fee in RVN.This is negative and only available for the 
        ///                              'send' category of transactions.
        ///  "confirmations" : n,     (numeric) The number of confirmations
        ///  "blockhash" : "hash",  (string) The block hash
        ///  "blockindex" : xx,       (numeric) The index of the transaction in the block that includes it
        ///  "blocktime" : ttt,       (numeric) The time in seconds since epoch(1 Jan 1970 GMT)
        ///  "txid" : "transactionid",   (string) The transaction id.
        ///  "time" : ttt, (numeric) The transaction time in seconds since epoch (1 Jan 1970 GMT)
        ///  "timereceived" : ttt,    (numeric) The time received in seconds since epoch(1 Jan 1970 GMT)
        ///  "bip125-replaceable": "yes|no|unknown",  (string) Whether this transaction could be replaced due to BIP125(replace-by-fee);
        ///        may be unknown for unconfirmed transactions not in the mempool
        ///  "details" : [
        ///    {
        ///      "account" : "accountname",      (string) DEPRECATED.The account name involved in the transaction, can be "" for the default account.
        ///      "address" : "address",          (string) The raven address involved in the transaction
        ///      "category" : "send|receive",    (string) The category, either 'send' or 'receive'
        ///      "amount" : x.xxx,                 (numeric) The amount in RVN
        ///      "label" : "label",              (string) A comment for the address/transaction, if any
        ///      "vout" : n,                       (numeric) the vout value
        ///      "fee": x.xxx,                     (numeric) The amount of the fee in RVN.This is negative and only available for the 
        ///                                           'send' category of transactions.
        ///      "abandoned": xxx(bool) 'true' if the transaction has been abandoned(inputs are respendable). Only available for the 
        ///                                           'send' category of transactions.
        ///    }
        ///    ,...
        ///  ],
        ///  "asset_details" : [
        ///    {
        ///      "asset_type" : "new_asset|transfer_asset|reissue_asset", (string) The type of asset transaction
        ///      "asset_name" : "asset_name",          (string) The name of the asset
        ///      "amount" : x.xxx,                 (numeric) The amount in RVN
        ///      "address" : "address",          (string) The raven address involved in the transaction
        ///      "vout" : n,                       (numeric) the vout value
        ///      "category" : "send|receive",    (string) The category, either 'send' or 'receive'
        ///    }
        ///    ,...
        ///  ],
        ///  "hex" : "data"(string) Raw data for transaction
        ///}
        /// </returns>
        public async Task<ServerResponse> GetTransaction(ServerConnection connection, string txid) {
            //Set up parameters to get the hex string of the transaction
            JObject commandParams = new JObject();
            commandParams.Add("txid", txid);

            //Set up the Ravcencoin Object
            ServerCommand request = new ServerCommand() {
                commandId = "0",
                commandMethod = "gettransaction",
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
                _logger?.Info($"Received Error code: {response.statusCode}. Error: {response.errorEx}.");
                return response;
            }
        }

    }
}
