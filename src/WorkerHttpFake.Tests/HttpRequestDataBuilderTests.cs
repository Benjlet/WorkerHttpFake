using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Security.Claims;
using WorkerHttpFake;

namespace FakeHttpTests.Tests
{
    public class HttpRequestDataBuilderTests
    {
        private HttpRequestDataBuilder _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new HttpRequestDataBuilder();
        }

        [Test]
        public async Task WithBody_Json_AddsBody()
        {
            string jsonBody = JsonConvert.SerializeObject(new
            {
                id = 1,
                example = "two"
            });

            HttpRequestData request = _sut
                .WithBody(jsonBody)
                .Build();

            string requestBody = await request.ReadAsStringAsync();

            Assert.That(requestBody, Is.EqualTo(jsonBody));
        }

        [Test]
        public async Task WithBody_Empty_AddsBody()
        {
            string body = "";

            HttpRequestData request = _sut
                .WithBody(string.Empty)
                .Build();

            string requestBody = await request.ReadAsStringAsync();

            Assert.That(requestBody, Is.EqualTo(body));
        }

        [Test]
        public async Task WithBody_Null_ReadAsEmptyBody()
        {
            HttpRequestData request = _sut
                .WithBody(null)
                .Build();

            string requestBody = await request.ReadAsStringAsync();

            Assert.That(requestBody, Is.Empty);
        }

        [Test]
        public void WithBasicAuthorization_AuthData_AddsAuthorizationHeader()
        {
            string basicAuthData = "ZGVtbzpwQDU1dzByZA==";

            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithBasicAuthorization(basicAuthData)
                .Build();

            string authorizationHeader = requestData.Headers.GetValues("Authorization").First();

            Assert.That(authorizationHeader, Is.EqualTo($"Basic {basicAuthData}"));
        }

