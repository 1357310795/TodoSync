using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using TodoSynchronizer.Core.Extensions;
using TodoSynchronizer.Core.Models;

namespace TodoSynchronizer.Core.Service
{
    public static class Web
    {
        public static WebResult Get(HttpClient client, string url, Dictionary<string, string> queryparas)
        {
            return Get(client, BuildUrl(url, queryparas));
        }

        public static WebResult Get(HttpClient client, string url, Dictionary<string, string> queryparas, Dictionary<string, string> headers)
        {
            ProcessHeaders(client, headers);
            var task = client.GetAsync(url);
            task.Wait();
            return GetFinalResult(task.GetAwaiter().GetResult());
        }

        public static WebResult Get(HttpClient client, string url)
        {
            var task = client.GetAsync(url);
            task.Wait();
            return GetFinalResult(task.GetAwaiter().GetResult());
        }

        public static void ProcessHeaders(HttpClient client, Dictionary<string, string> headers)
        {
            //if (headers.ContainsKey("Accept"))
            //{
            //    client.DefaultRequestHeaders.Add = headers["Accept"];
            //    headers.Remove("Accept");
            //}
            //if (headers.ContainsKey("User-Agent"))
            //{
            //    req.UserAgent = headers["User-Agent"];
            //    headers.Remove("User-Agent");
            //}
            //if (headers.ContainsKey("Referer"))
            //{
            //    req.Referer = headers["Referer"];
            //    headers.Remove("Referer");
            //}
            //if (headers.ContainsKey("Connection") && headers["Connection"] == "keep-alive")
            //{
            //    req.KeepAlive = true;
            //    headers.Remove("Connection");
            //}
            //if (headers.ContainsKey("Content-Type"))
            //{
            //    req.ContentType = headers["Content-Type"];
            //    headers.Remove("Content-Type");
            //}
            if (headers.ContainsKey("Host"))
            {
                headers.Remove("Host");
            }
            if (headers.ContainsKey("Content-Length"))
            {
                headers.Remove("Content-Length");
            }
            if (headers.ContainsKey("Cache-Control"))
            {
                headers.Remove("Cache-Control");
            }
            foreach (var i in headers)
            {
                client.DefaultRequestHeaders.Add(i.Key, i.Value);
            }
        }

        public static WebResult GetFinalResult(HttpResponseMessage responseMessage)
        {
            return new WebResult(responseMessage.StatusCode, true, responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult(), null, responseMessage.Headers);
        }


        public static WebResult Post(string url, Dictionary<string, string> queryparas, Dictionary<string, string> headers, string formdata, bool urlencode)
        {
            return Post(BuildUrl(url, queryparas), headers, formdata, urlencode);
        }

        public static WebResult Post(string url, Dictionary<string, string> queryparas, Dictionary<string, string> headers, Dictionary<string, string> formdata, bool urlencode)
        {
            return Post(BuildUrl(url, queryparas), headers, BuildForm(formdata, urlencode), urlencode);
        }

        public static WebResult Post(string url, Dictionary<string, string> headers, Dictionary<string, string> formdata, bool urlencode)
        {
            return Post(url, headers, BuildForm(formdata, urlencode), urlencode);
        }

        public static WebResult Post(string url, Dictionary<string, string> headers, string formdata, bool urlencode)
        {
            return Post(url, headers, formdata, urlencode, Encoding.Default);
        }

        public static WebResult Post(string url, Dictionary<string, string> headers, string formdata, bool urlencode, Encoding eco)
        {
            #region 初始化请求
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            ProcessHeaders(headers, req);
            #endregion

            #region 添加Post 参数
            try
            {
                byte[] data = eco.GetBytes(formdata);
                req.ContentLength = data.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }
            }
            catch (Exception ex)
            {
                return new WebResult(null, false, null, ex.ToString());
            }
            #endregion

            #region 获取响应
            return GetFinalResult(req);
            #endregion
        }

        public static WebResult Post(string url, Dictionary<string, string> headers, string formdata, Encoding eco)
        {
            return Post(url, headers, formdata, false, eco);
        }

        public static WebResult Post(string url, Dictionary<string, string> headers, Stream datastream)
        {
            #region 初始化请求
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            ProcessHeaders(headers, req);
            #endregion

            req.ContentLength = datastream.Length;

            #region 输入二进制流
            if (datastream != null)
            {
                datastream.Position = 0;
                Stream requestStream = req.GetRequestStream();
                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = datastream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                }
                datastream.Close();
            }
            #endregion

