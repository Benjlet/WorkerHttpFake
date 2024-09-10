using System.Collections.Specialized;
using System.Security.Claims;
using System.Text;
using System.Web;
using Azure.Core.Serialization;
using WorkerHttpFake.Implementation.Cookies;
using WorkerHttpFake.Implementation.Function;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using WorkerHttpFake.Implementation;

namespace WorkerHttpFake
{
    /// <summary>
    /// Initialises a new builder for creating a custom <see cref="HttpRequestData"/> object.
    /// Call the <see cref="Build"/> method to build the final request.
    /// </summary>
    public class HttpRequestDataBuilder
    {
        private readonly Encoding _encoding;
        private readonly ObjectSerializer _serializer;

        private Uri _url;
        private HttpMethod _method;
        private MemoryStream _bodyStream;
        private FunctionContext _context;
        private NameValueCollection _headers;
        private IEnumerable<ClaimsIdentity> _identities;
        private IReadOnlyCollection<IHttpCookie> _cookies;

        /// <summary>
        /// Initialises a new HTTP request builder with options for chaining custom data such as URL, query params, headers, and so forth.
        /// Call <see cref="Build"/> to build the final <see cref="HttpRequestData"/> request.
        /// </summary>
        /// <param name="encoding">Encoding for parsing HTTP query params, content body, and so forth, defaulting to UTF-8.</param>
        /// <param name="objectSerializer">Serializer to use for JSON serialization, defaulting to <see cref="JsonObjectSerializer"/>.</param>
        public HttpRequestDataBuilder(
            Encoding encoding = null,
            ObjectSerializer objectSerializer = null)
        {
            _method = HttpMethod.Get;
            _encoding = encoding ?? Encoding.UTF8;
            _serializer = objectSerializer ?? new JsonObjectSerializer();
            _url = new Uri("http://localhost/");

            _headers = [];
            _identities = [];
            _bodyStream = new MemoryStream();
            _context = new FakeFunctionContext([], _serializer);
            _cookies = new List<FakeHttpCookie>().AsReadOnly();
        }

        /// <summary>
        /// Adds the supplied URL to the builder.
        /// </summary>
        /// <param name="url">Valid (absolute) <see cref="Uri"/> value.</param>
        /// <returns><see cref="HttpRequestDataBuilder"/></returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public HttpRequestDataBuilder WithUrl(string url)
        {
            ArgumentNullException.ThrowIfNull(url);

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                throw new ArgumentException("Url must be in a valid (absolute) format.", nameof(url));
            }

            _url = uri;
            return this;
        }

        /// <summary>
        /// Adds the supplied query params to the builder.
        /// The Url field is overwritten to include the parsed query params.
        /// </summary>
        /// <param name="query">Query params in name/value format.</param>
        /// <returns><see cref="HttpRequestDataBuilder"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public HttpRequestDataBuilder WithQueryParams(NameValueCollection query)
        {
            ArgumentNullException.ThrowIfNull(query);

            HashSet<string> queryParams = [];

            for (int i = 0; i < query?.AllKeys?.Length; i++)
            {
                string key = query.AllKeys[i];

                if (key == null)
                {
                    continue;
                }

                string encodedKey = HttpUtility.UrlEncode(key, _encoding);
                string encodedValue = HttpUtility.UrlEncode(query[key], _encoding);
                queryParams.Add($"{encodedKey}={encodedValue}");
            }

            string urlWithQuery = $"{_url.GetLeftPart(UriPartial.Path)}?{string.Join("&", queryParams)}";
            _url = new Uri(urlWithQuery);

            return this;
        }

        /// <summary>
        /// Adds the supplied method to the builder.
        /// </summary>
        /// <param name="method">Http method type</param>
        /// <returns><see cref="HttpRequestDataBuilder"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public HttpRequestDataBuilder WithMethod(HttpMethod method)
        {
            ArgumentNullException.ThrowIfNull(method);

            _method = method;
            return this;
        }

        /// <summary>
        /// Adds the supplied body to the builder.
        /// You may want to set a custom header such as 'application/json' if your application validates this value.
        /// </summary>
        /// <param name="bodyContent">String body content.</param>
        /// <returns><see cref="HttpRequestDataBuilder"/></returns>
        public HttpRequestDataBuilder WithBody(string bodyContent)
        {
            _bodyStream = new MemoryStream(_encoding.GetBytes(bodyContent ?? string.Empty));
            return this;
        }

