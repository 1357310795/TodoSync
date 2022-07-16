using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Models.CanvasModels
{
    public class Assignment
    {
        [JsonProperty("all_dates")]
        public object AllDates { get; set; }

        [JsonProperty("allowed_attempts")]
        public long AllowedAttempts { get; set; }

        [JsonProperty("allowed_extensions")]
        public System.Collections.Generic.List<string> AllowedExtensions { get; set; }

        [JsonProperty("annotatable_attachment_id")]
        public object AnnotatableAttachmentId { get; set; }

        [JsonProperty("anonymize_students")]
        public bool AnonymizeStudents { get; set; }

        [JsonProperty("anonymous_grading")]
        public bool AnonymousGrading { get; set; }

        [JsonProperty("anonymous_submissions")]
        public bool AnonymousSubmissions { get; set; }

        [JsonProperty("assignment_group_id")]
        public long AssignmentGroupId { get; set; }

        [JsonProperty("assignment_visibility")]
        public System.Collections.Generic.List<long> AssignmentVisibility { get; set; }

        [JsonProperty("automatic_peer_reviews")]
        public bool AutomaticPeerReviews { get; set; }

        [JsonProperty("can_submit")]
        public bool CanSubmit { get; set; }

        [JsonProperty("course_id")]
        public long CourseId { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("discussion_topic")]
        public object DiscussionTopic { get; set; }

        [JsonProperty("due_at")]
        public string DueAt { get; set; }

        [JsonProperty("due_date_required")]
        public bool DueDateRequired { get; set; }

        [JsonProperty("external_tool_tag_attributes")]
        public object ExternalToolTagAttributes { get; set; }

        [JsonProperty("final_grader_id")]
        public long? FinalGraderId { get; set; }

        [JsonProperty("freeze_on_copy")]
        public bool FreezeOnCopy { get; set; }

        [JsonProperty("frozen")]
        public bool Frozen { get; set; }

        [JsonProperty("frozen_attributes")]
        public System.Collections.Generic.List<string> FrozenAttributes { get; set; }

        [JsonProperty("grade_group_students_individually")]
        public bool GradeGroupStudentsIndividually { get; set; }

        [JsonProperty("grader_comments_visible_to_graders")]
        public bool GraderCommentsVisibleToGraders { get; set; }

        [JsonProperty("grader_count")]
        public long? GraderCount { get; set; }

        [JsonProperty("grader_names_visible_to_final_grader")]
        public bool GraderNamesVisibleToFinalGrader { get; set; }

        [JsonProperty("graders_anonymous_to_graders")]
        public bool GradersAnonymousToGraders { get; set; }

        [JsonProperty("grading_standard_id")]
        public object GradingStandardId { get; set; }

        [JsonProperty("grading_type")]
        public string GradingType { get; set; }

        [JsonProperty("group_category_id")]
        public long? GroupCategoryId { get; set; }

        [JsonProperty("has_overrides")]
        public bool HasOverrides { get; set; }

        [JsonProperty("has_submitted_submissions")]
        public bool HasSubmittedSubmissions { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("important_dates")]
        public bool ImportantDates { get; set; }

        [JsonProperty("integration_id")]
        public string IntegrationId { get; set; }

        [JsonProperty("intra_group_peer_reviews")]
        public bool IntraGroupPeerReviews { get; set; }

        [JsonProperty("lock_at")]
        public string LockAt { get; set; }

        [JsonProperty("lock_explanation")]
        public string LockExplanation { get; set; }

        [JsonProperty("lock_info")]
        public LockInfo LockInfo { get; set; }

        [JsonProperty("locked_for_user")]
        public bool LockedForUser { get; set; }

        [JsonProperty("max_name_length")]
        public long MaxNameLength { get; set; }

        [JsonProperty("moderated_grading")]
        public bool ModeratedGrading { get; set; }

        [JsonProperty("muted")]
        public bool Muted { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("needs_grading_count")]
        public long NeedsGradingCount { get; set; }

        [JsonProperty("needs_grading_count_by_section")]
        public System.Collections.Generic.List<NeedsGradingCountBySection> NeedsGradingCountBySection { get; set; }

        [JsonProperty("omit_from_final_grade")]
        public bool OmitFromFinalGrade { get; set; }

        [JsonProperty("only_visible_to_overrides")]
        public bool OnlyVisibleToOverrides { get; set; }

        [JsonProperty("overrides")]
        public object Overrides { get; set; }

        [JsonProperty("peer_review_count")]
        public long PeerReviewCount { get; set; }

        [JsonProperty("peer_reviews")]
        public bool PeerReviews { get; set; }

        [JsonProperty("peer_reviews_assign_at")]
        public string PeerReviewsAssignAt { get; set; }

        [JsonProperty("points_possible")]
        public long PointsPossible { get; set; }

        [JsonProperty("position")]
        public long Position { get; set; }

        [JsonProperty("post_manually")]
        public bool PostManually { get; set; }

        [JsonProperty("post_to_sis")]
        public bool PostToSis { get; set; }

        [JsonProperty("published")]
        public bool Published { get; set; }

        [JsonProperty("quiz_id")]
        public long QuizId { get; set; }

        [JsonProperty("require_lockdown_browser")]
        public bool RequireLockdownBrowser { get; set; }

        [JsonProperty("rubric")]
        public object Rubric { get; set; }

        [JsonProperty("score_statistics")]
        public object ScoreStatistics { get; set; }

        [JsonProperty("submission")]
        public object Submission { get; set; }

        [JsonProperty("submission_types")]
        public System.Collections.Generic.List<string> SubmissionTypes { get; set; }

        [JsonProperty("submissions_download_url")]
        public string SubmissionsDownloadUrl { get; set; }

        [JsonProperty("turnitin_enabled")]
        public bool TurnitinEnabled { get; set; }

        [JsonProperty("turnitin_settings")]
        public object TurnitinSettings { get; set; }

        [JsonProperty("unlock_at")]
        public string UnlockAt { get; set; }

        [JsonProperty("unpublishable")]
        public bool Unpublishable { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("use_rubric_for_grading")]
        public bool UseRubricForGrading { get; set; }

        [JsonProperty("vericite_enabled")]
        public bool VericiteEnabled { get; set; }
    }

    public class NeedsGradingCountBySection
    {
        [JsonProperty("needs_grading_count")]
        public long NeedsGradingCount { get; set; }

        [JsonProperty("section_id")]
        public string SectionId { get; set; }
    }
}
