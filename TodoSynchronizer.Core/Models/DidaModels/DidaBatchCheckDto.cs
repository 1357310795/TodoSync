using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public System.Collections.Generic.List<string> ProjectGroups { get; set; }

        [JsonProperty("projectProfiles")]
        public System.Collections.Generic.List<DidaTaskList> ProjectProfiles { get; set; }

        [JsonProperty("syncTaskBean")]
        public SyncTaskBean SyncTaskBean { get; set; }

        [JsonProperty("tags")]
        public System.Collections.Generic.List<string> Tags { get; set; }
    }
    public partial class SyncTaskBean
    {
        [JsonProperty("update")]
        public System.Collections.Generic.List<DidaTask> Update { get; set; }
    }
}
