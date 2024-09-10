using Microsoft.Azure.Functions.Worker;
using System.Collections;

namespace WorkerHttpFake.Implementation.Invocation
{
    internal class FakeInvocationFeatures : IInvocationFeatures
    {
        private readonly Dictionary<Type, object> _features = [];

        public FakeInvocationFeatures()
        {
            Set(new FakeInvocationFeature()
            {
                InvocationId = Guid.NewGuid().ToString()
            });
        }

        public void Set<T>(T instance)
        {
            _features[typeof(T)] = instance!;
        }

        public T Get<T>()
        {
            if (_features.TryGetValue(typeof(T), out var value))
            {
                return (T)value;
            }

            return default;
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            return _features.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
