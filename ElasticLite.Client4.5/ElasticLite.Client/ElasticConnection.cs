using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ElasticLite.Client
{
    public class ElasticConnection
    {
        private Random random = new Random();
        public static Queue<string> connections = null;
        public int count = 0;
        public int Timeout { get; set; }
        public ICredentials Credentials { get; set; }
        public IWebProxy Proxy { get; set; }
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
            Timeout = timeout;
            count = connections.Count;
        }
        public string Delete(string command, string jsonData = null)
        {
            return ExecuteRequest("DELETE", command, jsonData);
        }
        public string Get(string command, string jsonData = null)
        {
            return ExecuteRequest("GET", command, jsonData);
        }
        public string Head(string command, string jsonData = null)
        {
            return ExecuteRequest("HEAD", command, jsonData);
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
                    using (WebResponse response = request.GetResponse())
                    {
                        string result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                        return result;
                    }
                }
                catch (WebException ex)
                {
                    //从队列获取的连接不可用
                    string unuseConnect = connections.Dequeue();
                    //把不可用的连接放入队尾
                    connections.Enqueue(unuseConnect);
                    //通知维护人员
                    //...
                }
            }
            throw new WebException("all connections are unreachable");
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
