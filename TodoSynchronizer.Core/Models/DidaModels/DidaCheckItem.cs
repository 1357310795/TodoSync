using Newtonsoft.Json;
using System;

namespace TodoSynchronizer.Core.Models.DidaModels
{
    public class DidaCheckItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("sortOrder")]
        public long? SortOrder { get; set; }

        [JsonProperty("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("completedTime")]
        public DateTime? CompletedTime { get; set; }

        [JsonProperty("isAllDay")]
        public bool? IsAllDay { get; set; }

        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }
    }
}
