﻿using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Core.CST;
using Axis.Pulsar.Core.Lang;
using Axis.Pulsar.Core.XBNF.Lang;
using System.Xml.Linq;

namespace Axis.Pulsar.Core.XBNF.Tests.E2E
{
    [TestClass]
    public class SampleLang
    {
        [TestMethod]
        public void SampleLangTest()
        {
            ILanguageContext? _lang = null;
            try
            {
                // get language string
                using var langDefStream = ResourceLoader.Load("SampleGrammar.SampleLang.xbnf");
                var langText = new StreamReader(langDefStream!).ReadToEnd();

                // build importer
                var importer = XBNFImporter.Builder
                    .NewBuilder()
                    .WithDefaultAtomicRuleDefinitions()
                    .Build();

                // import
                _lang = importer.ImportLanguage(langText);
            }
            catch (Exception ex)
            {
                ex.Throw();
            }
        }

        [TestMethod]
        public void SampleRecognition_Tests()
        {
        }
    }
}