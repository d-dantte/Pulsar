﻿using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Core.CST;
using Axis.Pulsar.Core.Grammar.Results;
using Axis.Pulsar.Core.Grammar.Nodes;
using Axis.Pulsar.Core.Utils;
using Axis.Pulsar.Core.Lang;

namespace Axis.Pulsar.Core.Grammar.Groups
{
    public class AtomicRuleRef : INodeRef<IAtomicRule>
    {
        public Cardinality Cardinality { get; }

        public IAtomicRule Ref { get; }

        public AtomicRuleRef(Cardinality cardinality, IAtomicRule rule)
        {
            Cardinality = cardinality.ThrowIfDefault(
                _ => new ArgumentException($"Invalid {nameof(cardinality)}: default"));
            Ref = rule ?? throw new ArgumentNullException(nameof(rule));
        }

        public static AtomicRuleRef Of(
            Cardinality cardinality,
            IAtomicRule rule)
            => new(cardinality, rule);

        public bool TryRecognize(
            TokenReader reader,
            ProductionPath parentPath,
            ILanguageContext context,
            out IRecognitionResult<INodeSequence> result)
        {
            ArgumentNullException.ThrowIfNull(reader);
            ArgumentNullException.ThrowIfNull(parentPath);

            var position = reader.Position;
            if (!Ref.TryRecognize(reader, parentPath, context, out var ruleResult))
            {
                reader.Reset(position);
                result = ruleResult
                    .TransformError(err => err switch
                    {
                        FailedRecognitionError
                        or PartialRecognitionError => GroupRecognitionError.Of((IRecognitionError)err, 0),
                        _ => err
                    })
                    .MapAs<INodeSequence>();

                return false;
            }

            result = ruleResult.Map(node => INodeSequence.Of(node));
            return true;
        }
    }
}
