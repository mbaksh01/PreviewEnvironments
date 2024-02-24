using System.Collections.Concurrent;
using PreviewEnvironments.Application.Models.Docker;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Services;

internal sealed class ContainerTracker : IContainerTracker
{
    private readonly ConcurrentDictionary<string, DockerContainer> _containers = [];
    
    public ICollection<string> GetKeys()
    {
        return _containers.Keys;
    }
    
    public void Add(string key, DockerContainer container)
    {
        _ = _containers.TryAdd(key, container);
    }
    
    public DockerContainer? Remove(string key)
    {
        _ = _containers.TryRemove(key, out DockerContainer? container);

        return container;
    }

    public DockerContainer? SingleOrDefault(Predicate<DockerContainer> predicate)
    {
        return _containers.Values.SingleOrDefault(predicate.Invoke);
    }
    
    public IEnumerable<DockerContainer> Where(Predicate<DockerContainer> predicate)
    {
        return _containers.Values.Where(predicate.Invoke);
    }
}