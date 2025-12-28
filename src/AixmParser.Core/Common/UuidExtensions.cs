namespace AixmParser.Core.Common;

/// <summary>
/// Provides extension methods for UUID normalization and extraction.
/// </summary>
internal static class UuidExtensions
{
    /// <summary>
    /// Normalizes a UUID string by removing common prefixes.
    /// </summary>
    /// <param name="uuid">The UUID string to normalize.</param>
    /// <returns>The normalized UUID without prefixes.</returns>
    public static string? NormalizeUuid(string? uuid)
    {
        if (string.IsNullOrEmpty(uuid))
            return uuid;

        // Remove leading # (used in xlink:href references)
        if (uuid.StartsWith("#"))
            uuid = uuid.Substring(1);

        // Remove common prefixes
        if (uuid.StartsWith("urn:uuid:", StringComparison.OrdinalIgnoreCase))
            return uuid.Substring(9);

        if (uuid.StartsWith("urn.uuid.", StringComparison.OrdinalIgnoreCase))
            return uuid.Substring(9);

        if (uuid.StartsWith("uuid.", StringComparison.OrdinalIgnoreCase))
            return uuid.Substring(5);

        return uuid;
    }
}
