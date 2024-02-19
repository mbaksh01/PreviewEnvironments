using FluentValidation;
using PreviewEnvironments.Application.Models;

namespace PreviewEnvironments.Application.Validators;

public class PreviewEnvironmentConfigurationValidator
    : AbstractValidator<PreviewEnvironmentConfiguration>
{
    public PreviewEnvironmentConfigurationValidator()
    {
        RuleFor(c => c.GitProvider)
            .NotEmpty()
            .NotEqual("Unknown");

        RuleFor(c => c.BuildServer)
            .NotEmpty()
            .NotEqual("Unknown");

        RuleFor(c => c.Deployment)
            .NotNull();

        RuleFor(c => c.AzureRepos)
            .NotNull()
            .When(c => c.GitProvider is Constants.GitProviders.AzureRepos);

        RuleFor(c => c.AzurePipelines)
            .NotNull()
            .When(c => c.BuildServer is Constants.BuildServers.AzurePipelines);

        ConfigureDeploymentRules();

        ConfigureGitProviderRules();

        ConfigureBuildServerRules();
    }

    private void ConfigureDeploymentRules()
    {
        When(c => c.Deployment is not null,
            () =>
            {
                RuleFor(c => c.Deployment.ContainerHostAddress)
                    .NotEmpty();

                RuleFor(c => c.Deployment.ImageName)
                    .NotEmpty();

                RuleFor(c => c.Deployment.ImageRegistry)
                    .NotEmpty();

                RuleFor(c => c.Deployment.AllowedDeploymentPorts)
                    .Must(p => p.Length == p.Distinct().Count())
                    .WithMessage($"{nameof(Deployment.AllowedDeploymentPorts)} must be a unique list.");

                RuleFor(c => c.Deployment.ContainerTimeoutSeconds)
                    .GreaterThanOrEqualTo(0);

                RuleFor(c => c.Deployment.CreateContainerRetryCount)
                    .GreaterThanOrEqualTo(0);
            });
    }

    private void ConfigureGitProviderRules()
    {
        When(c => c.GitProvider is Constants.GitProviders.AzureRepos,
            ConfigureAzureReposRules);
    }

    private void ConfigureBuildServerRules()
    {
        When(c => c.BuildServer is Constants.BuildServers.AzurePipelines,
            ConfigureAzurePipelinesRules);
    }

    private void ConfigureAzureReposRules()
    {
        When(c => c.AzureRepos is not null,
            () =>
            {
                RuleFor(c => c.AzureRepos!.BaseAddress)
                    .NotNull();

                RuleFor(c => c.AzureRepos!.OrganizationName)
                    .NotEmpty();

                RuleFor(c => c.AzureRepos!.ProjectName)
                    .NotEmpty();

                RuleFor(c => c.AzureRepos!.RepositoryName)
                    .NotEmpty();
            });
    }

    private void ConfigureAzurePipelinesRules()
    {
        When(c => c.AzurePipelines is not null,
            () =>
            {
                RuleFor(c => c.AzurePipelines!.ProjectName)
                    .NotEmpty();

                RuleFor(c => c.AzurePipelines!.BuildDefinitionId)
                    .GreaterThan(0);
            });
    }
}