using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Queeni.Data
{
    public class QueeniConfigModel
    {
        public string SecretKey { get; set; }
        public string ScratchpadSigningKey { get; set; }
        public string DatabaseKey { get; set; }
        public string OpenAiApiKey { get; set; }

        public string AblyKey { get; set; }

        public string AblyChannel { get; set; }

        public bool IsLive { get; set; } = true;

        [JsonIgnore]
        public string Messages { get; set; }
    }
}
