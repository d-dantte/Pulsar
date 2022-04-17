﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace Axis.Pulsar.Parser
{
    internal static class Extensions
    {
        public static bool IsNull(this object obj) => obj == null;
        public static bool IsEmpty<T>(this ICollection<T> collection) => collection.Count == 0;
        public static bool IsNullOrEmpty<T>(this T[] array) => array == null || array.IsEmpty();

        public static IEnumerable<T> Concat<T>(this T value, T otherValue)
        {
            yield return value;
            yield return otherValue;
        }

        public static IEnumerable<T> Concat<T>(this T value, IEnumerable<T> values)
        {
            yield return value;
            foreach (var t in values)
            {
                yield return t;
            }
        }

        public static IEnumerable<T> Concat<T>(this T value, params T[] values)
        {
            yield return value;
            foreach (var t in values)
            {
                yield return t;
            }
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> initialValues, T otherValue)
        {
            foreach (var t in initialValues)
            {
                yield return t;
            }
            yield return otherValue;
        }

        public static IEnumerable<T> Concat<T>(
            this IEnumerable<T> initialValues,
            params T[] otherValue)
            => Enumerable.Concat(initialValues, otherValue);

        public static IEnumerable<T> Concat<T>(
            this IEnumerable<T> initialValues,
            IEnumerable<T> otherValue)
            => Enumerable.Concat(initialValues, otherValue);

        public static IEnumerable<T> Enumerate<T>(this T value) => new[] { value };

        public static T ThrowIf<T>(this 
            T value, Func<T, bool> predicate,
            Func<T, Exception> exception = null)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            else if (predicate.Invoke(value))
            {
                var ex = exception?.Invoke(value) ?? new Exception("An exception occured");

                if (ex.StackTrace == null) 
                    throw ex;
                
                else 
                    ExceptionDispatchInfo.Capture(ex).Throw();
            }
            return value;
        }

        public static bool ContainsNull<T>(this IEnumerable<T> enm) => enm.Any(t => t == null);

        public static bool IsNegative(this int value) => value < 0;

        public static bool IsNegative(this int? value) => value < 0;

        public static Syntax.Symbol[] FlattenProduction(this Syntax.Symbol symbol)
        {
            //basically, if this isn't a production generated symbol, return it
            if (!symbol.Name.StartsWith("#"))
                return new[] { symbol };

            else
            {
                return symbol.Children
                    .Select(FlattenProduction)
                    .Aggregate(Enumerable.Empty<Syntax.Symbol>(), (enm, next) => enm.Concat(next))
                    .ToArray();
            }
        }

        public static void ForAll<T>(this IEnumerable<T> @enum, Action<T> action)
        {
            foreach (var t in @enum)
                action.Invoke(t);
        }

        public static Dictionary<TKey, TValue> Add<TKey, TValue>(this
            Dictionary<TKey, TValue> dictionary,
            KeyValuePair<TKey, TValue> keyValuePair)
        {
            dictionary.Add(keyValuePair.Key, keyValuePair.Value);
            return dictionary;
        }


        public static bool NullOrEquals<T>(this T operand1, T operand2)
        where T : class
        {
            if (operand1 == null && operand2 == null)
                return true;

            return operand1 != null
                && operand2 != null
                && operand1.Equals(operand2);
        }

        public static TOut Map<TIn, TOut>(this TIn @in, Func<TIn, TOut> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            return mapper.Invoke(@in);
        }

        public static bool ExactlyAll<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            var called = false;
            return enumerable
                .All(t => called = predicate.Invoke(t))
                && called;
        }
    }
}
