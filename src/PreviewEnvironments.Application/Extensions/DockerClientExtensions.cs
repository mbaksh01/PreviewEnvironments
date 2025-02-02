﻿using Docker.DotNet;
using Docker.DotNet.Models;

namespace PreviewEnvironments.Application.Extensions;

internal static class DockerClientExtensions
{
    public static async Task<ContainerListResponse?> GetContainerById(
        this IDockerClient dockerClient,
        string containerId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(containerId))
        {
            throw new ArgumentException(
                $"{nameof(containerId)} can not be null or whitespace.",
                nameof(containerId));
        }

        IList<ContainerListResponse> containers = await dockerClient
            .Containers
            .ListContainersAsync(new ContainersListParameters
            {
                All = true,
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    ["id"] = new Dictionary<string, bool>
                    {
                        [containerId] = true,
                    }
                }
            }, cancellationToken);

        return containers.SingleOrDefault();
    }
    
    public static async Task<ContainerListResponse?> GetContainerByName(
        this IDockerClient dockerClient,
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                $"{nameof(name)} can not be null or whitespace.",
                nameof(name));
        }

        IList<ContainerListResponse> containers = await dockerClient
            .Containers
            .ListContainersAsync(new ContainersListParameters
            {
                All = true,
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    ["name"] = new Dictionary<string, bool>
                    {
                        [name] = true,
                    }
                }
            }, cancellationToken);

        return containers.SingleOrDefault();
    }
}