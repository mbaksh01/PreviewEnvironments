namespace PreviewEnvironments.Application.Models;

internal sealed class ApplicationConfiguration
{
    public string Organization { get; set; } = string.Empty;

    public string Project { get; set; } = string.Empty;

    public Guid RepositoryId { get; set; }

    public string AzAccessToken { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public string Scheme { get; set; } = "https";
}
