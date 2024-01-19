using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace PreviewEnvironments.Application.Services;

internal sealed partial class DockerService
{
    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Debug, "Pulled image '{ImageName}' from registry '{ImageRegistry}'. Version '{ImageTag}'.", EventName = nameof(PulledDockerImage))]
        public static partial void PulledDockerImage(ILogger logger, string imageRegistry, string imageName, string imageTag);
        
        [LoggerMessage(2, LogLevel.Debug, "Attempting to run container with the following arguments. Image Registry: '{ImageRegistry}', Image Name: '{ImageName}', Image Tag: '{ImageTag}', Public Port: '{PublicPort}'.", EventName = nameof(AttemptingToRunContainer))]
        public static partial void AttemptingToRunContainer(ILogger logger, string imageRegistry, string imageName, string imageTag, int publicPort);
        
        [LoggerMessage(3, LogLevel.Debug, "No build definition with id {BuildDefinitionId} was found.", EventName = nameof(BuildDefinitionNotFound))]
        public static partial void BuildDefinitionNotFound(ILogger logger, int buildDefinitionId);
        
        [LoggerMessage(4, LogLevel.Debug, "Restarting container with the following arguments. Image Registry: '{ImageRegistry}', Image Name: '{ImageName}', Image Tag: '{ImageTag}', Public Port: '{PublicPort}'.", EventName = nameof(RestartingContainer))]
        public static partial void RestartingContainer(ILogger logger, string imageRegistry, string imageName, string imageTag, int publicPort);
        
        [LoggerMessage(5, LogLevel.Information, "Container not stopped. Not attempting to remove container. Container id: {ContainerId}.", EventName = nameof(ContainerNotStopped))]
        public static partial void ContainerNotStopped(ILogger logger, string containerId);
        
        [LoggerMessage(6, LogLevel.Error, "An error occurred when trying to remove a container. Container id: {ContainerId}.", EventName = nameof(ErrorRemovingContainer))]
        public static partial void ErrorRemovingContainer(ILogger logger, Exception exception, string containerId);
        
        [LoggerMessage(7, LogLevel.Debug, "Attempting to stop container. Container id: {ContainerId}.", EventName = nameof(AttemptingToStopContainer))]
        public static partial void AttemptingToStopContainer(ILogger logger, string containerId);
        
        [LoggerMessage(8, LogLevel.Debug, "No container found. Container id: {ContainerId}.", EventName = nameof(ContainerNotFound))]
        public static partial void ContainerNotFound(ILogger logger, string containerId);
        
        [LoggerMessage(9, LogLevel.Information, "Stopped container. Container id: {ContainerId}.", EventName = nameof(ContainerStopped))]
        public static partial void ContainerStopped(ILogger logger, string containerId);
        
        [LoggerMessage(10, LogLevel.Information, "Failed to stop container. Container id: {ContainerId}.", EventName = nameof(ErrorStoppingContainer))]
        public static partial void ErrorStoppingContainer(ILogger logger, string containerId);
        
        [LoggerMessage(11, LogLevel.Debug, "{TimeNano} [{Id}] Progress: {Progress}, Error message: {ErrorMessage}.", EventName = nameof(DockerProgressChanged))]
        public static partial void DockerProgressChanged(ILogger logger, long timeNano, string id, JSONProgress progress, string errorMessage);

        [LoggerMessage(12, LogLevel.Debug, "Attempting to pull an image with the following arguments. Image Registry: '{ImageRegistry}', Image Name: '{ImageName}', Image Tag: '{ImageTag}'.", EventName = nameof(AttemptingToPullDockerImage))]
        public static partial void AttemptingToPullDockerImage(ILogger logger, string imageRegistry, string imageName, string imageTag);
        
        [LoggerMessage(13, LogLevel.Debug, "Attempting to create a container with the following arguments. Image Registry: '{ImageRegistry}', Image Name: '{ImageName}', Image Tag: '{ImageTag}', Public port: {PublicPort}, Container name: {ContainerName}.", EventName = nameof(AttemptingToCreateContainer))]
        public static partial void AttemptingToCreateContainer(ILogger logger, string? imageRegistry, string imageName, string imageTag, int publicPort, string containerName);
        
        [LoggerMessage(14, LogLevel.Debug, "Create container attempt {Attempt}.", EventName = nameof(CreateContainerAttempt))]
        public static partial void CreateContainerAttempt(ILogger logger, int attempt);
        
        [LoggerMessage(15, LogLevel.Debug, "Could not start a container due to a conflict.", EventName = nameof(ErrorCreatingContainerConflict))]
        public static partial void ErrorCreatingContainerConflict(ILogger logger, Exception ex);
        
        [LoggerMessage(16, LogLevel.Debug, "Attempting to stop and remove conflicting container.", EventName = nameof(AttemptingToStopAndRemoveConflictContainer))]
        public static partial void AttemptingToStopAndRemoveConflictContainer(ILogger logger);
        
        [LoggerMessage(17, LogLevel.Information, "Created container. Image Registry: '{ImageRegistry}', Image Name: '{ImageName}', Image Tag: '{ImageTag}', Public port: {PublicPort}, Container name: {ContainerName}.", EventName = nameof(CreatedContainer))]
        public static partial void CreatedContainer(ILogger logger, string? imageRegistry, string imageName, string imageTag, int publicPort, string containerName);
        
        [LoggerMessage(18, LogLevel.Information, "Container started. Container id: {ContainerId}.", EventName = nameof(ContainerStarted))]
        public static partial void ContainerStarted(ILogger logger, string containerId);
        
        [LoggerMessage(19, LogLevel.Information, "Container not started. Container id: {ContainerId}.", EventName = nameof(ContainerNotStarted))]
        public static partial void ContainerNotStarted(ILogger logger, string containerId);
        
        [LoggerMessage(20, LogLevel.Debug, "Attempting to remove image. Image: '{ImageName}'.", EventName = nameof(AttemptingToRemoveImage))]
        public static partial void AttemptingToRemoveImage(ILogger logger, string imageName);
        
        [LoggerMessage(21, LogLevel.Information, "Successfully removed image. Image: '{ImageName}'.", EventName = nameof(ImageRemoved))]
        public static partial void ImageRemoved(ILogger logger, string imageName);
        
        [LoggerMessage(22, LogLevel.Debug, "Attempting to remove container and volumes. Container id: {ContainerId}.", EventName = nameof(AttemptingToRemoveContainer))]
        public static partial void AttemptingToRemoveContainer(ILogger logger, string containerId);
        
        [LoggerMessage(23, LogLevel.Information, "Removed container and volumes. Container id: {ContainerId}.", EventName = nameof(RemovedContainer))]
        public static partial void RemovedContainer(ILogger logger, string containerId);
    }
}