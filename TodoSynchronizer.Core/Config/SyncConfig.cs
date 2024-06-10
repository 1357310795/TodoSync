using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoSynchronizer.Core.Models.CanvasModels;

namespace TodoSynchronizer.Core.Config
{
    public class SyncConfig
    {
        //Static
        public static SyncConfig Default { get; set; }
        //List Names
        public ListNameMode ListNameMode { get; set; }
        public ListNamesForCategory ListNamesForCategory { get; set; }
        public string ListNameTemplateForCourse { get; set; }
        public string ListNameForNotification { get; set; }

        //Global Config
        public bool IgnoreTooOldItems { get; set; }
        public bool IgnoreErrors { get; set; }
        public bool VerboseMode { get; set; }

        //Announcement
        public AnnouncementConfig AnnouncementConfig { get; set; }

        //Assignment
        public AssignmentConfig AssignmentConfig { get; set; }

        //Quiz
        public QuizConfig QuizConfig { get; set; }

        //Discussion
        public DiscussionConfig DiscussionConfig { get; set; }

        //Notification
        public NotificationConfig NotificationConfig { get; set; }
    }

    public class ListNamesForCategory
    {
        public string QuizListName { get; set; }
        public string DiscussionListName { get; set; }
        public string AssignmentListName { get; set; }
        public string AnnouncementListName { get; set; }
    }

    public enum ListNameMode
    {
        Category, Course
    }

    public class AnnouncementConfig : ICanvasItemConfig
    {
        public bool Enabled { get; set; }
        public bool CreateContent { get; set; }
        public bool CreateDueDate { get; set; }
        public bool CreateAttachments { get; set; }
        public bool CreateRemind { get; set; }
        public bool CreateImportance { get; set; }

        public bool UpdateTitle { get; set; }
        public bool UpdateContent { get; set; }
        public bool UpdateDueDate { get; set; }
        public bool UpdateRemind { get; set; }
        public bool UpdateImportance { get; set; }

        public string TitleTemplate { get; set; }
        public bool SetImportance { get; set; }
        public TimeSpan RemindAfter { get; set; }
    }

    public class AssignmentConfig : ICanvasItemConfig
    {
        public bool Enabled { get; set; }
        public bool CreateContent { get; set; }
        public bool CreateDueDate { get; set; }
        public bool CreateRemind { get; set; }
        public bool CreateScoreAndCommit { get; set; }
        public bool CreateAttachments { get; set; }
        public bool CreateImportance { get; set; }
        public bool CreateComments { get; set; }


        public bool UpdateTitle { get; set; }
        public bool UpdateContent { get; set; }
        public bool UpdateDueDate { get; set; }
        public bool UpdateRemind { get; set; }
        public bool UpdateScoreAndCommit { get; set; }
        public bool UpdateImportance { get; set; }
        public bool UpdateComments { get; set; }

        public string TitleTemplate { get; set; }
        public bool SetImportance { get; set; }
        public DueDateMode DueDateMode { get; set; }
        public RemindMode RemindMode { get; set; }
        public TimeSpan BeforeTimeSpan { get; set; }

        public bool DueDateModeFallback { get; set; }
    }
    public enum DueDateMode
    {
        due_at, lock_at, start_at, end_at
    }

    public enum RemindMode
    {
        unlock_at, before_due_at, before_lock_at
    }

    public class QuizConfig : ICanvasItemConfig
    {
        public bool Enabled { get; set; }
        public bool CreateContent { get; set; }
        public bool CreateDueDate { get; set; }
        public bool CreateRemind { get; set; }
        public bool CreateScoreAndCommit { get; set; }
        public bool CreateAttachments { get; set; }
        public bool CreateImportance { get; set; }

        public bool UpdateTitle { get; set; }
        public bool UpdateContent { get; set; }
        public bool UpdateDueDate { get; set; }
        public bool UpdateRemind { get; set; }
        public bool UpdateScoreAndCommit { get; set; }
        public bool UpdateImportance { get; set; }

        public string TitleTemplate { get; set; }
        public bool SetImportance { get; set; }
        public DueDateMode DueDateMode { get; set; }
        public RemindMode RemindMode { get; set; }
        public TimeSpan BeforeTimeSpan { get; set; }
    }

    public class DiscussionConfig : ICanvasItemConfig
    {
        public bool Enabled { get; set; }
        public bool CreateContent { get; set; }
        public bool CreateDueDate { get; set; }
        public bool CreateRemind { get; set; }
        public bool CreateAttachments { get; set; }
        public bool CreateImportance { get; set; }

        public bool UpdateTitle { get; set; }
        public bool UpdateContent { get; set; }
        public bool UpdateDueDate { get; set; }
        public bool UpdateRemind { get; set; }
        public bool UpdateImportance { get; set; }

        public string TitleTemplate { get; set; }
        public bool SetImportance { get; set; }
        public TimeSpan RemindAfter { get; set; }
    }

    public class NotificationConfig : ICanvasItemConfig
    {
        public bool Enabled { get; set; }
        public bool CreateContent { get; set; }
        public bool CreateDueDate { get; set; }
        public bool CreateRemind { get; set; }
        public bool CreateImportance { get; set; }

        public bool UpdateTitle { get; set; }
        public bool UpdateContent { get; set; }
        public bool UpdateDueDate { get; set; }
        public bool UpdateRemind { get; set; }
        public bool UpdateImportance { get; set; }

        public string TitleTemplate { get; set; }
        public bool SetImportance { get; set; }
        public DueDateMode DueDateMode { get; set; }
        public TimeSpan RemindAfter { get; set; }
    }

    public interface ICanvasItemConfig
    {
        public bool CreateContent { get; set; }
        public bool CreateDueDate { get; set; }
        public bool CreateRemind { get; set; }
        public bool CreateImportance { get; set; }

        public bool UpdateTitle { get; set; }
        public bool UpdateContent { get; set; }
        public bool UpdateDueDate { get; set; }
        public bool UpdateRemind { get; set; }
        public bool UpdateImportance { get; set; }

        public string TitleTemplate { get; set; }
        public bool SetImportance { get; set; }
    }
}
