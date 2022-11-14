using System.CodeDom;
using System.Text.RegularExpressions;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// Argument for tracking the process of saving Designer state to a .Designer.cs
/// file.  This includes tracking what <see cref="FieldNamesUsed"/> as well as
/// where to put new code (see <see cref="InitMethod"/>) etc.
/// </summary>
public class CodeDomArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CodeDomArgs"/> class.
    /// </summary>
    /// <param name="rootClass">The CodeDOM object representing the root class
    /// being designed (see <see cref="Class"/>).</param>
    /// <param name="initMethod">The CodeDOM object representing the
    /// InitializeComponent method in .Designer.cs (see <see cref="InitMethod"/>).</param>
    public CodeDomArgs(CodeTypeDeclaration rootClass, CodeMemberMethod initMethod)
    {
        this.Class = rootClass;
        this.InitMethod = initMethod;
    }

    /// <summary>
    /// Gets the CodeDOM object representing the root class that is being designed
    /// (e.g. `public partial class MyView : View`) as it is declared in the .Designer.cs file.
    /// Use this property to add new fields for each sub-view and subcomponent needed
    /// by the class.
    /// </summary>
    public CodeTypeDeclaration Class { get; }

    /// <summary>
    /// Gets the CodeDOM object representing the InitializeComponent() method
    /// of the .Designer.cs file.
    /// </summary>
    public CodeMemberMethod InitMethod { get; }

    /// <summary>
    /// Gets all the members that have already been outputted.  Prevents
    /// any possibility of duplicate adding to the .Designer.cs.
    /// </summary>
    public HashSet<Design> OutputAlready { get; } = new();

    /// <summary>
    /// Gets all the declared fields / local variable names. Prevents
    /// creating 2+ members in the .Designer.cs with the same name.
    /// </summary>
    public HashSet<string> FieldNamesUsed { get; } = new();

    /// <summary>
    /// Removes all invalid bits of <paramref name="name"/> such that it could be used
    /// as a class member.  This includes removing spaces, preceding numbers etc.
    /// </summary>
    /// <remarks>
    /// Passing null or empty <paramref name="name"/> will return the value "blank".
    /// </remarks>
    /// <param name="name">Value you want to turn into a valid field name.</param>
    /// <returns><paramref name="name"/>.</returns>
    public static string MakeValidFieldName(string? name)
    {
        name = string.IsNullOrWhiteSpace(name) ? "blank" : name;
        name = Regex.Replace(name, "\\W", string.Empty);

        // remove leading digits
        name = Regex.Replace(name, "^\\d+", string.Empty);

        return name;
    }

    /// <summary>
    /// Returns a unique field name based on the passed value.
    /// Removes non word characters and applies a numerical
    /// suffix if name collides with an existing field (see <see cref="FieldNamesUsed"/>).
    /// </summary>
    /// <remarks>
    /// Passing null or empty <paramref name="name"/> will return the value "blank".
    /// </remarks>
    /// <param name="name">String to convert to a valid non duplicate
    /// member name.</param>
    /// <returns>Valid non duplicate non null member name.</returns>
    public string GetUniqueFieldName(string? name)
    {
        name = MakeValidFieldName(name);

        name = name.MakeUnique(this.FieldNamesUsed);

        this.FieldNamesUsed.Add(name);

        return name;
    }
}
