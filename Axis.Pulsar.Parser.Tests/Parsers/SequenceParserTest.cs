﻿using Axis.Pulsar.Parser.Builder;
using Axis.Pulsar.Parser.Input;
using Axis.Pulsar.Parser.Language;
using Axis.Pulsar.Parser.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Axis.Pulsar.Parser.Tests.Parsers
{
    [TestClass]
    public class SequenceParserTest
    {
        private static readonly StringMatcherParser OpenBraceParser = new(
            new StringTerminal(
                "open_brace",
                "("));

        private static readonly StringMatcherParser CloseBraceParser = new(
            new StringTerminal(
                "close_brace",
                ")"));

        private static readonly SingleSymbolParser WhitespaceParser = new(
            Cardinality.OccursNeverOrMore(),
            new PatternMatcherParser(
                new PatternTerminal(
                    "whitespace",
                    new Regex("^\\s*$", RegexOptions.IgnoreCase),
                    Cardinality.OccursAtLeast(1))));

        private static readonly PatternMatcherParser VariableParser = new(
            new PatternTerminal(
                "identifier_name",
                new Regex("^[a-z_\\$]\\w*$", RegexOptions.IgnoreCase),
                Cardinality.OccursAtLeast(1)));

        private static SequenceParser CreateSequenceParser(Cardinality? cardinality = null) => new(
            cardinality ?? Cardinality.OccursOnlyOnce(),
            OpenBraceParser,
            WhitespaceParser,
            VariableParser,
            WhitespaceParser,
            CloseBraceParser);

        [TestMethod]
        public void Constructor_Should_ReturnValidObject()
        {
            var parser = CreateSequenceParser();

            Assert.IsNotNull(parser);
        }


        [TestMethod]
        public void TryParse_WithValidInput_Should_ReturnValidParseResult()
        {
            var parser = CreateSequenceParser();

            var reader = new BufferedTokenReader("(some_identifier)");
            var succeeded = parser.TryParse(reader, out var result);

            Assert.IsTrue(succeeded);
            Assert.IsNotNull(result);
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Symbol);
            Assert.AreEqual(result.Symbol.Value, "(some_identifier)");

            //2
            reader = new BufferedTokenReader("(some_identifier )");
            succeeded = parser.TryParse(reader, out result);

            Assert.IsTrue(succeeded);
            Assert.IsNotNull(result);
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Symbol);
            Assert.AreEqual(result.Symbol.Value, "(some_identifier )");
        }


        [TestMethod]
        public void TryParse_WithinvalidInput_Should_ReturnErroredParseResult()
        {
            var parser = CreateSequenceParser();

            var reader = new BufferedTokenReader(" (some_identifier )");
            var succeeded = parser.TryParse(reader, out var result);

            Assert.IsFalse(succeeded);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Error);
            Assert.IsNull(result.Symbol);

            //2
            reader = new BufferedTokenReader("((some_identifier )");
            succeeded = parser.TryParse(reader, out result);

            Assert.IsFalse(succeeded);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Error);
            Assert.IsNull(result.Symbol);

            //3
            reader = new BufferedTokenReader("(some_ identifier)");
            succeeded = parser.TryParse(reader, out result);

            Assert.IsFalse(succeeded);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Error);
            Assert.IsNull(result.Symbol);
        }
    }
}
