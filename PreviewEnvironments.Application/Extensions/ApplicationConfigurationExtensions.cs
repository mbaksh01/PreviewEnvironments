using PreviewEnvironments.Application.Models;

namespace PreviewEnvironments.Application.Extensions;

public static class ApplicationConfigurationExtensions
{
    /// <summary>
    /// Gets a build definition using the <paramref name="definitionId"/> cross
    /// referencing the <see cref="AzureDevOpsConfiguration.ProjectName"/> with
    /// the <see cref="SupportedBuildDefinition.ProjectName"/>.
    /// </summary>
    /// <param name="configuration">Current configuration of the application.</param>
    /// <param name="definitionId">Build definition id to find.</param>
    /// <returns>
    /// The <see cref="SupportedBuildDefinition"/> associated with the
    /// <paramref name="definitionId"/> or <see langword="null"/> if the
    /// <see cref="SupportedBuildDefinition"/> is not found.
    /// </returns>
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