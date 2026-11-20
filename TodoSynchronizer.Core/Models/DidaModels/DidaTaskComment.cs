using Newtonsoft.Json;

namespace TodoSynchronizer.Core.Models.DidaModels
{
    public class DidaTaskComment
    {
        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
