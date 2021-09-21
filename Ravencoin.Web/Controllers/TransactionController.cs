using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ravencoin.ApplicationCore.Models;
using Ravencoin.ApplicationCore.BusinessLogic;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Ravencoin.Web.Controllers
{
    [ApiController]
    [Route("api/Transactions/[action]")]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger logger;

        //Inject Server Configuration
        private readonly IOptions<ServerConnection> serverConnection;
        public TransactionController(IOptions<ServerConnection> serverConnection, ILogger<TransactionController> logger)
        {
            this.serverConnection = serverConnection;
            this.logger = logger;
        }

        Transactions transactions = new Transactions();
        CustomTransactions customtransactions = new CustomTransactions();
        Wallet wallet = new Wallet();
        Blockchain blockchain = new Blockchain();

        [HttpGet]
        public async Task<ServerResponse> GetPublicTransaction(string txid)
        {
            logger.LogInformation($"Getting Transaction data for {txid}");
            try
            {
                ServerResponse response = await customtransactions.GetPublicTransaction(serverConnection.Value, txid);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception: {ex.Message}");
                ServerResponse errResponse = new ServerResponse()
                {
                    statusCode = System.Net.HttpStatusCode.InternalServerError,
                    errorEx = ex.Message
                };
                return errResponse;
            }
        }

        [HttpGet]
        public async Task<ServerResponse> GetTransaction(string txid)
        {
            logger.LogInformation($"Getting Transaction data for {txid}");
            try
            {
                ServerResponse response = await wallet.GetTransaction(serverConnection.Value, txid);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception: {ex.Message}");
                ServerResponse errResponse = new ServerResponse()
                {
                    statusCode = System.Net.HttpStatusCode.InternalServerError,
                    errorEx = ex.Message
                };
                return errResponse;
            }
        }

        [HttpGet]
        public async Task<ServerResponse> GetTxOut(string txid, int n)
        {
            logger.LogInformation($"Getting Transaction txout data for {txid}");
            try
            {
                ServerResponse response = await blockchain.GetTxOut(serverConnection.Value, txid, n);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception: {ex.Message}");
                ServerResponse errResponse = new ServerResponse()
                {
                    statusCode = System.Net.HttpStatusCode.InternalServerError,
                    errorEx = ex.Message
                };
                return errResponse;
            }
        }

        [HttpGet]
        public async Task<ServerResponse> GetTransactionConfirmations(string txid)
        {
            logger.LogInformation($"Getting Transaction confirmations data for {txid}");
            try
            {
                ServerResponse response = await customtransactions.GetTransactionConfirmations(serverConnection.Value, txid);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception: {ex.Message}");
                ServerResponse errResponse = new ServerResponse()
                {
                    statusCode = System.Net.HttpStatusCode.InternalServerError,
                    errorEx = ex.Message
                };
                return errResponse;
            }
        }

        [HttpGet]
        public async Task<ServerResponse> GetSenderAddress(string txid)
        {
            logger.LogInformation($"Getting Sender address data for {txid}");
            try
            {
                ServerResponse response = await customtransactions.GetSenderAddress(serverConnection.Value, txid);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception: {ex.Message}");
                ServerResponse errResponse = new ServerResponse()
                {
                    statusCode = System.Net.HttpStatusCode.InternalServerError,
                    errorEx = ex.Message
                };
                return errResponse;
            }
        }
    }
}
