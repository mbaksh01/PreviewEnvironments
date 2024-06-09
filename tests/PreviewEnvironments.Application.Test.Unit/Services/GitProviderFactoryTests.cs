using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Test.Unit.Services;

public class GitProviderFactoryTests
{
    private readonly IGitProviderFactory _sut;
    private readonly IKeyedServiceProvider _serviceProvider;

    public GitProviderFactoryTests()
    {
        _serviceProvider = Substitute.For<IKeyedServiceProvider>();
        
        _sut = new GitProviderFactory(_serviceProvider);
    }

    [Fact]
    public void CreateProvider_Should_Return_AzureReposGitProvider()
    {
        // Arrange
        _serviceProvider
            .GetRequiredKeyedService(typeof(IGitProvider), Constants.GitProviders.AzureRepos)
            .Returns(GetAzureReposGitProvider());

        // Act
        IGitProvider provider = _sut.CreateProvider(GitProvider.AzureRepos);

        // Assert
        provider.Should().BeOfType<AzureReposGitProvider>();
    }
    
    [Fact]
    public void CreateProvider_Should_Throw_When_GitProvider_Is_Not_Supported()
    {
        // Act
        Func<IGitProvider> act = () => _sut.CreateProvider((GitProvider)(-1));

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    private static AzureReposGitProvider GetAzureReposGitProvider()
    {
        return new AzureReposGitProvider(
            Substitute.For<ILogger<AzureReposGitProvider>>(),
            Options.Create(new ApplicationConfiguration()),
            Substitute.For<IConfigurationManager>(),
            new HttpClient());
    }
}