using log4net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Ravencoin.ApplicationCore.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Ravencoin.ApplicationCore.BusinessLogic
{
    public class CustomAssets
    {
        //Inject Logger
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CustomAssets));
        
        //Create a ravencore instance to call against
        private readonly Transactions transactions = new Transactions();
        private readonly Utilities utilities = new Utilities();
        private readonly CustomTransactions customtransactions = new CustomTransactions();
        private readonly Wallet wallet = new Wallet();

        public async Task<ServerResponse> ExchangeRvnForAsset(string txid, string rvnReceiveAddress, string asset, int? multiplier, int minConfirmations, ServerConnection serverConnection)
        {
            _logger?.Info($"TransactionID {txid}: Beginning ExchangeRvnForAsset Function. Transaction ID: {txid}, Ravencoin Receive Address: {rvnReceiveAddress}, Asset: {asset}, Multiplier: {multiplier}, Minimum Confirmations: {minConfirmations}");
            int quantity = new int();
        
            //Get our in-wallet transaction data. Unlike GetPublicTransaction, this will give us details like category, amount, confirmations etc.
            ServerResponse response = await wallet.GetTransaction(serverConnection, txid);
            JObject result = JObject.Parse(response.responseContent);

            if (result != null)
            {
                _logger?.Info($"TransactionID {txid}: Received Result for txid: {txid}. Result: {response.responseContent}.");
                //Get the data we need to ensure this transaction is something we want to send an asset to 
                //Check to see is this is a payment in RVN or a fee payment or asset transfer transaction
                JToken detailsArray = result["result"]["details"];
                if (detailsArray.HasValues)
                {
                    string category = result["result"]["details"][0]["category"].ToString();
                    string receiveAddress = result["result"]["details"][0]["address"].ToString();
                    double amountReceived = Double.Parse(result["result"]["amount"].ToString());
                    int confirmations = Int32.Parse(result["result"]["confirmations"].ToString());

                    //Round down the amount received. Rounding down discourages sending fractional RVN :)
                    int finalAmountReceived = Convert.ToInt32(Math.Floor(amountReceived));

                    //Check if we have a multiplier, and if its higher than one. If the multiplier is set to 100 for example, every 1 RVN Received will get 100 assets in response.
                    if (multiplier > 1 || multiplier != null) { quantity = (int)(finalAmountReceived * multiplier); }

                    //Check to see if we have enough of this asset to send.
                    Assets assets = new Assets();
                    ServerResponse assetBalanceRequest = await assets.ListMyAssets(serverConnection, asset);
                    JObject assetBalanceResponse = JObject.Parse(assetBalanceRequest.responseContent);
                    int assetBalance = Int32.Parse(assetBalanceResponse["result"][asset].ToString());
                    _logger?.Info($"TransactionID {txid}: Asset Balance of {asset}: {assetBalance}");

                    if (assetBalance >= quantity)
                    {
                        _logger?.Info($"TransactionID {txid}: Asset Balance of {asset} is greater than the amount being sent: Balance {assetBalance}, Sending: {quantity}");
                        // Requirements:
                        // Category must be "receive" aka an icoming transaction
                        // Our expecteded receive address must match the one we're watching for
                        // The total amount of RVN is 1 or more.
                        // Confirmations must be equal or greater to the minimum confirmations we want. 
                        if (category == "receive" && receiveAddress == rvnReceiveAddress && finalAmountReceived >= 1 && confirmations >= minConfirmations)
                        {
                            _logger?.Info($"TransactionID {txid}: Met the requirements to send Asset {asset}");
                            //Do a GetPublicTransaction on the txid to grab the vout address
                            _logger?.Info($"TransactionID {txid}: Getting Sender Address");
                            ServerResponse getSenderAddress = await customtransactions.GetSenderAddress(serverConnection, txid);
                            if (getSenderAddress != null)
                            {
                                _logger?.Info($"TransactionID {txid}: Got Sender Address: {getSenderAddress.responseContent}");
                                //Validate it's a valid Ravencoin Address  
                                _logger?.Info($"TransactionID {txid}: Checking if sender address is valid.");
                                ServerResponse checkIfValid = await utilities.ValidateAddress(getSenderAddress.responseContent, serverConnection);
                                JObject checkIfValidResponse = JObject.Parse(checkIfValid.responseContent);
                                bool isValid = bool.Parse(checkIfValidResponse["result"]["isvalid"].ToString());

                                if (isValid == true){
                                    _logger?.Info($"TransactionID {txid}: Sender Address is valid.");
                                    _logger?.Info($"TransactionID {txid}: Sending {quantity} of Asset {asset} to {getSenderAddress.responseContent}");
                                    ServerResponse sendAsset = await assets.Transfer(serverConnection, asset, quantity, getSenderAddress.responseContent);
                                    _logger?.Info($"TransactionID {txid}: Sent {quantity} of Asset {asset} to {getSenderAddress.responseContent}");
                                    return sendAsset;
                                }
                                else{
                                    _logger?.Error($"TransactionID {txid}: Invalid Sender Address.");
                                    return new ServerResponse { statusCode = HttpStatusCode.InternalServerError, errorEx = "Invalid sender address" };
                                }
                            }
                            else
                            {
                                _logger?.Error($"TransactionID {txid}: Could not find Sender Addreess.");
                                return new ServerResponse { statusCode = HttpStatusCode.InternalServerError, errorEx = "Could not find Sender Address" };
                            }
                        }
                        else
                        {
                            _logger?.Error($"TransactionID {txid}: The transaction didn't meet the minimum requirements. Incoming Address: {rvnReceiveAddress}, Amount: {finalAmountReceived}, Category: {category}, Confirmationss: { confirmations }");
                            return new ServerResponse { statusCode = HttpStatusCode.InternalServerError, errorEx = $"The transaction didn't meet the minimum requirements. Incoming Address: {rvnReceiveAddress}, Amount: {finalAmountReceived}, Category: {category}, Confirmationss: { confirmations }" };
                        }
                    }
                    else
                    {
                        _logger?.Error($"TransactionID {txid}: Not enough Asset: {asset} left in wallet.");
                        return new ServerResponse { statusCode = HttpStatusCode.InternalServerError, errorEx = $"Not enough Asset: {asset} left in wallet." };
                    }
                }
                else
                {
                    _logger?.Error($"TransactionID {txid}: Transaction {txid} has no details section. Likely a fee transaction.");
                    return new ServerResponse { statusCode = HttpStatusCode.InternalServerError, errorEx = $"Transaction {txid} has no details section. Likely a fee transaction." };
                }
            }
            else
            {
                _logger?.Error($"TransactionID {txid}: Could not look up Transaction {txid}.");
                return new ServerResponse { statusCode = HttpStatusCode.InternalServerError, errorEx = $"Could not look up Transaction {txid}." };
            }
        }

        public async Task<ServerResponse> ExchangeAssetForAsset(string txid, string assetReceiveAddress, string expectedIncomingAsset, string assetToSend, int? multiplier, int minConfirmations, ServerConnection serverConnection)
        {
            _logger?.Info($"TransactionID {txid}: Beginning ExchangeRvnForAsset Function. Transaction ID: {txid}, Asset Receive Address: {assetReceiveAddress}, Asset Received: {expectedIncomingAsset}, Asset To Send: {assetToSend}, Multiplier: {multiplier}, Minimum Confirmations: {minConfirmations}");
            int quantity = new int();
            Transactions transactions = new Transactions();
            Utilities utilities = new Utilities();

            //Get our in-wallet transaction data. Unlike GetPublicTransaction, this will give us details like category, amount, confirmations etc.
            ServerResponse response = await wallet.GetTransaction(serverConnection, txid);
            JObject result = JObject.Parse(response.responseContent);

            if (result != null)
            {
                //Get the data we need to ensure this transaction is something we want to send an asset to 
                //Check to see is this is a payment in RVN or a fee payment or asset transfer transaction
                JToken detailsArray = result["result"]["asset_details"];
                if (detailsArray.HasValues)
                {
                    string category = result["result"]["asset_details"][0]["category"].ToString();
                    string receiveAddress = result["result"]["asset_details"][0]["destination"].ToString();
                    double amountReceived = Double.Parse(result["result"]["asset_details"][0]["amount"].ToString());
                    string txAssetReceived = result["result"]["asset_details"][0]["asset_name"].ToString();
                    int confirmations = Int32.Parse(result["result"]["confirmations"].ToString());

                    //Round down the amount received. Rounding down discourages sending fractional RVN :)
                    int finalAmountReceived = Convert.ToInt32(Math.Floor(amountReceived));

                    //Check if we have a multiplier, and if its higher than one. If the multiplier is set to 100 for example, every 1 RVN Received will get 100 assets in response.
                    if (multiplier > 1 || multiplier != null) { quantity = (int)(finalAmountReceived * multiplier); }

                    //Check to see if assetReceived is set to * (wildcard). If it is we automatically set the assetMatch to true
                    bool assetMatch = false;
                    if (expectedIncomingAsset == "*") {
                        assetMatch = true;
                    } else if (expectedIncomingAsset == txAssetReceived) {
                        assetMatch = true;
                    };


                    //Check to see if we have enough of this asset to send.
                    Assets assets = new Assets();
                    ServerResponse assetBalanceRequest = await assets.ListMyAssets(serverConnection, assetToSend);
                    JObject assetBalanceResponse = JObject.Parse(assetBalanceRequest.responseContent);
                    int assetBalance = Int32.Parse(assetBalanceResponse["result"][assetToSend].ToString());

                    if (assetBalance >= quantity)
                    {
                        // Requirements:
                        // Category must be "receive" aka an icoming transaction
                        // Our expecteded receive address must match the one we're watching for
                        // The total amount of Assets is 1 or more.
                        // Confirmations must be equal or greater to the minimum confirmations we want. 
                        if (category == "receive" && receiveAddress == assetReceiveAddress && finalAmountReceived >= 1 && confirmations >= minConfirmations && assetMatch == true)
                        {
                            //Do a GetPublicTransaction on the txid to grab the vout address
                            ServerResponse getSenderAddress = await customtransactions.GetSenderAddress(serverConnection, txid);

                            if (getSenderAddress != null)
                            {
                                //Validate it's a valid Ravencoin Address  
                                ServerResponse checkIfValid = await utilities.ValidateAddress(getSenderAddress.responseContent, serverConnection);
                                JObject checkIfValidResponse = JObject.Parse(checkIfValid.responseContent);
                                bool isValid = bool.Parse(checkIfValidResponse["result"]["isvalid"].ToString());

                                if (isValid == true)
                                {
                                    ServerResponse sendAsset = await assets.Transfer(serverConnection, assetToSend, quantity, getSenderAddress.responseContent);
                                    sendAsset.errorEx = confirmations.ToString();
                                    return sendAsset;
                                }
                                else
                                {
                                    return new ServerResponse { statusCode = HttpStatusCode.InternalServerError, errorEx = "Invalid sender address" };
                                }
                            }
                            else
                            {
                                return new ServerResponse { statusCode = HttpStatusCode.InternalServerError, errorEx = "Could not find Sender Address" };
                            }
                        }
                        else
                        {
                            return new ServerResponse { statusCode = HttpStatusCode.InternalServerError, errorEx = $"The transaction didn't meet the minimum requirements. Incoming Address: {receiveAddress}, Amount: {finalAmountReceived}, Category: {category}, Confirmationss: { confirmations }" };
                        }
                    }
                    else
                    {
                        return new ServerResponse { statusCode = HttpStatusCode.InternalServerError, errorEx = $"Not enough Asset: {assetToSend} left in wallet." };
                    }
                }
                else
                {
                    return new ServerResponse { statusCode = HttpStatusCode.InternalServerError, errorEx = $"Transaction {txid} has no details section. Likely a fee transaction or an asset transfer transaction." };
                }
            }
            else
            {
                return new ServerResponse { statusCode = HttpStatusCode.InternalServerError, errorEx = $" Could not look up Transaction {txid}" };
            }
        }

        
    }
}
