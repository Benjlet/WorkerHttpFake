using Azure.Core.Serialization;
using WorkerHttpFake.Implementation.Invocation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;

namespace WorkerHttpFake.Implementation.Function
{
    internal class FakeFunctionContext : FunctionContext, IDisposable
    {
        private readonly FunctionDefinition _functionDefinition;
        private readonly FakeInvocationFeatures _features;
        private readonly BindingContext _bindingContext;

        private readonly string _invocationId;
        private readonly string _functionId;

        public FakeFunctionContext(NameValueCollection bindingData, ObjectSerializer serializer)
        {
            _features = new FakeInvocationFeatures();
            _bindingContext = new FakeBindingContext(bindingData);
            _functionDefinition = new FakeFunctionDefinition();

            _invocationId = Guid.NewGuid().ToString();
            _functionId = Guid.NewGuid().ToString();

            ServiceCollection services = new();

            services.AddOptions();
            services.AddFunctionsWorkerCore();

            services.Configure<WorkerOptions>(c =>
            {
                c.Serializer = serializer;
            });

            InstanceServices = services.BuildServiceProvider();
        }

        public bool IsDisposed { get; private set; }
        public override IServiceProvider InstanceServices { get; set; }
        public override FunctionDefinition FunctionDefinition => _functionDefinition;
        public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
        public override IInvocationFeatures Features => _features;
        public override string InvocationId => _invocationId;
        public override string FunctionId => _functionId;
        public override TraceContext TraceContext => new FakeTraceContext();
        public override BindingContext BindingContext => _bindingContext;
        public override RetryContext RetryContext => new FakeRetryContext();

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