        /// <summary>
        /// Adds the supplied header values to the builder.
        /// </summary>
        /// <param name="headers">Http headers.</param>
        /// <returns><see cref="HttpRequestDataBuilder"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public HttpRequestDataBuilder WithHeaders(NameValueCollection headers)
        {
            ArgumentNullException.ThrowIfNull(headers);

            _headers = headers;
            return this;
        }

        /// <summary>
        /// Adds the supplied binding data values to the builder, accessible from the function context.
        /// </summary>
        /// <param name="bindingData">Binding data within the faked function context.</param>
        /// <returns><see cref="HttpRequestDataBuilder"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public HttpRequestDataBuilder WithBindingContextData(NameValueCollection bindingData)
        {
            ArgumentNullException.ThrowIfNull(bindingData);

            _context = new FakeFunctionContext(bindingData, _serializer);
            return this;
        }

        /// <summary>
        /// Adds the supplied function context to the builder.
        /// </summary>
        /// <param name="context">Function context data.</param>
        /// <returns><see cref="HttpRequestDataBuilder"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public HttpRequestDataBuilder WithCustomContext(FunctionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _context = context;
            return this;
        }

        /// <summary>
        /// Adds an Authorization header with basic auth data.
        /// </summary>
        /// <param name="authData">The encoded data following the 'Basic' Authorization header value.</param>
        /// <returns><see cref="HttpRequestDataBuilder"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public HttpRequestDataBuilder WithBasicAuthorization(string authData)
        {
            ArgumentNullException.ThrowIfNull(authData);

            _headers["Authorization"] = $"Basic {authData}";
            return this;
        }

        /// <summary>
        /// Adds an Authorization header with bearer token data.
        /// </summary>
        /// <param name="bearerToken">The bearer token following the 'Bearer' Authorization header value.</param>
        /// <returns><see cref="HttpRequestDataBuilder"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public HttpRequestDataBuilder WithBearerAuthorization(string bearerToken)
        {
            ArgumentNullException.ThrowIfNull(bearerToken);

            _headers["Authorization"] = $"Bearer {bearerToken}";
            return this;
        }

        /// <summary>
        /// Adds an Authorization header with digest data.
        /// </summary>
        /// <param name="digestData">The digest data following the 'Digest' Authorization header value.</param>
        /// <returns><see cref="HttpRequestDataBuilder"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public HttpRequestDataBuilder WithDigestAuthorization(string digestData)
        {
            ArgumentNullException.ThrowIfNull(digestData);

            _headers["Authorization"] = $"Digest {digestData}";
            return this;
        }

        /// <summary>
        /// Adds the supplied http cookies to the builder.
        /// </summary>
        /// <param name="cookies">Http cookies.</param>
        /// <returns><see cref="HttpRequestDataBuilder"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public HttpRequestDataBuilder WithCookies(IReadOnlyCollection<IHttpCookie> cookies)
        {
            ArgumentNullException.ThrowIfNull(cookies);

            _cookies = cookies;
            return this;
        }

        /// <summary>
        /// Adds the supplied collection of identity claims to the builder.
        /// </summary>
        /// <param name="identities">Collection of identity claims.</param>
        /// <returns><see cref="HttpRequestDataBuilder"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public HttpRequestDataBuilder WithIdentities(IEnumerable<ClaimsIdentity> identities)
        {
            ArgumentNullException.ThrowIfNull(identities);

            _identities = identities;
            return this;
        }

        /// <summary>
        /// Builds the final <see cref="HttpRequestData"/> object based on the chained methods called and/or default faked values.
        /// </summary>
        /// <returns><see cref="HttpRequestData"/></returns>
        public HttpRequestData Build()
        {
            HttpHeadersCollection headers = [];

            foreach (string key in _headers.AllKeys)
            {
                if (key != null)
                {
                    headers.TryAddWithoutValidation(key, _headers[key]);
                }
            }

            return new FakeHttpRequestData(
                _context,
                _url,
                headers,
                _bodyStream,
                _identities,
                _cookies,
                _method);
        }
    }
}