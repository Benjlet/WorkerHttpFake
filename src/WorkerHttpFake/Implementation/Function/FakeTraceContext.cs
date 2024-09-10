using Microsoft.Azure.Functions.Worker;

namespace WorkerHttpFake.Implementation.Function
{
    internal class FakeTraceContext : TraceContext
    {
        public override string TraceParent => Guid.NewGuid().ToString();
        public override string TraceState => Guid.NewGuid().ToString();
    }
}