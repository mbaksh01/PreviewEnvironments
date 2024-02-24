using PreviewEnvironments.Application.Models.Docker;

namespace PreviewEnvironments.Application.Services.Abstractions;

internal interface IContainerTracker
{
    ICollection<string> GetKeys();
    
    void Add(string key, DockerContainer container);
    
    DockerContainer? Remove(string key);
    
    DockerContainer? SingleOrDefault(Predicate<DockerContainer> predicate);
    
    IEnumerable<DockerContainer> Where(Predicate<DockerContainer> predicate);
}