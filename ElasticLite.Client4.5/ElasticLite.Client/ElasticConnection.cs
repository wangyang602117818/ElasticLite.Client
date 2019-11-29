using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ElasticLite.Client
{
    public class ElasticConnection
    {
        public static Queue<string> connections = null;
        public int count = 0;
        public int Timeout { get; set; }
        public ICredentials Credentials { get; set; }
        public IWebProxy Proxy { get; set; }
        public SmtpClient SmtpClient { get; set; }
        public string MailSender { get; set; }
        public IEnumerable<string> MailTo { get; set; }
        public string Subject { get; set; }
        public static string date = DateTime.Now.ToString("yyyyMMddHH");
        public ElasticConnection(string url, int timeout = 6000)
        {
            if (connections == null)
                connections = new Queue<string>(new List<string>() { url });
            count = 1;
            Timeout = timeout;
        }
        public ElasticConnection(IEnumerable<string> urls, int timeout = 6000)
        {
            if (connections == null)
                connections = new Queue<string>(urls);
            count = connections.Count;
            Timeout = timeout;
        }
        public string Delete(string command, string jsonData = null)
        {
            return ExecuteRequest("DELETE", command, jsonData);
        }
        public string Get(string command, string jsonData = null)
        {
            return ExecuteRequest("GET", command, jsonData);
        }
        public bool Head(string command, string jsonData = null)
        {
            var result = ExecuteRequest("HEAD", command, jsonData);
            return result == "404" ? false : true;
        }
        public string Post(string command, string jsonData = null)
        {
            return ExecuteRequest("Post", command, jsonData);
        }
        public string Put(string command, string jsonData = null)
        {
            return ExecuteRequest("Put", command, jsonData);
        }
        private string ExecuteRequest(string method, string command, string jsonData)
        {
            WebException ex = null;
            for (var i = 0; i < count; i++)
            {
                //从队列获取一个连接
                string uri = connections.Peek();
                uri = uri.TrimEnd('/') + "/" + command.TrimStart('/');
                try
                {
                    HttpWebRequest request = CreateRequest(method, uri);
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(jsonData);
                        request.ContentLength = buffer.Length;
                        using (Stream requestStream = request.GetRequestStream())
                        {
                            requestStream.Write(buffer, 0, buffer.Length);
                        }
                    }
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
                catch (WebException webException)
                {
                    if (webException.Response != null && ((HttpWebResponse)webException.Response).StatusCode == HttpStatusCode.NotFound) return "404";
                    //从队列获取的连接不可用
                    string unuseConnect = connections.Dequeue();
                    //把不可用的连接放入队尾
                    connections.Enqueue(unuseConnect);
                    //通知维护人员
                    if (SmtpClient != null) SendEmail(webException.Message);
                    ex = webException;
                }
            }
            throw ex;
        }
        private void SendEmail(string message)
        {
            if (DateTime.Now.ToString("yyyyMMddHH") != date)
            {
                date = DateTime.Now.ToString("yyyyMMddHH");
                MailMessage mailMessage = new MailMessage()
                {
                    From = new MailAddress(MailSender),
                    Body = message,
                    Subject = Subject,
                    IsBodyHtml = false
                };
                Array.ForEach(MailTo.ToArray(), t => mailMessage.To.Add(t));
                SmtpClient.Send(mailMessage);
            }
        }
        protected virtual HttpWebRequest CreateRequest(string method, string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Timeout = Timeout;
            request.Method = method;
            if (Proxy != null) request.Proxy = Proxy;
            if (Credentials != null) request.Credentials = Credentials;
            return request;
        }

    }
}
