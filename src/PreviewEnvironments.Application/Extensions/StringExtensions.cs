namespace PreviewEnvironments.Application.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Checks if the <paramref name="token"/> is null or whitespace. If it is
    /// the <paramref name="fallback"/> is returned otherwise the
    /// <paramref name="token"/> is returned.
    /// </summary>
    /// <param name="token">Token to validate.</param>
    /// <param name="fallback">
    /// Value returned when the token is null or whitespace.
    /// </param>
    /// <returns>
    /// The <paramref name="token"/> if it is not null or whitespace otherwise
    /// the <paramref name="fallback"/>.
    /// </returns>
    public static string? WithFallback(this string? token, string? fallback)
    {
        if (string.IsNullOrWhiteSpace(token) == false)
        {
            return token;
        }

        return fallback;
    }
}
