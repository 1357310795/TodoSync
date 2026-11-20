using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TodoSynchronizer.Core.Models.DidaModels
{
    public partial class DidaBatchCheckDto
    {
        [JsonProperty("checkPoint")]
        public long CheckPoint { get; set; }

        [JsonProperty("filters")]
        public object Filters { get; set; }

        [JsonProperty("inboxId")]
        public string InboxId { get; set; }

        [JsonProperty("projectGroups")]
        public JToken ProjectGroups { get; set; }

        [JsonProperty("projectProfiles")]
        public List<DidaTaskList> ProjectProfiles { get; set; }

        [JsonProperty("syncTaskBean")]
        public SyncTaskBean SyncTaskBean { get; set; }

        [JsonProperty("tags")]
        public List<JToken> Tags { get; set; }
    }
    public partial class SyncTaskBean
    {
        [JsonProperty("update")]
        public List<DidaTask> Update { get; set; }
    }
}
