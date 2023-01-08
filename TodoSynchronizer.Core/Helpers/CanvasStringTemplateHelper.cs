using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TodoSynchronizer.Core.Config;
using TodoSynchronizer.Core.Models.CanvasModels;

namespace TodoSynchronizer.Core.Helpers
{
    public static class CanvasStringTemplateHelper
    {
        public static string GetTrigger(double time)
        {
            return $"TRIGGER:-PT{((int)time)}M";
        }

        public static string GetTitle(Course course, ICanvasItem item)
        {
            if (item is Assignment assignment)
            {
                return SyncConfig.Default.AssignmentConfig.TitleTemplate
                    .ReplaceCourse(course)
                    .Replace("{assignment.title}", assignment.Title);
            }
            if (item is Anouncement anouncement)
            {
                return SyncConfig.Default.AnouncementConfig.TitleTemplate
                    .ReplaceCourse(course)
                    .Replace("{anouncement.title}", anouncement.Title)
                    .Replace("{anouncement.author}", anouncement.Author.DisplayName);
            }
            if (item is Quiz quiz)
            {
                return SyncConfig.Default.QuizConfig.TitleTemplate
                    .ReplaceCourse(course)
                    .Replace("{quiz.title}", quiz.Title);
            }
            if (item is Discussion discussion)
            {
                return SyncConfig.Default.DiscussionConfig.TitleTemplate
                    .ReplaceCourse(course)
                    .Replace("{discussion.title}", discussion.Title);
            }
            if (item is Notification notification)
            {
                return SyncConfig.Default.NotificationConfig.TitleTemplate
                    .Replace("{notification.title}", notification.Subject);
            }
            return "Error";
        }

        public static string GetListNameForCourse(Course course)
        {
            return SyncConfig.Default.ListNameTemplateForCourse.ReplaceCourse(course);
        }

        public static string GetNotificationListName()
        {
            return SyncConfig.Default.ListNameForNotification;
        }

        public static string ReplaceCourse(this string s, Course course)
        {
            return s.Replace("{course.name}", course.Name)
                .Replace("{course.coursecode}", course.CourseCode)
                .Replace("{course.coursecodeshort}", ExtractCourseCodeShort(course.CourseCode))
                .Replace("{course.originalname}", course.OriginalName ?? course.Name);
        }

        public static string ExtractCourseCodeShort(string s)
        {
            var reg = new Regex(@"\(\d{4}-\d{4}-[123]\)-([a-zA-z0-9]+?)-");
            var match = reg.Match(s);
            return match.Success ? match.Groups[1].Value : s;
        }

        public static string GetContent(ICanvasItem item)
        {
            HtmlHelper convert = new HtmlHelper();
            return convert.Convert(item.Content);
        }

        public static string GetContent(string content)
        {
            HtmlHelper convert = new HtmlHelper();
            return convert.Convert(content);
        }

        public static string GetSubmissionDesc(Assignment assignment, AssignmentSubmission submission)
        {
            if (submission.SubmittedAt != null)
                return $"已提交（提交时间：{submission.SubmittedAt.Value.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}）";
            else
                return "未提交";
        }

        public static string GetSubmissionComment(SubmissionComment comment)
        {
            return $"{comment.AuthorName} 评论：{comment.Comment}";
        }

        public static string GetGradeDesc(Assignment assignment, AssignmentSubmission submission)
        {
            if (submission.Grade != null)
                return $"已评分：{submission.Grade}/{assignment.PointsPossible??0}（评分时间：{submission.GradedAt.Value.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}）";
            else
                return "未评分";
        }

        public static string GetSubmissionDesc(Assignment assignment, QuizSubmission submission)
        {
            if (submission.WorkflowState == "untaken")
                return $"正在进行测验（开始时间：{submission.StartedAt.Value.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}）";
            else if (submission.WorkflowState == "complete")
                return $"尝试 {submission.Attempt}：{submission.Score}/{submission.QuizPointsPossible}（提交时间：{submission.FinishedAt.Value.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}）";
            else if (submission.WorkflowState == "pending_review")
                return $"尝试 {submission.Attempt}：{submission.Score}/{submission.QuizPointsPossible}（提交时间：{submission.FinishedAt.Value.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}）（*有些问题尚未计分）";
            else
                return $"无法识别的尝试";
        }

        public static string GetItemName(this ICanvasItem item)
        {
            if (item is Quiz quiz) return "测验";
            if (item is Assignment assignment) return "作业";
            if (item is Anouncement anouncement) return "公告";
            if (item is Discussion discussion) return "讨论";
            return null;
        }
    }
}
