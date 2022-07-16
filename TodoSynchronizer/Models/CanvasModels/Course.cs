using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Models.CanvasModels
{
    public class Course
    {
        [JsonProperty("account_id")]
        public long AccountId { get; set; }

        [JsonProperty("apply_assignment_group_weights")]
        public bool ApplyAssignmentGroupWeights { get; set; }

        [JsonProperty("blueprint")]
        public bool Blueprint { get; set; }

        [JsonProperty("calendar")]
        public Calendar Calendar { get; set; }

        [JsonProperty("course_code")]
        public string CourseCode { get; set; }

        [JsonProperty("default_view")]
        public string DefaultView { get; set; }

        [JsonProperty("end_at")]
        public string EndAt { get; set; }

        [JsonProperty("enrollment_term_id")]
        public long EnrollmentTermId { get; set; }

        [JsonProperty("enrollments")]
        public System.Collections.Generic.List<Enrollment> Enrollments { get; set; }

        [JsonProperty("grading_standard_id")]
        public object GradingStandardId { get; set; }

        [JsonProperty("hide_final_grades")]
        public bool HideFinalGrades { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("is_public")]
        public bool? IsPublic { get; set; }

        [JsonProperty("is_public_to_auth_users")]
        public bool IsPublicToAuthUsers { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("original_name")]
        public string OriginalName { get; set; }

        [JsonProperty("public_syllabus")]
        public bool PublicSyllabus { get; set; }

        [JsonProperty("public_syllabus_to_auth")]
        public bool PublicSyllabusToAuth { get; set; }

        [JsonProperty("restrict_enrollments_to_course_dates")]
        public bool RestrictEnrollmentsToCourseDates { get; set; }

        [JsonProperty("root_account_id")]
        public long RootAccountId { get; set; }

        [JsonProperty("start_at")]
        public string StartAt { get; set; }

        [JsonProperty("storage_quota_mb")]
        public long StorageQuotaMb { get; set; }

        [JsonProperty("time_zone")]
        public string TimeZone { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("workflow_state")]
        public string WorkflowState { get; set; }
    }

    public class Enrollment
    {
        [JsonProperty("enrollment_state")]
        public string EnrollmentState { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("role_id")]
        public long RoleId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }
    }
}
