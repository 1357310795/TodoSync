using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TodoSynchronizer.Core.Models
{
    public class WebResult : CommonResult
    {
        public HttpStatusCode? code;
        public string message;
        public WebHeaderCollection headers;
        public HttpResponseHeaders headers1;

        public WebResult(HttpStatusCode? code, bool success, string result, string message)
        {
            this.code = code;
            this.success = success;
            this.result = result;
            this.message = message;
        }

        public WebResult(HttpStatusCode? code, bool success, string result, string message, HttpResponseHeaders headers)
        {
            this.code = code;
            this.success = success;
            this.result = result;
            this.message = message;
            this.headers1 = headers;
        }

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
