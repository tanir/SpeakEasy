using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace HttpSpeak
{
    public abstract class PostLikeRequest : HttpRequest
    {
        protected PostLikeRequest(Resource resource)
            : base(resource)
        {
        }

        protected PostLikeRequest(Resource resource, object body)
            : base(resource)
        {
            Body = body;
        }

        protected PostLikeRequest(Resource resource, IEnumerable<FileUpload> files)
            : base(resource)
        {
            Files = files;
        }

        public object Body { get; private set; }

        public IEnumerable<FileUpload> Files { get; private set; }

        public bool HasSerializableBody
        {
            get { return Body != null || Resource.HasParameters; }
        }

        public override HttpWebRequest BuildWebRequest(ITransmissionSettings transmissionSettings)
        {
            var baseRequest = base.BuildWebRequest(transmissionSettings);
            baseRequest.Method = GetHttpMethod();

            if (HasSerializableBody)
            {
                var serialized = GetSerializedBody(transmissionSettings);
                var bytes = Encoding.Default.GetBytes(serialized);

                baseRequest.ContentLength = bytes.Length;

                using (var stream = baseRequest.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            return baseRequest;
        }

        private string GetSerializedBody(ITransmissionSettings transmissionSettings)
        {
            if (Body != null)
            {
                return transmissionSettings.Serialize(Body);
            }

            if (Resource.HasParameters)
            {
                return Resource.GetEncodedParameters();
            }

            throw new NotSupportedException(
                "Something has gone wrong... trying to get serialized body of a request when the request has nothing worth serializing");
        }

        protected abstract string GetHttpMethod();

        protected override string BuildRequestUrl(Resource resource)
        {
            return resource.Path;
        }

        protected override string CalculateContentType(ITransmissionSettings transmissionSettings)
        {
            return Resource.HasParameters
                ? "application/x-www-form-urlencoded"
                : transmissionSettings.DefaultSerializerContentType;
        }
    }
}