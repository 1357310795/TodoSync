using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models.CanvasModels
{
    public partial class Announcement : ICanvasItem
    {
        [JsonProperty("allow_rating")]
        public bool AllowRating { get; set; }

        [JsonProperty("assignment_id")]
        public long? AssignmentId { get; set; }

        [JsonProperty("attachments")]
        public List<Attachment> Attachments { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("can_group")]
        public bool CanGroup { get; set; }

        [JsonProperty("can_lock")]
        public bool CanLock { get; set; }

        [JsonProperty("can_unpublish")]
        public bool CanUnpublish { get; set; }

        [JsonProperty("comments_disabled")]
        public bool CommentsDisabled { get; set; }

        [JsonProperty("delayed_post_at")]
        public DateTime? DelayedPostAt { get; set; }

        [JsonProperty("discussion_subentry_count")]
        public long DiscussionSubentryCount { get; set; }

        [JsonProperty("discussion_type")]
        public string DiscussionType { get; set; }

        [JsonProperty("group_category_id")]
        public long? GroupCategoryId { get; set; }

        [JsonProperty("group_topic_children")]
        public System.Collections.Generic.List<string> GroupTopicChildren { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("is_section_specific")]
        public bool IsSectionSpecific { get; set; }

        [JsonProperty("last_reply_at")]
        public DateTime? LastReplyAt { get; set; }

        [JsonProperty("lock_at")]
        public DateTime? LockAt { get; set; }

        [JsonProperty("lock_explanation")]
        public string LockExplanation { get; set; }

        [JsonProperty("lock_info")]
        public LockInfo LockInfo { get; set; }

        [JsonProperty("locked")]
        public bool Locked { get; set; }

        [JsonProperty("locked_for_user")]
        public bool LockedForUser { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("only_graders_can_rate")]
        public bool OnlyGradersCanRate { get; set; }

        [JsonProperty("permissions")]
        public AnnouncementPermissions Permissions { get; set; }

        [JsonProperty("pinned")]
        public bool Pinned { get; set; }

        [JsonProperty("podcast_has_student_posts")]
        public bool PodcastHasStudentPosts { get; set; }

        [JsonProperty("podcast_url")]
        public string PodcastUrl { get; set; }

        [JsonProperty("position")]
        public long Position { get; set; }

        [JsonProperty("posted_at")]
        public DateTime? PostedAt { get; set; }

        [JsonProperty("published")]
        public bool Published { get; set; }

        [JsonProperty("read_state")]
        public string ReadState { get; set; }

        [JsonProperty("require_initial_post")]
        public bool? RequireInitialPost { get; set; }

        [JsonProperty("root_topic_id")]
        public long? RootTopicId { get; set; }

        [JsonProperty("sort_by_rating")]
        public bool SortByRating { get; set; }

        [JsonProperty("subscribed")]
        public bool Subscribed { get; set; }

        [JsonProperty("subscription_hold")]
        public string SubscriptionHold { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("todo_date")]
        public string TodoDate { get; set; }

        [JsonProperty("topic_children")]
        public System.Collections.Generic.List<string> TopicChildren { get; set; }

        [JsonProperty("unread_count")]
        public long UnreadCount { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("user_can_see_posts")]
        public bool UserCanSeePosts { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }
        public string Content { get => $"{UserName}：\n{Message}"; set => throw new NotImplementedException(); }
    }

    public class AnnouncementPermissions
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
