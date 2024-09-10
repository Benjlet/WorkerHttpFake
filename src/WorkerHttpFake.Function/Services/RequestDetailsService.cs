using Microsoft.Azure.Functions.Worker.Http;
using WorkerHttpFake.Function.Models;

namespace WorkerHttpFake.Function.Services
{
    internal class RequestDetailsService : IRequestDetailsService
    {
        public async Task<RequestDetails> ExtractRequestDetails(HttpRequestData request)
        {
            string body = await request.ReadAsStringAsync();

            Dictionary<string, string> cookies = request.Cookies.ToDictionary(c => c.Name, c => c.Value);
            Dictionary<string, string> queryParams = request.Query.AllKeys.ToDictionary(k => k, k => request.Query[k]);
            Dictionary<string, string> contextData = request.FunctionContext.BindingContext.BindingData.ToDictionary(a => a.Key, a => string.Join(";", a.Value));
            Dictionary<string, string> headers = request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value));

            Dictionary<string, string> claims = request.Identities
                .SelectMany(identity => identity.Claims)
                .GroupBy(claim => claim.Type)
                .ToDictionary(c => c.Key, c => string.Join(", ", c.Select(claim => claim.Value)));

            string contentType = request.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypes) ? contentTypes.First() : string.Empty;

            return new RequestDetails()
            {
                Cookies = cookies,
                Method = request.Method,
                Url = request.Url.ToString(),
                QueryParams = queryParams,
                ContextData = contextData,
                ContentType = contentType,
                Headers = headers,
                Claims = claims,
                Body = body
            };
        }
    }
}
