﻿using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar;
using Axis.Pulsar.Grammar.CST;
using Axis.Pulsar.Grammar.Language.Rules.CustomTerminals;
using Axis.Pulsar.Grammar.Recognizers.Results;
using Axis.Pulsar.Languages.xBNF;
using System.Text;
using System.Text.RegularExpressions;
using static Axis.Pulsar.Grammar.Language.Rules.CustomTerminals.DelimitedString;

namespace Axis.Pulsar.Languages.Tests.xBnf
{
    [TestClass]
    public class ImporterTests
    {
        [TestMethod]
        public void MiscImportTests()
        {
            try
            {
                #region others
                var timer = System.Diagnostics.Stopwatch.StartNew();
                var ruleImporter = new Importer();
                timer.Stop();
                Console.WriteLine("Time to Create Importer: " + timer.Elapsed);

                timer.Restart();
                var x = ruleImporter.ImportGrammar(new MemoryStream(Encoding.UTF8.GetBytes(SampleBNF1)));
                var result = x.RootRecognizer().Recognize(new BufferedTokenReader("foO"));
                timer.Stop();
                Console.WriteLine("Time to Import: " + timer.Elapsed);



                timer.Restart();
                ruleImporter = new Importer();
                timer.Stop();
                Console.WriteLine("Time to Create Importer: " + timer.Elapsed);

                timer.Restart();
                x = ruleImporter.ImportGrammar(new MemoryStream(Encoding.UTF8.GetBytes(SampleBNF2)));
                timer.Stop();
                Console.WriteLine("Time to Import: " + timer.Elapsed);




                timer.Restart();
                ruleImporter = new Importer();
                timer.Stop();
                Console.WriteLine("Time to Create Importer: " + timer.Elapsed);

                timer.Restart();
                x = ruleImporter.ImportGrammar(new MemoryStream(Encoding.UTF8.GetBytes(SampleBNF3)));
                timer.Stop();
                Console.WriteLine("Time to Import: " + timer.Elapsed);



                timer.Restart();
                ruleImporter = new Importer();
                timer.Stop();
                Console.WriteLine("Time to Create Importer: " + timer.Elapsed);

                timer.Restart();
                x = ruleImporter.ImportGrammar(new MemoryStream(Encoding.UTF8.GetBytes(SampleBNF4)));
                timer.Stop();
                Console.WriteLine("Time to Import: " + timer.Elapsed);



                timer.Restart();
                ruleImporter = new Importer();
                timer.Stop();
                Console.WriteLine("Time to Create Importer: " + timer.Elapsed);

                timer.Restart();
                x = ruleImporter.ImportGrammar(new MemoryStream(Encoding.UTF8.GetBytes(SampleBNF5)));
                timer.Stop();
                Console.WriteLine("Time to Import: " + timer.Elapsed);



                timer.Restart();
                ruleImporter = new Importer();
                timer.Stop();
                Console.WriteLine("Time to Create Importer: " + timer.Elapsed);

                timer.Restart();
                x = ruleImporter.ImportGrammar(new MemoryStream(Encoding.UTF8.GetBytes(SampleBNF6)));
                timer.Stop();
                Console.WriteLine("Time to Import: " + timer.Elapsed);
                #endregion

                #region TestGrammar
                timer.Restart();
                ruleImporter = new Importer();
                timer.Stop();
                Console.WriteLine("Time to Create Importer: " + timer.Elapsed);

                timer.Restart();
                using var sampleGrammarStream = typeof(ImporterTests).Assembly
                    .GetManifestResourceStream($"{typeof(ImporterTests).Namespace}.TestGrammar.xbnf");
                x = ruleImporter.ImportGrammar(sampleGrammarStream);
                timer.Stop();
                Console.WriteLine("Time to Import: " + timer.Elapsed);
                #endregion

                #region TestGrammar2
                timer.Restart();
                ruleImporter = new Importer();
                timer.Stop();
                Console.WriteLine("Time to Create Importer: " + timer.Elapsed);

                #region register custom terminals
                string MultilineSingleQuoteDelimitedString = "Multiline-3SQDString";
                string SinglelineSingleQuoteDelimitedString = "Singleline-SQDString";
                string SinglelineDoubleQuoteDelimitedString = "Singleline-DQDString";

                _ = ruleImporter.RegisterTerminal(
                    new DelimitedString(
                        MultilineSingleQuoteDelimitedString,
                        "'''",
                        new BSolGeneralEscapeMatcher()));

                // register singleline-sqdstring
                _ = ruleImporter.RegisterTerminal(
                    new DelimitedString(
                        SinglelineSingleQuoteDelimitedString,
                        "\'",
                        new[] { "\n", "\r" },
                        new BSolGeneralEscapeMatcher()));

                // register singleline-dqdstring
                _ = ruleImporter.RegisterTerminal(
                    new DelimitedString(
                        SinglelineDoubleQuoteDelimitedString,
                        "\"",
                        new[] { "\n", "\r" },
                        new BSolGeneralEscapeMatcher()));
                #endregion

                timer.Restart();
                using var sampleGrammarStream2 = typeof(ImporterTests).Assembly
                    .GetManifestResourceStream($"{typeof(ImporterTests).Namespace}.TestGrammar2.xbnf");
                x = ruleImporter.ImportGrammar(sampleGrammarStream2);
                timer.Stop();
                Console.WriteLine("Time to Import: " + timer.Elapsed);
                #endregion

                var recognition = x
                    .GetRecognizer("annotation-list")
                    .Recognize("$the_identifier::")
                    .As<SuccessResult>();

                var queryPath = $"annotation.identifier|quoted-symbol";
                var nodes = recognition.Symbol
                    .FindNodes(queryPath)
                    .Select(cstnode => cstnode.TokenValue())
                    .ToArray();
            }
            catch (Exception e)
            {
                throw;
            }
        }


