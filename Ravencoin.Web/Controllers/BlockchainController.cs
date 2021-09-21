using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Ravencoin.ApplicationCore.BusinessLogic;
using Ravencoin.ApplicationCore.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using log4net;

namespace Ravencoin.Web.Controllers
{

    [ApiController]
    [Route("/api/Blockchain/[action]")]
    public class BlockchainController : Controller
    {
        //Inject Server Configuration
        private static readonly ILog _logger = LogManager.GetLogger(typeof(BlockchainController));

        private readonly IOptions<ServerConnection> serverConnection;
        public BlockchainController(IOptions<ServerConnection> serverConnection)
        {
            this.serverConnection = serverConnection;
        }

        Blockchain blockchain = new Blockchain();

        public async Task<ServerResponse> GetBlockchainInfo(){
            _logger.Info("Stating GetBlockchainInfo method");
            ServerResponse response = await blockchain.GetBlockchainInfo(serverConnection.Value);
            return response;
        }

        public async Task<ServerResponse> GetBlockCount(){
            ServerResponse response = await blockchain.GetBlockCount(serverConnection.Value);
            return response;
        }
    }
}
