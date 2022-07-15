using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Models.CanvasModels
{
    public class CanvasLoginResult : CommonResult
    {
        public UserProfile User { get; set; }

        public CanvasLoginResult(bool success, string result)
        {
            this.success = success;
            this.result = result;
        }

        public CanvasLoginResult(UserProfile userProfile)
        {
            this.User = userProfile;
        }
    }
}
