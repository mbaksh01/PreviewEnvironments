﻿using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PreviewEnvironments.Application.Models;
using PreviewEnvironments.Application.Models.AzureDevOps;
using PreviewEnvironments.Application.Models.AzureDevOps.Builds;
using PreviewEnvironments.Application.Models.AzureDevOps.PullRequests;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Test.Unit.Services;

public class PreviewEnvironmentManagerTests
{
    private readonly IPreviewEnvironmentManager _sut;
    private readonly IAzureDevOpsService _azureDevOpsService;
    private readonly IValidator<ApplicationConfiguration> _validator;
    private readonly IOptions<ApplicationConfiguration> _options;
    private readonly IDockerService _dockerService;
    
    public PreviewEnvironmentManagerTests()
    {
        _azureDevOpsService = Substitute.For<IAzureDevOpsService>();
        _validator = Substitute.For<IValidator<ApplicationConfiguration>>();
        _options = Options.Create(new ApplicationConfiguration());
        _dockerService = Substitute.For<IDockerService>();
        
        _sut = new PreviewEnvironmentManager(
            Substitute.For<ILogger<PreviewEnvironmentManager>>(),
            _validator,
            _options,
            _azureDevOpsService,
            _dockerService);
    }
    
    [Fact]
    public async Task BuildComplete_Should_Return_Early_When_Source_Branch_Is_Invalid()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

