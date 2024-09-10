using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Security.Claims;
using WorkerHttpFake.Function.HttpTrigger;
using WorkerHttpFake.Function.Models;
using WorkerHttpFake.Function.Services;

namespace WorkerHttpFake.Tests
{
    internal class ExampleFunctionTests
    {
        private ExampleFunction _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new ExampleFunction(
                Mock.Of<ILogger<ExampleFunction>>(),
                new RequestDetailsService());
        }

        [Test]
        public async Task ExampleFunction_WithBody_WasRead()
        {
            string jsonBody = JsonConvert.SerializeObject(new
            {
                id = 1,
                value = "test"
            });

            HttpRequestData request = new HttpRequestDataBuilder()
                .WithBody(jsonBody)
                .Build();

            HttpResponseData response = await _sut.Run(request);
            RequestDetails requestDetails = await GetRequestDetails(response);

            Assert.That(requestDetails.Body, Is.EqualTo(jsonBody));

            PrintOutput(requestDetails);
        }

        [Test]
        public async Task ExampleFunction_WithUrl_WasRead()
        {
            string testUrl = "https://localhost:8080/";

            HttpRequestData request = new HttpRequestDataBuilder()
                .WithUrl(testUrl)
                .Build();

            HttpResponseData response = await _sut.Run(request);
            RequestDetails requestDetails = await GetRequestDetails(response);

            Assert.That(requestDetails.Url, Is.EqualTo(testUrl));

            PrintOutput(requestDetails);
        }

        [Test]
        public async Task ExampleFunction_WithUrlQuery_WasRead()
        {
            string urlWithQuery = $"https://localhost:8080/?q=123&r=456&s=789";

            HttpRequestData request = new HttpRequestDataBuilder()
                .WithUrl(urlWithQuery)
                .Build();

            HttpResponseData response = await _sut.Run(request);
            RequestDetails requestDetails = await GetRequestDetails(response);

            Assert.Multiple(() =>
            {
                Assert.That(requestDetails.Url, Is.EqualTo(urlWithQuery));
                Assert.That(requestDetails.QueryParams["q"], Is.EqualTo("123"));
                Assert.That(requestDetails.QueryParams["r"], Is.EqualTo("456"));
                Assert.That(requestDetails.QueryParams["s"], Is.EqualTo("789"));
            });

            PrintOutput(requestDetails);
        }

        [Test]
        public async Task ExampleFunction_WithBasicAuthorization_WasRead()
        {
            string authData = "dXNlcjpwYXNzd29yZA==";

            HttpRequestData request = new HttpRequestDataBuilder()
                .WithBasicAuthorization(authData)
                .Build();

            HttpResponseData response = await _sut.Run(request);
            RequestDetails requestDetails = await GetRequestDetails(response);

            Assert.That(requestDetails.Headers["Authorization"], Is.EqualTo($"Basic {authData}"));

            PrintOutput(requestDetails);
        }

        [Test]
        public async Task ExampleFunction_WithBearerAuthorization_WasRead()
        {
            string bearerToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjUxMTg4ODU5MSJ9.eyJpc3MiOiJodHRwczovL2V4YW1wbGUuY29tIiwiYXVkIjoiYXBpLnRlc3QuY29tIiwic3ViIjoiMTIzNDU2Nzg5MCIsImV4cCI6MTYxNDY0MDEyMn0.C29vklAs_mhNzZgT1MQI8RO1IzlknnIWWJupLoXObZ";

            HttpRequestData request = new HttpRequestDataBuilder()
                .WithBearerAuthorization(bearerToken)
                .Build();

            HttpResponseData response = await _sut.Run(request);
            RequestDetails requestDetails = await GetRequestDetails(response);

            Assert.That(requestDetails.Headers["Authorization"], Is.EqualTo($"Bearer {bearerToken}"));

            PrintOutput(requestDetails);
        }

        [Test]
        public async Task ExampleFunction_WithDigestAuthorization_WasRead()
        {
            string digestData = "username=\"user\", realm=\"example.com\", nonce=\"dcd98b7102dd2f0e8b11d0f600bfb0c093\", uri=\"/api/resource\", response=\"6629fae49393a05397450978507c4ef1\", opaque=\"5ccc069c403ebaf9f0171e9517f40e41\", qop=auth, nc=00000001, cnonce=\"0a4f113b\"";

            HttpRequestData request = new HttpRequestDataBuilder()
                .WithDigestAuthorization(digestData)
                .Build();

            HttpResponseData response = await _sut.Run(request);
            RequestDetails requestDetails = await GetRequestDetails(response);

            Assert.That(requestDetails.Headers["Authorization"], Is.EqualTo($"Digest {digestData}"));

            PrintOutput(requestDetails);
        }

