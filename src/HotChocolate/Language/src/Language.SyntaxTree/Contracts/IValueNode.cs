using System;

namespace HotChocolate.Language;

/// <summary>
/// A GraphQL value literal.
/// </summary>
public interface IValueNode : ISyntaxNode, IEquatable<IValueNode>
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    object? Value { get; }
}
