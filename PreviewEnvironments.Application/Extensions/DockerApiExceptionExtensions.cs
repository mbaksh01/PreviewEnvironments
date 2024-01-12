using Docker.DotNet;

namespace PreviewEnvironments.Application.Extensions;

internal static class DockerApiExceptionExtensions
{
    private const string Marker = "by container \\\"";
    
    /// <summary>
    /// Get the container id from the
    /// <see cref="DockerApiException.ResponseBody"/>.
    /// </summary>
    /// <param name="exception">Exception thrown by docker API.</param>
    /// <returns>The id of the container linked to this exception.</returns>
    public static string GetContainerId(this DockerApiException exception)
    {
        ReadOnlySpan<char> response = exception.ResponseBody;

        int start = response.IndexOf(Marker) + Marker.Length;

        ReadOnlySpan<char> containerId = response.Slice(start, 64);

        return containerId.ToString();
    }
}