        [Test]
        public async Task ExampleFunction_WithCookies_WasRead()
        {
            List<IHttpCookie> cookies =
            [
                new HttpCookie("CookieName", "CookieValue")
            ];

            HttpRequestData request = new HttpRequestDataBuilder()
                .WithCookies(cookies)
                .Build();

            HttpResponseData response = await _sut.Run(request);
            RequestDetails requestDetails = await GetRequestDetails(response);

            Assert.That(requestDetails.Cookies["CookieName"], Is.EqualTo(cookies[0].Value));

            PrintOutput(requestDetails);
        }

        [Test]
        public async Task ExampleFunction_WithIdentities_WasRead()
        {
            List<ClaimsIdentity> identities =
            [
                new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, "ExampleName"),
                    new Claim(ClaimTypes.Email, "ExampleEmail")
                ])
            ];

            HttpRequestData request = new HttpRequestDataBuilder()
                .WithIdentities(identities)
                .Build();

            HttpResponseData response = await _sut.Run(request);
            RequestDetails requestDetails = await GetRequestDetails(response);

            Assert.That(requestDetails.Claims[ClaimTypes.Name], Is.EqualTo(identities[0].Claims.ToList()[0].Value));
            Assert.That(requestDetails.Claims[ClaimTypes.Email], Is.EqualTo(identities[0].Claims.ToList()[1].Value));

            PrintOutput(requestDetails);
        }

        [Test]
        public async Task ExampleFunction_WithCustomContext_WasRead()
        {
            Dictionary<string, object> bindingData = new()
            {
                { "MockBindingData", "123" }
            };

            Mock<FunctionContext> functionContext = new();

            IHost workersHost = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .Build();

            functionContext.Setup(x => x.InstanceServices).Returns(workersHost.Services);

            functionContext.SetupGet(x => x.BindingContext.BindingData).Returns(bindingData);

            HttpRequestData request = new HttpRequestDataBuilder()
                .WithCustomContext(functionContext.Object)
                .Build();

            HttpResponseData response = await _sut.Run(request);
            RequestDetails requestDetails = await GetRequestDetails(response);

            foreach (string key in requestDetails.ContextData.Keys)
            {
                Assert.That(bindingData[key], Is.EqualTo(requestDetails.ContextData[key]));
            }

            PrintOutput(requestDetails);
        }

        [Test]
        public async Task ExampleFunction_WithQueryParams_WasRead()
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

            HttpResponseData response = await _sut.Run(requestData);
            RequestDetails requestDetails = await GetRequestDetails(response);

            foreach (string key in requestData.Query.AllKeys)
            {
                Assert.That(queryParams[key], Is.EqualTo(requestData.Query[key]));
            }

            PrintOutput(requestDetails);
        }

        [Test]
        public async Task ExampleFunction_WithHeaders_WasRead()
        {
            NameValueCollection headers = new()
            {
                { "q", "123" },
                { "r", "456" },
                { "s", "789" }
            };

            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithHeaders(headers)
                .Build();

            HttpResponseData response = await _sut.Run(requestData);
            RequestDetails requestDetails = await GetRequestDetails(response);

            foreach (KeyValuePair<string, IEnumerable<string>> header in requestData.Headers)
            {
                Assert.That(header.Value.First(), Is.EqualTo(headers[header.Key]));
            }

            PrintOutput(requestDetails);
        }

        [Test]
        public async Task ExampleFunction_WithMultipleChained_WasRead()
        {
            string authData = "dXNlcjpwYXNzd29yZA==";

            NameValueCollection headers = new()
            {
                { "q", "123" },
                { "r", "456" },
                { "s", "789" }
            };

            NameValueCollection queryParams = new()
            {
                { "q", "123" },
                { "r", "456" },
                { "s", "789" }
            };

            List<ClaimsIdentity> identities =
            [
                new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, "ExampleName"),
                    new Claim(ClaimTypes.Email, "ExampleEmail")
                ])
            ];

            HttpRequestData requestData = new HttpRequestDataBuilder()
                .WithHeaders(headers)
                .WithQueryParams(queryParams)
                .WithIdentities(identities)
                .WithBasicAuthorization(authData)
                .Build();

            HttpResponseData response = await _sut.Run(requestData);
            RequestDetails requestDetails = await GetRequestDetails(response);

            foreach (KeyValuePair<string, IEnumerable<string>> header in requestData.Headers)
            {
                Assert.That(header.Value.First(), Is.EqualTo(headers[header.Key]));
            }

            foreach (string key in requestData.Query.AllKeys)
            {
                Assert.That(queryParams[key], Is.EqualTo(requestData.Query[key]));
            }

            Assert.That(requestDetails.Headers["Authorization"], Is.EqualTo($"Basic {authData}"));

            PrintOutput(requestDetails);
        }

        private static async Task<RequestDetails> GetRequestDetails(HttpResponseData response)
        {
            if (response?.Body == null)
            {
                return default;
            }

            response.Body.Position = 0;
            string responseBody = await new StreamReader(response.Body).ReadToEndAsync();

            return JsonConvert.DeserializeObject<RequestDetails>(responseBody);
        }

        private static void PrintOutput(RequestDetails request)
        {
            Console.WriteLine(JsonConvert.SerializeObject(request, Formatting.Indented));
        }
    }
}
