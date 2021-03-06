using System;

namespace SpeakEasy
{
    public class AfterRequestEventArgs : EventArgs
    {
        public AfterRequestEventArgs(IHttpRequest request, IHttpResponse response)
        {
            Request = request;
            Response = response;
        }

        public IHttpRequest Request { get; private set; }

        public IHttpResponse Response { get; private set; }
    }
}