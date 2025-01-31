using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Extensions;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.AzureDevOps;
using PreviewEnvironments.Application.Models.AzureDevOps.Contracts;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;
using PreviewEnvironments.Application.Test.Unit.TestHelpers;

namespace PreviewEnvironments.Application.Test.Unit.Services;

public class AzureReposGitProviderTests
{
    private const string DefaultAccessToken = "my-test-access-token";
    private const string TestInternalBuildId = "test-internal-build-id";
    
    private readonly string _expectedAccessTokenHeaderValue =
        Convert.ToBase64String(Encoding.ASCII.GetBytes($":{DefaultAccessToken}"));

    private readonly IConfigurationManager _configurationManager =
        Substitute.For<IConfigurationManager>();
    
    [Fact]
    public async Task PostPreviewAvailableMessageAsync_Should_Use_Correct_Http_Request_Message()
    {
        // Arrange
        (IGitProvider sut, MockHttpMessageHandler messageHandler) =
            GetSystemUnderTest();

        const string expectedScheme = "https";
        const string expectedHost = "dev.azure.com";
        const int expectedPullRequestNumber = 10;
        const string expectedOrganization = "MyTestOrganization";
        const string expectedProject = "MyTestProject";
        const string expectedRepositoryName = "MyTestRepo";

        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(new PreviewEnvironmentConfiguration
            {
                GitProvider = Constants.GitProviders.AzureRepos,
                AzureRepos = new AzureRepos
                {
                    OrganizationName = expectedOrganization,
                    ProjectName = expectedProject,
                    BaseAddress = new Uri($"{expectedScheme}://{expectedHost}"),
                    RepositoryName = expectedRepositoryName,
                    PersonalAccessToken = DefaultAccessToken
                }
            });

        string expectedPath =
            $"/{expectedOrganization}/{expectedProject}/_apis/git/repositories/{expectedRepositoryName}/pullRequests/{expectedPullRequestNumber}/threads";

        // Act
        await sut.PostPreviewAvailableMessageAsync(
            TestInternalBuildId,
            expectedPullRequestNumber,
            new Uri("https://test.domain.com"));

        // Assert
        messageHandler.Messages.Should().HaveCount(1);

        (HttpRequestMessage actualMessage, string actualContent) =
            messageHandler.Messages[0];
        
        Uri? actualUri = actualMessage.RequestUri;

        actualUri.Should().NotBeNull();

        using (new AssertionScope())
        {
            actualMessage.Method.Should().Be(HttpMethod.Post);
            actualUri!.Scheme.Should().Be(expectedScheme);
            actualUri!.Host.Should().Be(expectedHost);
            actualUri!.AbsolutePath.Should().Be(expectedPath);
            actualMessage.Content.Should().NotBeNull();
        }

        AuthenticationHeaderValue? header = actualMessage.Headers.Authorization;

        header.Should().NotBeNull();

        using (new AssertionScope())
        {
            header!.Scheme.Should().Be("Basic");
            header.Parameter.Should().Be(_expectedAccessTokenHeaderValue);
        }

        PullRequestThreadRequest? request =
            JsonSerializer.Deserialize<PullRequestThreadRequest>(actualContent);

        request.Should().NotBeNull();

        request!.Status.Should().Be("closed");
        
        request.Comments.Should().HaveCount(1);

        request.Comments[0].CommentType.Should().Be("system");
    }
    
