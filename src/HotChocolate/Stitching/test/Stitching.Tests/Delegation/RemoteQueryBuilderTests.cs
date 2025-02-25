using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using HotChocolate.Language;
using HotChocolate.Language.Utilities;
using HotChocolate.Stitching.Processing;
using Snapshooter.Xunit;
using Xunit;

namespace HotChocolate.Stitching.Delegation;

public class RemoteQueryBuilderTests
{
    [Fact]
    public void BuildRemoteQuery()
    {
        // arrange
        IImmutableStack<SelectionPathComponent> path =
            SelectionPathParser.Parse("a.b.c.d(a: $fields:bar)");

        DocumentNode initialQuery =
            Utf8GraphQLParser.Parse(
                @"{
                        foo {
                          bar {
                            baz {
                              ... on Baz {
                                qux
                              }
                            }
                          }
                        }
                      }
                    ");

        FieldNode field = initialQuery.Definitions
            .OfType<OperationDefinitionNode>().Single()
            .SelectionSet.Selections
            .OfType<FieldNode>().Single()
            .SelectionSet.Selections
            .OfType<FieldNode>().Single();

        // act
        DocumentNode newQuery = RemoteQueryBuilder.New()
            .SetOperation(null, OperationType.Query)
            .SetSelectionPath(path)
            .SetRequestField(field)
            .AddVariable("__fields_bar", new NamedTypeNode(null, new NameNode("String")))
            .Build("abc", new Dictionary<(NameString Type, NameString Schema), NameString>());

        // assert
        newQuery.Print().MatchSnapshot();
    }

    [Fact]
    public void BuildRemoteQueryCanOverrideOperationName()
    {
        // arrange
        IImmutableStack<SelectionPathComponent> path =
            SelectionPathParser.Parse("a.b.c.d(a: $fields:bar)");

        DocumentNode initialQuery =
            Utf8GraphQLParser.Parse(
                @"{
                        foo {
                          bar {
                            baz {
                              ... on Baz {
                                qux
                              }
                            }
                          }
                        }
                      }
                    ");

        FieldNode field = initialQuery.Definitions
            .OfType<OperationDefinitionNode>().Single()
            .SelectionSet.Selections
            .OfType<FieldNode>().Single()
            .SelectionSet!.Selections
            .OfType<FieldNode>().Single();


        // act
        DocumentNode newQuery = RemoteQueryBuilder.New()
            .SetOperation(new NameNode(
                nameof(BuildRemoteQueryCanOverrideOperationName)),
                OperationType.Query)
            .SetSelectionPath(path)
            .SetRequestField(field)
            .AddVariable("__fields_bar", new NamedTypeNode(null, new NameNode("String")))
            .Build("abc", new Dictionary<(NameString Type, NameString Schema), NameString>());

        // assert
        newQuery.Print().MatchSnapshot();
    }
}
