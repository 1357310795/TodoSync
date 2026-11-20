using Newtonsoft.Json;
using System.Collections.Generic;

namespace TodoSynchronizer.Core.Models.DidaModels
{
    internal class DidaProjectData
    {
        [JsonProperty("tasks")]
        public List<DidaTask> Tasks { get; set; }
    }
}