    [Fact]
    public async Task PostPullRequestStatusAsync_Should_Use_Correct_Http_Request_Message()
    {
        // Arrange
        const string expectedScheme = "https";
        const string expectedHost = "dev.azure.com";
        const int expectedPullRequestNumber = 10;
        const string expectedOrganization = "MyTestOrganization";
        const string expectedProject = "MyTestProject";
        const string expectedRepositoryName = "MyTestRepo";
        const int iterationId = 10;
        
        (IGitProvider sut, MockHttpMessageHandler messageHandler) =
            GetSystemUnderTest($$"""
                 {
                    "count": "{{iterationId}}"
                 }  
                 """);

        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(new PreviewEnvironmentConfiguration
            {
                GitProvider = Constants.GitProviders.AzureRepos,
                AzureRepos = new AzureRepos
                {
                    OrganizationName = expectedOrganization,
                    ProjectName = expectedProject,
                    BaseAddress = new Uri($"{expectedScheme}://{expectedHost}"),
                    RepositoryName = expectedRepositoryName,
                    PersonalAccessToken = DefaultAccessToken
                }
            });

        string expectedStatusPath =
            $"/{expectedOrganization}/{expectedProject}/_apis/git/repositories/{expectedRepositoryName}/pullRequests/{expectedPullRequestNumber}/statuses";
        
        string expectedIterationPath =
            $"/{expectedOrganization}/{expectedProject}/_apis/git/repositories/{expectedRepositoryName}/pullRequests/{expectedPullRequestNumber}/iterations";

        // Act
        await sut.PostPullRequestStatusAsync(
            TestInternalBuildId,
            expectedPullRequestNumber,
            PullRequestStatusState.Succeeded);

        // Assert
        messageHandler.Messages.Should().HaveCount(2);

        (HttpRequestMessage getIterationIdMessage, _) = messageHandler.Messages[0];
        
        Uri? actualUri = getIterationIdMessage.RequestUri;

        actualUri.Should().NotBeNull();

        using (new AssertionScope())
        {
            getIterationIdMessage.Method.Should().Be(HttpMethod.Get);
            actualUri!.Scheme.Should().Be(expectedScheme);
            actualUri!.Host.Should().Be(expectedHost);
            actualUri!.AbsolutePath.Should().Be(expectedIterationPath);
        }

        AuthenticationHeaderValue? header = getIterationIdMessage.Headers.Authorization;

        header.Should().NotBeNull();

        using (new AssertionScope())
        {
            header!.Scheme.Should().Be("Basic");
            header.Parameter.Should().Be(_expectedAccessTokenHeaderValue);
        }
        
        (HttpRequestMessage actualMessage, string actualContent) =
            messageHandler.Messages[1];
        
        actualUri = actualMessage.RequestUri;

        actualUri.Should().NotBeNull();

        using (new AssertionScope())
        {
            actualMessage.Method.Should().Be(HttpMethod.Post);
            actualUri!.Scheme.Should().Be(expectedScheme);
            actualUri!.Host.Should().Be(expectedHost);
            actualUri!.AbsolutePath.Should().Be(expectedStatusPath);
            actualMessage.Content.Should().NotBeNull();
        }

        header = actualMessage.Headers.Authorization;

        header.Should().NotBeNull();

        using (new AssertionScope())
        {
            header!.Scheme.Should().Be("Basic");
            header.Parameter.Should().Be(_expectedAccessTokenHeaderValue);
        }

        PullRequestStatusRequest? statusRequest =
            JsonSerializer.Deserialize<PullRequestStatusRequest>(actualContent);

        statusRequest.Should().NotBeNull();

        using (new AssertionScope())
        {
            statusRequest!.State.Should().Be("succeeded");
            statusRequest.Description.Should().NotBeEmpty();
            statusRequest.TargetUrl.Should().BeEmpty();
            statusRequest.IterationId.Should().Be(iterationId);
        }
    }
    
    [Fact]
    public async Task PostPullRequestStatusAsync_Should_Use_1_For_IterationId_When_The_Api_Call_Fails()
    {
        // Arrange
        const int expectedPullRequestNumber = 10;
        const int expectedIterationId = 1;
        
        (IGitProvider sut, MockHttpMessageHandler messageHandler) =
            GetSystemUnderTest();

        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(new PreviewEnvironmentConfiguration
            {
                GitProvider = Constants.GitProviders.AzureRepos,
                AzureRepos = new AzureRepos
                {
                    PersonalAccessToken = DefaultAccessToken
                }
            });

        // Act
        await sut.PostPullRequestStatusAsync(
            TestInternalBuildId,
            expectedPullRequestNumber,
            PullRequestStatusState.Succeeded);

        // Assert
        messageHandler.Messages.Should().HaveCount(2);
        
        (_, string actualContent) = messageHandler.Messages[1];

        PullRequestStatusRequest? statusRequest =
            JsonSerializer.Deserialize<PullRequestStatusRequest>(actualContent);

        statusRequest.Should().NotBeNull();
        statusRequest!.IterationId.Should().Be(expectedIterationId);
    }
    
    [Fact]
    public async Task PostPullRequestStatusAsync_Should_Return_Early_When_Configuration_Is_Not_Found()
    {
        // Arrange
        const int expectedPullRequestNumber = 10;
        
        (IGitProvider sut, MockHttpMessageHandler messageHandler) =
            GetSystemUnderTest();

        // Act
        await sut.PostPullRequestStatusAsync(
            TestInternalBuildId,
            expectedPullRequestNumber,
            PullRequestStatusState.Succeeded);

        // Assert
        messageHandler.Messages.Should().HaveCount(0);
    }
    
