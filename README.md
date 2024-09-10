# WorkerHttpFake

Fake test data builder for HttpRequestData objects.

This library allows you to create faked `HttpRequestData` objects which can be used for integration testing Azure Isolated Worker Functions, providing a builder (`HttpRequestDataBuilder`) that allows you to specify properties such as Headers, Body, Queries, Cookies, etc.

## Background

There isn't an official Azure integration test example with the new `HttpRequestData` input for Azure Isolated Worker Functions which has been mentioned in Issue items with the Azure team:

- [2263](https://github.com/Azure/azure-functions-dotnet-worker/issues/2263)
- [541](https://github.com/Azure/azure-functions-dotnet-worker/issues/541)
- [304](https://github.com/Azure/azure-functions-dotnet-worker/issues/304)
- [281](https://github.com/Azure/azure-functions-dotnet-worker/issues/281)

Without an official 'elegant' solution presented, this library offers a fluent wrapper for a faked implementation of `HttpRequestData` which covered my personal use cases.

The code has been based on comments in the above issues and inspired by the below libraries (thanks to @lohithgn and @simon-k) for creating a .NET 8 implementation:

- https://github.com/lohithgn/az-fx-isolated-unittest
- https://github.com/simon-k/fake-http-request-data/

## Examples

The below examples result in a `HttpRequestData` object being built based on the configured builder options; this can be used for integration tests or with Moq unit tests against your Azure Function.

In the `WorkerHttpFake.Test` test project there are examples of each method and how to implement this with Moq against an Azure Isolated Worker Function.

### URL and Query Params

Below is a GET request featuring a custom URL with a query param:

```
string endpoint = "https://your-api/api/stores?sortBy=Nearest"

HttpRequestData requestData = new HttpRequestDataBuilder()
	.WithUrl(endpoint)
        .Build();
```

Query params can then be extracted from the `Query` field from the function `HttpRequestData` parameter e.g. `req.Headers["sortBy"]`.

This alternatively could have been specified as following:

```
string endpoint = "https://your-api/api/stores"

NameValueCollection queryParams = new()
{
	{ "sortBy", "Nearest" }
};

HttpRequestData requestData = new HttpRequestDataBuilder()
	.WithUrl(endpoint)
	.WithQueryParams(endpoint)
        .Build();
```

Specifying `WithUrl` is not mandatory and will default to "http://localhost/" if not set, i.e. in the case of unit tests where Moq is used and URL may be irrelevant.

### Headers

You can set the `Authorization` header for basic authorization requests as follows:

```
string basicAuthData = "ZGVtbzpwQDU1dzByZA==";

HttpRequestData requestData = new HttpRequestDataBuilder()
	.WithBasicAuthorization(basicAuthData)
        .Build();
```

Or if you are using Bearer or digest, their methods (`WithBearerAuthorization` and `WithDigestAuthorization`) are called the same way with a string value.

Alternatively you can set headers yourself, including:

```
string basicAuthData = "ZGVtbzpwQDU1dzByZA==";

NameValueCollection headers = new()
{
	{ "Custom-Header-1", "one" },
        { "Custom-Header-2", "two" },
	{ "Authorization", basicAuthData }
};

HttpRequestData requestData = new HttpRequestDataBuilder()
	.WithHeaders(headers)
        .Build();
```

The `WithHeaders` method replaces all headers, so the authorization method should be chained afterwards:

```
string basicAuthData = "ZGVtbzpwQDU1dzByZA==";

NameValueCollection headers = new()
{
	{ "Custom-Header-1", "one" },
        { "Custom-Header-2", "two" }
};

HttpRequestData requestData = new HttpRequestDataBuilder()
	.WithHeaders(headers)
	.WithBasicAuthorization(basicAuthData)
        .Build();
```

### Body and Method

Below is an example POST request with a JSON body:

```
string jsonBody = JsonConvert.SerializeObject(new
{
	id = 1,
        example = "one"
});

HttpRequestData requestData = new HttpRequestDataBuilder()
	.WithMethod(HttpMethod.Post)
	.WithBody(jsonBody)
        .Build();

```

If your application validates `Content-Type` you can chain `.WithHeaders` as described in **Headers**.

### Binding Context

Similar to Http Headers, you may also set the custom Function Context binding data:

```
NameValueCollection contextData = new()
{
	{ "Custom-Header-1", "one" },
        { "Custom-Header-2", "two" }
};

HttpRequestData requestData = new HttpRequestDataBuilder()
	.WithBindingContextData(contextData)
        .Build();
```

### Cookies

You can set cookies in your request as follows:

```
List<IHttpCookie> cookies = [
	new HttpCookie("CookieName", "CookieValue")
];

HttpRequestData requestData = new HttpRequestDataBuilder()
	.WithCookies(cookies)
        .Build();
```

### Identities

You can set a collection of Identity claims as follows:

```
List<ClaimsIdentity> identities = [
	new ClaimsIdentity(
        [
        	new Claim(ClaimTypes.Name, "ExampleName"),
                new Claim(ClaimTypes.Email, "ExampleEmail")
	])
];

HttpRequestData requestData = new HttpRequestDataBuilder()
	.WithIdentities(identities)
        .Build();
```

### Custom Function Context

If you have an advanced middleware use case, you can provide your own `FunctionContext` data, though at that point you may want to derive a new class from `HttpRequestData` in your own implementation - here is a more basic example:

```
Dictionary<string, object> bindingData = new()
{
	{ "MockBindingData", "123" }
};

Mock<FunctionContext> functionContext = new();

IHost workersHost = new HostBuilder()
	.ConfigureFunctionsWebApplication()
        .Build();

functionContext.Setup(x => x.InstanceServices).Returns(workersHost.Services);
functionContext.SetupGet(x => x.BindingContext.BindingData).Returns(bindingData);

HttpRequestData request = new HttpRequestDataBuilder()
	.WithCustomContext(functionContext.Object)
        .Build();
```