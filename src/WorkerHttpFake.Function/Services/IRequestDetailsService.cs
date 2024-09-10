using Microsoft.Azure.Functions.Worker.Http;
using WorkerHttpFake.Function.Models;

namespace WorkerHttpFake.Function.Services
{
    public interface IRequestDetailsService
    {
        Task<RequestDetails> ExtractRequestDetails(HttpRequestData request);
    }
}