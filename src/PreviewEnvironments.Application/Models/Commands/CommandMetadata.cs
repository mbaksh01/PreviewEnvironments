namespace PreviewEnvironments.Application.Models.Commands;

public sealed class CommandMetadata
{
    public int PullRequestId { get; set; }

    public string GitProvider { get; set; } = string.Empty;
}