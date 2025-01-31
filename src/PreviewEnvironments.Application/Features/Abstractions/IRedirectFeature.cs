namespace PreviewEnvironments.Application.Features.Abstractions;

public interface IRedirectFeature
{
    /// <summary>
    /// Gets the redirect URI for the environment linked to the
    /// <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Id of environment.</param>
    /// <returns>
    /// A URI which navigates to the associated preview environment.
    /// <see langword="null"/> when no preview environment was found.
    /// </returns>
    Task<Uri?> GetRedirectUriAsync(string id);
}