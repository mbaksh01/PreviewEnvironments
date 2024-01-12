using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.AzureDevOps;

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

    internal static TMessage CreateAzureDevOpsMessage<TMessage>(
        this ApplicationConfiguration configuration, string accessToken = "")
        where TMessage : AzureDevOpsMessage
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            accessToken = configuration.AzureDevOps.AzAccessToken ?? string.Empty;
        }
        
        return (TMessage)new AzureDevOpsMessage
        {
            Scheme = configuration.AzureDevOps.Scheme,
            Host = configuration.AzureDevOps.Host,
            Organization = configuration.AzureDevOps.Organization,
            Project = configuration.AzureDevOps.ProjectName,
            RepositoryId = configuration.AzureDevOps.RepositoryId,
            AccessToken = accessToken,
        };
    }
}