namespace PreviewEnvironments.Application.Models.Commands;

public sealed record CommandMetadata
{
    public int PullRequestId { get; init; }

    public string GitProvider { get; init; } = string.Empty;

    public string OrganizationName { get; init; } = string.Empty;

    public string ProjectName { get; init; } = string.Empty;

    public string RepositoryName { get; init; } = string.Empty;

    public Uri Host { get; set; } = default!;
}