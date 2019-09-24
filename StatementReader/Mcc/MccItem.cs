using Newtonsoft.Json;

namespace StatementReader.Mcc
{
    public class MccItem
    {
        public int Code { get; set; }
        public string Description { get; set; }
    }

    public class IrsUsdaMccItem
    {
        [JsonProperty("mcc")]
        public int Code { get; set; }
        [JsonProperty("combined_description")]
        public string Description { get; set; }
    }
}
