using System.Net;

namespace PreviewEnvironments.Application.Test.Unit.TestHelpers;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly string _response;
    private readonly HttpStatusCode _statusCode;

    public List<(HttpRequestMessage Message, string Content)> Messages { get; } = [];

    public MockHttpMessageHandler()
    {
        _response = string.Empty;
        _statusCode = HttpStatusCode.OK;
    }
    
    public MockHttpMessageHandler(string response, HttpStatusCode statusCode)
    {
        _response = response;
        _statusCode = statusCode;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Content is not null)
        {
            Messages.Add((Clone(request), await request.Content.ReadAsStringAsync(cancellationToken)));
        }
        else
        {
            Messages.Add((Clone(request), string.Empty));
        }
        
        return new HttpResponseMessage
        {
            StatusCode = _statusCode,
            Content = new StringContent(_response)
        };
    }

    private static HttpRequestMessage Clone(HttpRequestMessage message)
    {
        HttpRequestMessage clone = new(message.Method, message.RequestUri)
        {
            Content = message.Content,
            Version = message.Version,
            VersionPolicy = message.VersionPolicy,
        };

        foreach (KeyValuePair<string, IEnumerable<string>> header in message.Headers)
        {
            clone.Headers.Add(header.Key, header.Value);
        }

        foreach (KeyValuePair<string, object?> option in message.Options)
        {
            clone.Options.TryAdd(option.Key, option.Value);
        }

        return clone;
    }
}