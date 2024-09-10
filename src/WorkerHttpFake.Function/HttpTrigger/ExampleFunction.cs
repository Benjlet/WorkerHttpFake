using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;
using WorkerHttpFake.Function.Models;
using WorkerHttpFake.Function.Services;

namespace WorkerHttpFake.Function.HttpTrigger
{
    public class ExampleFunction
    {
        private readonly ILogger<ExampleFunction> _logger;
        private readonly IRequestDetailsService _detailsService;

        public ExampleFunction(
            ILogger<ExampleFunction> logger,
            IRequestDetailsService detailsService)
        {
            _logger = logger;
            _detailsService = detailsService;
        }

        /// <summary>
        /// An example function that returns details of the <see cref="HttpRequestData"/> object received.
        /// </summary>
        [Function(nameof(ExampleFunction))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("HttpTrigger function {app} processed a request.", nameof(ExampleFunction));

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);

            RequestDetails requestDetails = await _detailsService.ExtractRequestDetails(req);

            await response.WriteAsJsonAsync(requestDetails);

            return response;
        }
    }
}
