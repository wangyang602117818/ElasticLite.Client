using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ElasticLite.Client
{
    public class ElasticConnection
    {
        private Random random = new Random();
        public List<string> connections = new List<string>();
        public List<Uri> uri = new List<Uri>();
        public int? index = null;
        public ElasticConnection(string url, int timeout = 6000)
        {
            uri.Add(new Uri(url));
            connections.Add(url);
            Timeout = timeout;
        }
        public ElasticConnection(IEnumerable<string> urls, int timeout = 6000)
        {
            foreach (string url in urls)
            {
                uri.Add(new Uri(url));
                connections.Add(url);
            }
        }
        /// <summary>
        /// Timeout in milliseconds Default 6000ms=6s
        /// </summary>
        public int Timeout { get; set; }
        public ICredentials Credentials { get; set; }
        public IWebProxy Proxy { get; set; }
        public string Delete(string command, string jsonData = null)
        {
            return ExecuteRequest(null, "DELETE", command, jsonData);
        }
        public string Get(string command, string jsonData = null)
        {
            return ExecuteRequest(null, "GET", command, jsonData);
        }
        public string Head(string command, string jsonData = null)
        {
            return ExecuteRequest(null, "HEAD", command, jsonData);
        }
        public string Post(string command, string jsonData = null)
        {
            return ExecuteRequest(null, "Post", command, jsonData);
        }
        public string Put(string command, string jsonData = null)
        {
            try
            {
                return ExecuteRequest(null, "Put", command, jsonData);
            }
            catch (WebException ex)
            {
                if (connections.Count == index + 1) throw ex;
                for (var i = 0; i < connections.Count; i++)
                {
                    if (i == index) continue;

                }

            }
            return "";
        }
        private string ExecuteRequest(string url, string method, string command, string jsonData)
        {
            try
            {
                index = random.Next(connections.Count());
                string uri = url ?? connections[index.Value];
                uri = uri.TrimEnd('/') + "/" + command.TrimStart('/');
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
                throw ex;
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
