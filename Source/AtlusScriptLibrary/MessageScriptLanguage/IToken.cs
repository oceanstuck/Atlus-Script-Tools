using System;
using System.Collections.Generic;

namespace AtlusScriptLibrary.MessageScriptLanguage;

/// <summary>
/// Common interface for message script line tokens.
/// </summary>
public interface IToken : IEquatable<IToken>
{
    /// <summary>
    /// Gets the type of token.
    /// </summary>
    TokenKind Kind { get; }
}

public class TokenComparer : EqualityComparer<IToken>
{
    public override bool Equals(IToken x, IToken y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return y is null;
        //if (x.Kind != y.Kind) return false;

        return x.Equals(y);
    }

    public override int GetHashCode(IToken obj) => obj.GetHashCode();
}