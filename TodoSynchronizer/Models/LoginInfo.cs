using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TodoSynchronizer.Models
{
    public class LoginInfoModel
    {
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public BitmapSource UserAvatar { get; set; }
        public bool IsLogin { get; set; }
    }
}
