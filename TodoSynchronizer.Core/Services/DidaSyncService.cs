using System;
using System.Collections.Generic;
using System.Linq;
using TodoSynchronizer.Core.Config;
using TodoSynchronizer.Core.Extensions;
using TodoSynchronizer.Core.Helpers;
using TodoSynchronizer.Core.Models;
using TodoSynchronizer.Core.Models.CanvasModels;
using TodoSynchronizer.Core.Models.DidaModels;

namespace TodoSynchronizer.Core.Services
{
    public class DidaSyncService
    {
        private const string SourcePrefix = "TodoSync: ";
        private readonly DidaService didaService;
        private Dictionary<string, DidaTask> dicUrl;
        private Dictionary<string, DidaTaskList> dicCategory;
        public int CourseCount, ItemCount, UpdateCount, FailedCount;

        public DidaSyncService(DidaService didaService)
        {
            this.didaService = didaService;
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { 
                message = value;
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Progress, value));
            }
        }

        public delegate void ReportProgressDelegate(SyncState state);

        public event ReportProgressDelegate OnReportProgress;

        public void Go()
        {
            #region 初始化
            CourseCount = 0;
            ItemCount = 0;
            UpdateCount = 0;
            FailedCount = 0;
            #endregion

            List<Course> courses;
            #region 读取 Canvas 课程列表
            Message = "读取 Canvas 课程列表";
            try
            {
                courses = CanvasService.ListCourses();
                if (courses == null)
                    throw new Exception("Canvas 课程列表为空");
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
            #endregion

            #region 检查滴答清单
            Message = "检查滴答清单";
            try
            {
                dicCategory = new Dictionary<string, DidaTaskList>();
                var projects = didaService.ListProjects();
                var resolvedLists = new Dictionary<string, DidaTaskList>();

                void FindList(string cat, string name)
                {
                    if (resolvedLists.TryGetValue(name, out var resolvedTaskList))
                    {
                        dicCategory.Add(cat, resolvedTaskList);
                        return;
                    }

                    var taskList = projects.Find(x => x.Closed != true && x.Kind != "NOTE" && x.Name?.CleanEmoji() == name);

                    if (taskList == null)
                        taskList = didaService.AddTaskList(name);

                    Message = $"找到滴答清单：{taskList.Name}";
                    resolvedLists[name] = taskList;
                    dicCategory.Add(cat, taskList);
                }

                if (SyncConfig.Default.NotificationConfig.Enabled)
                    FindList("notification", CanvasStringTemplateHelper.GetNotificationListName());

                if (SyncConfig.Default.QuizConfig.Enabled)
                    FindList("quiz", SyncConfig.Default.ListNamesForCategory.QuizListName);
                if (SyncConfig.Default.DiscussionConfig.Enabled)
                    FindList("discussion", SyncConfig.Default.ListNamesForCategory.DiscussionListName);
                if (SyncConfig.Default.AssignmentConfig.Enabled)
                    FindList("assignment", SyncConfig.Default.ListNamesForCategory.AssignmentListName);
                if (SyncConfig.Default.AnouncementConfig.Enabled)
                    FindList("anouncement", SyncConfig.Default.ListNamesForCategory.AnouncementListName);
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
            #endregion

            #region 读取滴答清单任务与 Canvas 来源
            Message = "读取滴答清单任务与 Canvas 来源";
            try
            {
                dicUrl = new Dictionary<string, DidaTask>();
                foreach (var projectId in dicCategory.Values.Select(project => project.Id).Distinct())
                {
                    var isNotificationList = dicCategory.TryGetValue("notification", out var notificationList)
                        && notificationList.Id == projectId;
                    var tasks = didaService.GetTasks(projectId).Concat(didaService.GetCompleted(projectId));
                    foreach (var task in tasks.DistinctBy(task => task.Id))
                    {
                        var source = ReadSource(task.Desc) ?? ReadSource(task.Content);
                        if (source == null)
                        {
                            // 旧版本将 Canvas 链接或全局通知 ID 写入任务评论。
                            source = didaService.ListTaskComments(projectId, task.Id)
                                .Select(comment => comment.Title?.Trim())
                                .FirstOrDefault(title => IsCanvasUrl(title) || (isNotificationList && long.TryParse(title, out _)));
                        }

                        if (source != null)
                            dicUrl.TryAdd(source, task);
                    }
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
            #endregion

            #region 处理全局通知
            if (SyncConfig.Default.NotificationConfig.Enabled)
            {
                ProcessNotifications();
            }
            #endregion

            #region Main
            try
            {
                foreach (var course in courses)
                {
                    CourseCount++;
                    if (SyncConfig.Default.AssignmentConfig.Enabled)
                        ProcessAssignments(GetCourseMessage(course), course, dicCategory["assignment"]);
                    if (SyncConfig.Default.AnouncementConfig.Enabled)
                        ProcessAnouncements(GetCourseMessage(course), course, dicCategory["anouncement"]);
                    if (SyncConfig.Default.QuizConfig.Enabled)
                        ProcessQuizes(GetCourseMessage(course), course, dicCategory["quiz"]);
                    if (SyncConfig.Default.DiscussionConfig.Enabled)
                        ProcessDiscussions(GetCourseMessage(course), course, dicCategory["discussion"]);
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
            #endregion

            #region 完成
            OnReportProgress.Invoke(new SyncState(
                SyncStateEnum.Finished,
                $"完成！已处理 {CourseCount} 门课程中的 {ItemCount} 个项目，更新 {UpdateCount} 个项目"
            ));
            #endregion
        }

        #region Assignments
        public void ProcessAssignments(string message_prefix, Course course, DidaTaskList taskList)
        {
            Message = message_prefix + "作业";
            try
            {
                var assignments = CanvasService.ListAssignments(course.Id.ToString());
                if (assignments == null)
                    return;
                if (assignments.Count == 0)
                    return;

                foreach (var assignment in assignments)
                {
                    if (assignment.IsQuizAssignment) continue;
                    if (SyncConfig.Default.IgnoreTooOldItems)
                        if (assignment?.DueAt?.ToUniversalTime() < DateTime.Now.AddDays(-14).ToUniversalTime())
                            continue;
                    ItemCount++;
                    Message = message_prefix + GetItemMessage(assignment);
                    var source = assignment.HtmlUrl;
                    var isNew = !dicUrl.TryGetValue(source, out var didaTask);
                    didaTask ??= new DidaTask { ProjectId = taskList.Id };
                    var updated = UpdateCanvasItem(course, assignment, didaTask, SyncConfig.Default.AssignmentConfig);

                    //---Submissions -> CheckItems---//
                    var assignmentConfig = SyncConfig.Default.AssignmentConfig;
                    if ((isNew ? assignmentConfig.CreateScoreAndCommit : assignmentConfig.UpdateScoreAndCommit)
                        && assignment.HasSubmittedSubmissions)
                    {
                        var submission = CanvasService.GetAssignmentSubmisson(course.Id.ToString(), assignment.Id.ToString());
                        if (submission != null)
                        {
                            bool checkitemupdated = false;
                            if (didaTask.Items == null)
                                didaTask.Items = new List<DidaCheckItem>();
                            var links = didaTask.Items;

                            DidaCheckItem checkitem1 = null;
                            if (links.Count >= 1)
                                checkitem1 = links[0];
                            else
                            {
                                links.Add(new DidaCheckItem());
                                checkitem1 = links[0];
                            }
                                
                            var res2 = UpdateSubmissionInfo(assignment, submission, checkitem1, CanvasStringTemplateHelper.GetSubmissionDesc);
                            checkitemupdated |= res2;
                            
                            DidaCheckItem checkitem2 = null;
                            if (links.Count >= 2)
                                checkitem2 = links[1];
                            else
                            {
                                links.Add(new DidaCheckItem());
                                checkitem2 = links[1];
                            }

                            var res3 = UpdateSubmissionInfo(assignment, submission, checkitem2, CanvasStringTemplateHelper.GetGradeDesc);
                            checkitemupdated |= res3;

                            //---Comments---//
                            if (SyncConfig.Default.AssignmentConfig.CreateComments && isNew
                                || SyncConfig.Default.AssignmentConfig.UpdateComments && !isNew)
                            {
                                if (submission.SubmissionComments?.Count > 0)
                                {
                                    int i = 2;
                                    foreach (var comment in submission.SubmissionComments)
                                    {
                                        DidaCheckItem checkitem3 = null;
                                        if (links.Count >= i + 1)
                                            checkitem3 = links[i];
                                        else
                                        {
                                            links.Add(new DidaCheckItem());
                                            checkitem3 = links[i];
                                        }
                                        i++;

                                        var res4 = UpdateSubmissionComment(comment, checkitem3);
                                        checkitemupdated |= res4;
                                    }
                                }
                            }

                            updated |= checkitemupdated;
                        }
                    }

                    if (updated)
                        SaveTask(source, didaTask);
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
        }
        #endregion

        #region Discussions
        private void ProcessDiscussions(string message_prefix, Course course, DidaTaskList taskList)
        {
            Message = message_prefix + "讨论";
            try
            {
                var discussions = CanvasService.ListDiscussions(course.Id.ToString());
                if (discussions == null)
                    return;
                if (discussions.Count == 0)
                    return;

                foreach (var discussion in discussions)
                {
                    if (SyncConfig.Default.IgnoreTooOldItems)
                        if (discussion?.PostedAt?.ToUniversalTime() < DateTime.Now.AddDays(-14).ToUniversalTime())
                            continue;
                    ItemCount++;
                    Message = message_prefix + GetItemMessage(discussion);
                    var source = discussion.HtmlUrl;
                    dicUrl.TryGetValue(source, out var didaTask);
                    didaTask ??= new DidaTask { ProjectId = taskList.Id };
                    var updated = UpdateCanvasItem(course, discussion, didaTask, SyncConfig.Default.DiscussionConfig);

                    if (updated)
                        SaveTask(source, didaTask);
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
        }
        #endregion

        #region Quizes
        private void ProcessQuizes(string message_prefix, Course course, DidaTaskList taskList)
        {
            Message = message_prefix + "测验";
            try
            {
                var assignments = CanvasService.ListAssignments(course.Id.ToString());
                if (assignments == null)
                    return;
                if (assignments.Count == 0)
                    return;

                foreach (var assignment in assignments)
                {
                    if (!assignment.IsQuizAssignment) continue;
                    if (SyncConfig.Default.IgnoreTooOldItems)
                        if (assignment?.DueAt?.ToUniversalTime() < DateTime.Now.AddDays(-14).ToUniversalTime())
                            continue;
                    ItemCount++;
                    Message = message_prefix + GetItemMessage(assignment);
                    var source = assignment.HtmlUrl;
                    var isNew = !dicUrl.TryGetValue(source, out var didaTask);
                    didaTask ??= new DidaTask { ProjectId = taskList.Id };
                    var updated = UpdateCanvasItem(course, assignment, didaTask, SyncConfig.Default.QuizConfig);

                    //---Submissions -> CheckItems---//
                    if (SyncConfig.Default.QuizConfig.CreateScoreAndCommit && isNew
                        || SyncConfig.Default.QuizConfig.UpdateScoreAndCommit && !isNew)
                        if (assignment.HasSubmittedSubmissions)
                        {
                            if (didaTask.Items == null)
                                didaTask.Items = new List<DidaCheckItem>();
                            var links = didaTask.Items;
                            var quizsubmissions = CanvasService.ListQuizSubmissons(course.Id.ToString(), assignment.QuizId.ToString());

                            if (quizsubmissions != null)
                            {
                                for (int i = 0; i < quizsubmissions.Count; i++)
                                {
                                    DidaCheckItem checkitem0 = null;
                                    if (links.Count >= i + 1)
                                        checkitem0 = links[i];
                                    else
                                    {
                                        links.Add(new DidaCheckItem());
                                        checkitem0 = links[i];
                                    }

                                    updated |= UpdateSubmissionInfo(assignment, quizsubmissions[i], checkitem0, CanvasStringTemplateHelper.GetSubmissionDesc);
                                }
                            }
                        
                        }

                    if (updated)
                        SaveTask(source, didaTask);
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
        }
        #endregion

        #region Anouncements
        private void ProcessAnouncements(string message_prefix, Course course, DidaTaskList taskList)
        {
            Message = message_prefix + "公告";
            try
            {
                var anouncements = CanvasService.ListAnouncements(course.Id.ToString());
                if (anouncements == null)
                    return;
                if (anouncements.Count == 0)
                    return;

                foreach (var anouncement in anouncements)
                {
                    if (SyncConfig.Default.IgnoreTooOldItems)
                        if (anouncement?.PostedAt?.ToUniversalTime() < DateTime.Now.AddDays(-14).ToUniversalTime())
                            continue;
                    ItemCount++;
                    Message = message_prefix + GetItemMessage(anouncement);
                    var source = anouncement.HtmlUrl;
                    dicUrl.TryGetValue(source, out var didaTask);
                    didaTask ??= new DidaTask { ProjectId = taskList.Id };
                    var updated = UpdateCanvasItem(course, anouncement, didaTask, SyncConfig.Default.AnouncementConfig);

                    if (updated)
                        SaveTask(source, didaTask);
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
        }
        #endregion

        #region Notification
        public void ProcessNotifications()
        {
            try
            {
                Message = "处理全局通知";

                var notifications = CanvasService.ListNotifications();
                if (notifications == null)
                    return;
                if (notifications.Count == 0)
                    return;

                
                var notilist = dicCategory["notification"];
                foreach (var notification in notifications)
                {
                    ItemCount++;
                    Message = "处理全局通知 " + notification.Subject;
                    var source = notification.Id.ToString();
                    if (!dicUrl.TryGetValue(source, out var didaTask))
                        didaTask = new DidaTask { ProjectId = notilist.Id };

                    var updated = UpdateCanvasItem(null, notification, didaTask, SyncConfig.Default.NotificationConfig);
                    if (updated)
                        SaveTask(source, didaTask);
                }
            }
            catch (Exception ex)
            {
                OnReportProgress.Invoke(new SyncState(SyncStateEnum.Error, ex.ToString()));
                return;
            }
        }
        #endregion

        #region Common
        private bool UpdateSubmissionInfo<T>(Assignment assignment, T submission, DidaCheckItem checklistitem, Func<Assignment, T, string> func)
        {
            var modified = false;
            var desc = func(assignment, submission);
            var check = !desc.Contains("未") && !desc.Contains("正在");

            if (checklistitem.Status != (check ? 1 : 0))
            {
                checklistitem.Status = (check ? 1 : 0);
                modified = true;
            }
            if (checklistitem == null || checklistitem.Title != desc)
            {
                checklistitem.Title = desc;
                modified = true;
            }
            return modified;
        }

        private bool UpdateSubmissionComment(SubmissionComment comment, DidaCheckItem checklistitem)
        {
            var modified = false;
            var desc = CanvasStringTemplateHelper.GetSubmissionComment(comment);
            var check = true;

            if (checklistitem.Status != (check ? 1 : 0))
            {
                checklistitem.Status = (check ? 1 : 0);
                modified = true;
            }
            if (checklistitem == null || checklistitem.Title != desc)
            {
                checklistitem.Title = desc;
                modified = true;
            }
            return modified;
        }

        private void SaveTask(string source, DidaTask task)
        {
            if (task.Items?.Count > 0 && task.Kind != "CHECKLIST")
            {
                task.Desc = WithoutSource(task.Content) ?? WithoutSource(task.Desc);
                task.Content = null;
                task.Kind = "CHECKLIST";
            }

            if (task.Kind == "CHECKLIST")
                task.Desc = WithSource(task.Desc, source);
            else
                task.Content = WithSource(task.Content ?? task.Desc, source);

            didaService.SaveTask(task);
            dicUrl[source] = task;
            UpdateCount++;
        }

        private static bool IsCanvasUrl(string source)
        {
            return Uri.TryCreate(source, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                && string.Equals(uri.Host, CanvasService.Client?.BaseAddress?.Host, StringComparison.OrdinalIgnoreCase);
        }

        private static string ReadSource(string description)
        {
            var lastLine = description?.TrimEnd().Split('\n').LastOrDefault();
            return lastLine != null && lastLine.StartsWith(SourcePrefix, StringComparison.Ordinal)
                ? lastLine.Substring(SourcePrefix.Length).Trim()
                : null;
        }

        private static string WithoutSource(string description)
        {
            if (ReadSource(description) == null)
                return description;

            var index = description.TrimEnd().LastIndexOf('\n');
            return index < 0 ? "" : description.Substring(0, index).TrimEnd();
        }

        private static string WithSource(string description, string source)
        {
            var text = WithoutSource(description)?.TrimEnd();
            var marker = SourcePrefix + source;
            return string.IsNullOrEmpty(text) ? marker : text + "\n\n" + marker;
        }

        public bool UpdateCanvasItem(Course course, ICanvasItem item, DidaTask didaTask, ICanvasItemConfig config)
        {
            var isNew = didaTask.Id == null;
            var modified = isNew;
            if (isNew || config.UpdateTitle)
            {
                var title = CanvasStringTemplateHelper.GetTitle(course, item);
                if (title.Trim() != didaTask.Title?.Trim())
                {
                    didaTask.Title = title;
                    modified = true;
                }
            }

            if (isNew ? config.CreateContent : config.UpdateContent)
            {
                var content = CanvasStringTemplateHelper.GetContent(item);
                var current = didaTask.Kind == "CHECKLIST" ? didaTask.Desc : didaTask.Content ?? didaTask.Desc;
                if (content.Trim() != (WithoutSource(current)?.Trim() ?? ""))
                {
                    if (didaTask.Kind == "CHECKLIST")
                        didaTask.Desc = content;
                    else
                        didaTask.Content = content;
                    modified = true;
                }
            }

            if (isNew ? config.CreateDueDate : config.UpdateDueDate)
            {
                var date = CanvasPreference.GetDueTime(item)?.ToUniversalTime();
                if (date != didaTask.DueDate)
                {
                    didaTask.DueDate = didaTask.StartDate = date;
                    if (date.HasValue)
                        didaTask.TimeZone = "Asia/Shanghai";
                    modified = true;
                }
            }

            if (isNew ? config.CreateRemind : config.UpdateRemind)
            {
                var remindTime = CanvasPreference.GetRemindBefore(config);
                if (remindTime.HasValue && didaTask.StartDate.HasValue)
                {
                    var trigger = CanvasStringTemplateHelper.GetTrigger(remindTime.Value);
                    if (trigger != didaTask.Reminders?.FirstOrDefault())
                    {
                        didaTask.Reminders = new List<string> { trigger };
                        modified = true;
                    }
                }
            }

            if (isNew ? config.CreateImportance : config.UpdateImportance)
            {
                var priority = config.SetImportance ? 5 : 0;
                if (didaTask.Priority != priority)
                {
                    didaTask.Priority = priority;
                    modified = true;
                }
            }

            return modified;
        }

        private string GetCourseMessage(Course course)
        {
            return $"处理课程 {(SyncConfig.Default.VerboseMode ? course.Name : CourseCount)} ";
        }

        private string GetItemMessage(ICanvasItem item)
        {
            return $"{item.GetItemName()} {(SyncConfig.Default.VerboseMode ? item.Title : ItemCount)} ";
        }


        #endregion
    }
}
