﻿using Axis.Luna.Common.StringEscape;
using Axis.Luna.Extensions;
using Axis.Pulsar.Core.Grammar.Atomic;
using Axis.Pulsar.Core.Utils;
using Axis.Pulsar.Core.XBNF.Lang;
using System.Collections.Immutable;
using System.Globalization;
using static Axis.Pulsar.Core.XBNF.IAtomicRuleFactory;

namespace Axis.Pulsar.Core.XBNF;

/// <summary>
/// SEQUENCES - Syntax
/// <code>
/// abcd, ^efgh ,\n\n\^xyz, \^\,\s
/// </code>
/// <list type="number">
/// <item>Whitespaces are ignored</item>
/// <item>Commas signify the start of another sequence</item>
/// <item>Special Escaped characters include: "'", ",", "\", "^", " ". Single space is escaped with "\s".</item>
/// <item>Regular Escaped characters are recognized: "\n", "\t", "\b", etc</item>
/// <item>Utf escape is recognized for all characters: \uffff</item>
/// <item> </item>
/// <item> </item>
/// </list>
/// <para/>
/// RANGES - Syntax
/// <para/>
/// See <see cref="CharRangeRuleFactory"/>
/// <para/>
/// Start/End/Escaped-End Delimiters
/// <para/>
/// These are expected to be unicode-escaped string literals - meaning 
/// </summary>
public class DelimitedStringRuleFactory : IAtomicRuleFactory
{
    private static readonly CommonStringEscaper Escaper = new CommonStringEscaper();

    #region Arguments

    /// <summary>
    /// Character ranges - includes and excludes
    /// </summary>
    public static IArgument RangesArgument => IArgument.Of("ranges");

    /// <summary>
    /// Character sequences - includes and excludes
    /// </summary>
    public static IArgument SequencesArgument => IArgument.Of("sequences");

    /// <summary>
    /// Start delimiter argument.
    /// <para/>
    /// String containing escape-sequences. The factory is responsible for unescaping the sequences so the raw string
    /// is passed into the <see cref="DelimitedString"/> instance.
    /// </summary>
    public static IArgument StartDelimArgument => IArgument.Of("start");

    /// <summary>
    /// End delimiter argument
    /// <para/>
    /// String containing escape-sequences. The factory is responsible for unescaping the sequences so the raw string
    /// is passed into the <see cref="DelimitedString"/> instance.
    /// </summary>
    public static IArgument EndDelimArgument => IArgument.Of("end");

    /// <summary>
    /// Accepts empty argument
    /// </summary>
    public static IArgument AcceptsEmptyArgument => IArgument.Of("accepts-empty");

    /// <summary>
    /// The optoinal escaped version of the end delimiter
    /// </summary>
    public static IArgument EscapedEndDelimiterArgument => IArgument.Of("escaped-delimiter");

    #endregion

    public IAtomicRule NewRule(
        string ruleId,
        LanguageMetadata context,
        ImmutableDictionary<IArgument, string> arguments)
    {
        ValidateArgs(arguments);

        var ranges = arguments.TryGetValue(RangesArgument, out var xvalue)
            ? CharRangeRuleFactory.ParseRanges(xvalue)
            : (Includes: Enumerable.Empty<CharRange>(), Excludes: Enumerable.Empty<CharRange>());

        var sequences = arguments.TryGetValue(SequencesArgument, out var yvalue)
            ? ParseSequences(yvalue)
            : (Includes: Enumerable.Empty<Tokens>(), Excludes: Enumerable.Empty<Tokens>());

        var delimiters = ParseDelimiters(arguments);
        var acceptsEmpty = ParseAcceptsEmpty(arguments);
        var escapedDelimiter = arguments.TryGetValue(EscapedEndDelimiterArgument, out var value)
            ? Tokens.Of(value)
            : Tokens.Default;
        var unescapedStartDelimiter = Escaper.UnescapeString(delimiters.Start);
        var unescapedEndDelimiter = Escaper.UnescapeString(delimiters.End!);

        return DelimitedString.Of(
            ruleId,
            acceptsEmpty,
            unescapedStartDelimiter,
            unescapedEndDelimiter,
            sequences.Includes,
            sequences.Excludes,
            ranges.Includes,
            ranges.Excludes,
            escapedDelimiter);
    }

    private static void ValidateArgs(ImmutableDictionary<IArgument, string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        if (!arguments.ContainsKey(StartDelimArgument))
            throw new ArgumentException($"Invalid arguments: '{StartDelimArgument}' is missing");
    }

    internal static (string Start, string? End) ParseDelimiters(ImmutableDictionary<IArgument, string> arguments)
    {
        var start = arguments[StartDelimArgument] ?? throw new FormatException("Invalid start delimiter: null");
        var end = arguments.TryGetValue(EndDelimArgument, out var delim)
            ? delim
            : null!;

        return (start, end);
    }

