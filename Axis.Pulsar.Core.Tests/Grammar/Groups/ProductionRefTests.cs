﻿using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Core.CST;
using Axis.Pulsar.Core.Grammar;
using Axis.Pulsar.Core.Grammar.Errors;
using Axis.Pulsar.Core.Grammar.Groups;
using Axis.Pulsar.Core.Grammar.Nodes;
using Axis.Pulsar.Core.Lang;
using Axis.Pulsar.Core.Utils;
using Moq;

namespace Axis.Pulsar.Core.Tests.Grammar.Groups
{
    [TestClass]
    public class ProductionRefTests
    {
        internal static IGrammar MockGrammar(params KeyValuePair<string, IProduction>[] productions)
        {
            var mock = new Mock<IGrammar>();

            // setup inverses
            var symbols = productions.Select(p => p.Key).ToArray();
            mock.Setup(grammar => grammar.ContainsProduction(It.IsNotIn(symbols)))
                .Returns(false);
            mock.Setup(grammar => grammar.GetProduction(It.IsNotIn(symbols)))
                .Throws((string key) => new KeyNotFoundException(key));

            return productions
                .Aggregate(mock, (mockInstance, production) =>
                {
                    // setup grammar.Contains
                    mockInstance
                        .Setup(grammar => grammar.ContainsProduction(production.Key))
                        .Returns(true);

                    // setup grammar.GetProduction
                    mockInstance
                        .Setup(grammar => grammar.GetProduction(production.Key))
                        .Returns(production.Value);

                    return mockInstance;
                })
                .Object;
        }

        internal static IProduction MockProduction(
            string symbol,
            bool executionStatus,
            IResult<ICSTNode> executionResult)
        {
            var mock = new Mock<IProduction>();

            mock.Setup(m => m.Symbol).Returns(symbol);
            mock
                .Setup(m => m.TryProcessRule(
                    It.IsAny<TokenReader>(),
                    It.IsAny<ProductionPath?>(),
                    It.IsAny<ILanguageContext>(),
                    out It.Ref<IResult<ICSTNode>>.IsAny))
                .Returns(new TryRecognizeNode((
                        TokenReader reader,
                        ProductionPath? path,
                        ILanguageContext languageContext,
                        out IResult<ICSTNode> result) =>
                {
                    result = executionResult;
                    return executionStatus;
                }));

            return mock.Object;
        }

        internal static ILanguageContext MockContext(IGrammar grammar)
        {
            var mock = new Mock<ILanguageContext>();

            mock.Setup(l => l.Grammar).Returns(grammar);

            return mock.Object;
        }

        [TestMethod]
        public void TryProcessRule_Tests()
        {
            // passing production
            var passingProduction = MockProduction(
                "sp", true,
                ICSTNode
                    .Of("sp", "first passing token")
                    .ApplyTo(Result.Of<ICSTNode>));

            // unrecognized production
            var unrecognizedProduction = MockProduction(
                "up", false,
                FailedRecognitionError
                    .Of(ProductionPath.Of("up"), 2)
                    .ApplyTo(Result.Of<ICSTNode>));

            // partially recognized production
            var partialyRecognizedProduction = MockProduction(
                "pp", false,
                PartialRecognitionError
                    .Of(ProductionPath.Of("pp"), 2, 6)
                    .ApplyTo(Result.Of<ICSTNode>));

            // runtime error production
            var runtimeErrorProduction = MockProduction(
                "re", false,
                Result.Of<ICSTNode>(new Exception()));

            // grammar
            var grammar = MockGrammar(
                KeyValuePair.Create("up", unrecognizedProduction),
                KeyValuePair.Create("pp", partialyRecognizedProduction),
                KeyValuePair.Create("re", runtimeErrorProduction),
                KeyValuePair.Create("sp", passingProduction));

            // lang context
            var context = MockContext(grammar);


            var path = ProductionPath.Of("parent");
            var pref = ProductionRef.Of(Cardinality.OccursOnlyOnce(), "sp");

            var success = pref.TryRecognize("some tokens", path, context, out var result);
            Assert.IsTrue(success);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsDataResult());


            pref = ProductionRef.Of(Cardinality.OccursOnlyOnce(), "up");
            success = pref.TryRecognize("some tokens", path, context, out result);
            Assert.IsFalse(success);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsErrorResult());
            Assert.IsTrue(result.IsErrorResult(out GroupRecognitionError ge));
            Assert.IsTrue(ge.Cause is FailedRecognitionError);


            pref = ProductionRef.Of(Cardinality.OccursOnlyOnce(), "pp");
            success = pref.TryRecognize("some tokens", path, context, out result);
            Assert.IsFalse(success);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsErrorResult());
            Assert.IsTrue(result.IsErrorResult(out ge));
            Assert.IsTrue(ge.Cause is PartialRecognitionError);


            pref = ProductionRef.Of(Cardinality.OccursOnlyOnce(), "re");
            success = pref.TryRecognize("some tokens", path, context, out result);
            Assert.IsFalse(success);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsErrorResult(out Exception _));
        }
    }
}
