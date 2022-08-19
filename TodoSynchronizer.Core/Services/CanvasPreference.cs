using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoSynchronizer.Core.Models.CanvasModels;

namespace TodoSynchronizer.Core.Services
{
    public static class CanvasPreference
    {
        public static DateTime? GetRemindMeTime(this Quiz quiz)
        {
            //return assignment.UnlockAt;
            return quiz.DueAt == null ? null : quiz.DueAt - TimeSpan.FromHours(1);
        }

        public static DateTime? GetDueTime(this Quiz quiz)
        {
            return quiz.UnlockAt;
            //return assignment.DueAt;
        }
        public static DateTime? GetRemindMeTime(this Assignment assignment)
        {
            //return assignment.UnlockAt;
            return assignment.DueAt == null ? null : assignment.DueAt - TimeSpan.FromHours(1);
        }

        public static DateTime? GetDueTime(this Assignment assignment)
        {
            return assignment.UnlockAt;
            //return assignment.DueAt;
        }

        public static DateTime? GetRemindMeTime(this Anouncement anouncement)
        {
            //return assignment.UnlockAt;
            return DateTime.Now + TimeSpan.FromMinutes(1);
        }

        public static DateTime? GetDueTime(this Anouncement anouncement)
        {
            return anouncement.PostedAt;
            //return assignment.DueAt;
        }

        public static DateTime? GetRemindMeTime(this Discussion discussion)
        {
            //return assignment.UnlockAt;
            return DateTime.Now + TimeSpan.FromMinutes(1);
        }

        public static DateTime? GetDueTime(this Discussion discussion)
        {
            return discussion.PostedAt;
            //return assignment.DueAt;
        }

        public static bool GetAssignmentImprotance()
        {
            return true;
        }
    }
}