    [Fact]
    public async Task PostPreviewAvailableMessageAsync_Should_Not_Throw_When_Status_Code_Does_Not_Indicate_Success()
    {
        // Arrange
        (IGitProvider sut, _) =
            GetSystemUnderTest(statusCode: HttpStatusCode.InternalServerError);

        PreviewAvailableMessage message = new();

        // Act
        Func<Task> action = () => sut.PostPreviewAvailableMessageAsync(
            TestInternalBuildId,
            message.PullRequestNumber,
            new Uri("https://test.domain.com"));

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetPullRequestById_Should_Return_Correct_Pull_Request()
    {
        // Arrange
        const string expectedScheme = "https";
        const string expectedHost = "dev.azure.com";
        const int expectedPullRequestNumber = 10;
        const string expectedOrganization = "MyTestOrganization";
        const string expectedProject = "MyTestProject";
        const string expectedRepositoryName = "MyTestRepo";
        const string expectedPullRequestState = "active";
        
        (IGitProvider sut, MockHttpMessageHandler messageHandler) =
            GetSystemUnderTest($$"""
                {
                    "pullRequestId": "{{expectedPullRequestNumber}}",
                    "status": "{{expectedPullRequestState}}"
                }
                """);

        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(new PreviewEnvironmentConfiguration
            {
                GitProvider = Constants.GitProviders.AzureRepos,
                AzureRepos = new AzureRepos
                {
                    OrganizationName = expectedOrganization,
                    ProjectName = expectedProject,
                    BaseAddress = new Uri($"{expectedScheme}://{expectedHost}"),
                    RepositoryName = expectedRepositoryName,
                    PersonalAccessToken = DefaultAccessToken
                }
            });

        string expectedPath =
            $"/{expectedOrganization}/{expectedProject}/_apis/git/pullRequests/{expectedPullRequestNumber}";

        // Act
        PullRequestResponse? pullRequest = await sut.GetPullRequestById(
            TestInternalBuildId,
            expectedPullRequestNumber);

        // Assert
        messageHandler.Messages.Should().HaveCount(1);

        (HttpRequestMessage message, _) = messageHandler.Messages[0];

        Uri? actualUri = message.RequestUri;

        actualUri.Should().NotBeNull();

        using (new AssertionScope())
        {
            message.Method.Should().Be(HttpMethod.Get);
            actualUri!.Scheme.Should().Be(expectedScheme);
            actualUri.Host.Should().Be(expectedHost);
            actualUri.AbsolutePath.Should().Be(expectedPath);
        }

        AuthenticationHeaderValue? header = message.Headers.Authorization;

        header.Should().NotBeNull();

        using (new AssertionScope())
        {
            header!.Scheme.Should().Be("Basic");
            header.Parameter.Should().Be(_expectedAccessTokenHeaderValue);
        }

        pullRequest.Should().NotBeNull();

        using (new AssertionScope())
        {
            pullRequest!.PullRequestId.Should().Be(expectedPullRequestNumber);
            pullRequest.Status.Should().Be(expectedPullRequestState);
        }
    }
    
    [Fact]
    public async Task GetPullRequestById_Should_Return_Early_When_Configuration_Is_Not_Found()
    {
        // Arrange
        const int expectedPullRequestNumber = 10;
        
        (IGitProvider sut, _) =
            GetSystemUnderTest();
        
        _configurationManager
            .GetConfigurationById(TestInternalBuildId)
            .Returns(new PreviewEnvironmentConfiguration
            {
                GitProvider = Constants.GitProviders.AzureRepos,
                AzureRepos = new AzureRepos()
            });

        // Act
        Func<Task> act = () => sut.GetPullRequestById(
            TestInternalBuildId,
            expectedPullRequestNumber);

        // Assert
        Exception? exception = (await act.Should().ThrowAsync<Exception>())
            .Subject.First();

        exception.Message.Should().Contain(Constants.EnvVariables.AzAccessToken);
    }

    [Fact]
    public async Task GetAccessToken_Should_Throw_When_No_Token_Is_Found()
    {
        // Arrange
        const int expectedPullRequestNumber = 10;
        
        (IGitProvider sut, MockHttpMessageHandler messageHandler) =
            GetSystemUnderTest();

        // Act
        PullRequestResponse? response = await sut.GetPullRequestById(
            TestInternalBuildId,
            expectedPullRequestNumber);

        // Assert
        response.Should().BeNull();
        messageHandler.Messages.Should().HaveCount(0);
    }

    private (IGitProvider, MockHttpMessageHandler) GetSystemUnderTest(
        string response = "",
        HttpStatusCode statusCode = HttpStatusCode.OK,
        ApplicationConfiguration? configuration = null)
    {
        configuration ??= new ApplicationConfiguration();

        MockHttpMessageHandler messageHandler = new(response, statusCode);

        IGitProvider sut = new AzureReposGitProvider(
            Substitute.For<ILogger<AzureReposGitProvider>>(),
            Options.Create(configuration),
            _configurationManager,
            new HttpClient(messageHandler));
        
        return (sut, messageHandler);
    }
}