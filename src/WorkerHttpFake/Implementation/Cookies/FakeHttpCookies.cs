using Microsoft.Azure.Functions.Worker.Http;

namespace WorkerHttpFake.Implementation.Cookies
{
    internal class FakeHttpCookies : HttpCookies
    {
        private readonly Dictionary<string, string> _cookies = [];

        public override void Append(string name, string value)
        {
            _cookies.Add(name, value);
        }

        public override void Append(IHttpCookie cookie)
        {
            if (cookie != null)
            {
                _cookies.Add(cookie.Name, cookie.Value);
            }
        }

        public override IHttpCookie CreateNew()
        {
            return new FakeHttpCookie("FakeCookie", "FakeCookieValue");
        }

        public IReadOnlyDictionary<string, string> Cookies => _cookies;
    }
}