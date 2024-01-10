namespace PreviewEnvironments.Application.Extensions;

public static class StringExtensions
{
    public static string? WithFallback(this string? token, string? fallback)
    {
        if (string.IsNullOrWhiteSpace(token) == false)
        {
            return token;
        }

        return fallback;
    }
}
