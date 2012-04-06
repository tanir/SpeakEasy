using System.Net;
using System.Text;

namespace Resticle
{
    public class PutRestRequest : RestRequest
    {
        public PutRestRequest(string url, object body)
            : base(url)
        {
            Body = body;
        }

        public object Body { get; set; }

        public bool HasBody
        {
            get { return Body != null; }
        }

        public override HttpWebRequest BuildWebRequest(ITransmission transmission)
        {
            var baseRequest = base.BuildWebRequest(transmission);
            baseRequest.Method = "PUT";

            if (HasBody)
            {
                var serializer = transmission.DefaultSerializer;

                var serialized = serializer.Serialize(Body);
                var bytes = Encoding.Default.GetBytes(serialized);

                baseRequest.ContentLength = bytes.Length;
                using (var stream = baseRequest.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            return baseRequest;
        }
    }
}