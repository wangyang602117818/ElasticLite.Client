using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ElasticLite.Client
{
    public class ElasticConnection
    {
        private IEnumerable<string> connections = new List<string>();
        public ElasticConnection(string url, int port, int timeout = 6000)
        {

        }
        public int DefaultPort { get; set; }
        public string DefaultScheme { get; set; }
        public string DefaultHost { get; set; }
        /// <summary>
        /// Timeout in milliseconds Default 6000ms=6s
        /// </summary>
        public int Timeout { get; set; }
        public string Delete(string command, string jsonData = null)
        {
            return null;
        }
        public string Get(string command, string jsonData = null)
        {
            return null;
        }
        public string Head(string command, string jsonData = null)
        {
            return null;
        }
        public string Post(string command, string jsonData = null)
        {
            return null;
        }
        public string Put(string command, string jsonData = null)
        {
            return null;
        }
        private OperationException HandleWebException(WebException webException)
        {
            string message = webException.Message;
            WebResponse response = webException.Response;
            if (response != null)
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    message = new StreamReader(responseStream, detectEncodingFromByteOrderMarks: true).ReadToEnd();
                }
            }
            int statusCode = 0;
            if (response is HttpWebResponse)
            {
                statusCode = (int)((HttpWebResponse)response).StatusCode;
            }
            return new OperationException(message, statusCode, webException);
        }

    }
}
