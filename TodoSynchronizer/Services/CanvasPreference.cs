using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoSynchronizer.Models.CanvasModels;

namespace TodoSynchronizer.Services
{
    public static class CanvasPreference
    {
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

        public static bool GetAssignmentImprotance()
        {
            return true;
        }
    }
}
