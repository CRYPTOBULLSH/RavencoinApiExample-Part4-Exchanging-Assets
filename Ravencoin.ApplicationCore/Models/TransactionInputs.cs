using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ravencoin.ApplicationCore.Models {
    public class TransactionInputs {
        [JsonProperty(PropertyName = "txid")]
        public string txid { get; set; }

        [JsonProperty(PropertyName = "vout")]
        public int vout { get; set; }

        [JsonProperty(PropertyName = "sequence")]
        public int sequence { get; set; } = 0;
    }
}
