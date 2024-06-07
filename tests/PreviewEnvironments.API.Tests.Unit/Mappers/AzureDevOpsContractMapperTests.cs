using Microsoft.AspNetCore.Http;
using PreviewEnvironments.API.Mappers;
using PreviewEnvironments.Application.Helpers;
using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;
using PreviewEnvironments.Application.Models.Commands;
using PreviewEnvironments.Contracts.AzureDevOps.v1;
using PreviewEnvironments.Contracts.AzureDevOps.v2;

namespace PreviewEnvironments.API.Tests.Unit.Mappers;

public class AzureDevOpsContractMapperTests
{
    [Theory]
    [InlineData("succeeded", BuildStatus.Succeeded)]
    [InlineData("partiallySucceeded", BuildStatus.PartiallySucceeded)]
    [InlineData("failed", BuildStatus.Failed)]
    public void BuildCompleteContract_ToModel_Should_Return_Correct_Model(
        string rawBuildStatus,
        BuildStatus expectedStatus)
    {
        // Arrange
        const string branchName = "tests/my-branch";
        const int prNumber = 10;
        const int buildDefinitionId = 10;
        Uri buildUrl = new("https://dev.azure.com");
        
        BuildCompleteContract contract = new()
        {
            Resource = new BCResource
            {
                SourceBranch = branchName,
                Result = rawBuildStatus,
                TriggerInfo = new BCTriggerInfo
                {
                    PrNumber = prNumber
                },
                Definition = new BCDefinition
                {
                    Id = buildDefinitionId
                },
                Links = new BCResourceLinks
                {
                    Web = new BCBadge
                    {
                        Href = buildUrl
                    }
                }
            }
        };
        
        // Act
        BuildComplete model = contract.ToModel();

        // Assert
        model.Should().NotBeNull();

        using (new AssertionScope())
        {
            model.BuildStatus.Should().Be(expectedStatus);
            model.InternalBuildId.Should().Be(IdHelper.GetAzurePipelinesId(contract));
            model.SourceBranch.Should().Be(branchName);
            model.PullRequestId.Should().Be(prNumber);
            model.BuildUrl.Should().Be(buildUrl);
        }
    }

    [Theory]
    [InlineData("completed", PullRequestState.Completed)]
    [InlineData("active", PullRequestState.Active)]
    [InlineData("abandoned", PullRequestState.Abandoned)]
    public void PullRequestUpdatedContract_ToModel_Should_Return_Correct_Model(
        string rawPullRequestStatus,
        PullRequestState expectedState)
    {
        // Arrange
        const int pullRequestId = 10;
        
        PullRequestUpdatedContract contract = new()
        {
            Resource = new PrResource
            {
                PullRequestId = pullRequestId,
                Status = rawPullRequestStatus
            }
        };
        
        // Act
        PullRequestUpdated model = contract.ToModel();

        // Assert
        model.Should().NotBeNull();

        using (new AssertionScope())
        {
            model.Id.Should().Be(pullRequestId);
            model.State.Should().Be(expectedState);
        }
    }

    [Fact]
    public void PullRequestCommentedOnContract_ToMetadata_Should_Map_Correctly()
    {
        // Arrange
        const int pullRequestId = 1;
        const string testOrganization = "TestOrganization";
        const string testProject = "TestProject";
        const string testRepository = "TestRepository";
        
        PullRequestCommentedOnContract contract = new()
        {
            Resource = new PRCOResource
            {
                PullRequest = new PRCOPullRequest
                {
                    PullRequestId = pullRequestId,
                    Repository = new PRCORepository
                    {
                        Name = testRepository,
                        RemoteUrl = $"https://dev.azure.com/{testOrganization}/{testProject}/_git/{testRepository}"
                    }
                }
            },
        };

        // Act
        CommandMetadata metadata = contract.ToMetadata();

        // Assert
        metadata.Should().NotBeNull();

        using (new AssertionScope())
        {
            metadata.PullRequestId.Should().Be(pullRequestId);
            metadata.GitProvider.Should().Be(Application.Constants.GitProviders.AzureRepos);
            metadata.OrganizationName.Should().Be(testOrganization);
            metadata.ProjectName.Should().Be(testProject);
            metadata.RepositoryName.Should().Be(testRepository);
        }
    }

    [Fact]
    public void BuildComplete_WithHost_Should_Set_Host_Correctly()
    {
        // Arrange
        BuildComplete buildComplete = new()
        {
            BuildStatus = BuildStatus.Succeeded,
            BuildUrl = new Uri("https://build.address.com"),
            SourceBranch = "test/branch",
            InternalBuildId = "TestInternalBuildId",
            PullRequestId = 1
        };

        HttpRequest request = new TestHttpRequest
        {
            Host = new HostString("test.application.com", 1234),
            Scheme = "https",
        };

        // Act
        buildComplete.WithHost(request);

        // Assert
        buildComplete.Host.Should().BeEquivalentTo(new Uri("https://test.application.com:1234"));
    }

    class TestHttpRequest : HttpRequest
    {
        public override Task<IFormCollection> ReadFormAsync(
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public override HttpContext HttpContext { get; }
        public override string Method { get; set; }
        public override string Scheme { get; set; }
        public override bool IsHttps { get; set; }
        public override HostString Host { get; set; }
        public override PathString PathBase { get; set; }
        public override PathString Path { get; set; }
        public override QueryString QueryString { get; set; }
        public override IQueryCollection Query { get; set; }
        public override string Protocol { get; set; }
        public override IHeaderDictionary Headers { get; }
        public override IRequestCookieCollection Cookies { get; set; }
        public override long? ContentLength { get; set; }
        public override string? ContentType { get; set; }
        public override Stream Body { get; set; }
        public override bool HasFormContentType { get; }
        public override IFormCollection Form { get; set; }
    }
}