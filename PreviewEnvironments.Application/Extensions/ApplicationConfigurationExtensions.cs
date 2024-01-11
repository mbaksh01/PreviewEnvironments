using PreviewEnvironments.Application.Models;

namespace PreviewEnvironments.Application.Extensions;

public static class ApplicationConfigurationExtensions
{
    public static SupportedBuildDefinition? GetBuildDefinition(this ApplicationConfiguration configuration, int definitionId)
    {
        return configuration
            .AzureDevOps
            .SupportedBuildDefinitions
            .FirstOrDefault(sbd =>
                sbd.BuildDefinitionId == definitionId
                && sbd.ProjectName == configuration.AzureDevOps.ProjectName);
    }
}