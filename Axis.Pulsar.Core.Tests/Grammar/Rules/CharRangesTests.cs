﻿using Axis.Luna.Common.Results;
using Axis.Pulsar.Core.Grammar;
using Axis.Pulsar.Core.Grammar.Rules;

namespace Axis.Pulsar.Core.Tests.Grammar.Rules
{
    [TestClass]
    public class CharRangesTests
    {
        [TestMethod]
        public void ParseRanges_Tests()
        {
            var ranges = CharacterRanges.ParseRanges("a-d, c-f, x, y, 1-4");
            Assert.AreEqual(4, ranges.Length);
            Assert.AreEqual('1', ranges[0].LowerBound);
            Assert.AreEqual('4', ranges[0].UpperBound);
            Assert.AreEqual('a', ranges[1].LowerBound);
            Assert.AreEqual('f', ranges[1].UpperBound);
            Assert.AreEqual('x', ranges[2].LowerBound);
            Assert.AreEqual('x', ranges[2].UpperBound);
            Assert.AreEqual('y', ranges[3].LowerBound);
            Assert.AreEqual('y', ranges[3].UpperBound);
        }

        [TestMethod]
        public void TryRecognize_Tests()
        {
            var includes = CharacterRanges.ParseRanges("a-d, c-f, x, y, 1-4");
            var excludes = CharacterRanges.ParseRanges("e");
            var xterRanges = CharacterRanges.Of("xter", includes, excludes);

            var success = xterRanges.TryRecognize(
                "b",
                ProductionPath.Of("xyz"),
                null!,
                out var result);
            Assert.IsTrue(success);
            Assert.IsTrue(
                result.IsDataResult(out var data)
                && "xter".Equals(data.Name)
                && data.Tokens.Equals("b"));

            success = xterRanges.TryRecognize(
                "2",
                ProductionPath.Of("xyz"),
                null!,
                out result);
            Assert.IsTrue(success);
            Assert.IsTrue(
                result.IsDataResult(out data)
                && "xter".Equals(data.Name)
                && data.Tokens.Equals("2"));

            success = xterRanges.TryRecognize(
                "e",
                ProductionPath.Of("xyz"),
                null!,
                out result);
            Assert.IsFalse(success);
            Assert.IsTrue(result.IsErrorResult(out FailedRecognitionError _));
        }
    }
}
