using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models.CanvasModels
{
    public class Notification : ICanvasItem
    {
        [JsonProperty("end_at")]
        public DateTime? EndAt { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("role_ids")]
        public long[] RoleIds { get; set; }

        [JsonProperty("roles")]
        public string[] Roles { get; set; }

        [JsonProperty("start_at")]
        public DateTime? StartAt { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        public string Title { get => Subject; set => throw new NotImplementedException(); }
        public string Content { get => Message; set => throw new NotImplementedException(); }
        public string HtmlUrl { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
