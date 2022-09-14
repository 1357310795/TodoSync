using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wpf.Ui.Services;

namespace TodoSynchronizer.QuickTool.Services
{
    public static class NaviService
    {
        static Frame _frame;
        public static void SetFrame(Frame frame)
        {
            _frame = frame;
            _frame.Navigated += _frame_Navigated;
        }

        private static void _frame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Content != null)
            {
                TransitionService.ApplyTransition(e.Content, TransitionType.SlideBottom, 300);
            }
        }

        public static void Navigate(object obj)
        {
            _frame.Navigate(obj);
        }
    }
}
