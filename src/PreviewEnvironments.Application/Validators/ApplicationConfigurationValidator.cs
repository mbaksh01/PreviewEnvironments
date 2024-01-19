using FluentValidation;
using PreviewEnvironments.Application.Models;

namespace PreviewEnvironments.Application.Validators;

public class ApplicationConfigurationValidator : AbstractValidator<ApplicationConfiguration>
{
    public ApplicationConfigurationValidator()
    {
        ConfigureBaseRules();
        ConfigureAzureDevOpsRules();
        ConfigureDockerRules();
    }

    private void ConfigureBaseRules()
    {
        RuleFor(c => c.Scheme)
            .Must(BeAValidScheme);

        RuleFor(c => c.Host)
            .NotEmpty();

        RuleFor(c => c.ContainerTimeoutIntervalSeconds)
            .GreaterThanOrEqualTo(0);
    }

    private void ConfigureAzureDevOpsRules()
    {
        RuleFor(c => c.AzureDevOps.Scheme)
            .Must(BeAValidScheme);

        RuleFor(c => c.AzureDevOps.Host)
            .NotEmpty();

        RuleFor(c => c.AzureDevOps.Organization)
            .NotEmpty();

        RuleFor(c => c.AzureDevOps.ProjectName)
            .NotEmpty();

        RuleFor(c => c.AzureDevOps.RepositoryId)
            .NotEmpty();

        RuleFor(c => c.AzureDevOps.AzAccessToken)
            .NotEmpty();
    }

    private void ConfigureDockerRules()
    {
        RuleFor(c => c.Docker.CreateContainerRetryCount)
            .GreaterThanOrEqualTo(0);

        RuleFor(c => c.Docker.ContainerTimeoutSeconds)
            .GreaterThanOrEqualTo(0);
    }
    
    private static bool BeAValidScheme(string arg)
    {
        return arg is "http" or "https";
    }
}