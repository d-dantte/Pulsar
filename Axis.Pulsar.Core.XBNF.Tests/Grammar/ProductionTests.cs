﻿using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Core.CST;
using Axis.Pulsar.Core.Grammar;
using Axis.Pulsar.Core.Grammar.Errors;
using Axis.Pulsar.Core.Utils;
using Moq;

namespace Axis.Pulsar.Core.XBNF.Tests.Grammar
{
    [TestClass]
    public class ProductionTests
    {
        [TestMethod]
        public void TryProcessRule_Tests()
        {
            var passingRuleMock = new Mock<IRule>();
            passingRuleMock
                .Setup(r => r.TryRecognize(
                    It.IsAny<TokenReader>(),
                    It.IsAny<ProductionPath>(),
                    It.IsAny<ILanguageContext>(),
                    out It.Ref<IResult<ICSTNode>>.IsAny))
                .Returns(new TryRecognizeNode((TokenReader reader, ProductionPath? path, ILanguageContext cxt, out IResult<ICSTNode> x) =>
                {
                    x = ICSTNode
                        .Of("symbol", "tokens")
                        .ApplyTo(Result.Of<ICSTNode>);
                    return true;
                }));

            var failingRuleMock = new Mock<IRule>();
            failingRuleMock
                .Setup(r => r.TryRecognize(
                    It.IsAny<TokenReader>(),
                    It.IsAny<ProductionPath>(),
                    It.IsAny<ILanguageContext>(),
                    out It.Ref<IResult<ICSTNode>>.IsAny))
                .Returns(new TryRecognizeNode((TokenReader reader, ProductionPath? path, ILanguageContext cxt, out IResult<ICSTNode> x) =>
                {
                    x = UnrecognizedTokens
                        .Of(path!, 2)
                        .ApplyTo(Result.Of<ICSTNode>);
                    return false;
                }));

            var production = XBNFProduction.Of("symbol", passingRuleMock.Object);
            var success = production.TryProcessRule("some tokens", null, null!, out var result);
            Assert.IsTrue(success);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsDataResult());


            production = XBNFProduction.Of("symbol", failingRuleMock.Object);
            success = production.TryProcessRule("some tokens", null, null!, out result);
            Assert.IsFalse(success);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsErrorResult());
        }
    }
}
