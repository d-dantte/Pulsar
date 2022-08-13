﻿using Axis.Pulsar.Parser.Input;
using Axis.Pulsar.Parser.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Pulsar.Parser.Recognizers
{
    /// <summary>
    /// Recognizes tokens for a given <see cref="Grammar.SymbolGroup"/> that is configured with <see cref="Grammar.SymbolGroup.GroupingMode.Choice"/>.
    /// </summary>
    public class ChoiceRecognizer : IRecognizer
    {
        /// <summary>
        /// The name given to this node in the Concrete Syntax Tree node. (<see  cref="CST.CSTNode"/>)
        /// </summary>
        public static readonly string PSEUDO_NAME = "#Choice";

        private readonly IRecognizer[] _recognizers;

        ///<inheritdoc/>
        public Cardinality Cardinality { get; }

        public ChoiceRecognizer(Cardinality cardinality, params IRecognizer[] recognizers)
        {
            Cardinality = cardinality;
            _recognizers = recognizers
                .ThrowIf(Extensions.IsNull, new ArgumentNullException(nameof(recognizers)))
                .ThrowIf(Extensions.IsEmpty, new ArgumentException("Empty recognizer array supplied"))
                .ThrowIf(Extensions.ContainsNull, new ArgumentException("Recognizer array must not contain nulls"));
        }

        ///<inheritdoc/>
        public bool TryRecognize(BufferedTokenReader tokenReader, out IResult result)
        {
            var position = tokenReader.Position;
            try
            {
                IResult choice = null;
                var results = new List<IResult.Success>();

                do
                {
                    choice = _recognizers
                        .Select(recognizer => recognizer.Recognize(tokenReader))
                        .Where(result => result is IResult.Success)
                        .FirstOrDefault();

                    if (choice is IResult.Success success)
                        results.Add(success);

                    else break;
                }
                while (Cardinality.CanRepeat(results.Count));

                #region success
                if (Cardinality.IsValidRange(results.Count))
                {
                    result = results
                        .SelectMany(result => result.Symbols)
                        .Map(symbols => new IResult.Success(symbols));
                    return true;
                }
                #endregion

                #region Failed
                // else - Not enough symbols were recognized; this was a failed attempt.
                var currentPosition = tokenReader.Position + 1;
                _ = tokenReader.Reset(position);
                result = choice switch
                {
                    IResult.FailedRecognition failed => new IResult.FailedRecognition(
                        failed.ExpectedSymbolName, // or should the SymbolRef of the current recognizer be used?
                        results.Count,
                        currentPosition),

                    IResult.Exception exception => exception,

                    _ => new IResult.Exception(
                        new InvalidOperationException($"Invalid result type: {choice?.GetType()}"),
                        currentPosition)
                };
                return false;
                #endregion
            }
            catch (Exception e)
            {
                #region Fatal
                _ = tokenReader.Reset(position);
                result = new IResult.Exception(e, position + 1);
                return false;
                #endregion
            }
        }

        ///<inheritdoc/>
        public IResult Recognize(BufferedTokenReader tokenReader)
        {
            _ = TryRecognize(tokenReader, out var result);
            return result;
        }

        ///<inheritdoc/>
        public override string ToString() => Helper.AsString(Grammar.SymbolGroup.GroupingMode.Choice, Cardinality, _recognizers);
    }
}
