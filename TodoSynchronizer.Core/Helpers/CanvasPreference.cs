using System;
using TodoSynchronizer.Core.Config;
using TodoSynchronizer.Core.Models.CanvasModels;

namespace TodoSynchronizer.Core.Helpers
{
    public static class CanvasPreference
    {
        public static DateTime? GetRemindTime(this ICanvasItem item)
        {
            if (item is Quiz quiz) return GetRemindTime(quiz);
            if (item is Assignment assignment) return GetRemindTime(assignment);
            if (item is Announcement announcement) return GetRemindTime(announcement);
            if (item is Discussion discussion) return GetRemindTime(discussion);
            if (item is Notification notification) return GetRemindTime(notification);
            return null;
        }

        public static double? GetRemindBefore(this ICanvasItemConfig item)
        {
            if (item is QuizConfig quizConfig) return GetRemindBefore(quizConfig);
            if (item is AssignmentConfig assignmentConfig) return GetRemindBefore(assignmentConfig);
            return null;
        }

        public static DateTime? GetDueTime(this ICanvasItem item)
        {
            if (item is Quiz quiz) return GetDueTime(quiz);
            if (item is Assignment assignment) return GetDueTime(assignment);
            if (item is Announcement announcement) return GetDueTime(announcement);
            if (item is Discussion discussion) return GetDueTime(discussion);
            if (item is Notification notification) return GetDueTime(notification);
            return null;
        }

        #region Quiz
        public static double GetRemindBefore(this QuizConfig quizConfig)
        {
            return quizConfig.BeforeTimeSpan.TotalMinutes;
        }

        public static DateTime? GetRemindTime(this Quiz quiz)
        {
            switch (SyncConfig.Default.QuizConfig.RemindMode)
            {
                case RemindMode.unlock_at:
                    return quiz.UnlockAt;
                case RemindMode.before_due_at:
                    return quiz.DueAt == null ? null : quiz.DueAt - SyncConfig.Default.QuizConfig.BeforeTimeSpan;
                case RemindMode.before_lock_at:
                    return quiz.LockAt == null ? null : quiz.LockAt - SyncConfig.Default.QuizConfig.BeforeTimeSpan;
                default:
                    return null;
            }
        }

        public static DateTime? GetDueTime(this Quiz quiz)
        {
            switch (SyncConfig.Default.QuizConfig.DueDateMode)
            {
                case DueDateMode.due_at:
                    return quiz.DueAt;
                case DueDateMode.lock_at:
                    return quiz.LockAt;
                default:
                    return null;
            }
        }
        #endregion

        #region Assignment

        public static double GetRemindBefore(this AssignmentConfig assignmentConfig)
        {
            return assignmentConfig.BeforeTimeSpan.TotalMinutes;
        }

        public static DateTime? GetRemindTime(this Assignment assignment)
        {
            switch (SyncConfig.Default.AssignmentConfig.RemindMode)
            {
                case RemindMode.unlock_at:
                    return assignment.UnlockAt;
                case RemindMode.before_due_at:
                    return assignment.DueAt == null ? null : assignment.DueAt - SyncConfig.Default.AssignmentConfig.BeforeTimeSpan;
                case RemindMode.before_lock_at:
                    return assignment.LockAt == null ? null : assignment.LockAt - SyncConfig.Default.AssignmentConfig.BeforeTimeSpan;
                default:
                    return null;
            }
        }

        public static DateTime? GetDueTime(this Assignment assignment)
        {
            if (SyncConfig.Default.AssignmentConfig.DueDateModeFallback)
            {
                switch (SyncConfig.Default.AssignmentConfig.DueDateMode)
                {
                    case DueDateMode.due_at:
                        return assignment.DueAt ?? assignment.LockAt;
                    case DueDateMode.lock_at:
                        return assignment.LockAt ?? assignment.DueAt;
                    default:
                        return null;
                }
            }
            else
            {
                switch (SyncConfig.Default.AssignmentConfig.DueDateMode)
                {
                    case DueDateMode.due_at:
                        return assignment.DueAt;
                    case DueDateMode.lock_at:
                        return assignment.LockAt;
                    default:
                        return null;
                }
            }
        }
        #endregion

        #region Announcement
        public static DateTime? GetRemindTime(this Announcement announcement)
        {
            return DateTime.Now + SyncConfig.Default.AnnouncementConfig.RemindAfter;
        }

        public static DateTime? GetDueTime(this Announcement announcement)
        {
            return announcement.PostedAt;
        }
        #endregion

        #region Discussion
        public static DateTime? GetRemindTime(this Discussion discussion)
        {
            return DateTime.Now + SyncConfig.Default.DiscussionConfig.RemindAfter;
        }

        public static DateTime? GetDueTime(this Discussion discussion)
        {
            return discussion.PostedAt;
        }
        #endregion

        #region Notification
        public static DateTime? GetRemindTime(this Notification notification)
        {
            return DateTime.Now + SyncConfig.Default.NotificationConfig.RemindAfter;
        }

        public static DateTime? GetDueTime(this Notification notification)
        {
            return SyncConfig.Default.NotificationConfig.DueDateMode == DueDateMode.start_at? notification.StartAt : notification.EndAt;
        }
        #endregion

    }
}
