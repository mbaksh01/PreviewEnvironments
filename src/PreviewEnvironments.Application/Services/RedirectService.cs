using System.Collections.Concurrent;
using PreviewEnvironments.Application.Services.Abstractions;

namespace PreviewEnvironments.Application.Services;

internal sealed class RedirectService : IRedirectService
{
    private static readonly ConcurrentDictionary<string, Uri> Redirects = new();

    /// <inheritdoc />
    public Uri Add(string key, Uri redirectUri, Uri hostUri)
    { 
        Redirects.TryAdd(key, redirectUri);

        Uri uri = new(hostUri, $"/environments/{key}");
        
        return uri;
    }

    /// <inheritdoc />
    public Uri? GetRedirectUri(string key)
    {
        Redirects.TryGetValue(key, out Uri? uri);
        return uri;
    }
}