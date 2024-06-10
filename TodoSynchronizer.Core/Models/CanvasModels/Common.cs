using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models.CanvasModels
{
    public class LockInfo
    {
        [JsonProperty("asset_string")]
        public string? AssetString { get; set; }

        [JsonProperty("lock_at")]
        public string? LockAt { get; set; }

        [JsonProperty("manually_locked")]
        public bool? ManuallyLocked { get; set; }

        [JsonProperty("unlock_at")]
        public string? UnlockAt { get; set; }
    }

    public class Author
    {
        [JsonProperty("avatar_image_url")]
        public string AvatarImageUrl { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("id")]
        public long? Id { get; set; }
    }

    public class Calendar
    {
        [JsonProperty("ics")]
        public string Ics { get; set; }
    }

    public class Attachment
    {
        [JsonProperty("content-type")]
        public string ContentType { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("folder_id")]
        public long? FolderId { get; set; }

        [JsonProperty("hidden")]
        public bool Hidden { get; set; }

        [JsonProperty("hidden_for_user")]
        public bool HiddenForUser { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("lock_at")]
        public object LockAt { get; set; }

        [JsonProperty("locked")]
        public bool Locked { get; set; }

        [JsonProperty("locked_for_user")]
        public bool LockedForUser { get; set; }

        [JsonProperty("media_entry_id")]
        public object MediaEntryId { get; set; }

        [JsonProperty("mime_class")]
        public string MimeClass { get; set; }

        [JsonProperty("modified_at")]
        public string ModifiedAt { get; set; }

        [JsonProperty("size")]
        public long? Size { get; set; }

        [JsonProperty("thumbnail_url")]
        public object ThumbnailUrl { get; set; }

        [JsonProperty("unlock_at")]
        public object UnlockAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }
}
