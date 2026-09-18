using System.Security.Cryptography;

namespace Ruptura.Infrastructure.Services;

/// <summary>
/// Generates the throwaway password a GM hands to a player during account recovery.
/// Uses a CSPRNG and skips look-alike characters (0/O, 1/l/I) because it is read and copied by hand.
/// </summary>
public static class TemporaryPasswordGenerator
{
    private const int Length = 12;
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "23456789";
    private const string All = Upper + Lower + Digits;

    public static string Generate()
    {
        // One of each required class first, so the Identity policy is always met…
        var chars = new List<char>(Length)
        {
            Pick(Upper),
            Pick(Lower),
            Pick(Digits)
        };

        while (chars.Count < Length)
            chars.Add(Pick(All));

        // …then Fisher–Yates so those guaranteed characters don't sit in fixed positions.
        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars.ToArray());
    }

    private static char Pick(string alphabet) => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
}
