﻿using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Core.CST;
using Axis.Pulsar.Core.Grammar.Errors;
using Axis.Pulsar.Core.Utils;
using System.Collections.Immutable;

namespace Axis.Pulsar.Core.Grammar.Groups
{
    /// <summary>
    /// 
    /// </summary>
    public class Choice : IGroup
    {
        public ImmutableArray<IGroupElement> Elements { get; }

        public Cardinality Cardinality { get; }


        public Choice(Cardinality cardinality, params IGroupElement[] elements)
        {
            Cardinality = cardinality;
            Elements = elements
                .ThrowIfNull(new ArgumentNullException(nameof(elements)))
                .ThrowIfAny(e => e is null, new ArgumentException($"Invalid element: null"))
                .ApplyTo(ImmutableArray.CreateRange);
        }

        public static Choice Of(
            Cardinality cardinality,
            params IGroupElement[] elements)
            => new(cardinality, elements);

        public bool TryRecognize(
            TokenReader reader,
            ProductionPath parentPath,
            ILanguageContext context,
            out IResult<NodeSequence> result)
        {
            ArgumentNullException.ThrowIfNull(reader);
            ArgumentNullException.ThrowIfNull(parentPath);

            var position = reader.Position;
            foreach(var element in Elements)
            {
                if (element.Cardinality.TryRepeat(reader, parentPath, context, element, out result))
                    return true;

                reader.Reset(position);

                if (result.IsErrorResult(out GroupRecognitionError gre)
                    && gre.Cause is FailedRecognitionError)
                    continue;
                
                else return false;
            }

            reader.Reset(position);
            result = FailedRecognitionError
                .Of(parentPath, position)
                .ApplyTo(GroupRecognitionError.Of)
                .ApplyTo(Result.Of<NodeSequence>);

            return false;
        }
    }
}
