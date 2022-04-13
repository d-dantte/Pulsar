﻿using Axis.Pulsar.Parser.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Axis.Pulsar.Parser.Tests")]

namespace Axis.Pulsar.Parser.Parsers
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGrammarContext
    {
        public string RootName { get; }

        public IEnumerable<string> ProductionNames { get; }

        public IParser RootParser();

        public IParser GetParser(string name);
    }

    /// <summary>
    /// 
    /// </summary>
    public class GrammarContext: IGrammarContext
    {
        private readonly Dictionary<string, IParser> _parserMap = new();

        public string RootName { get; }

        public IEnumerable<string> ProductionNames => _parserMap.Keys.AsEnumerable();

        public IParser RootParser() => GetParser(RootName);

        public IParser GetParser(string name) => _parserMap[name];

        public GrammarContext(Grammar.Grammar ruleMap)
        {
            ruleMap
                .Productions()
                .Select(BuildProductionParser)
                .ForAll(map => _parserMap.Add(map));
            RootName = ruleMap.RootSymbol;
        }

        internal KeyValuePair<string, IParser> BuildProductionParser(KeyValuePair<string, IRule> production)
        {
            return new(
                production.Key,
                new ProductionParser(
                    production.Key,
                    BuildRuleParser(production.Value)));
        }


        internal RuleParser BuildRuleParser(IRule rule)
        {
            return rule switch
            {
                PatternRule p => new PatternMatcherParser(p),

                LiteralRule l => new LiteralParser(l),

                RuleRef r => new RefParser(
                    cardinality: r.Cardinality,
                    @ref: r)
                    .SetGrammarContext(this),

                SymbolExpressionRule n when n.GroupingMode == GroupingMode.Choice => new ChoiceParser(
                    n.Cardinality,
                    n.Rules.Select(BuildRuleParser).ToArray()),

                SymbolExpressionRule n when n.GroupingMode == GroupingMode.Sequence => new SequenceParser(
                    n.Cardinality,
                    n.Rules.Select(BuildRuleParser).ToArray()),

                SymbolExpressionRule n when n.GroupingMode == GroupingMode.Set => new SetParser(
                    n.Cardinality,
                    n.Rules.Select(BuildRuleParser).ToArray()),

                _ => throw new ArgumentException($"Invalid rule type: {typeof(IGrammarContext)}")
            };
        }
    }
}
