using System.Collections.Immutable;
using Microsoft.Azure.Functions.Worker;

namespace WorkerHttpFake.Implementation.Function
{
    internal class FakeFunctionDefinition : FunctionDefinition
    {
        private static readonly string s_defaultPathToAssembly = typeof(FakeFunctionDefinition).Assembly.Location;
        private static readonly string s_defaultEntryPoint = $"{nameof(FakeFunctionDefinition)}.Run";
        private static readonly string s_defaultName = nameof(FakeFunctionDefinition);
        private static readonly string s_defaultId = "1835d7b55c984790815d072cc94c6f71";

        private readonly ImmutableDictionary<string, BindingMetadata> _outputBindings;
        private readonly ImmutableDictionary<string, BindingMetadata> _inputBindings;
        private readonly ImmutableArray<FunctionParameter> _parameters;

        public override string PathToAssembly => s_defaultPathToAssembly;
        public override string EntryPoint => s_defaultEntryPoint;
        public override string Name => s_defaultName;
        public override string Id => s_defaultId;

        public override IImmutableDictionary<string, BindingMetadata> OutputBindings => _outputBindings;
        public override IImmutableDictionary<string, BindingMetadata> InputBindings => _inputBindings;
        public override ImmutableArray<FunctionParameter> Parameters => _parameters;

        public FakeFunctionDefinition()
        {
            Dictionary<string, BindingMetadata> inputs = new()
            {
                { $"triggerName", new FakeBindingMetadata("TestTrigger", BindingDirection.In) },
                { $"inputName", new FakeBindingMetadata("TestInput", BindingDirection.In) }
            };

            Dictionary<string, BindingMetadata> outputs = new()
            {
                { $"outputName1", new FakeBindingMetadata($"TestOutput1", BindingDirection.Out) }
            };

            Dictionary<string, object> properties = new()
            {
                { "TestPropertyKey", "TestPropertyValue" }
            };

            List<FunctionParameter> parameters = [
                new FunctionParameter($"Parameter1", typeof(string), properties.ToImmutableDictionary())
            ];

            _inputBindings = inputs.ToImmutableDictionary();
            _outputBindings = outputs.ToImmutableDictionary();
            _parameters = [.. parameters];
        }
    }
}