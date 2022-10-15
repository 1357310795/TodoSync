using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models.DidaModels
{
    public partial class DidaTaskComment
    {
        [JsonProperty("createdTime")]
        public DateTime? CreatedTime { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isNew")]
        public bool? IsNew { get; set; }

        [JsonProperty("projectId")]
        public string ProjectId { get; set; }

        [JsonProperty("taskId")]
        public string TaskId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("userProfile")]
        public UserProfile UserProfile { get; set; }
    }

    public partial class UserProfile
    {
        [JsonProperty("isMyself")]
        public bool IsMyself { get; set; }
    }
}
