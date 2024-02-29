using System.Text.Json.Serialization;

namespace PreviewEnvironments.Application.Models.AzureDevOps.Contracts;

public class PullRequestIterationsResponse
{
    [JsonPropertyName("count")]
    public int IterationCount { get; set; }
}