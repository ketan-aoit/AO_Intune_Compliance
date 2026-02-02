using AOIntuneAlerts.Domain.Common;
using AOIntuneAlerts.Domain.Enums;

namespace AOIntuneAlerts.Domain.ValueObjects;

public class OperatingSystemInfo : ValueObject
{
    public OperatingSystemType Type { get; private set; }
    public string Name { get; private set; }
    public SemVer Version { get; private set; }
    public string? Edition { get; private set; }
    public string? BuildNumber { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value - Required by EF Core
    private OperatingSystemInfo() { }
#pragma warning restore CS8618

    private OperatingSystemInfo(
        OperatingSystemType type,
        string name,
        SemVer version,
        string? edition = null,
        string? buildNumber = null)
    {
        Type = type;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Edition = edition;
        BuildNumber = buildNumber;
    }

    public static OperatingSystemInfo Create(
        OperatingSystemType type,
        string name,
        SemVer version,
        string? edition = null,
        string? buildNumber = null)
    {
        return new OperatingSystemInfo(type, name, version, edition, buildNumber);
    }

    public static OperatingSystemInfo CreateFromString(string osString)
    {
        if (string.IsNullOrWhiteSpace(osString))
            return Create(OperatingSystemType.Unknown, "Unknown", SemVer.Create(0));

        var normalized = osString.ToLowerInvariant();

        if (normalized.Contains("windows"))
            return ParseWindowsOs(osString);
        if (normalized.Contains("macos") || normalized.Contains("mac os") || normalized.Contains("darwin"))
            return ParseMacOs(osString);
        if (normalized.Contains("ios") || normalized.Contains("iphone") || normalized.Contains("ipad"))
            return ParseIOs(osString);
        if (normalized.Contains("android"))
            return ParseAndroid(osString);
        if (normalized.Contains("linux") || normalized.Contains("ubuntu") || normalized.Contains("debian"))
            return ParseLinux(osString);

        return Create(OperatingSystemType.Unknown, osString, SemVer.Create(0));
    }

    private static OperatingSystemInfo ParseWindowsOs(string osString)
    {
        var version = ExtractVersion(osString) ?? SemVer.Create(10);
        string? edition = null;

        if (osString.Contains("Pro", StringComparison.OrdinalIgnoreCase))
            edition = "Pro";
        else if (osString.Contains("Enterprise", StringComparison.OrdinalIgnoreCase))
            edition = "Enterprise";
        else if (osString.Contains("Home", StringComparison.OrdinalIgnoreCase))
            edition = "Home";

        var name = version.Major >= 11 ? "Windows 11" : "Windows 10";
        return Create(OperatingSystemType.Windows, name, version, edition);
    }

    private static OperatingSystemInfo ParseMacOs(string osString)
    {
        var version = ExtractVersion(osString) ?? SemVer.Create(13);
        var name = version.Major switch
        {
            >= 15 => "macOS Sequoia",
            14 => "macOS Sonoma",
            13 => "macOS Ventura",
            12 => "macOS Monterey",
            11 => "macOS Big Sur",
            _ => "macOS"
        };
        return Create(OperatingSystemType.MacOS, name, version);
    }

    private static OperatingSystemInfo ParseIOs(string osString)
    {
        var version = ExtractVersion(osString) ?? SemVer.Create(17);
        return Create(OperatingSystemType.iOS, $"iOS {version.Major}", version);
    }

    private static OperatingSystemInfo ParseAndroid(string osString)
    {
        var version = ExtractVersion(osString) ?? SemVer.Create(14);
        return Create(OperatingSystemType.Android, $"Android {version.Major}", version);
    }

    private static OperatingSystemInfo ParseLinux(string osString)
    {
        var version = ExtractVersion(osString) ?? SemVer.Create(1);
        return Create(OperatingSystemType.Linux, "Linux", version);
    }

    private static SemVer? ExtractVersion(string input)
    {
        var parts = input.Split([' ', '-', '_', '(', ')'], StringSplitOptions.RemoveEmptyEntries);
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
        yield return Edition;
        yield return BuildNumber;
    }

    public override string ToString()
    {
        var result = Name;
        if (!string.IsNullOrEmpty(Edition))
            result += $" {Edition}";
        result += $" {Version}";
        if (!string.IsNullOrEmpty(BuildNumber))
            result += $" ({BuildNumber})";
        return result;
    }
}