    internal static bool ParseAcceptsEmpty(ImmutableDictionary<IArgument, string> arguments)
    {
        return !arguments.TryGetValue(AcceptsEmptyArgument, out var value) || bool.Parse(value);
    }

    internal static (IEnumerable<Tokens> Includes, IEnumerable<Tokens> Excludes) ParseSequences(string sequences)
    {
        var primedSequences = sequences
            .Replace("\\,", "\\u002c")
            .Split(',')
            .Select(part => part.Trim())
            .Select(range => range.Replace("\\u002c", ","))
            .ThrowIfAny(
                string.Empty.Equals,
                _ => new FormatException($"Invalid range: {sequences}"));

        return (
            primedSequences
                .Where(sequence => !'^'.Equals(sequence[0]))
                .Select(SequencesEscapeTransformer.Unescape)
                .Select(sequence => Tokens.Of(sequence))
                .ToArray(),
            primedSequences
                .Where(sequence => '^'.Equals(sequence[0]))
                .Select(SequencesEscapeTransformer.Unescape)
                .Select(sequence => Tokens.Of(sequence[1..]))
                .ToArray());
    }

    #region Nested Types

    /// <summary>
    /// Escapes and Unescapes regular ASCII escape characters, with the addition of escaping the following characters:
    /// <list type="number">
    /// <item> ' </item>
    /// <item> ^ </item>
    /// <item> , </item>
    /// <item> \ </item>
    /// <item> new-line </item>
    /// <item> carriage-return </item>
    /// <item> space </item>
    /// </list>
    /// </summary>
    internal static class SequencesEscapeTransformer
    {
        private readonly static ImmutableHashSet<char> EscapeArgs = ImmutableHashSet.Create('\'', '\\', '^', ',', ' ');
       
        private readonly static ImmutableHashSet<int> UnprintableAsciiCharCodes = ImmutableHashSet.Create(
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
            31, 129, 143, 144, 157, 160, 173);

        internal static string Unescape(string escapedString)
        {
            if (escapedString is null)
                return escapedString!;

            var substrings = new List<Tokens>();
            var offset = 0;

            do
            {
                var newOffset = escapedString.IndexOf("\\", offset);

                if (newOffset < 0)
                    substrings.Add(Tokens.Of(escapedString, offset));

                else 
                {
                    if (newOffset > offset)
                        substrings.Add(Tokens.Of(escapedString, offset, newOffset - offset));

                    // read the escape sequence
                    var argChar = escapedString[newOffset + 1];
                    Tokens replacement;

                    if (EscapeArgs.Contains(argChar))
                        replacement = Tokens.Of(argChar.ToString());

                    else if ('n'.Equals(argChar))
                        replacement = Tokens.Of("\n");

                    else if ('r'.Equals(argChar))
                        replacement = Tokens.Of("\r");

                    else if ('s'.Equals(argChar))
                        replacement = Tokens.Of(" ");

                    else if ('u'.Equals(argChar))
                        replacement = short
                            .Parse(
                                escapedString.AsSpan(newOffset + 2, 4),
                                NumberStyles.HexNumber)
                            .ApplyTo(code => (char)code)
                            .ApplyTo(@char => Tokens.Of(@char.ToString()));

                    else throw new InvalidOperationException($"Invalid escape arg charactre: {argChar}");

                    substrings.Add(replacement);
                    offset = newOffset + argChar switch
                    {
                        '\'' or '\\'
                        or '^' or ','
                        or 'n' or 'r' or 's' => 2,

                        'u' or _ => 6
                    };
                }
            }
            while (offset < escapedString.Length);

            return substrings
                .Select(s => s.ToString())
                .JoinUsing("");
        }

        internal static string Escape(string rawString)
        {
            if (rawString is null)
                return rawString!;

            var substrings = new List<Tokens>();
            var offset = 0;
            for (int index = 0; index < rawString.Length; index++)
            {
                if (EscapeArgs.Contains(rawString[index]))
                {
                    var prev = Tokens.Of(rawString, offset, index - offset);
                    if (!prev.IsEmpty)
                        substrings.Add(prev);

                    offset = index + 1;
                    var escapeArg = rawString[index] switch
                    {
                        '\n' => 'n',
                        '\r' => 'r',
                        ' ' => 's',
                        char c => c
                    };
                    substrings.Add(Tokens.Of($"\\{escapeArg}"));
                }
                else if (UnprintableAsciiCharCodes.Contains(rawString[index])
                    || rawString[index] > 255)
                {
                    var prev = Tokens.Of(rawString, offset, index - offset);
                    if (!prev.IsEmpty)
                        substrings.Add(prev);

                    offset = index + 1;
                    substrings.Add(Tokens.Of($"\\u{(int)rawString[index]:x4}"));
                }
            }

            return substrings
                .Select(s => s.ToString())
                .JoinUsing("");
        }
    }
    #endregion
}
