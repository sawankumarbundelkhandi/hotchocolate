using System;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Neo4J.Language;
using HotChocolate.Language;
using HotChocolate.Types;

namespace HotChocolate.Data.Neo4J.Filtering;

/// <summary>
/// This filter operation handler maps a NotLowerThan operation field to a
/// <see cref="FilterDefinition{TDocument}"/>
/// </summary>
public class Neo4JComparableNotLowerThanHandler
    : Neo4JComparableOperationHandler
{
    public Neo4JComparableNotLowerThanHandler(InputParser inputParser)
        : base(inputParser)
    {
        CanBeNull = false;
    }

    /// <inheritdoc />
    protected override int Operation => DefaultFilterOperations.NotLowerThan;

    /// <inheritdoc />
    public override Condition HandleOperation(
        Neo4JFilterVisitorContext context,
        IFilterOperationField field,
        IValueNode value,
        object? parsedValue)
    {
        if (parsedValue is null)
        {
            throw new InvalidOperationException();
        }

        return context
            .GetNode()
            .Property(context.GetNeo4JFilterScope().GetPath())
            .LessThan(Cypher.LiteralOf(parsedValue))
            .Not();
    }
}