        [TestMethod]
        public void TestGrammar3()
        {
            var importer = new Importer();

            // register singleline-dqd string
            _ = importer.RegisterTerminal(
                new DelimitedString(
                    "dqd-string",
                    "\"",
                    new[] { "\n", "\r" },
                    new BSolGeneralEscapeMatcher()));

            // register singleline-sqd string
            _ = importer.RegisterTerminal(
                new DelimitedString(
                    "qas",
                    "\'",
                    new[] { "\n", "\r" },
                    new BSolGeneralEscapeMatcher()));

            using var sampleGrammarStream2 = typeof(ImporterTests).Assembly
                .GetManifestResourceStream($"{typeof(ImporterTests).Namespace}.TestGrammar3.xbnf");

            var grammar = importer.ImportGrammar(sampleGrammarStream2);

            var recognizer = grammar.GetRecognizer("duration-expression");
            var result = recognizer.Recognize("'D 17.23:12'");
            Assert.IsTrue(result is SuccessResult);
        }


        [TestMethod]
        public void SampleGrammarTest()
        {
            try
            {
                using var sampleGrammarStream = typeof(ImporterTests).Assembly
                    .GetManifestResourceStream($"{typeof(ImporterTests).Namespace}.TestGrammar.xbnf");

                var ruleImporter = new Importer();
                var x = ruleImporter.ImportGrammar(sampleGrammarStream);

                var result = x.RootRecognizer().Recognize(new BufferedTokenReader("1_000_000_000"));

                Assert.IsNotNull(x);
            }
            catch (Exception e)
            {
                throw;
            }
        }


        public static readonly string SampleBNF1 =
@"$grama -> +[
    ?[$stuff
      $other-stuff.2
      $more-stuff 'foo'] EOF]
# comments occupy a whole line.
$more-stuff -> $stuff

$stuff ::= /bleh/.i.5
$other-stuff ::= ""meh""
";

        public static readonly string SampleBNF2 =
@"$grama -> ?[#[$other-stuff $main-stuff].1,4 $nothing  $stuff ]>2
$stuff -> /\w+/
$other-stuff -> ""meh""

$main-stuff -> '34'

$nothing -> 'moja hiden'
";

        public static readonly string SampleBNF3 =
@"$grama ::= ?[$stuff $other-stuff].?>3
$stuff ::= /bleh+/.4,6
$other-stuff ::= ""meh""
";

        public static readonly string SampleBNF4 =
@"$grama ::= ?[$stuff $other-stuff $main-stuff].*
$stuff ::= /bleh+/.4,6
$other-stuff ::= ""meh""
$main-stuff ::= ""hem""
";

        public static readonly string SampleBNF5 =
@"
# some
# comments
# to
# kick start
# things
$grama ::= ?[$stuff $other-stuff $main-stuff].+>1
$stuff ::= /bleh+/.4,6
$other-stuff ::= ""meh""
$main-stuff ::= ""hem""
";

        public static readonly string SampleBNF6 =
@"
# only
# comments
# to
# kick start
# things";

        #region nested types
        public class BSolGeneralAndBraceEscapeMatcher : IEscapeSequenceMatcher
        {
            private readonly Regex HexPattern = new(@"^u[a-fA-F0-9]{0,4}$", RegexOptions.Compiled);

            public string EscapeDelimiter => "\\";

            public bool IsSubMatch(ReadOnlySpan<char> subTokens)
            {
                if (subTokens[0] == 'u')
                    return subTokens.Length <= 5
                        && HexPattern.IsMatch(new string(subTokens));

                return subTokens.Length == 1 && subTokens[0] switch
                {
                    '\'' => true,
                    '\"' => true,
                    '\\' => true,
                    'n' => true,
                    'r' => true,
                    'f' => true,
                    'b' => true,
                    't' => true,
                    'v' => true,
                    '0' => true,
                    'a' => true,
                    '{' => true,
                    '}' => true,
                    _ => false
                };
            }

            public bool IsMatch(ReadOnlySpan<char> escapeTokens)
            {
                if (escapeTokens.Length == 5)
                    return HexPattern.IsMatch(new string(escapeTokens));

                if (escapeTokens.Length == 1 && escapeTokens[0] != 'u')
                    return IsSubMatch(escapeTokens);

                return false;
            }
        }
        #endregion
    }
}
