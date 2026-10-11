namespace CURSOVA.Models;

public static class UkrainianPlural
{
    public static string Format(int count, string one, string few, string many)
    {
        var lastTwo = Math.Abs(count) % 100;
        var last = lastTwo % 10;

        var word = lastTwo is >= 11 and <= 14 ? many
            : last == 1 ? one
            : last is >= 2 and <= 4 ? few
            : many;

        return $"{count} {word}";
    }
}
