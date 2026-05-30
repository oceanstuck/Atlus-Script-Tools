using System;
using System.Collections;
using System.Collections.Generic;

namespace AtlusScriptLibrary.MessageScriptLanguage;

/// <summary>
/// Represents a named dialogue message speaker.
/// </summary>
public sealed class NamedSpeaker : Speaker, IEnumerable<IToken>, IEquatable<NamedSpeaker>
{
    /// <summary>
    /// Gets the name of the speaker.
    /// </summary>
    public TokenText Name { get; }

    /// <summary>
    /// Constructs a new speaker.
    /// </summary>
    /// <param name="name">The name of the speaker.</param>
    public NamedSpeaker(TokenText name)
    {
        Name = name;
    }

    public NamedSpeaker(string name)
    {
        Name = new TokenTextBuilder()
            .AddString(name)
            .Build();
    }

    /// <summary>
    /// Converts this speaker to its string representation.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string str = string.Empty;

        if (Name is not null && Name.Tokens.Count > 0)
        {
            foreach (var token in Name.Tokens)
                str += token + " ";
        }

        return str;
    }

    public IEnumerator<IToken> GetEnumerator()
    {
        return ((IEnumerable<IToken>)Name).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<IToken>)Name).GetEnumerator();
    }

    public override bool Equals(NamedSpeaker other) => Name == other.Name;
    public override bool Equals(VariableSpeaker other) => false;

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null || obj is not NamedSpeaker) return false;
        return Equals(obj as NamedSpeaker);
    }

    public override int GetHashCode() => Name.GetHashCode();

    /// <summary>
    /// Gets the speaker type.
    /// </summary>
    public override SpeakerKind Kind => SpeakerKind.Named;
}