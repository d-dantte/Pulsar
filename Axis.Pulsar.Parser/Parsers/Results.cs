﻿using Axis.Pulsar.Parser.CST;
using System;

namespace Axis.Pulsar.Parser.Parsers
{
    /// <summary>
    /// Represents a discriminated union of possible results of a parsing process.
    /// 
    /// <para>
    /// Note: consider making the inner types '<c>record struct</c>' types.
    /// </para>
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// Represents a successful recognition.
        /// </summary>
        public record Success : IResult
        {
            public ICSTNode Symbol { get; }

            public Success(ICSTNode node)
            {
                Symbol = node ?? throw new ArgumentNullException(nameof(node));
            }
        }

        /// <summary>
        /// Creates an instance of the <see cref="Success"/> class.
        /// </summary>
        public static Success Of(ICSTNode node) => new(node);

        /// <summary>
        /// Represents partial recognition of symbols
        /// </summary>
        public record PartialRecognition: IResult
        {
            /// <summary>
            /// A count of the recognized symbols.
            /// </summary>
            public int RecognitionCount { get; }

            /// <summary>
            /// The expected symbol name
            /// </summary>
            public string ExpectedSymbolName { get; }

            /// <summary>
            /// The position where the expected symbol was expected to appear
            /// </summary>
            public int InputPosition { get; }

            /// <summary>
            /// An inner Failed recognition result, if the failure originated from a symbol ref
            /// </summary>
            public Parsers.IResult Reason { get; }

            public PartialRecognition(
                int recognitionCount,
                string expectedSymbolName,
                int inputPosition,
                IResult reason = null)
            {
                RecognitionCount = recognitionCount.ThrowIf(
                    Extensions.IsNegative,
                    new ArgumentException($"{nameof(recognitionCount)} cannot be < 0"));

                ExpectedSymbolName = expectedSymbolName.ThrowIf(
                    string.IsNullOrWhiteSpace,
                    new ArgumentException($"Invalid {nameof(expectedSymbolName)}"));

                InputPosition = inputPosition.ThrowIf(
                    Extensions.IsNegative,
                    new ArgumentException($"{nameof(InputPosition)} must be >= 0"));

                Reason = reason.ThrowIf(
                    r => r is Success || r is IResult.Exception,
                    new ArgumentException($"Invalid reason type: {reason?.GetType()}"));
            }
        }

        /// <summary>
        /// Creates an instance of the <see cref="PartialRecognition"/> class.
        /// </summary>
        public static PartialRecognition Of(
                int recognitionCount,
                string expectedSymbolName,
                int inputPosition,
                IResult reason = null)
            => new(recognitionCount, expectedSymbolName, inputPosition, reason);

        /// <summary>
        /// Represents failed recognition.
        /// </summary>
        public record FailedRecognition: IResult
        {
            /// <summary>
            /// Name of the symbol whose recognition failed
            /// </summary>
            public string ExpectedSymbolName { get; }

            /// <summary>
            /// Position at which the symbol was expected to be
            /// </summary>
            public int InputPosition { get; }

            /// <summary>
            /// An inner Failed recognition result, if the failure originated from a symbol ref
            /// </summary>
            public Parsers.IResult Reason { get; }

            public FailedRecognition(
                string symbolName,
                int inputPosition,
                IResult reason = null)
            {
                ExpectedSymbolName = symbolName.ThrowIf(
                    string.IsNullOrWhiteSpace,
                    _ => new ArgumentException($"Invalid {nameof(symbolName)}"));

                InputPosition = inputPosition.ThrowIf(
                    Extensions.IsNegative,
                    new ArgumentException($"{nameof(InputPosition)} must be >= 0"));

                Reason = reason.ThrowIf(
                    r => r is IResult.Success || r is IResult.Exception,
                    new ArgumentException($"Invalid reason type: {reason?.GetType()}"));
            }
        }

        /// <summary>
        /// Creates an instance of the <see cref="FailedRecognition"/> class.
        /// </summary>
        public static FailedRecognition Of(
                string symbolName,
                int inputPosition,
                IResult reason = null)
            => new(symbolName, inputPosition, reason);

        /// <summary>
        /// Represents a fatally faulted recognition - a situation not accounted for by algorithm.
        /// </summary>
        public record Exception: IResult
        {
            /// <summary>
            /// The exception that was thrown during the recognition process
            /// </summary>
            public System.Exception Error { get; }

            /// <summary>
            /// The position at which the symbol whose recognition failed was exepcted to be.
            /// </summary>
            public int InputPosition { get; }

            public Exception(System.Exception error, int inputPosition)
            {
                Error = error ?? throw new ArgumentNullException(nameof(error));

                InputPosition = inputPosition.ThrowIf(
                    Extensions.IsNegative,
                    new ArgumentException($"{nameof(InputPosition)} must be >= 0"));
            }
        }

        /// <summary>
        /// Creates an instance of the <see cref="Exception"/> class.
        /// </summary>
        public static Exception Of(
            System.Exception error,
            int inputPosition)
            => new(error, inputPosition);
    }
}
