using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models.CanvasModels
{
    public partial class QuizSubmissionDto
    {
        [JsonProperty("quiz_submissions")]
        public System.Collections.Generic.List<QuizSubmission> QuizSubmissions { get; set; }
    }

    public partial class QuizSubmission
    {
        [JsonProperty("attempt")]
        public long Attempt { get; set; }

        [JsonProperty("end_at")]
        public DateTime EndAt { get; set; }

        [JsonProperty("extra_attempts")]
        public long? ExtraAttempts { get; set; }

        [JsonProperty("extra_time")]
        public long? ExtraTime { get; set; }

        [JsonProperty("finished_at")]
        public DateTime? FinishedAt { get; set; }

        [JsonProperty("fudge_points")]
        public float? FudgePoints { get; set; }

        [JsonProperty("has_seen_results")]
        public bool? HasSeenResults { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("kept_score")]
        public float? KeptScore { get; set; }

        [JsonProperty("manually_unlocked")]
        public bool? ManuallyUnlocked { get; set; }

        [JsonProperty("overdue_and_needs_submission")]
        public bool? OverdueAndNeedsSubmission { get; set; }

        [JsonProperty("quiz_id")]
        public long QuizId { get; set; }

        [JsonProperty("quiz_points_possible")]
        public float? QuizPointsPossible { get; set; }

        [JsonProperty("score")]
        public float? Score { get; set; }

        [JsonProperty("score_before_regrade")]
        public float? ScoreBeforeRegrade { get; set; }

        [JsonProperty("started_at")]
        public DateTime? StartedAt { get; set; }

        [JsonProperty("submission_id")]
        public long SubmissionId { get; set; }

        [JsonProperty("time_spent")]
        public long? TimeSpent { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("workflow_state")]
        public string WorkflowState { get; set; }
    }

}
