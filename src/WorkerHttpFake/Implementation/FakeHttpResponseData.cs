using System.Net;
using WorkerHttpFake.Implementation.Cookies;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace WorkerHttpFake.Implementation
{
    internal class FakeHttpResponseData : HttpResponseData
    {
        private readonly HttpCookies _cookies;

        public FakeHttpResponseData(FunctionContext functionContext, HttpStatusCode status) : base(functionContext)
        {
            StatusCode = status;
            _cookies = new FakeHttpCookies();
        }

        public override HttpStatusCode StatusCode { get; set; }
        public override HttpHeadersCollection Headers { get; set; } = [];
        public override Stream Body { get; set; } = new MemoryStream();
        public override HttpCookies Cookies => _cookies;
    }
}