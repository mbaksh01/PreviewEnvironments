namespace PreviewEnvironments.Application.Models.Docker;

/// <summary>
/// Model used to track a docker container started by this application.
/// </summary>
public sealed class DockerContainer
{
    /// <summary>
    /// Id of container.
    /// </summary>
    public required string ContainerId { get; set; }

    /// <summary>
    /// Name of image the container is running.
    /// </summary>
    public required string ImageName { get; set; }

    /// <summary>
    /// Tag of image being run by the container.
    /// </summary>
    public required string ImageTag { get; set; }

    /// <summary>
    /// Time when the container was created.
    /// </summary>
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Marked indicating if the container has been stopped.
    /// </summary>
    public bool Expired { get; set; }

    /// <summary>
    /// Marked indicating if a container can be stopped.
    /// </summary>
    public bool CanExpire { get; set; } = true;

    /// <summary>
    /// Pull request id linked to container.
    /// </summary>
    public int PullRequestId { get; set; }

    /// <summary>
    /// Public port of the running container.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Internal id of the build that triggered the creation of this container.
    /// </summary>
    public string InternalBuildId { get; set; } = string.Empty;
}
