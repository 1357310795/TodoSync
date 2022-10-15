using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models.DidaModels
{
    public partial class LoginDto
    {
        [JsonProperty("activeTeamUser")]
        public bool ActiveTeamUser { get; set; }

        [JsonProperty("ds")]
        public bool Ds { get; set; }

        [JsonProperty("freeTrial")]
        public bool FreeTrial { get; set; }

        [JsonProperty("inboxId")]
        public string InboxId { get; set; }

        [JsonProperty("needSubscribe")]
        public bool NeedSubscribe { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("pro")]
        public bool Pro { get; set; }

        [JsonProperty("proEndDate")]
        public string ProEndDate { get; set; }

        [JsonProperty("teamUser")]
        public bool TeamUser { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }
}
