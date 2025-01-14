namespace Axis.Pulsar.Core.Utils;

using Axis.Luna.Common;
using Axis.Luna.Extensions;
using System.Diagnostics.CodeAnalysis;


/// <summary>
/// Represents a range of characters as an inclusive lower and uppoer bound.
/// </summary>
public readonly struct CharRange:
    IDefaultValueProvider<CharRange>,
    IEquatable<CharRange>
{
    public readonly char LowerBound { get; }

    public readonly char UpperBound { get; }

    public bool IsRange => LowerBound != UpperBound;

    public bool IsDefault => LowerBound == '\0' && UpperBound == '\0';

    public static CharRange Default => default;

    public CharRange(char lowerBound, char upperBound)
    {
        LowerBound = lowerBound;
        UpperBound = upperBound;

        if (lowerBound > upperBound)
            throw new ArgumentException(
                $"{nameof(lowerBound)} character must be less thatn {nameof(upperBound)} character");
    }

    public static CharRange Of(char lowerBound, char upperBound) => new(lowerBound, upperBound);

    public static CharRange Of(char @char) => new(@char, @char);

    public static implicit operator CharRange(string input) => Parse(input);
    public static implicit operator CharRange(char @char) => CharRange.Of(@char);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="range"></param>
    /// <param name="mergeDisjointedRanges"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public CharRange MergeWith(CharRange range, bool mergeDisjointedRanges = false)
    {
        if (Intersects(this, range) || mergeDisjointedRanges)
        {
            return new CharRange(
                Min(LowerBound, range.LowerBound),
                Max(UpperBound, range.UpperBound));
        }

        else throw new InvalidOperationException($"Invalid merge: disjointed ranges.");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="range"></param>
    /// <param name="mergeDisjointedRanges"></param>
    /// <param name="merged"></param>
    /// <returns></returns>
    public bool TryMergeWith(
        CharRange range,
        bool mergeDisjointedRanges,
        out CharRange merged)
    {
        try
        {
            merged = MergeWith(range, mergeDisjointedRanges);
            return true;
        }
        catch
        {
            merged = default;
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="range"></param>
    /// <param name="merged"></param>
    /// <returns></returns>
    public bool TryMergeWith(
        CharRange range,
        out CharRange merged)
        => TryMergeWith(range, false, out merged);

    /// <summary>
    /// Checks if the given character falls within the range of characters represented by this instance.
    /// </summary>
    /// <param name="char">The character to test</param>
    /// <returns>True if the character is contained in this range, false otherwise.</returns>
    public bool Contains(char @char)
    {
        return LowerBound <= @char && UpperBound >= @char;
    }

    public override string ToString() => IsRange ? $"{LowerBound}-{UpperBound}" : LowerBound.ToString();

    public bool Equals(CharRange other)
    {
        return LowerBound == other.LowerBound
            && UpperBound == other.UpperBound;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is CharRange other && Equals(other);
    }

    public override int GetHashCode() => HashCode.Combine(UpperBound, LowerBound);

    public static bool operator ==(CharRange left, CharRange right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CharRange left, CharRange right)
    {
        return !(left == right);
    }

    private static char Min(char first, char second) => first < second ? first : second;

    private static char Max(char first, char second) => first > second ? first : second;

    #region Static Helpers
    /// <summary>
    /// Checks if the 2 given ranges intersect.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool Intersects(CharRange first, CharRange second)
    {
        return Extensions.Intersects(
            (first.LowerBound, first.UpperBound),
            (second.LowerBound, second.UpperBound));
    }

    /// <summary>
    /// Accepts a sting containing a 2-character representation separated by a dash. Whitespaces are
    /// allowed between these 3 elements.
    /// <para/>
    /// Recognized special characters that must appear escaped are: '-', ' ', '\n', '\r', '\t'. these
    /// must be represented using ascii or utf escaping. E.g, for '-', ascii: \x2d, utf: \u002d.
    /// </summary>
    /// <param name="input">The string containing character range</param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static CharRange Parse(string input)
    {
        return input
            .ThrowIf(
                string.IsNullOrWhiteSpace,
                _ => new ArgumentException($"Invalid input"))
            .Split('-', StringSplitOptions.RemoveEmptyEntries)
            .ApplyTo(bounds =>
            {
                if (bounds.Length == 1)
                    return CharRange.Of(ParseChar(bounds[0].Trim()));

                if (bounds.Length == 2)
                    return CharRange.Of(
                        ParseChar(bounds[0].Trim()),
                        ParseChar(bounds[1].Trim()));

                else throw new FormatException($"Invalid range format: {input}");
            });
    }

    /// <summary>
    /// Parses a string representation of a character. This string expects a string containing a single character,
    /// or a string containing Ascii or utf escaped characters.
    /// <para/>
    /// * Ascii escaping is represented as a 4-character string containing: '\', 'x', and a 2-digit hex number.
    /// <para/>
    /// * Utf escaping is represented as a 6-character string containing: '\', 'u', and a 4-digit hex number.
    /// <para/>
    /// * The only case where a 2-character length string is accepted is to escape the '\' character, i.e "\\". 
    /// All other characters should be represented using Ascii-Escaping, or Utf-Escaping.
    /// </summary>
    /// <param name="charString">The character string</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static char ParseChar(string charString)
    {
        var styles = System.Globalization.NumberStyles.HexNumber;

        if ((charString.Length == 4 && charString.StartsWith("\\x"))
            || (charString.Length == 6 && charString.StartsWith("\\u")))
        {
            if (ushort.TryParse(charString[2..], styles, null, out var num))
                return (char)num;

            else throw new FormatException($"Invalid character text: {charString}");
        }

        else if (charString.Equals("\\\\"))
            return charString[1];

        else if (charString.Length == 1)
            return charString[0];

        else throw new FormatException($"Invalid character text: {charString}");
    }

    /// <summary>
    /// Orders and then merges overlapping ranges.
    /// </summary>
    /// <param name="charRanges">The list of ranges</param>
    /// <returns>The normalized collection of ranges</returns>
    public static IEnumerable<CharRange> NormalizeRanges(IEnumerable<CharRange> charRanges)
    {
        return charRanges
            .ThrowIfNull(() => new ArgumentNullException(nameof(charRanges)))
            .OrderBy(range => range.LowerBound)
            .Aggregate(new List<CharRange>(), (list, range) =>
            {
                if (list.Count == 0)
                    list.Add(range);

                else
                {
                    var last = list[^1];

                    if (last.TryMergeWith(range, out var merged))
                        list[^1] = merged;

                    else list.Add(range);
                }

                return list;
            });
    }
    #endregion
}