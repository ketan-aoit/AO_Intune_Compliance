using System.Text.RegularExpressions;
using AOIntuneAlerts.Domain.Common;

namespace AOIntuneAlerts.Domain.ValueObjects;

public partial class SemVer : ValueObject, IComparable<SemVer>
{
    public int Major { get; private set; }
    public int Minor { get; private set; }
    public int Patch { get; private set; }
    public string? PreRelease { get; private set; }
    public string? Build { get; private set; }

    // Required by EF Core for owned entity materialization
    private SemVer() { }

    private SemVer(int major, int minor, int patch, string? preRelease = null, string? build = null)
    {
        if (major < 0) throw new ArgumentException("Major version cannot be negative", nameof(major));
        if (minor < 0) throw new ArgumentException("Minor version cannot be negative", nameof(minor));
        if (patch < 0) throw new ArgumentException("Patch version cannot be negative", nameof(patch));

        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
        Build = build;
    }

    public static SemVer Create(int major, int minor = 0, int patch = 0, string? preRelease = null, string? build = null)
    {
        return new SemVer(major, minor, patch, preRelease, build);
    }

    public static SemVer? TryParse(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return null;

        var match = VersionRegex().Match(version.Trim());
        if (!match.Success)
            return null;

        var major = int.Parse(match.Groups["major"].Value);
        var minor = match.Groups["minor"].Success ? int.Parse(match.Groups["minor"].Value) : 0;
        var patch = match.Groups["patch"].Success ? int.Parse(match.Groups["patch"].Value) : 0;
        var preRelease = match.Groups["prerelease"].Success ? match.Groups["prerelease"].Value : null;

        // Handle Windows-style versions (X.Y.Z.W) - store revision in Build property
        var build = match.Groups["build"].Success ? match.Groups["build"].Value : null;
        if (match.Groups["revision"].Success)
        {
            // For Windows versions, the 4th part is the revision
            build = match.Groups["revision"].Value;
        }

        return new SemVer(major, minor, patch, preRelease, build);
    }

    public static SemVer Parse(string version)
    {
        return TryParse(version) ?? throw new FormatException($"Invalid version format: {version}");
    }

    public int CompareTo(SemVer? other)
    {
        if (other is null)
            return 1;

        var majorCompare = Major.CompareTo(other.Major);
        if (majorCompare != 0) return majorCompare;

        var minorCompare = Minor.CompareTo(other.Minor);
        if (minorCompare != 0) return minorCompare;

        var patchCompare = Patch.CompareTo(other.Patch);
        if (patchCompare != 0) return patchCompare;

        // Pre-release versions have lower precedence
        if (PreRelease is null && other.PreRelease is not null)
            return 1;
        if (PreRelease is not null && other.PreRelease is null)
            return -1;
        if (PreRelease is not null && other.PreRelease is not null)
            return string.Compare(PreRelease, other.PreRelease, StringComparison.Ordinal);

        return 0;
    }

    public static bool operator <(SemVer left, SemVer right) => left.CompareTo(right) < 0;
    public static bool operator >(SemVer left, SemVer right) => left.CompareTo(right) > 0;
    public static bool operator <=(SemVer left, SemVer right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SemVer left, SemVer right) => left.CompareTo(right) >= 0;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Major;
        yield return Minor;
        yield return Patch;
        yield return PreRelease;
        yield return Build;
    }

    public override string ToString()
    {
        var version = $"{Major}.{Minor}.{Patch}";
        if (!string.IsNullOrEmpty(PreRelease))
            version += $"-{PreRelease}";
        if (!string.IsNullOrEmpty(Build))
            version += $"+{Build}";
        return version;
    }

    // Regex handles both SemVer (X.Y.Z) and Windows-style versions (X.Y.Z.W)
    // For Windows versions like 10.0.22621.1234, we treat the third part as the build/patch
    [GeneratedRegex(@"^(?<major>\d+)(\.(?<minor>\d+))?(\.(?<patch>\d+))?(\.(?<revision>\d+))?(-(?<prerelease>[0-9A-Za-z\-\.]+))?(\+(?<build>[0-9A-Za-z\-\.]+))?$")]
    private static partial Regex VersionRegex();
}
