using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models.CanvasModels
{
    public class SubmissionComment
    {
        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("author_id")]
        public long? AuthorId { get; set; }

        [JsonProperty("author_name")]
        public string AuthorName { get; set; }

        [JsonProperty("avatar_path")]
        public string AvatarPath { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("edited_at")]
        public DateTime? EditedAt { get; set; }

        [JsonProperty("id")]
        public long? Id { get; set; }
    }
}
