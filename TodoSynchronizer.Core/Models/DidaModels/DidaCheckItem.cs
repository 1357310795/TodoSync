using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models.DidaModels
{
    public class DidaCheckItem
    {
        [JsonProperty("completedTime")]
        public DateTime? CompletedTime { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isAllDay")]
        public bool IsAllDay { get; set; }

        [JsonProperty("snoozeReminderTime")]
        public DateTime? SnoozeReminderTime { get; set; }

        [JsonProperty("sortOrder")]
        public long SortOrder { get; set; }

        [JsonProperty("startDate")]
        public object StartDate { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
