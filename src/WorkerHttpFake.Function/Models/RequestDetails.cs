namespace WorkerHttpFake.Function.Models
{
    public class RequestDetails
    {
        public Dictionary<string, string> Headers { get; set; }
        public Dictionary<string, string> QueryParams { get; set; }
        public Dictionary<string, string> ContextData { get; set; }
        public string Body { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }
        public Dictionary<string, string> Cookies { get; set; }
        public Dictionary<string, string> Claims { get; set; }
        public string Method { get; set; }
    }
}
