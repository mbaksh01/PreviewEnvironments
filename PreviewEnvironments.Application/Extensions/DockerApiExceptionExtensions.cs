using Docker.DotNet;

namespace PreviewEnvironments.Application.Extensions;

internal static class DockerApiExceptionExtensions
{
    public static string GetContainerId(this DockerApiException exception)
    {
        ReadOnlySpan<char> response = exception.ResponseBody;

        ReadOnlySpan<char> marker = "by container \\\"";

        int start = response.IndexOf(marker) + marker.Length;

        ReadOnlySpan<char> containerId = response.Slice(start, 64);

        return containerId.ToString();
    }

    //{"message":"Conflict. The container name \"/preview-images-registry\" is already in use by container \"0eac7cd01f0e8a703344a7f41f5a2fb726c06485bdf27eb6b5e31ea6aad9d55c\". You have to remove (or rename) that container to be able to reuse that name."}
}
