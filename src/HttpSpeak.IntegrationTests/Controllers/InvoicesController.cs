using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HttpSpeak.IntegrationTests.Controllers
{
    public class InvoicesController : ApiController
    {
        public HttpResponseMessage<IEnumerable<string>> Post(int id)
        {
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return new HttpResponseMessage<IEnumerable<string>>(HttpStatusCode.UnsupportedMediaType);
            }

            var streamProvider = new MultipartFileStreamProvider();

            var bodyParts = Request.Content.ReadAsMultipartAsync(streamProvider);
            bodyParts.Wait();

            var fileNames = streamProvider.BodyPartFileNames;

            return new HttpResponseMessage<IEnumerable<string>>(fileNames, HttpStatusCode.Created);
        }
    }
}