            #region 获取响应
            return GetFinalResult(req);
            #endregion
        }

        public static string BuildUrl(string url, Dictionary<string, string> queryparas)
        {
            StringBuilder builder1 = new StringBuilder();
            builder1.Append(url);

            if (queryparas.Count > 0)
            {
                builder1.Append("?");
                int i = 0;
                foreach (var item in queryparas)
                {
                    if (i > 0)
                        builder1.Append("&");
                    builder1.AppendFormat("{0}={1}", item.Key, item.Value);
                    i++;
                }
            }
            return builder1.ToString();
        }

        public static string BuildForm(Dictionary<string, string> formdata, bool urlencode)
        {
            StringBuilder builder = new StringBuilder();

            if (formdata.Count > 0)
            {
                int i = 0;
                foreach (var item in formdata)
                {
                    if (i > 0)
                        builder.Append("&");
                    builder.AppendFormat("{0}={1}", item.Key, urlencode ? item.Value.UrlUnescape() : item.Value);
                    i++;
                }
            }
            return builder.ToString();
        }

        public static void ProcessHeaders(Dictionary<string, string> headers, HttpWebRequest req)
        {
            if (headers.ContainsKey("Accept"))
            {
                req.Accept = headers["Accept"];
                headers.Remove("Accept");
            }
            if (headers.ContainsKey("User-Agent"))
            {
                req.UserAgent = headers["User-Agent"];
                headers.Remove("User-Agent");
            }
            if (headers.ContainsKey("Referer"))
            {
                req.Referer = headers["Referer"];
                headers.Remove("Referer");
            }
            if (headers.ContainsKey("Connection") && headers["Connection"] == "keep-alive")
            {
                req.KeepAlive = true;
                headers.Remove("Connection");
            }
            if (headers.ContainsKey("Content-Type"))
            {
                req.ContentType = headers["Content-Type"];
                headers.Remove("Content-Type");
            }
            if (headers.ContainsKey("Host"))
            {
                headers.Remove("Host");
            }
            if (headers.ContainsKey("Content-Length"))
            {
                headers.Remove("Content-Length");
            }
            if (headers.ContainsKey("Cache-Control"))
            {
                headers.Remove("Cache-Control");
            }
            foreach (var i in headers)
            {
                req.Headers[i.Key] = i.Value;
            }
        }

        public static WebResult Get(string url, Dictionary<string, string> headers)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            ProcessHeaders(headers, req);
            return GetFinalResult(req);
        }

        public static WebResult Get(string url, Dictionary<string, string> headers, Dictionary<string, string> queryparas)
        {
            return Get(BuildUrl(url, queryparas),headers);
        }

        public static WebResult Patch(string url, Dictionary<string, string> headers, string formdata, bool urlencode, Encoding eco)
        {
            #region 初始化请求
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "PATCH";
            ProcessHeaders(headers, req);
            #endregion

            #region 添加 body 参数
            try
            {
                byte[] data = eco.GetBytes(formdata);
                req.ContentLength = data.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }
            }
            catch (Exception ex)
            {
                return new WebResult(null, false, null, ex.ToString());
            }
            #endregion

            #region 获取响应
            return GetFinalResult(req);
            #endregion
        }

        public static WebResult Patch(string url, Dictionary<string, string> headers, Dictionary<string, string> queryparas, string formdata)
        {
            return Patch(BuildUrl(url, queryparas), headers, formdata, false, Encoding.Default);
        }

        public static WebResult GetFinalResult(HttpWebRequest req)
        {
            HttpWebResponse resp = null;
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
                resp = (HttpWebResponse)ex.Response;
                if (resp == null)
                    return new WebResult(null, false, null, ex.ToString(), resp?.Headers);
            }
            Stream stream = null;
            try
            {
                stream = resp.GetResponseStream();
            }
            catch (Exception ex)
            {
                if (stream == null)
                    return new WebResult(resp.StatusCode, false, null, ex.ToString(), resp.Headers);
            }
            try
            {
                var body = GetResponseBody(resp, stream);
                return new WebResult(resp.StatusCode, true, body, null, resp.Headers);
            }
            catch (Exception ex)
            {
                return new WebResult(resp.StatusCode, false, null, ex.ToString(), resp.Headers);
            }
        }

        public static string GetResponseBody(HttpWebResponse resp, Stream stream)
        {
            string result;
            //获取响应内容
            if (resp.ContentEncoding != null && resp.ContentEncoding.ToLower() == "gzip")
            {
                GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress);
                //对解压缩后的字符串信息解析
                using (StreamReader reader = new StreamReader(gzip, Encoding.UTF8))//中文编码处理
                {
                    result = reader.ReadToEnd();
                }
            }
            else if (resp.ContentEncoding != null && resp.ContentEncoding.ToLower() == "br")
            {
                BrotliStream br = new BrotliStream(stream, CompressionMode.Decompress);
                //对解压缩后的字符串信息解析
                using (StreamReader reader = new StreamReader(br, Encoding.UTF8))//中文编码处理
                {
                    result = reader.ReadToEnd();
                }
            }
            else
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }

        public static Dictionary<string, string> PostCommonHeaders()
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            d.Add("Accept-Encoding", "gzip, deflate, br");
            d.Add("Accept-Language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            d.Add("Cache-Control", "no-cache");
            d.Add("Connection", "keep-alive");
            d.Add("Content-Type", "application/x-www-form-urlencoded;charset=UTF-8");
            d.Add("Pragma", "no-cache");
            d.Add("Referer", "");
            d.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/82.0.3202.9 Safari/537.36");
            d.Add("X-Requested-With", "XMLHttpRequest");
            return d;
        }
    }

}
