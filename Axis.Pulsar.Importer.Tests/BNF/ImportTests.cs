﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace Axis.Pulsar.Importer.Tests.BNF
{
    [TestClass]
    public class ImportTests
    {
        [TestMethod]
        public void MiscTest()
        {
            var now = DateTimeOffset.Now;
            var ruleImporter = new Common.BNF.RuleImporter();
            var elapsed = DateTimeOffset.Now - now;
            Console.WriteLine("Time to Create Importer: " + elapsed);

            now = DateTimeOffset.Now;
            var x = ruleImporter.ImportRule(new MemoryStream(Encoding.UTF8.GetBytes(SampleBNF2)));
            elapsed = DateTimeOffset.Now - now;
            Console.WriteLine("Time to Importer: " + elapsed);


            now = DateTimeOffset.Now;
            ruleImporter = new Common.BNF.RuleImporter();
            elapsed = DateTimeOffset.Now - now;
            Console.WriteLine("Time to Create Importer: " + elapsed);

            now = DateTimeOffset.Now;
            x = ruleImporter.ImportRule(new MemoryStream(Encoding.UTF8.GetBytes(SampleBNF2)));
            elapsed = DateTimeOffset.Now - now;
            Console.WriteLine("Time to Importer: " + elapsed);
        }


        public static readonly string SampleBNF1 =
@"$grama ::= ?[$stuff $other-stuff]
$stuff ::= /bleh/
$other-stuff ::= ""meh""
";

        public static readonly string SampleBNF2 =
@"$grama ::= ?[$stuff #[$other-stuff $main-stuff].1,2 $nothing  ]
$stuff ::= /bleh+/.4,
$other-stuff ::= ""meh""
$main-stuff ::= '34'
$nothing ::= 'moja hiden'
";

        public static readonly string SampleBNF3 =
@"$grama ::= ?[$stuff $other-stuff]
$stuff ::= /bleh+/.4,6
$other-stuff ::= ""meh""
";
    }
}
