using System.Text.Json.Serialization;

namespace PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

/// <summary>
/// Model used to create a status on a pull request.
/// Learn more here: https://learn.microsoft.com/en-us/rest/api/azure/devops/git/pull-request-statuses?view=azure-devops-rest-7.1
/// </summary>
public sealed class PullRequestStatus
{
    /// <summary>
    /// Id of the iteration to associate status with. Minimum value is 1.
    /// </summary>
    [JsonPropertyName("iterationId")]
    public int IterationId { get; set; } = 1;

    /// <summary>
    /// State of the status. Support values include: error, failed,
    /// notApplicable, notSet, pending, succeeded.
    /// </summary>
    [JsonPropertyName("state")]
    public required string State { get; set; }

    /// <summary>
    /// Status description. Typically describes current state of the status.
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    /// <summary>
    /// Context of the status.
    /// </summary>
    [JsonPropertyName("context")]
    public required Context Context { get; set; }

    /// <summary>
    /// URL with status details.
    /// </summary>
    [JsonPropertyName("targetUrl")]
    public required string TargetUrl { get; set; }
}

public sealed class Context
{
    /// <summary>
    /// Genre of the status. Typically name of the service/tool generating the
    /// status, can be empty.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Name identifier of the status, cannot be null or empty.
    /// </summary>
    [JsonPropertyName("genre")]
    public required string Genre { get; set; }
}
