using System.Text;
using System.Text.Json;

namespace PreviewEnvironments.Application.Extensions;

internal static class HttpRequestMessageExtensions
{
    public static HttpRequestMessage WithAuthorization(this HttpRequestMessage message, string accessToken)
    {
        message.Headers.Authorization = new(
            scheme: "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($":{accessToken}"))
        );

        return message;
    }

    public static HttpRequestMessage WithBody<T>(this HttpRequestMessage message, T body)
    {
        string bodyAsString = JsonSerializer.Serialize(body);

        message.Content = new StringContent(
            bodyAsString,
            mediaType: new("application/json")
        );

        return message;
    }
}