        [Test]
        public void WithBasicAuthorization_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpRequestDataBuilder().WithBasicAuthorization(null));
        }

        [Test]
        public void WithBearerAuthorization_BearerToken_AddsAuthorizationHeader()
        {
            string bearerToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjUxMTg4ODU5MSJ9.eyJpc3MiOiJodHRwczovL2V4YW1wbGUuY29tIiwiYXVkIjoiYXBpLnRlc3QuY29tIiwic3ViIjoiMTIzNDU2Nzg5MCIsImV4cCI6MTYxNDY0MDEyMn0.C29vklAs_mhNzZgT1MQI8RO1IzlknnIWWJupLoXObZ";

            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithBearerAuthorization(bearerToken)
                .Build();

            string authorizationHeader = requestData.Headers.GetValues("Authorization").First();

            Assert.That(authorizationHeader, Is.EqualTo($"Bearer {bearerToken}"));
        }

        [Test]
        public void WithBearerAuthorization_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpRequestDataBuilder().WithBearerAuthorization(null));
        }

        [Test]
        public void WithDigestAuthorization_DigestData_AddsAuthorizationHeader()
        {
            string digestData = "username=\"user\", realm=\"example.com\", nonce=\"dcd98b7102dd2f0e8b11d0f600bfb0c093\", uri=\"/api/resource\", response=\"6629fae49393a05397450978507c4ef1\", opaque=\"5ccc069c403ebaf9f0171e9517f40e41\", qop=auth, nc=00000001, cnonce=\"0a4f113b\"";

            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithDigestAuthorization(digestData)
                .Build();

            string authorizationHeader = requestData.Headers.GetValues("Authorization").First();

            Assert.That(authorizationHeader, Is.EqualTo($"Digest {digestData}"));
        }

        [Test]
        public void WithDigestAuthorization_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpRequestDataBuilder().WithDigestAuthorization(null));
        }

        [Test]
        public void WithUrl_Valid_AddsUrl()
        {
            string url = "https://localhost:8080/api/endpoint";

            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithUrl(url)
                .Build();

            string requestUrl = requestData.Url.ToString();

            Assert.That(url, Is.EqualTo(requestUrl));
        }

        [Test]
        public void WithUrl_HasQueryParams_AddsUrlAndParams()
        {
            string url = "https://localhost:8080/api/endpoint?q=123&r=456&s=789";

            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithUrl(url)
                .Build();

            string requestUrl = requestData.Url.ToString();

            Assert.That(requestData.Query, Has.Count.EqualTo(3));

            Assert.Multiple(() =>
            {
                Assert.That(requestData.Query["q"], Is.EqualTo("123"));
                Assert.That(requestData.Query["r"], Is.EqualTo("456"));
                Assert.That(requestData.Query["s"], Is.EqualTo("789"));
                Assert.That(url, Is.EqualTo(requestUrl));
            });
        }

        [Test]
        public void WithUrl_BadFormat_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new HttpRequestDataBuilder().WithUrl("localhost.com"));
        }

        [Test]
        public void WithUrl_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpRequestDataBuilder().WithUrl(null));
        }

        [Test]
        public void WithMethod_HttpMethod_AddsMethod()
        {
            HttpMethod method = HttpMethod.Post;

            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithMethod(method)
                .Build();

            Assert.That(requestData.Method, Is.EqualTo(method.Method));
        }

        [Test]
        public void WithMethod_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpRequestDataBuilder().WithMethod(null));
        }

        [Test]
        public void WithBindingContextData_ContextData_AddsContextData()
        {
            NameValueCollection contextData = new()
            {
                { "Custom-Header-1", "one" },
                { "Custom-Header-2", "two" }
            };

            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithBindingContextData(contextData)
                .Build();

            foreach (string key in requestData.FunctionContext.BindingContext.BindingData.Keys)
            {
                Assert.That(contextData[key], Is.EqualTo(requestData.FunctionContext.BindingContext.BindingData[key]));
            }
        }

        [Test]
        public void WithBindingContextData_Default_HasEmptyContextData()
        {
            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithBindingContextData([])
                .Build();

            Assert.That(requestData.FunctionContext.BindingContext.BindingData, Is.Empty);
        }

        [Test]
        public void WithBindingContextData_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpRequestDataBuilder().WithBindingContextData(null));
        }

        [Test]
        public void WithHeaders_Headers_AddsHeaders()
        {
            NameValueCollection headers = new()
            {
                { "Custom-Header-1", "one" },
                { "Custom-Header-2", "two" }
            };

            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithHeaders(headers)
                .Build();

            foreach (string key in headers.AllKeys)
            {
                Assert.That(headers[key], Is.EqualTo(requestData.Headers.GetValues(key).First()));
            }
        }

        [Test]
        public void WithHeaders_Default_HasEmptyHeaders()
        {
            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithHeaders([])
                .Build();

            Assert.That(requestData.Headers, Is.Empty);
        }

        [Test]
        public void WithHeaders_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpRequestDataBuilder().WithHeaders(null));
        }

        [Test]
        public void WithQueryParams_Query_AddsQueryParams()
        {
            NameValueCollection queryParams = new()
            {
                { "q", "123" },
                { "r", "456" },
                { "s", "789" }
            };

            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithQueryParams(queryParams)
                .Build();

            foreach (string key in requestData.Query.AllKeys)
            {
                Assert.That(queryParams[key], Is.EqualTo(requestData.Query[key]));
            }
        }

        [Test]
        public void WithQueryParams_Default_HasEmptyQuery()
        {
            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithQueryParams([])
                .Build();

            Assert.That(requestData.Query, Is.Empty);
        }

        [Test]
        public void WithQueryParams_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpRequestDataBuilder().WithQueryParams(null));
        }

        [Test]
        public void WithCookies_Cookies_AddsCookies()
        {
            List<IHttpCookie> cookies = [
                new HttpCookie("CookieName", "CookieValue")
            ];

            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithCookies(cookies)
                .Build();

            Assert.That(requestData.Cookies.First(), Is.EqualTo(cookies.First()));
        }

        [Test]
        public void WithCookies_Default_HasEmptyCookies()
        {
            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithCookies([])
                .Build();

            Assert.That(requestData.Cookies, Is.Empty);
        }

        [Test]
        public void WithCookies_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpRequestDataBuilder().WithCookies(null));
        }

        [Test]
        public void WithIdentities_IdentityClaims_AddsIdentities()
        {
            List<ClaimsIdentity> identities = [
                new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, "ExampleName"),
                    new Claim(ClaimTypes.Email, "ExampleEmail")
                ])
            ];

            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithIdentities(identities)
                .Build();

            Assert.That(requestData.Identities.First(), Is.EqualTo(identities.First()));
        }

        [Test]
        public void WithIdentities_Default_HasEmptyIdentities()
        {
            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithIdentities([])
                .Build();

            Assert.That(requestData.Identities, Is.Empty);
        }

        [Test]
        public void WithIdentities_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpRequestDataBuilder().WithIdentities(null));
        }

        [Test]
        public void WithCustomContext_FunctionContext_UsesFunctionContext()
        {
            Dictionary<string, object> bindingData = new()
            {
                { "MockBindingData", "123" }
            };

            Mock<FunctionContext> mockContext = new();

            IHost hostBuilder = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .Build();

            mockContext.SetupGet(x => x.BindingContext.BindingData).Returns(bindingData);

            IServiceProvider services = hostBuilder.Services;

            mockContext.SetupGet(x => x.InstanceServices).Returns(hostBuilder.Services);

            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithCustomContext(mockContext.Object)
                .Build();

            foreach (string key in requestData.FunctionContext.BindingContext.BindingData.Keys)
            {
                Assert.That(bindingData[key], Is.EqualTo(requestData.FunctionContext.BindingContext.BindingData[key]));
            }
        }
    }
}