namespace PreviewEnvironments.Application.Services.Abstractions;

internal interface IRedirectService
{
    /// <summary>
    /// Adds a <see cref="Uri"/> mapping with a key.
    /// </summary>
    /// <param name="key">Key of mapping.</param>
    /// <param name="redirectUri">
    /// <see cref="Uri"/> to return when requesting the rediect
    /// <see cref="Uri"/> for <paramref name="key"/>.
    /// </param>
    /// <param name="hostUri">
    /// Base <see cref="Uri"/> of the returned <see cref="Uri"/>.
    /// </param>
    /// <returns>
    /// A new <see cref="Uri"/> with the environments path and id appended.
    /// </returns>
    Uri Add(string key, Uri redirectUri, Uri hostUri);
    
    /// <summary>
    /// Gets the <see cref="Uri"/> for the associated <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to look up.</param>
    /// <returns>
    /// <see cref="Uri"/> linked to <paramref name="key"/>.
    /// <see langword="null"/> when the <paramref name="key"/> was not found.
    /// </returns>
    Uri? GetRedirectUri(string key);
}