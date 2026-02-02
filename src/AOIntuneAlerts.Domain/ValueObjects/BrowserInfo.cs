using AOIntuneAlerts.Domain.Common;
using AOIntuneAlerts.Domain.Enums;

namespace AOIntuneAlerts.Domain.ValueObjects;

public class BrowserInfo : ValueObject
{
    public BrowserType Type { get; private set; }
    public string Name { get; private set; }
    public SemVer Version { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value - Required by EF Core
    private BrowserInfo() { }
#pragma warning restore CS8618

    private BrowserInfo(BrowserType type, string name, SemVer version)
    {
        Type = type;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Version = version ?? throw new ArgumentNullException(nameof(version));
    }

    public static BrowserInfo Create(BrowserType type, string name, SemVer version)
    {
        return new BrowserInfo(type, name, version);
    }

    public static BrowserInfo CreateFromString(string browserString)
    {
        if (string.IsNullOrWhiteSpace(browserString))
            return Create(BrowserType.Unknown, "Unknown", SemVer.Create(0));

        var normalized = browserString.ToLowerInvariant();

        if (normalized.Contains("edge") || normalized.Contains("edg/"))
            return ParseBrowser(BrowserType.Edge, "Microsoft Edge", browserString);
        if (normalized.Contains("chrome") && !normalized.Contains("edge"))
            return ParseBrowser(BrowserType.Chrome, "Google Chrome", browserString);
        if (normalized.Contains("firefox"))
            return ParseBrowser(BrowserType.Firefox, "Mozilla Firefox", browserString);
        if (normalized.Contains("safari") && !normalized.Contains("chrome"))
            return ParseBrowser(BrowserType.Safari, "Safari", browserString);

        return Create(BrowserType.Unknown, browserString, SemVer.Create(0));
    }

    private static BrowserInfo ParseBrowser(BrowserType type, string name, string browserString)
    {
        var version = ExtractVersion(browserString) ?? SemVer.Create(0);
        return Create(type, name, version);
    }

    private static SemVer? ExtractVersion(string input)
    {
        var parts = input.Split(['/', ' ', '-', '_', '(', ')'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var version = SemVer.TryParse(part);
            if (version is not null)
                return version;
        }
        return null;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Name;
        yield return Version;
    }

    public override string ToString()
    {
        return $"{Name} {Version}";
    }
}
