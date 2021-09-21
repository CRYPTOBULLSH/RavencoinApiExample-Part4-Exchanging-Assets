using log4net;
using Newtonsoft.Json.Linq;
using Ravencoin.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ravencoin.ApplicationCore.BusinessLogic
{
    public class CustomTransactions
    {
        //Inject Logger
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CustomTransactions));
        
        //Create a ravencore instance to call against
        Transactions transactions = new Transactions();
        Blockchain blockchain = new Blockchain();
        Wallet wallet = new Wallet();
        /// <summary>
        /// Gets a transaction outside of your own wallet.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async Task<ServerResponse> GetPublicTransaction(ServerConnection connection, string txid) {
            //Get the RawTransaction and return the hexcode
            ServerResponse hexcode = await transactions.GetRawTransaction(connection, txid);

            //Get Full Transaction from Hexcode
            ServerResponse response = await transactions.DecodeRawTransaction(connection, hexcode.responseContent);

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
        /// Looks up the Transaction info from TxOut and responsds with the number of confirmations.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="txid">(Required) TransactionID</param>
        /// <param name="n">(Required) VOut number. Defaulted to 0 since this is the most common case, but overrideable.</param>
        /// <returns>string representation of number of confirmations</returns>
        public async Task<ServerResponse> GetTransactionConfirmations(ServerConnection connection, string txid, int n = 0 ) {
            //Get the RawTransaction and return the hexcode
            ServerResponse response = await blockchain.GetTxOut(connection, txid, n);

            //Parse the result for the confirmations
            JObject result = JObject.Parse(response.responseContent);

            //put the confirmations back into the ServerResponse object
            response.responseContent = result["result"]["confirmations"].ToString();

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
        /// Looks up a sender address from an incoming transaction to your wallet.
        /// WARNING: This will likely work with simple transactions to ravencore and other basic ravencoin wallets, but probably wont work with exchanges etc.
        /// USE AT YOUR OWN RISK
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async Task<ServerResponse> GetSenderAddress(ServerConnection connection, string txid) {
            //Get the transaction info for the incoming txid
            ServerResponse firstTxRequest = await GetPublicTransaction(connection, txid);
            JObject firstTxResponse = JObject.Parse(firstTxRequest.responseContent);

            //The assumption is, if we look up the vin txid from the incoming transaction, and look at the first address from the matched vout - this SHOULD be the owners wallet. 
            //USE AT YOUR OWN RISK, THIS IS NOT GUARANTEED
            string secondTxId = firstTxResponse["result"]["vin"][0]["txid"].ToString();
            //Grab the vout from the vin of the first transaction. We'll use this to match the previous transactions index of the vout.
            int voutToMatch = Int32.Parse(firstTxResponse["result"]["vin"][0]["vout"].ToString());

            ServerResponse response = await GetPublicTransaction(connection, secondTxId);
            JObject secondTxResponse = JObject.Parse(response.responseContent);
            //Parse out for the first address in vout

            response.responseContent = secondTxResponse["result"]["vout"][voutToMatch]["scriptPubKey"]["addresses"][0].ToString();

            //Check to see if this response is ok or not.
            if (response.statusCode == System.Net.HttpStatusCode.OK) {
                _logger?.Info($"Recieved Response: {response.responseContent}");
                return response;
            } else {
                _logger?.Error($"Received Error code: {response.statusCode}. Error: {response.errorEx}.");
                return response;
            }
        }

        public async Task<ServerResponse> CreateRawTransactionRVN(ServerConnection connection, List<TransactionInputs> transactionInputs, string to_address, double amount) {

            _logger?.Info($"Beginning CreateRawTransactionAssetTransfer Function. TXIDs: {transactionInputs.ToString()}, To Address: {to_address}, Amount: {amount}");

            JArray arrInputs = new JArray();
            // We taka an array list for inputs because we may need multiple txid's with unspent balance outputs to create the new transaction.
            // Loop through all the transactionInputs and put them in the array.
            foreach (var transactionInput in transactionInputs) {
                TransactionInputs tinput = new TransactionInputs {
                    txid = transactionInput.txid,
                    vout = transactionInput.vout,
                    sequence = transactionInput.sequence
                };
                arrInputs.Add(JObject.FromObject(tinput));
            }

            // Create and Output parameter object that is value:value (the address you're sending to and how much). Limit the amount to 8 decimal places.
            JObject outputParams = new JObject();
            outputParams.Add(to_address, Math.Round(amount, 8));

            ServerResponse response = await transactions.CreateRawTransaction(connection, arrInputs, outputParams);
            
            //Check to see if this response is ok or not.
            if (response.statusCode == System.Net.HttpStatusCode.OK) {
                _logger?.Info($"Recieved Response: {response.responseContent}");
                return response;
            } else {
                _logger?.Error($"Received Error code: {response.statusCode}. Error: {response.errorEx}.");
                return response;
            }
        }

        public async Task<ServerResponse> CreateRawTransactionAssetTransfer(ServerConnection connection, List<TransactionInputs> transactionInputs, string to_address, string asset_name, double asset_amount) {

            _logger?.Info($"Beginning CreateRawTransactionAssetTransfer Function. TXIDs: {transactionInputs.ToString()}, To Address: {to_address}, Asset Name: {asset_name}, Asset Amount: {asset_amount}");

            JArray arrInputs = new JArray();
            // We taka an array list for inputs because we may need multiple txid's with unspent balance outputs to create the new transaction.
            // Loop through all the transactionInputs and put them in the array.
            foreach (var transactionInput in transactionInputs) {
                TransactionInputs tinput = new TransactionInputs {
                    txid = transactionInput.txid,
                    vout = transactionInput.vout,
                    sequence = transactionInput.sequence
                };
                arrInputs.Add(JObject.FromObject(tinput));
            }

            // Add The asset name and amount to a JObject. This is value:value not key:value
            JObject assetParams = new JObject();
            assetParams.Add(asset_name, Math.Round(asset_amount, 8));

            // Wrap the asset parameters in a "transfer" JObject.
            JObject assetParamsRoot = new JObject();
            assetParamsRoot.Add("transfer", assetParams);

            // Wrap tnhe "transfer" JObject with yet another value:value object. Not a fan of this.
            JObject outputParams = new JObject();
            outputParams.Add(to_address, assetParamsRoot);

            ServerResponse response = await transactions.CreateRawTransaction(connection, arrInputs, outputParams);

            //Check to see if this response is ok or not.
            if (response.statusCode == System.Net.HttpStatusCode.OK) {
                _logger?.Info($"Recieved Response: {response.responseContent}");
                return response;
            } else {
                _logger?.Error($"Received Error code: {response.statusCode}. Error: {response.errorEx}.");
                return response;
            }
        }

        public async Task<TransactionType> GetTransactionType(string txid, ServerConnection serverConnection) {
            Transactions transactions = new Transactions();
            //Get our in-wallet transaction data. Unlike GetPublicTransaction, this will give us details like category, amount, confirmations etc.
            ServerResponse response = await wallet.GetTransaction(serverConnection, txid);
            JObject result = JObject.Parse(response.responseContent);
            JToken resultDetails = result["result"]["details"];
            //Get the data we need to ensure this transaction is something we want to send an asset to 
            //Check to see is this is a payment in RVN or a fee payment or asset transfer transaction
            //Check to see if there are asset details in the transaction to determine if it's an asset transaction
            JToken assetDetails = result["result"]["asset_details"];
            JToken feeDetails = result["result"]["fee"];
            //If it has values in asset_details, it's an asset transaction.
            if (assetDetails.HasValues && feeDetails == null) {
                if (assetDetails[0]["category"].ToString() == "receive") {
                    return new TransactionType { transactionTypes = TType.Asset, categoryTypes = CType.Receive };
                } else { return new TransactionType { transactionTypes = TType.Asset, categoryTypes = CType.Receive }; }
            }
            //If it doesn't have asset_details, but still has result details, its a RVN Transaction
            else if (resultDetails.HasValues) {
                if (resultDetails[0]["category"].ToString() == "receive") { return new TransactionType { transactionTypes = TType.RVN, categoryTypes = CType.Receive }; } else { return new TransactionType { transactionTypes = TType.RVN, categoryTypes = CType.Receive }; }
            }
        // If it doesn't have details at all, it's a fee.
        else {
                return new TransactionType { transactionTypes = TType.Fee };
            }
        }
    }
}
