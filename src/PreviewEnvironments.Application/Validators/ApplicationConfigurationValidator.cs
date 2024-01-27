using FluentValidation;
using PreviewEnvironments.Application.Models;

namespace PreviewEnvironments.Application.Validators;

public class ApplicationConfigurationValidator : AbstractValidator<ApplicationConfiguration>
{
    public ApplicationConfigurationValidator()
    {
        RuleFor(c => c.ContainerTimeoutIntervalSeconds)
            .GreaterThanOrEqualTo(0);
    }
}