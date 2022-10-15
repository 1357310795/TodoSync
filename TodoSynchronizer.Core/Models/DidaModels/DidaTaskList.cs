using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models.DidaModels
{
    public partial class DidaTaskList
    {
        [JsonProperty("closed")]
        public object Closed { get; set; }

        [JsonProperty("color")]
        public object Color { get; set; }

        [JsonProperty("etag")]
        public string Etag { get; set; }

        [JsonProperty("groupId")]
        public object GroupId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("inAll")]
        public bool? InAll { get; set; }

        [JsonProperty("isOwner")]
        public bool? IsOwner { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("modifiedTime")]
        public string ModifiedTime { get; set; }

        [JsonProperty("muted")]
        public bool? Muted { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("notificationOptions")]
        public object NotificationOptions { get; set; }

        [JsonProperty("permission")]
        public object Permission { get; set; }

        [JsonProperty("sortOrder")]
        public long? SortOrder { get; set; }

        [JsonProperty("sortType")]
        public string SortType { get; set; }

        [JsonProperty("teamId")]
        public object TeamId { get; set; }

        [JsonProperty("timeline")]
        public object Timeline { get; set; }

        [JsonProperty("transferred")]
        public object Transferred { get; set; }

        [JsonProperty("userCount")]
        public long? UserCount { get; set; }

        [JsonProperty("viewMode")]
        public string ViewMode { get; set; }
    }
}
