using Microsoft.Extensions.Options;
using PreviewEnvironments.API.Contracts.AzureDevOps.v2;
using PreviewEnvironments.Application.Models;

namespace PreviewEnvironments.API.Extensions;

public static class OptionsExtensions
{
    public static IOptions<ApplicationConfiguration> Apply(this IOptions<ApplicationConfiguration> options, BuildCompleteContract buildComplete)
    {
        var azureDevOpsConfig = options.Value.AzureDevOps;

        if (string.IsNullOrWhiteSpace(buildComplete.Resource.Project?.Name) == false)
        {
            azureDevOpsConfig.Project = buildComplete.Resource.Project.Name;
        }
        
        azureDevOpsConfig.RepositoryId = buildComplete.Resource.Repository.Id;
        
        return options;
    }
}