using System.Xml.Linq;

namespace AixmParser.Core.Common;

/// <summary>
/// Provides XML namespace constants for AIXM parsing.
/// </summary>
internal static class Namespaces
{
    /// <summary>
    /// AIXM 5.1 namespace.
    /// </summary>
    public static readonly XNamespace Aixm = "http://www.aixm.aero/schema/5.1";

    /// <summary>
    /// GML (Geography Markup Language) namespace.
    /// </summary>
    public static readonly XNamespace Gml = "http://www.opengis.net/gml/3.2";

    /// <summary>
    /// XLink namespace.
    /// </summary>
    public static readonly XNamespace Xlink = "http://www.w3.org/1999/xlink";

    /// <summary>
    /// XSI (XML Schema Instance) namespace.
    /// </summary>
    public static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";
}
