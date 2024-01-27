using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.AzureDevOps;
using PreviewEnvironments.Application.Models.AzureDevOps.Contracts;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;
using PreviewEnvironments.Application.Test.Unit.TestHelpers;

namespace PreviewEnvironments.Application.Test.Unit.Services;

public class AzureReposGitProviderTests
{
    private const string DefaultAccessToken = "my-test-access-token";
    private const string TestInternalBuildId = "test-internal-build-it";
    
    private readonly string _expectedAccessTokenHeaderValue =
        Convert.ToBase64String(Encoding.ASCII.GetBytes($":{DefaultAccessToken}"));
    
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
        Guid expectedRepositoryId = Guid.NewGuid();

        string expectedPath =
            $"/{expectedOrganization}/{expectedProject}/_apis/git/repositories/{expectedRepositoryId}/pullRequests/{expectedPullRequestNumber}/threads";

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

    private static (IGitProvider, MockHttpMessageHandler) GetSystemUnderTest(
        string response = "",
        HttpStatusCode statusCode = HttpStatusCode.OK,
        ApplicationConfiguration? configuration = null)
    {
        configuration ??= new ApplicationConfiguration();

        MockHttpMessageHandler messageHandler = new(response, statusCode);

        IGitProvider sut = new AzureReposGitProvider(
            Substitute.For<ILogger<AzureReposGitProvider>>(),
            Options.Create(configuration),
            Substitute.For<IConfigurationManager>(),
            new HttpClient(messageHandler));
        
        return (sut, messageHandler);
    }
}