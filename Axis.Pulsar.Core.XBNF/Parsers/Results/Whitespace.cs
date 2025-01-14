﻿using Axis.Luna.Extensions;
using Axis.Pulsar.Core.Utils;
using System.Collections.Immutable;

namespace Axis.Pulsar.Core.XBNF;

internal class Whitespace :
    ISilentElement,
    IEquatable<Whitespace>
{
    private static readonly ImmutableHashSet<char> _WhitespaceChars = Enum
        .GetValues<WhitespaceChar>()
        .Select(c => (char)c)
        .ToImmutableHashSet();

    public WhitespaceChar Char => (WhitespaceChar)Content[0];

    public Tokens Content { get; }

    public Whitespace(Tokens whitespaceToken)
    {
        Content = whitespaceToken
            .ThrowIf(
                t => t.Segment.Count != 1,
                _ => new ArgumentException($"Invalid token: {whitespaceToken}"))
            .ThrowIfNot(
                t => _WhitespaceChars.Contains(t[0]),
                _ => new ArgumentException($"Invalid whitespace character: {whitespaceToken}"));
    }

    public static Whitespace Of(Tokens whitespaceToken) => new(whitespaceToken);

    public static implicit operator Whitespace(Tokens whitespaceToken) => new(whitespaceToken);

    public override string ToString() => Content.ToString()!;

    public override int GetHashCode() => HashCode.Combine(Char);

    public override bool Equals(object? obj)
    {
        return obj is Whitespace other && Equals(other);
    }

    public bool Equals(Whitespace? other)
    {
        return other is not null
            && other.Char == Char;
    }

    public static bool operator ==(Whitespace first, Whitespace second)
    {
        return
            first is null && second is null ? true :
            first is null ^ second is null ? false :
            first!.Equals(second);
    }

    public static bool operator !=(Whitespace first, Whitespace second)
    {
        return !(first == second);
    }

    #region Nested types
    public enum WhitespaceChar
    {
        Space = ' ',

        Tab = '\t',

        LineFeed = '\n',

        CarriageReturn = '\r'
    }
    #endregion
}
