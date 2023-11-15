﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Axis.Luna.Common;
using Axis.Luna.Extensions;
using Axis.Pulsar.Core.Grammar;

namespace Axis.Pulsar.Core.XBNF;

/// <summary>
/// Implementations of this interfaces are charged with producing instances of the <see cref="IAtomicRule"/> interface,
/// given a list of parameters.
/// 
/// Syntax: <para/>
/// <code>
/// @name-form-with-params{param: 'value', param-2: 'value 2', flag-param}
/// @name-form-without-params
/// 
/// 'content form with params'{param: 'value', param-2: 'value 2'}
/// 'content form without params'
/// </code>
/// </summary>
public interface IAtomicRuleFactory
{
    #region Special Arguments

    /// <summary>
    /// When presented in "content" form, AtomicRules will be passed the content via an argument named "content"
    /// </summary>
    public static readonly Argument ContentArgument = "content";

    #endregion

    /// <summary>
    /// Creates a new <see cref="IAtomicRule"/> instance given a list of arguments.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    IAtomicRule NewRule(
        MetaContext context,
        ImmutableDictionary<Argument, string> arguments);

    #region Nested Types
    public readonly struct Argument :
        IEquatable<Argument>,
        IDefaultValueProvider<Argument>
    {
        internal static readonly Regex ArgumentPattern = new Regex(
            "^[a-zA-Z_][a-zA-Z0-9-_]*\\z",
            RegexOptions.Compiled);

        private readonly string _key;

        public Argument(string key)
        {
            _key = key.ThrowIfNot(
                ArgumentPattern.IsMatch,
                new ArgumentException($"Invalid argument key: {key}"));
        }

        public static Argument Of(string key) => new(key);

        public static implicit operator Argument(string key) => new(key);

        #region DefaultValueProvider
        public static Argument Default => default;

        public bool IsDefault => default(Argument).Equals(this);
        #endregion

        public override string ToString() => _key;

        public override int GetHashCode() => HashCode.Combine(_key);

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is Argument arg && Equals(arg);
        }

        public bool Equals(Argument arg)
        {
            return EqualityComparer<string>.Default.Equals(_key, arg._key);
        }

        public bool Equals(string key)
        {
            return EqualityComparer<string>.Default.Equals(_key, key);
        }

        public static bool operator ==(Argument left, Argument right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Argument left, Argument right)
        {
            return !(left == right);
        }
    }

    public readonly struct ArgumentPair :
        IEquatable<ArgumentPair>,
        IDefaultValueProvider<ArgumentPair>
    {
        public Argument Argument { get; }

        public string? Value { get; }

        public ArgumentPair(Argument argument, string? value)
        {
            Argument = argument;
            Value = value;
        }

        public static ArgumentPair Of(
            Argument argument,
            string? value)
            => new(argument, value);

        #region DefaultValueProvider
        public static ArgumentPair Default => default;

        public bool IsDefault => Argument.IsDefault && Value is null;
        #endregion

        public bool Equals(ArgumentPair other)
        {
            return Argument.Equals(other.Argument)
                && EqualityComparer<string>.Default.Equals(Value, other.Value);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is ArgumentPair other
                && Equals(other);
        }

        public override int GetHashCode() => HashCode.Combine(Argument, Value);

        public override string ToString()
        {
            return IsDefault
                ? "{}"
                : $"{{key: {Argument}, value: {Value}}}";
        }

        public static bool operator ==(ArgumentPair left, ArgumentPair right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ArgumentPair left, ArgumentPair right)
        {
            return !(left == right);
        }
    }
    #endregion
}
