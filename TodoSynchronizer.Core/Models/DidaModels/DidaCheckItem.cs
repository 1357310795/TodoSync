using Newtonsoft.Json;
using System;
using System.Globalization;

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
        [JsonConverter(typeof(DidaCompletedTimeConverter))]
        public DateTime? CompletedTime { get; set; }

        [JsonProperty("isAllDay")]
        public bool? IsAllDay { get; set; }

        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }
    }

    internal sealed class DidaCompletedTimeConverter : JsonConverter<DateTime?>
    {
        public override bool CanWrite => false;

        public override DateTime? ReadJson(JsonReader reader, Type objectType, DateTime? existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                // 历史检查项使用 Unix 毫秒，Open API 写入使用日期字符串。
                var timestamp = Convert.ToInt64(reader.Value, CultureInfo.InvariantCulture);
                return timestamp > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime : null;
            }

            return serializer.Deserialize<DateTime?>(reader);
        }

        public override void WriteJson(JsonWriter writer, DateTime? value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
