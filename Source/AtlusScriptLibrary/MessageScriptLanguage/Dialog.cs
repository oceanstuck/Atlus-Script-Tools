using System;
using System.Collections;
using System.Collections.Generic;

namespace AtlusScriptLibrary.MessageScriptLanguage;

/// <summary>
/// Common interface for message script dialog windows.
/// </summary>
public abstract class Dialog : IEnumerable<TokenText>, IEquatable<Dialog>
{
    /// <summary>
    /// Gets the dialog type of this dialog.
    /// </summary>
    public abstract DialogKind Kind { get; }

    /// <summary>
    /// Gets the text identifier of this dialog window.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets the list of lines contained in this dialog.
    /// </summary>
    public abstract List<TokenText> Lines { get; }

    public bool Equals(Dialog other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null || this.Kind != other.Kind) return false;

        switch (Kind)
        {
            case DialogKind.Message:
                return Equals(other as MessageDialog);
            case DialogKind.Selection:
                return Equals(other as SelectionDialog);
            default:
                throw new Exception("Invalid dialog kind");
        }
    }

    public abstract bool Equals(MessageDialog other);
    public abstract bool Equals(SelectionDialog other);

    public override bool Equals(object obj) => Equals(obj as Dialog);
    public static bool Equals(Dialog x, Dialog y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Equals(y);
    }

    public abstract IEnumerator<TokenText> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public abstract override int GetHashCode();

    public static bool operator ==(Dialog x, Dialog y) => Equals(x, y);
    public static bool operator !=(Dialog x, Dialog y) => !Equals(x, y);
}