        buildComplete.SourceBranch = "refs/origin/main";

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _azureDevOpsService
            .Received(0)
            .PostPullRequestStatusAsync(Arg.Any<PullRequestStatusMessage>());
    }
    
    [Theory]
    [InlineData(BuildStatus.Failed)]
    [InlineData(BuildStatus.PartiallySucceeded)]
    public async Task BuildComplete_Should_Return_Early_When_Build_Status_Is_Invalid(BuildStatus status)
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();
        
        buildComplete.BuildStatus = status;

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _azureDevOpsService
            .Received(0)
            .PostPullRequestStatusAsync(Arg.Any<PullRequestStatusMessage>());
    }
    
    [Fact]
    public async Task BuildComplete_Should_Return_Early_When_Supported_Build_Is_Not_Found()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();

        buildComplete.BuildDefinitionId = 1;

        _options.Value.AzureDevOps.SupportedBuildDefinitions =
        [
            new SupportedBuildDefinition
            {
                BuildDefinitionId = 10
            }
        ];

        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _azureDevOpsService
            .Received(0)
            .PostPullRequestStatusAsync(Arg.Any<PullRequestStatusMessage>());
    }

    [Fact]
    public async Task BuildComplete_Should_Post_Two_Statuses_When_Container_Started_Successfully()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();
        
        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());
        
        _options.Value.AzureDevOps.SupportedBuildDefinitions =
        [
            new SupportedBuildDefinition
            {
                BuildDefinitionId = buildComplete.BuildDefinitionId
            }
        ];

        List<PullRequestStatusMessage> messages = [];

        _azureDevOpsService
            .PostPullRequestStatusAsync(Arg.Any<PullRequestStatusMessage>())
            .Returns(Task.CompletedTask)
            .AndDoes(x => messages.Add(x.Arg<PullRequestStatusMessage>()));

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _azureDevOpsService
            .Received(2)
            .PostPullRequestStatusAsync(Arg.Any<PullRequestStatusMessage>());

        messages.Should().HaveCount(2);

        PullRequestStatusMessage pendingMessage = messages[0];

        using (new AssertionScope())
        {
            pendingMessage.PullRequestNumber.Should().Be(buildComplete.PullRequestNumber);
            pendingMessage.BuildPipelineAddress.Should().Be(buildComplete.BuildUrl.ToString());
            pendingMessage.State.Should().Be(PullRequestStatusState.Pending);
        }
        
        PullRequestStatusMessage succeededMessage = messages[1];

        using (new AssertionScope())
        {
            succeededMessage.PullRequestNumber.Should().Be(buildComplete.PullRequestNumber);
            succeededMessage.BuildPipelineAddress.Should().Be(buildComplete.BuildUrl.ToString());
            succeededMessage.State.Should().Be(PullRequestStatusState.Succeeded);
            
            succeededMessage
                .Port
                .Should()
                .BeGreaterThanOrEqualTo(10_000)
                .And
                .BeLessThanOrEqualTo(60_000);
        }
    }

    [Fact]
    public async Task BuildComplete_Should_Use_Port_From_Allowed_Image_Ports()
    {
        // Arrange
        const int expectedPort = 7000;
        BuildComplete buildComplete = GetValidBuildComplete();
        
        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());
        
        _options.Value.AzureDevOps.SupportedBuildDefinitions =
        [
            new SupportedBuildDefinition
            {
                BuildDefinitionId = buildComplete.BuildDefinitionId,
                AllowedImagePorts = [ expectedPort ]
            }
        ];

        int port = 0;
        
        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>())
            .Returns(new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = string.Empty,
                ImageTag = string.Empty
            })
            .AndDoes(x => port = x.ArgAt<int>(3));

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        port.Should().Be(expectedPort);
    }
    
    [Fact]
    public async Task BuildComplete_Should_Use_Next_Available_Port_From_Allowed_Image_Ports()
    {
        // Arrange
        const int expectedPort = 7001;
        BuildComplete buildComplete = GetValidBuildComplete();
        
        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());
        
        _options.Value.AzureDevOps.SupportedBuildDefinitions =
        [
            new SupportedBuildDefinition
            {
                BuildDefinitionId = buildComplete.BuildDefinitionId,
                AllowedImagePorts = [ 7000, expectedPort ]
            }
        ];

        int port = 0;
        
        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.BuildDefinitionId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = string.Empty,
                ImageTag = string.Empty,
                Port = x.ArgAt<int>(3),
                BuildDefinitionId = buildComplete.BuildDefinitionId,
            })
            .AndDoes(x => port = x.ArgAt<int>(3));
        
        await _sut.BuildCompleteAsync(buildComplete);

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        port.Should().Be(expectedPort);
    }
    
    [Fact]
    public async Task BuildComplete_Should_Post_Failed_Status_When_No_Ports_Available()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();
        
        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());
        
        _options.Value.AzureDevOps.SupportedBuildDefinitions =
        [
            new SupportedBuildDefinition
            {
                BuildDefinitionId = buildComplete.BuildDefinitionId,
                AllowedImagePorts = [ 7000 ]
            }
        ];
        
        List<PullRequestStatusMessage> messages = [];

        _azureDevOpsService
            .PostPullRequestStatusAsync(Arg.Any<PullRequestStatusMessage>())
            .Returns(Task.CompletedTask)
            .AndDoes(x => messages.Add(x.Arg<PullRequestStatusMessage>()));
        
        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.BuildDefinitionId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = string.Empty,
                ImageTag = string.Empty,
                Port = x.ArgAt<int>(3),
                BuildDefinitionId = buildComplete.BuildDefinitionId,
            });
        
        // Act
        await _sut.BuildCompleteAsync(buildComplete);
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _dockerService
            .Received(1)
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.BuildDefinitionId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>());

        messages.Should().HaveCount(4);

        PullRequestStatusMessage failedMessage = messages[3];

        using (new AssertionScope())
        {
            failedMessage.PullRequestNumber.Should().Be(buildComplete.PullRequestNumber);
            failedMessage.BuildPipelineAddress.Should().Be(buildComplete.BuildUrl.ToString());
            failedMessage.State.Should().Be(PullRequestStatusState.Failed);
        }
    }

    [Fact]
    public async Task BuildComplete_Should_Start_Container_When_Existing_Container_Is_Not_Found()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();
        
        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());
        
        _options.Value.AzureDevOps.SupportedBuildDefinitions =
        [
            new SupportedBuildDefinition
            {
                BuildDefinitionId = buildComplete.BuildDefinitionId
            }
        ];

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _dockerService
            .Received(1)
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.BuildDefinitionId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>());
    }
    
    [Fact]
    public async Task BuildComplete_Should_Restart_Container_When_Existing_Container_Is_Found()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();
        
        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());
        
        _options.Value.AzureDevOps.SupportedBuildDefinitions =
        [
            new SupportedBuildDefinition
            {
                BuildDefinitionId = buildComplete.BuildDefinitionId,
                ImageName = "test-image",
                DockerRegistry = "docker.io"
            }
        ];
        
        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.BuildDefinitionId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = $"{x.ArgAt<string>(4)}/{x.ArgAt<string>(0)}",
                ImageTag = x.ArgAt<string>(1),
                BuildDefinitionId = x.ArgAt<int>(2),
                Port = x.ArgAt<int>(3),
            });

        // Act
        await _sut.BuildCompleteAsync(buildComplete);
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _dockerService
            .Received(1)
            .RestartContainerAsync(
                Arg.Any<DockerContainer>(),
                Arg.Any<int>(),
                cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BuildComplete_Should_Correct_Environment_Address_When_Container_Started_Successfully()
    {
        // Arrange
        BuildComplete buildComplete = GetValidBuildComplete();
        
        _validator
            .Validate(Arg.Any<ApplicationConfiguration>())
            .Returns(new ValidationResult());
        
        _options.Value.AzureDevOps.SupportedBuildDefinitions =
        [
            new SupportedBuildDefinition
            {
                BuildDefinitionId = buildComplete.BuildDefinitionId,
                ImageName = "test-image",
                DockerRegistry = "docker.io"
            }
        ];

        int port = 0;
        
        _dockerService
            .RunContainerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                buildComplete.BuildDefinitionId,
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<int>())
            .Returns(x => new DockerContainer
            {
                ContainerId = string.Empty,
                ImageName = $"{x.ArgAt<string>(4)}/{x.ArgAt<string>(0)}",
                ImageTag = x.ArgAt<string>(1)
            })
            .AndDoes(x => port = x.ArgAt<int>(3));

        PreviewAvailableMessage? message = null;

        _azureDevOpsService
            .PostPreviewAvailableMessageAsync(Arg.Any<PreviewAvailableMessage>())
            .Returns(Task.CompletedTask)
            .AndDoes(x => message = x.Arg<PreviewAvailableMessage>());

        // Act
        await _sut.BuildCompleteAsync(buildComplete);

        // Assert
        await _azureDevOpsService
            .Received(1)
            .PostPreviewAvailableMessageAsync(Arg.Any<PreviewAvailableMessage>());
        
        string expectedAddress =
            $"{_options.Value.Scheme}://{_options.Value.Host}:{port}";
        
        message.Should().NotBeNull();

        using (new AssertionScope())
        {
            message!.PullRequestNumber.Should().Be(buildComplete.PullRequestNumber);
            message.PreviewEnvironmentAddress.Should().Be(expectedAddress);
        }
    }

    private static BuildComplete GetValidBuildComplete()
    {
        return new BuildComplete
        {
            SourceBranch = "refs/pull/1",
            BuildDefinitionId = 1,
            BuildStatus = BuildStatus.Succeeded,
            BuildUrl = new Uri("https://dev.azure.com"),
            PullRequestNumber = 1
        };
    }
}