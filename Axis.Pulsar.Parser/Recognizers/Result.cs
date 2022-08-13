﻿using Axis.Pulsar.Parser.CST;
using Axis.Pulsar.Parser.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Pulsar.Parser.Recognizers
{
    /// <summary>
    /// Recognizer result
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// Represent a successful recognition of 
        /// </summary>
        public record Success : IResult
        {
            public ICSTNode[] Symbols { get; }

            public Success(params ICSTNode[] symbols)
                :this((IEnumerable<ICSTNode>)symbols)
            { }

            public Success(IEnumerable<ICSTNode> symbols)
            {
                Symbols = symbols?
                    .ToArray()
                    .ThrowIf(
                        Extensions.IsNull,
                        new ArgumentNullException(nameof(symbols)))
                    .ThrowIf(
                        Extensions.ContainsNull,
                        new ArgumentException("CSTNode list cannot contain null"));
            }
        }

        /// <summary>
        /// Represents fialed recognition of symbols
        /// </summary>
        public record FailedRecognition : IResult
        {
            /// <summary>
            /// The partially recognized symbols
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

            public FailedRecognition(
                string expectedSymbolName,
                int recognitionCount,
                int inputPosition)
            {
                RecognitionCount = recognitionCount.ThrowIf(
                    Extensions.IsNegative,
                    new ArgumentException($"Invalid {nameof(recognitionCount)}"));

                ExpectedSymbolName = expectedSymbolName ?? throw new ArgumentNullException(nameof(expectedSymbolName));

                InputPosition = inputPosition.ThrowIf(
                    Extensions.IsNegative,
                    new ArgumentException($"{nameof(InputPosition)} must be >= 0"));
            }
        }

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
    }
}
