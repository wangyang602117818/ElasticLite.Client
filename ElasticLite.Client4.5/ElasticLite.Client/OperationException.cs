using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticLite.Client
{
    public class OperationException : Exception
    {
        public int HttpStatusCode { get; set; }
        public OperationException(string message, int httpStatusCode, Exception innerException)
        : base(message, innerException)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}
