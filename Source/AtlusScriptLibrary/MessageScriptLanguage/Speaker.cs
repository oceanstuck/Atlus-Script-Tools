using System;
using System.Collections.Generic;
namespace AtlusScriptLibrary.MessageScriptLanguage;

/// <summary>
/// Common interface for dialogue message speakers.
/// </summary>
public abstract class Speaker : IEquatable<Speaker>
{
    /// <summary>
    /// Gets the speaker type.
    /// </summary>
    public abstract SpeakerKind Kind { get; }

    public bool Equals(Speaker other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null || other.Kind != Kind) return false;

        switch (Kind)
        {
            case SpeakerKind.Named:
                return Equals(other as NamedSpeaker);
            case SpeakerKind.Variable:
                return Equals(other as VariableSpeaker);
            default:
                throw new Exception("Unrecognized speaker kind");
        }
    }

    public abstract bool Equals(NamedSpeaker other);
    public abstract bool Equals(VariableSpeaker other);

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null || obj is not Speaker) return false;
        return Equals(obj as Speaker);
    }

    public static bool Equals(Speaker x, Speaker y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return y is null;
        return x.Equals(y);
    }

    public abstract override int GetHashCode();

    public static bool operator ==(Speaker x, Speaker y) => Equals(x, y);
    public static bool operator !=(Speaker x, Speaker y) => !Equals(x, y);
}