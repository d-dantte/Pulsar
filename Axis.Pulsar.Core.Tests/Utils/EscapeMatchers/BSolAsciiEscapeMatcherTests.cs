﻿using Axis.Pulsar.Core.Utils.EscapeMatchers;

namespace Axis.Pulsar.Core.Tests.Utils.EscapeMatchers;

[TestClass]
public class BSolAsciiEscapeMatcherTests
{
    [TestMethod]
    public void Decode_Tests()
    {
        var matcher = new BSolAsciiEscapeMatcher();

        var encoded = "\\x0a\\x0ip\\x07";
        var raw = matcher.Decode(encoded);
        Assert.AreEqual("\n\\x0ip\a", raw);

        encoded = "\\x0a";
        raw = matcher.Decode(encoded);
        Assert.AreEqual("\n", raw);

        encoded = "\\p";
        raw = matcher.Decode(encoded);
        Assert.AreEqual("\\p", raw);
    }

    [TestMethod]
    public void Encode_Tests()
    {
        var matcher = new BSolAsciiEscapeMatcher();

        var encoded = "\n\\x0ip\a" ;
        var raw = matcher.Encode(encoded);
        Assert.AreEqual("\\x0a\\x0ip\\x07", raw);

        encoded = "\n";
        raw = matcher.Encode(encoded);
        Assert.AreEqual("\\x0a", raw);

        encoded = "the quck brown fox jumps over the lazy duckling";
        raw = matcher.Encode(encoded);
        Assert.AreEqual(encoded, raw);

        encoded = "the quck brown fox\n jumps over the lazy duckling";
        raw = matcher.Encode(encoded);
        Assert.AreEqual(
            "the quck brown fox\\x0a jumps over the lazy duckling",
            raw);
    }
}
