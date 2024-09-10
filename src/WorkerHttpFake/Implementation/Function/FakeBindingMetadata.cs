using Microsoft.Azure.Functions.Worker;

namespace WorkerHttpFake.Implementation.Function
{
    internal class FakeBindingMetadata : BindingMetadata
    {
        public FakeBindingMetadata(string type, BindingDirection direction)
        {
            Type = type;
            Direction = direction;
        }

        public override string Type { get; }
        public override BindingDirection Direction { get; }
        public override string Name => nameof(FakeBindingMetadata);
    }
}
