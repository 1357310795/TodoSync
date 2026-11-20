using Newtonsoft.Json;

namespace TodoSynchronizer.Core.Models.DidaModels
{
    public class DidaTaskList
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("closed")]
        public bool? Closed { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }
    }
}
