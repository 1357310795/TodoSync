using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Models.CanvasModels
{
    public class UserProfile
    {
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("bio")]
        public object Bio { get; set; }

        [JsonProperty("calendar")]
        public Calendar Calendar { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("integration_id")]
        public object IntegrationId { get; set; }

        [JsonProperty("locale")]
        public object Locale { get; set; }

        [JsonProperty("login_id")]
        public string LoginId { get; set; }

        [JsonProperty("lti_user_id")]
        public string LtiUserId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("primary_email")]
        public string PrimaryEmail { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("sortable_name")]
        public string SortableName { get; set; }

        [JsonProperty("time_zone")]
        public string TimeZone { get; set; }

        [JsonProperty("title")]
        public object Title { get; set; }
    }
}
