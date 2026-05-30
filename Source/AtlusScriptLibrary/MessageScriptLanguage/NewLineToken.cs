using System;

namespace AtlusScriptLibrary.MessageScriptLanguage;

/// <summary>
/// Represents a single newline token.
/// </summary>
public class NewLineToken : IToken, IEquatable<NewLineToken>
{
    /// <summary>
    /// The constant value of a newline token.
    /// </summary>
    public const byte ASCIIValue = 0x0A;

    /// <summary>
    /// Gets the type of this token.
    /// </summary>
    public TokenKind Kind => TokenKind.NewLine;

    /// <summary>
    /// Converts this token to its string reprentation.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "<new line>";
    }

    public bool Equals(NewLineToken other) => true;
    public override bool Equals(object obj) => obj is NewLineToken;

    public bool Equals(IToken other) => other.Kind == TokenKind.NewLine;

    public override int GetHashCode() => HashCode.Combine(Kind, ASCIIValue);

    public static bool Equals(NewLineToken a, IToken b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null) return b is null;
        return a.Equals(b);
    }
    public static bool Equals(IToken a, NewLineToken b) => Equals(b, a);

    public static bool operator ==(NewLineToken x, IToken y) => Equals(x, y);
    public static bool operator !=(NewLineToken x, IToken y) => !Equals(x, y);

    public static bool operator ==(IToken x, NewLineToken y) => Equals(y, x);
    public static bool operator !=(IToken x, NewLineToken y) => !Equals(y, x);
}
