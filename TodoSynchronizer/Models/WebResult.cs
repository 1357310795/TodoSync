using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Models
{
    public class WebResult : CommonResult
    {
        public HttpStatusCode? code;
        public string message;

        public WebResult(HttpStatusCode? code, bool success, string result, string message)
        {
            this.code = code;
            this.success = success;
            this.result = result;
            this.message = message;
        }
    }
}
