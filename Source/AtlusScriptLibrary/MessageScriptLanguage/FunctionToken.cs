using System;
using System.Collections.Generic;
using System.Linq;

namespace AtlusScriptLibrary.MessageScriptLanguage;

/// <summary>
/// Represents a message script function token.
/// </summary>
public struct FunctionToken : IToken, IEquatable<FunctionToken>
{
    /// <summary>
    /// Gets the function table index.
    /// </summary>
    public int FunctionTableIndex { get; }

    /// <summary>
    /// Gets the function index within the table.
    /// </summary>
    public int FunctionIndex { get; }

    /// <summary>
    /// Gets the list of arguments.
    /// </summary>
    public List<ushort> Arguments { get; } = new();

    /// <summary>
    /// Prefixes every message function with an 0xFE byte (Persona 3 Reload)
    /// </summary>
    public bool UseIdentifierByte { get; }

    /// <summary>
    /// Constructs a new message script function token with no arguments.
    /// </summary>
    /// <param name="functionTableIndex">The function table index.</param>
    /// <param name="functionIndex">The function index within the table.</param>
    public FunctionToken(int functionTableIndex, int functionIndex, bool useIdentifierByte)
    {
        FunctionTableIndex = functionTableIndex;
        FunctionIndex = functionIndex;
        Arguments = new List<ushort>();
        UseIdentifierByte = useIdentifierByte;
    }

    /// <summary>
    /// Constructs a new message script function token with arguments.
    /// </summary>
    /// <param name="functionTableIndex">The function table index.</param>
    /// <param name="functionIndex">The function index within the table.</param>
    /// <param name="arguments">The function arguments.</param>
    public FunctionToken(int functionTableIndex, int functionIndex, List<ushort> arguments, bool useIdentifierByte)
    {
        FunctionTableIndex = functionTableIndex;
        FunctionIndex = functionIndex;
        Arguments = arguments;
        UseIdentifierByte = useIdentifierByte;
    }

    /// <summary>
    /// Constructs a new message script function token with arguments.
    /// </summary>
    /// <param name="functionTableIndex">The function table index.</param>
    /// <param name="functionIndex">The function index within the table.</param>
    /// <param name="arguments">The function arguments.</param>
    public FunctionToken(int functionTableIndex, int functionIndex, bool useIdentifierByte, params ushort[] arguments)
    {
        FunctionTableIndex = functionTableIndex;
        FunctionIndex = functionIndex;
        Arguments = arguments.ToList();
        UseIdentifierByte = useIdentifierByte;
    }

    /// <summary>
    /// Converts this message script function token to its string representation.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string str = $"func_{FunctionTableIndex}_{FunctionIndex}(";
        for (int i = 0; i < Arguments.Count; i++)
        {
            str += Arguments[i];
            if (i + 1 != Arguments.Count)
                str += ",";
        }
        str += ")";

        return str;
    }

    public bool Equals(FunctionToken other)
    {
        if (FunctionTableIndex != other.FunctionTableIndex) return false;
        if (FunctionIndex != other.FunctionIndex) return false;
        if (UseIdentifierByte != other.UseIdentifierByte) return false;
        return Arguments.SequenceEqual(other.Arguments);
    }

    public override bool Equals(object obj) => Equals(obj as IToken);

    public override int GetHashCode() => HashCode.Combine(FunctionTableIndex, FunctionIndex, UseIdentifierByte, Arguments);

    public bool Equals(IToken other)
    {
        if (other is null || other.Kind != TokenKind.Function) return false;
        return Equals((FunctionToken)other);
    }

    public static bool Equals(FunctionToken a, IToken b)
    {
        if (a == null) return b is null;
        return a.Equals(b);
    }
    public static bool Equals(IToken a, FunctionToken b) => Equals(b, a);

    public static bool operator ==(FunctionToken x, IToken y) => Equals(x, y);
    public static bool operator !=(FunctionToken x, IToken y) => !Equals(x, y);

    public static bool operator ==(IToken x, FunctionToken y) => Equals(y, x);
    public static bool operator !=(IToken x, FunctionToken y) => !Equals(y, x);

    /// <summary>
    /// Gets the token type.
    /// </summary>
    TokenKind IToken.Kind => TokenKind.Function;
}
