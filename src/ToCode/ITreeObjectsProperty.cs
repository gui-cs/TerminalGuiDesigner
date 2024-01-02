namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// Interface for generic class <see cref="TreeObjectsProperty{T}"/>
/// </summary>
public interface ITreeObjectsProperty
{
    /// <summary>
    /// Returns True if the T type the property was constructed with is well
    /// supported by the designer (i.e. user can pick objects for their tree).
    /// </summary>
    /// <returns></returns>
    bool IsSupported();

    /// <summary>
    /// Returns true if the collection currently held on the property is empty
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty();
}
