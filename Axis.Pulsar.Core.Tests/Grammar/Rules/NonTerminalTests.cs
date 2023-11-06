﻿using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Core.CST;
using Axis.Pulsar.Core.Exceptions;
using Axis.Pulsar.Core.Grammar;
using Axis.Pulsar.Core.Grammar.Groups;
using Axis.Pulsar.Core.Grammar.Rules;
using Axis.Pulsar.Core.Utils;
using Moq;

namespace Axis.Pulsar.Core.Tests.Grammar.Rules
{
    [TestClass]
    public class NonTerminalTests
    {
        internal static IGroupElement MockElement(
            Cardinality cardinality,
            bool recognitionStatus,
            IResult<NodeSequence> recognitionResult)
        {
            var mock = new Mock<IGroupElement>();

            mock.Setup(m => m.Cardinality).Returns(cardinality);
            mock
                .Setup(m => m.TryRecognize(
                    It.IsAny<TokenReader>(),
                    It.IsAny<ProductionPath>(),
                    out It.Ref<IResult<NodeSequence>>.IsAny))
                .Returns(new TryRecognizeNodeSequence((
                        TokenReader reader,
                        ProductionPath? path,
                        out IResult<NodeSequence> result) =>
                {
                    result = recognitionResult;
                    return recognitionStatus;
                }));

            return mock.Object;
        }

        [TestMethod]
        public void TryRecognize_Tests()
        {
            var path = ProductionPath.Of("parent");

            // passing element
            var element = MockElement(
                Cardinality.OccursOnlyOnce(),
                true,
                Result.Of(NodeSequence.Empty));
            var nt = NonTerminal.Of(element);
            var success = nt.TryRecognize("stuff", path, out var result);
            Assert.IsTrue(success);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsDataResult());

            // unrecognized element
            element = MockElement(
                Cardinality.OccursOnlyOnce(),
                false,
                UnrecognizedTokens
                    .Of(path, 0)
                    .ApplyTo(GroupError.Of)
                    .ApplyTo(Result.Of<NodeSequence>));
            nt = NonTerminal.Of(element);
            success = nt.TryRecognize("stuff", path, out result);
            Assert.IsFalse(success);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsErrorResult(out UnrecognizedTokens ge));

            // partially recognized element
            element = MockElement(
                Cardinality.OccursOnlyOnce(),
                false,
                PartiallyRecognizedTokens
                    .Of(path, 0, "partial tokens")
                    .ApplyTo(g => GroupError.Of(g, NodeSequence.Of(ICSTNode.Of("partial", "partial tokens"))))
                    .ApplyTo(Result.Of<NodeSequence>));
            nt = NonTerminal.Of(element);
            success = nt.TryRecognize("stuff", path, out result);
            Assert.IsFalse(success);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsErrorResult(out PartiallyRecognizedTokens pe));

            // recognition threshold element
            element = MockElement(
                Cardinality.OccursOnlyOnce(),
                false,
                UnrecognizedTokens
                    .Of(path, 0)
                    .ApplyTo(e => GroupError.Of(e,
                        NodeSequence.Of(NodeSequence.Of(ICSTNode.Of("partial", "partial tokens")))))
                    .ApplyTo(Result.Of<NodeSequence>));
            nt = NonTerminal.Of(element);
            success = nt.TryRecognize("stuff", path, out result);
            Assert.IsFalse(success);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsErrorResult(out pe));

            // runtime recognized element
            element = MockElement(
                Cardinality.OccursOnlyOnce(),
                false,
                RecognitionRuntimeError
                    .Of(new Exception())
                    .ApplyTo(Result.Of<NodeSequence>));
            nt = NonTerminal.Of(element);
            success = nt.TryRecognize("stuff", path, out result);
            Assert.IsFalse(success);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsErrorResult(out RecognitionRuntimeError rre));

        }
    }
}
