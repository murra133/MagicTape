using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MagicTape
{
    public class ClientConfiguration
    {
        [JsonPropertyName("serverSocketUrl")]
        public string ServerSocketUrl { get; set; }
    }
}
