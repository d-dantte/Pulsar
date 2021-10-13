﻿using Axis.Pulsar.Importer.Common.Json.Models;
using Axis.Pulsar.Importer.Common.Json.Utils;
using Axis.Pulsar.Parser.Grammar;
using Axis.Pulsar.Parser.Input;
using Axis.Pulsar.Parser.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Axis.Pulsar.Importer.Common.Json
{
    public class RuleImporter : IRuleImporter
    {
        private static readonly JsonSerializerSettings SerializerSettings = new()
        {
            Converters = new List<JsonConverter>
            {
                new RuleJsonConverter()
            }
        };

        public RuleMap ImportRule(Stream inputStream)
        {
            using var reader = new StreamReader(inputStream);
            var json = reader.ReadToEnd();

            var grammar = JsonConvert.DeserializeObject<Grammar>(json, SerializerSettings);

            return ToRuleMap(grammar);
        }

        public async Task<RuleMap> ImportRuleAsync(Stream inputStream)
        {
            using var reader = new StreamReader(inputStream);
            var json = await reader.ReadToEndAsync();

            var grammar = JsonConvert.DeserializeObject<Grammar>(json, SerializerSettings);

            return ToRuleMap(grammar);
        }


        public static RuleMap ToRuleMap(Grammar grammar, RuleMap ruleMap = null)
        {
            var _ruleMap = ruleMap ?? new RuleMap();

            grammar.Productions.ForAll(production =>
            {
                _ruleMap.AddRule(
                    production.Name,
                    ToRule(production.Rule, _ruleMap),
                    production.Name.Equals(grammar.Language, StringComparison.InvariantCulture));
            });

            _ruleMap.Validate();

            return _ruleMap;
        }

        public static Rule ToRule(IRule rule, RuleMap ruleMap)
        {
            return rule switch
            {
                Literal l => new LiteralRule(l.Value, l.IsCaseSensitive),

                Pattern p => new PatternRule(
                    new Regex(p.Regex, p.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase),
                    new Cardinality(p.MinMatch, p.MaxMatch)),

                Ref r => new RuleRef(
                    r.Symbol,
                    new Cardinality(r.MinOccurs, r.MaxOCcurs)),

                Grouping g when g.Mode == GroupMode.Sequence => GroupingRule.Sequence(
                    new Cardinality(g.MinOccurs, g.MaxOccurs),
                    g.Rules.Select(gr => ToRule(gr, ruleMap)).ToArray()),

                Grouping g when g.Mode == GroupMode.Set => GroupingRule.Set(
                    new Cardinality(g.MinOccurs, g.MaxOccurs),
                    g.Rules.Select(gr => ToRule(gr, ruleMap)).ToArray()),

                Grouping g when g.Mode == GroupMode.Choice => GroupingRule.Choice(
                    new Cardinality(g.MinOccurs, g.MaxOccurs),
                    g.Rules.Select(gr => ToRule(gr, ruleMap)).ToArray()),

                _ => throw new Exception($"Invalid rule: {rule}")
            };
        }

        public static GroupingMode ToGroupingMode(GroupMode mode)
        {
            return mode switch
            {
                GroupMode.Choice => GroupingMode.Choice,
                GroupMode.Sequence => GroupingMode.Sequence,
                GroupMode.Set => GroupingMode.Set,
                _ => throw new Exception($"Invalid group-mode: {mode}")
            };
        }
    }
}
