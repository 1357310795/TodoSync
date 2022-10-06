using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models.CanvasModels
{
    public class AssignmentSubmission
    {
        [JsonProperty("anonymous_id")]
        public string AnonymousId { get; set; }

        [JsonProperty("assignment")]
        public object Assignment { get; set; }

        [JsonProperty("assignment_id")]
        public long AssignmentId { get; set; }

        [JsonProperty("assignment_visible")]
        public bool AssignmentVisible { get; set; }

        [JsonProperty("attempt")]
        public long? Attempt { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("course")]
        public object Course { get; set; }

        [JsonProperty("excused")]
        public bool? Excused { get; set; }

        [JsonProperty("extra_attempts")]
        public long? ExtraAttempts { get; set; }

        [JsonProperty("grade")]
        public string Grade { get; set; }

        [JsonProperty("grade_matches_current_submission")]
        public bool GradeMatchesCurrentSubmission { get; set; }

        [JsonProperty("graded_at")]
        public DateTime? GradedAt { get; set; }

        [JsonProperty("grader_id")]
        public long? GraderId { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("late")]
        public bool Late { get; set; }

        [JsonProperty("late_policy_status")]
        public string LatePolicyStatus { get; set; }

        [JsonProperty("missing")]
        public bool Missing { get; set; }

        [JsonProperty("points_deducted")]
        public double? PointsDeducted { get; set; }

        [JsonProperty("posted_at")]
        public string PostedAt { get; set; }

        [JsonProperty("preview_url")]
        public string PreviewUrl { get; set; }

        [JsonProperty("read_status")]
        public string ReadStatus { get; set; }

        [JsonProperty("redo_request")]
        public bool RedoRequest { get; set; }

        [JsonProperty("score")]
        public double? Score { get; set; }

        [JsonProperty("seconds_late")]
        public long SecondsLate { get; set; }

        [JsonProperty("submission_comments")]
        public List<SubmissionComment> SubmissionComments { get; set; }

        [JsonProperty("submission_type")]
        public string SubmissionType { get; set; }

        [JsonProperty("submitted_at")]
        public DateTime? SubmittedAt { get; set; }

        [JsonProperty("url")]
        public object Url { get; set; }

        [JsonProperty("user")]
        public object User { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("workflow_state")]
        public string WorkflowState { get; set; }

        [JsonProperty("attachments")]
        public List<Attachment> Attachments { get; set; }
    }
}
