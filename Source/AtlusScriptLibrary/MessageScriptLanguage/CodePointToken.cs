using System;
using System.Collections.Generic;
using System.Linq;

namespace AtlusScriptLibrary.MessageScriptLanguage;

/// <summary>
/// Represents a code point token. This maps to a glyph on the game's font.
/// </summary>
public struct CodePointToken : IToken, IEquatable<CodePointToken>
{
    public IReadOnlyList<byte> Bytes { get; }

    /// <summary>
    /// Constructs a new code point token from a high and low surrogate byte.
    /// </summary>
    /// <param name="high">The high surrogate byte.</param>
    /// <param name="low">The low surrogate byte.</param>
    public CodePointToken(byte high, byte low)
    {
        Bytes = new List<byte>() { high, low };
    }

    /// <summary>
    /// Constructs a new code point token from a byte.
    /// </summary>
    public CodePointToken(byte value)
    {
        Bytes = new List<byte>() { value };
    }

    /// <summary>
    /// Constructs a new code point token from a list of bytes.
    /// </summary>
    public CodePointToken(List<byte> bytes)
    {
        Bytes = bytes;
    }

    /// <summary>
    /// Gets the token type of this token.
    /// </summary>
    TokenKind IToken.Kind => TokenKind.CodePoint;

    /// <summary>
    /// Converts this token to its string representation.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"[{string.Join(" ", Bytes.Select(x => x.ToString("X2")))}]";
    }

    public bool Equals(CodePointToken other) => Bytes.SequenceEqual(other.Bytes);
    public override bool Equals(object obj) => Equals(obj as IToken);

    public bool Equals(IToken other)
    {
        if (other is null || other.Kind != TokenKind.CodePoint) return false;
        return Equals((CodePointToken)other);
    }

    public static bool Equals(CodePointToken a, IToken b)
    {
        if (a == null) return b is null;
        return a.Equals(b);
    }
    public static bool Equals(IToken a, CodePointToken b) => Equals(b, a);

    public static bool operator ==(CodePointToken x, IToken y) => Equals(x, y);
    public static bool operator !=(CodePointToken x, IToken y) => !Equals(x, y);

    public static bool operator ==(IToken x, CodePointToken y) => Equals(y, x);
    public static bool operator !=(IToken x, CodePointToken y) => !Equals(y, x);

    public override int GetHashCode() => Bytes.GetHashCode();
}
