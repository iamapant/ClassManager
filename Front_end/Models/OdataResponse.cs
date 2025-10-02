using Newtonsoft.Json;

namespace Front_end.Models
{
    public class OdataResponse<T> 
    {
        [JsonProperty("@odata.context")]
        public string Context { get; set; }
        [JsonProperty("value")]
        public T Value { get; set; }
    }
}
