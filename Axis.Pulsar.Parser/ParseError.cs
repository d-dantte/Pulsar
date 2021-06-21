﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Pulsar.Parser
{
    public class ParseError
    {
        private readonly ParseError[] _causes;

        public IEnumerable<ParseError> Causes => _causes.AsEnumerable();
        
        public string SymbolName { get; }

        public int CharacterIndex { get; }

        public ParseError(string symbolName, int characterIndex, params ParseError[] causes)
        {
            _causes = causes.ThrowIf(
                Extensions.ContainsNull,
                n => new ArgumentException("Causes cannot contain null"))
                ?? throw new ArgumentNullException(nameof(causes));

            CharacterIndex = characterIndex.ThrowIf(
                Extensions.IsNegative,
                n => new ArgumentException($"Invalid {nameof(characterIndex)}"));

            SymbolName = symbolName.ThrowIf(
                string.IsNullOrWhiteSpace,
                n => new ArgumentException($"Invalid {nameof(symbolName)}"));
        }
    }
}
