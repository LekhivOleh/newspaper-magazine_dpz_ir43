namespace CURSOVA.Domain.Common;

public static class Guard
{
    public static string NotEmpty(string? value, string name, int maxLength = int.MaxValue)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{name} не може бути порожнім.");

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainException($"{name} не може бути довшим за {maxLength} символів.");

        return trimmed;
    }

    public static string? Optional(string? value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return NotEmpty(value, name, maxLength);
    }
}
