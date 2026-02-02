using System.Text.RegularExpressions;
using AOIntuneAlerts.Domain.Common;

namespace AOIntuneAlerts.Domain.ValueObjects;

public partial class EmailAddress : ValueObject
{
    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email address cannot be empty", nameof(email));

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalizedEmail))
            throw new ArgumentException($"Invalid email format: {email}", nameof(email));

        return new EmailAddress(normalizedEmail);
    }

    public static EmailAddress? TryCreate(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalizedEmail))
            return null;

        return new EmailAddress(normalizedEmail);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(EmailAddress email) => email.Value;

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
}
