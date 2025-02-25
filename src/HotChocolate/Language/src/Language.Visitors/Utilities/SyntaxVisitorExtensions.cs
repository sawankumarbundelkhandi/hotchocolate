namespace HotChocolate.Language.Visitors;

public static class SyntaxVisitorExtensions
{
    private static readonly EmptySyntaxVisitorContext _empty = new();

    public static ISyntaxVisitorAction Visit(this ISyntaxVisitor visitor, ISyntaxNode node)
        => visitor.Visit(node, _empty);

    private sealed class EmptySyntaxVisitorContext : ISyntaxVisitorContext
    {
    }
}
