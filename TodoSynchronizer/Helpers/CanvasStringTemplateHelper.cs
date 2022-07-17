using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoSynchronizer.Models.CanvasModels;

namespace TodoSynchronizer.Helpers
{
    public class CanvasStringTemplateHelper
    {
        public static string TitleTemplate { get; set; } = "{course.name} - {item.title}";

        public static string GetTitle(Course course, ICanvasItem item)
        {
            return TitleTemplate.Replace("{course.name}", course.Name)
                .Replace("{course.id}", course.Id.ToString())
                .Replace("{item.title}", item.Title.ToString())
                .Replace("{item.id}", item.Id.ToString());
        }

        public static string GetContent(ICanvasItem item)
        {
            HtmlHelper convert = new HtmlHelper();
            return convert.Convert(item.Content);
        }

        public static string GetSubmissionDesc(Assignment assignment, AssignmentSubmission submission)
        {
            if (submission.SubmittedAt != null)
                return $"已提交（提交时间：{submission.SubmittedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")}）";
            else
                return "未提交";
        }

        public static string GetGradeDesc(Assignment assignment, AssignmentSubmission submission)
        {
            if (submission.Grade != null)
                return $"已评分：{submission.Grade}/{assignment.PointsPossible}（评分时间：{submission.GradedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")}）";
            else
                return "未评分";
        }

        public static string GetSubmissionDesc(Assignment assignment, QuizSubmission submission)
        {
            return $"尝试 {submission.Attempt}：{submission.Score}/{submission.QuizPointsPossible}（提交时间：{submission.FinishedAt.ToString("yyyy-MM-dd HH:mm:ss")}）";
        }
    }
}
