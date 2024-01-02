namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// Interface for generic class <see cref="TreeObjectsProperty{T}"/>
/// </summary>
public interface ITreeObjectsProperty
{
    /// <summary>
    /// Returns true if the collection currently held on the property is empty
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty();
}
