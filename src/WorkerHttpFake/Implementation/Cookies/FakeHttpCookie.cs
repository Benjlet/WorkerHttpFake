using Microsoft.Azure.Functions.Worker.Http;

namespace WorkerHttpFake.Implementation.Cookies
{
    internal class FakeHttpCookie : IHttpCookie
    {
        private readonly string _cookieName;
        private readonly string _cookieValue;

        public FakeHttpCookie(string name, string value)
        {
            _cookieName = name;
            _cookieValue = value;
        }

        public string Name => _cookieName;
        public string Value => _cookieValue;

        public string Domain { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public bool? HttpOnly { get; set; }
        public double? MaxAge { get; set; }
        public string Path { get; set; }
        public SameSite SameSite { get; set; } = SameSite.Lax;
        public bool? Secure { get; set; }
    }
}