using System.Collections.Specialized;

namespace WorkerHttpFake.Implementation.Extensions
{
    internal static class WorkerHttpFakeExtensions
    {
        public static IReadOnlyDictionary<string, object> ToReadOnlyDictionary(this NameValueCollection nameValueCollection)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (string key in nameValueCollection.AllKeys)
            {
                if (key != null)
                {
                    dictionary.Add(key, nameValueCollection[key]);
                }
            }

            return new Dictionary<string, object>(dictionary).AsReadOnly();
        }
    }
}
