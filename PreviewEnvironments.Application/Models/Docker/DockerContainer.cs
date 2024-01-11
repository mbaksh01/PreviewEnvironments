namespace PreviewEnvironments.Application.Models.Docker;

public sealed class DockerContainer
{
    public required string ContainerId { get; set; }

    public required string ImageName { get; set; }

    public required string ImageTag { get; set; }

    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;

    public bool Expired { get; set; }

    public bool CanExpire { get; set; } = true;

    public int PullRequestId { get; set; }

    public int Port { get; set; }

    public int BuildDefinitionId { get; set; }
}
