using System.Collections.Specialized;
using System.Net;
using System.Security.Claims;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace WorkerHttpFake.Implementation
{
    internal class FakeHttpRequestData : HttpRequestData
    {
        private readonly IReadOnlyCollection<IHttpCookie> _cookies;
        private readonly IEnumerable<ClaimsIdentity> _identities;
        private readonly HttpHeadersCollection _headers;
        private readonly string _method;
        private readonly Stream _body;
        private readonly Uri _url;

        public FakeHttpRequestData(
            FunctionContext functionContext,
            Uri url,
            HttpHeadersCollection headers,
            Stream body,
            IEnumerable<ClaimsIdentity> claimsIdentities,
            IReadOnlyCollection<IHttpCookie> cookies,
            HttpMethod method) : base(functionContext)
        {
            _url = url;
            _body = body;
            _cookies = cookies;
            _headers = headers;
            _method = method.Method;
            _identities = claimsIdentities;
        }

        public override Uri Url => _url;
        public override Stream Body => _body;
        public override string Method => _method;
        public override HttpHeadersCollection Headers => _headers;
        public override IReadOnlyCollection<IHttpCookie> Cookies => _cookies;
        public override IEnumerable<ClaimsIdentity> Identities => _identities;
        public override NameValueCollection Query => HttpUtility.ParseQueryString(_url.Query);

        public override HttpResponseData CreateResponse()
        {
            return new FakeHttpResponseData(FunctionContext, HttpStatusCode.OK);
        }
    }
}