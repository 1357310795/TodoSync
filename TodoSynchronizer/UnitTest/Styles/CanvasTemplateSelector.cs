using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TodoSynchronizer.Core.Models.CanvasModels;

namespace TodoSynchronizer.UnitTest
{
    public class CanvasTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CourseTemplate { get; set; }
        public DataTemplate AssignmentTemplate { get; set; }
        public DataTemplate QuizTemplate { get; set; }
        public DataTemplate AnnouncementTemplate { get; set; }
        public DataTemplate DiscussionTemplate { get; set; }
        public override DataTemplate SelectTemplate(object obj, DependencyObject container)
        {
            if (obj == null) return null;
            if (obj is Course) return CourseTemplate;
            if (obj is Assignment) return AssignmentTemplate;
            if (obj is Quiz) return QuizTemplate;
            if (obj is Announcement) return AnnouncementTemplate;
            if (obj is Discussion) return DiscussionTemplate;
            return null;
        }
    }
}
