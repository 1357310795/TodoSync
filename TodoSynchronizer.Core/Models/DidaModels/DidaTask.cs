using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models.DidaModels
{
    public partial class DidaTask
    {
        [JsonProperty("columnId")]
        public string ColumnId { get; set; }

        [JsonProperty("completedTime")]
        public DateTime? CompletedTime { get; set; }

        [JsonProperty("commentCount")]
        public long? CommentCount { get; set; }

        [JsonProperty("completedUserId")]
        public long? CompletedUserId { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("createdTime")]
        public DateTime? CreatedTime { get; set; }

        [JsonProperty("creator")]
        public long? Creator { get; set; }

        [JsonProperty("deleted")]
        public long? Deleted { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }

        [JsonProperty("dueDate")]
        public DateTime? DueDate { get; set; }

        [JsonProperty("etag")]
        public string Etag { get; set; }

        [JsonProperty("exDate")]
        public System.Collections.Generic.List<string> ExDate { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isAllDay")]
        public bool? IsAllDay { get; set; }

        [JsonProperty("isFloating")]
        public bool? IsFloating { get; set; }

        [JsonProperty("items")]
        public System.Collections.Generic.List<DidaCheckItem> Items { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("modifiedTime")]
        public DateTime? ModifiedTime { get; set; }

        [JsonProperty("priority")]
        public long? Priority { get; set; }

        [JsonProperty("progress")]
        public long? Progress { get; set; }

        [JsonProperty("projectId")]
        public string ProjectId { get; set; }

        [JsonProperty("repeatFirstDate")]
        public DateTime? RepeatFirstDate { get; set; }

        [JsonProperty("reminder")]
        public string Reminder { get; set; }

        [JsonProperty("reminders")]
        public System.Collections.Generic.List<Reminder> Reminders { get; set; }

        [JsonProperty("sortOrder")]
        public long? SortOrder { get; set; }

        [JsonProperty("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("status")]
        public long? Status { get; set; }

        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }


    public class Reminder
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("trigger")]
        public string Trigger { get; set; }
    }
}
