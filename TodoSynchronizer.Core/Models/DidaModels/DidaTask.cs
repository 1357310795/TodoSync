using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TodoSynchronizer.Core.Models.DidaModels
{
    public class DidaTask
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("projectId")]
        public string ProjectId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }

        [JsonProperty("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("dueDate")]
        public DateTime? DueDate { get; set; }

        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }

        [JsonProperty("isAllDay")]
        public bool? IsAllDay { get; set; }

        [JsonProperty("priority")]
        public long? Priority { get; set; }

        [JsonProperty("sortOrder")]
        public long? SortOrder { get; set; }

        [JsonProperty("status")]
        public long? Status { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("repeatFlag")]
        public string RepeatFlag { get; set; }

        [JsonProperty("reminders")]
        public List<string> Reminders { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("items")]
        public List<DidaCheckItem> Items { get; set; }
    }
}
