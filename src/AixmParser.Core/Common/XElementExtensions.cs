using System.Xml.Linq;

namespace AixmParser.Core.Common;

/// <summary>
/// Provides extension methods for XElement to simplify AIXM parsing.
/// </summary>
internal static class XElementExtensions
{
    /// <summary>
    /// Extracts the UUID identifier from an AIXM element.
    /// Prefers gml:identifier over gml:id to ensure correct UUID extraction for cross-references.
    /// </summary>
    /// <param name="element">The XML element to extract the identifier from.</param>
    /// <returns>The normalized UUID string.</returns>
    public static string? ExtractIdentifier(this XElement element)
    {
        // Prefer gml:identifier element over gml:id attribute
        // The gml:identifier element contains the actual UUID used in xlink:href references
        var id = (string?)element.Element(Namespaces.Gml + "identifier")
                 ?? (string?)element.Elements().FirstOrDefault(e => e.Name.LocalName == "identifier")?.Value
                 ?? (string?)element.Attribute(Namespaces.Gml + "id");

        return UuidExtensions.NormalizeUuid(id);
    }

    /// <summary>
    /// Extracts a UUID reference from an xlink:href attribute.
    /// </summary>
    /// <param name="element">The XML element containing the xlink:href.</param>
    /// <returns>The normalized UUID from the href attribute.</returns>
    public static string? ExtractHrefUuid(this XElement? element)
    {
        var href = (string?)element?.Attribute(Namespaces.Xlink + "href");
        return UuidExtensions.NormalizeUuid(href);
    }

    /// <summary>
    /// Gets a double value from a child element.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="childName">The child element name.</param>
    /// <returns>The parsed double value or null.</returns>
    public static double? GetDoubleValue(this XElement? element, XName childName)
    {
        var valueStr = (string?)element?.Element(childName);
        if (double.TryParse(valueStr, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }
        return null;
    }

    /// <summary>
    /// Gets a string value from a child element.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="childName">The child element name.</param>
    /// <returns>The string value or null.</returns>
    public static string? GetStringValue(this XElement? element, XName childName)
    {
        return (string?)element?.Element(childName);
    }
}
