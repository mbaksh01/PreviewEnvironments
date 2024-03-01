using PreviewEnvironments.Application.Models;

namespace PreviewEnvironments.Application.Services.Abstractions;

internal interface IConfigurationManager
{
    Task LoadConfigurationsAsync(CancellationToken cancellationToken = default);
    
    void ValidateConfigurations();
    
    PreviewEnvironmentConfiguration? GetConfigurationById(
        string id);
}