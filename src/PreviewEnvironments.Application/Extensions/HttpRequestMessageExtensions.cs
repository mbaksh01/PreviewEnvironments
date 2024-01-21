using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PreviewEnvironments.Application.Extensions;

internal static class HttpRequestMessageExtensions
{
    /// <summary>
    /// Adds the <paramref name="accessToken"/> to the authorization header of
    /// the <param name="message"></param>.
    /// </summary>
    /// <param name="message">Message to add authorization to.</param>
    /// <param name="accessToken">Access token to add to header.</param>
    /// <returns>
    /// The <paramref name="message"/> with the authorization header appended.
    /// </returns>
    public static HttpRequestMessage WithBasicAuthorization(this HttpRequestMessage message, string accessToken)
    {
        message.Headers.Authorization = new AuthenticationHeaderValue(
            scheme: "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($":{accessToken}"))
        );

        return message;
    }

    /// <summary>
    /// Adds the <paramref name="body"/> to the current <paramref name="message"/>.
    /// </summary>
    /// <param name="message">Message to append body to.</param>
    /// <param name="body">Body to append.</param>
    /// <typeparam name="T">Type of body.</typeparam>
    /// <returns>
    /// The <paramref name="message"/> with the body header appended.
    /// </returns>
    public static HttpRequestMessage WithJsonBody<T>(this HttpRequestMessage message, T body)
    {
        string bodyAsString = JsonSerializer.Serialize(body);

        message.Content = new StringContent(
            bodyAsString,
            mediaType: new MediaTypeHeaderValue("application/json")
        );

        return message;
    }
}
