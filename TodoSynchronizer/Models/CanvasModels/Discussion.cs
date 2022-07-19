using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Models.CanvasModels
{
    public class Discussion : ICanvasItem
    {
        [JsonProperty("allow_rating")]
        public bool AllowRating { get; set; }

        [JsonProperty("assignment_id")]
        public object AssignmentId { get; set; }

        [JsonProperty("attachments")]
        public System.Collections.Generic.List<Attachment> Attachments { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("delayed_post_at")]
        public DateTime? DelayedPostAt { get; set; }

        [JsonProperty("discussion_subentry_count")]
        public long DiscussionSubentryCount { get; set; }

        [JsonProperty("discussion_type")]
        public string DiscussionType { get; set; }

        [JsonProperty("group_category_id")]
        public object GroupCategoryId { get; set; }

        [JsonProperty("group_topic_children")]
        public System.Collections.Generic.List<GroupTopicChild> GroupTopicChildren { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("last_reply_at")]
        public DateTime? LastReplyAt { get; set; }

        [JsonProperty("lock_at")]
        public DateTime? LockAt { get; set; }

        [JsonProperty("lock_explanation")]
        public string LockExplanation { get; set; }

        [JsonProperty("lock_info")]
        public object LockInfo { get; set; }

        [JsonProperty("locked")]
        public bool Locked { get; set; }

        [JsonProperty("locked_for_user")]
        public bool LockedForUser { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("only_graders_can_rate")]
        public bool OnlyGradersCanRate { get; set; }

        [JsonProperty("permissions")]
        public DiscussionPermissions Permissions { get; set; }

        [JsonProperty("pinned")]
        public bool Pinned { get; set; }

        [JsonProperty("podcast_url")]
        public string PodcastUrl { get; set; }

        [JsonProperty("posted_at")]
        public DateTime? PostedAt { get; set; }

        [JsonProperty("published")]
        public bool Published { get; set; }

        [JsonProperty("read_state")]
        public string ReadState { get; set; }

        [JsonProperty("require_initial_post")]
        public bool? RequireInitialPost { get; set; }

        [JsonProperty("root_topic_id")]
        public object RootTopicId { get; set; }

        [JsonProperty("sort_by_rating")]
        public bool SortByRating { get; set; }

        [JsonProperty("subscribed")]
        public bool Subscribed { get; set; }

        [JsonProperty("subscription_hold")]
        public string SubscriptionHold { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("topic_children")]
        public System.Collections.Generic.List<long> TopicChildren { get; set; }

        [JsonProperty("unread_count")]
        public long UnreadCount { get; set; }

        [JsonProperty("user_can_see_posts")]
        public bool UserCanSeePosts { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }
        public string Content { get => $"{UserName}：\n{Message}"; set => throw new NotImplementedException(); }
    }

    public class GroupTopicChild
    {
        [JsonProperty("group_id")]
        public long GroupId { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }
    }

    public class DiscussionPermissions
    {
        [JsonProperty("attach")]
        public bool Attach { get; set; }

        [JsonProperty("delete")]
        public bool Delete { get; set; }

        [JsonProperty("reply")]
        public bool Reply { get; set; }

        [JsonProperty("update")]
        public bool Update { get; set; }
    }
}
