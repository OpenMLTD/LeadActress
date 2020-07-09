using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LeadActress.Runtime.Dancing {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class ExpressionMap {

        [JsonProperty]
        public int Version { get; set; }

        [JsonProperty]
        public Expression[] Expressions { get; set; }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public sealed class Expression {

            [JsonProperty]
            public int Key { get; set; }

            [JsonProperty]
            public string Description { get; set; }

            [JsonProperty("data")]
            public Dictionary<string, float> Table { get; set; }

        }

    }
}
