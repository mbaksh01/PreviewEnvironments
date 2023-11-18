namespace PreviewEnvironments.Application.Models.AzureDevOps.Builds;

public sealed class BuildComplete
{
    public required string SourceBranch { get; set; }

    public required BuildStatus BuildStatus { get; set; }

    public required string ProjectName { get; set; }

    public required int PrNumber { get; set; }

    public required Uri BuildUrl { get; set; }
}
