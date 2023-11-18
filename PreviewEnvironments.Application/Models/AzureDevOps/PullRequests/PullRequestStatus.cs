using System.Text.Json.Serialization;

namespace PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;

public sealed class PullRequestStatus
{
    [JsonPropertyName("iterationId")]
    public int IterationId { get; set; } = 1;

    [JsonPropertyName("state")]
    public required string State { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("context")]
    public required Context Context { get; set; }

    [JsonPropertyName("targetUrl")]
    public required string TargetUrl { get; set; }
}

public partial class Context
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("genre")]
    public required string Genre { get; set; }
}
