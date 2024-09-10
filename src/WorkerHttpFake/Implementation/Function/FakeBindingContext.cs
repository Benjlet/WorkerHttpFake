using Microsoft.Azure.Functions.Worker;
using System.Collections.Specialized;
using WorkerHttpFake.Implementation.Extensions;

namespace WorkerHttpFake.Implementation.Function
{
    internal class FakeBindingContext : BindingContext
    {
        private readonly IReadOnlyDictionary<string, object> _bindingData;

        public FakeBindingContext(NameValueCollection bindingData)
        {
            _bindingData = bindingData.ToReadOnlyDictionary();
        }

        public override IReadOnlyDictionary<string, object> BindingData => _bindingData;
    }
}