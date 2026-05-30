using System;

namespace AtlusScriptLibrary.MessageScriptLanguage;

public sealed class VariableSpeaker : Speaker, IEquatable<VariableSpeaker>
{
    /// <summary>
    /// Gets the index of the speaker name variable.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Constructs a new variable speaker.
    /// </summary>
    /// <param name="index">The index of the speaker name variable.</param>
    public VariableSpeaker(int index)
    {
        Index = index;
    }

    /// <summary>
    /// Converts this speaker to its string representation.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"<variable name {Index}>";
    }

    public override bool Equals(NamedSpeaker other) => false;
    public override bool Equals(VariableSpeaker other) => Index == other.Index;

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null || obj is not VariableSpeaker) return false;
        return Equals(obj as VariableSpeaker);
    }

    public override int GetHashCode() => HashCode.Combine(Index);

    /// <summary>
    /// Gets the speaker type.
    /// </summary>
    public override SpeakerKind Kind => SpeakerKind.Variable;
}