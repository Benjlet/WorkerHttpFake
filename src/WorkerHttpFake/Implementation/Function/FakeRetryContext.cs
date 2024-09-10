using Microsoft.Azure.Functions.Worker;

namespace WorkerHttpFake.Implementation.Function
{
    internal class FakeRetryContext : RetryContext
    {
        public override int RetryCount => 0;
        public override int MaxRetryCount => 3;
    }
}