using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models.CanvasModels
{
    public class Quiz : ICanvasItem
    {
        [JsonProperty("access_code")]
        public string AccessCode { get; set; }

        [JsonProperty("all_dates")]
        public System.Collections.Generic.List<AllDate> AllDates { get; set; }

        [JsonProperty("allowed_attempts")]
        public long AllowedAttempts { get; set; }

        [JsonProperty("anonymous_submissions")]
        public bool AnonymousSubmissions { get; set; }

        [JsonProperty("assignment_group_id")]
        public long AssignmentGroupId { get; set; }

        [JsonProperty("cant_go_back")]
        public bool CantGoBack { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("due_at")]
        public DateTime? DueAt { get; set; }

        [JsonProperty("hide_correct_answers_at")]
        public string HideCorrectAnswersAt { get; set; }

        [JsonProperty("hide_results")]
        public string HideResults { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("ip_filter")]
        public string IpFilter { get; set; }

        [JsonProperty("lock_at")]
        public DateTime? LockAt { get; set; }

        [JsonProperty("lock_explanation")]
        public string LockExplanation { get; set; }

        [JsonProperty("lock_info")]
        public LockInfo LockInfo { get; set; }

        [JsonProperty("locked_for_user")]
        public bool LockedForUser { get; set; }

        [JsonProperty("mobile_url")]
        public string MobileUrl { get; set; }

        [JsonProperty("one_question_at_a_time")]
        public bool OneQuestionAtATime { get; set; }

        [JsonProperty("one_time_results")]
        public bool OneTimeResults { get; set; }

        [JsonProperty("permissions")]
        public QuizPermissions Permissions { get; set; }

        [JsonProperty("points_possible")]
        public long PointsPossible { get; set; }

        [JsonProperty("preview_url")]
        public string PreviewUrl { get; set; }

        [JsonProperty("published")]
        public bool Published { get; set; }

        [JsonProperty("question_count")]
        public long QuestionCount { get; set; }

        [JsonProperty("question_types")]
        public System.Collections.Generic.List<string> QuestionTypes { get; set; }

        [JsonProperty("quiz_extensions_url")]
        public string QuizExtensionsUrl { get; set; }

        [JsonProperty("quiz_type")]
        public string QuizType { get; set; }

        [JsonProperty("scoring_policy")]
        public string ScoringPolicy { get; set; }

        [JsonProperty("show_correct_answers")]
        public bool ShowCorrectAnswers { get; set; }

        [JsonProperty("show_correct_answers_at")]
        public DateTime? ShowCorrectAnswersAt { get; set; }

        [JsonProperty("show_correct_answers_last_attempt")]
        public bool ShowCorrectAnswersLastAttempt { get; set; }

        [JsonProperty("shuffle_answers")]
        public bool ShuffleAnswers { get; set; }

        [JsonProperty("speedgrader_url")]
        public string SpeedgraderUrl { get; set; }

        [JsonProperty("time_limit")]
        public long TimeLimit { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("unlock_at")]
        public DateTime? UnlockAt { get; set; }

        [JsonProperty("unpublishable")]
        public bool Unpublishable { get; set; }

        [JsonProperty("version_number")]
        public long VersionNumber { get; set; }
        public string Content { get => Description; set => throw new NotImplementedException(); }
    }

    public class AllDate
    {
        [JsonProperty("base")]
        public bool Base { get; set; }

        [JsonProperty("due_at")]
        public string DueAt { get; set; }

        [JsonProperty("lock_at")]
        public string LockAt { get; set; }

        [JsonProperty("unlock_at")]
        public string UnlockAt { get; set; }
    }

    public class QuizPermissions
    {
        [JsonProperty("create")]
        public bool Create { get; set; }

        [JsonProperty("manage")]
        public bool Manage { get; set; }

        [JsonProperty("read")]
        public bool Read { get; set; }

        [JsonProperty("read_statistics")]
        public bool ReadStatistics { get; set; }

        [JsonProperty("review_grades")]
        public bool ReviewGrades { get; set; }

        [JsonProperty("submit")]
        public bool Submit { get; set; }

        [JsonProperty("update")]
        public bool Update { get; set; }
    }
}
