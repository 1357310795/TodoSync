using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models
{
    public class WebResult : CommonResult
    {
        public HttpStatusCode? code;
        public string message;
        public WebHeaderCollection headers;

        public WebResult(HttpStatusCode? code, bool success, string result, string message, WebHeaderCollection headers)
        {
            this.code = code;
            this.success = success;
            this.result = result;
            this.message = message;
            this.headers = headers;
        }
    }
}
