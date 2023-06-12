using Microsoft.CSharp;
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
    /// as it is declared in the .Designer.cs file e.g.:
    /// <code>public partial class MyWindow : Terminal.Gui.Window</code>
    /// <para>Use this property to add new fields for each sub-view and subcomponent needed
    /// by the class.</para>
    /// </summary>
    public CodeTypeDeclaration Class { get; }

    /// <summary>
    /// Gets the CodeDOM object representing the InitializeComponent() method
    /// of the .Designer.cs file e.g.
    /// <code>private void InitializeComponent()</code>
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
    /// To check against C# reserved keywords.
    /// </summary>
    private static CSharpCodeProvider cSharpCodeProvider = new();

    /// <summary>
    /// Removes all invalid bits of <paramref name="name"/> such that it could be used
    /// as a class member.  This includes removing spaces, preceding numbers etc.
    /// </summary>
    /// <remarks>
    /// Passing null or empty <paramref name="name"/> will return the value "blank".
    /// </remarks>
    /// <param name="name">Value you want to turn into a valid field name.</param>
    /// <param name="isPublic"><see langword="true"/> if the name is for a public member
    /// so should have a capital letter at start. <see langword="false"/> for private members
    /// which should start with a lower case letter.</param>
    /// <returns><paramref name="name"/>.</returns>
    public static string MakeValidFieldName(string? name, bool isPublic = false)
    {
        name = string.IsNullOrWhiteSpace(name) ? "blank" : name;

        // if space is removed and next character is not upper then upper it
        // to produce camel casing (e.g. "bob is great" becomes "bobIsGreat")
        name = Regex.Replace(
            name,
            "(\\s[a-z])",
            (m) => char.ToUpper(m.Groups[1].Value[1]).ToString());

        // replace any remaining whitespace, punctuation etc
        name = Regex.Replace(name, "\\W", string.Empty);

        // remove leading digits
        name = Regex.Replace(name, "^\\d+", string.Empty);

        if (isPublic)
        {
            // if public and starts with lower case letter, replace it with upper
            name = Regex.Replace(
                name,
                "^([a-z])",
                (m) => char.ToUpper(m.Groups[1].Value[0]).ToString());
        }
        else
        {
            // if private and starts with an upper case letter, replace it with lower
            name = Regex.Replace(
                name,
                "^([A-Z])",
                (m) => char.ToLower(m.Groups[1].Value[0]).ToString());
        }

        // reject name if it is a C# reserved keyword
        if (!cSharpCodeProvider.IsValidIdentifier(name))
            return "blank";

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
    /// <param name="isPublic"><see langword="true"/> if the name is for a public member
    /// so should have a capital letter at start. <see langword="false"/> for private members
    /// which should start with a lower case letter.</param>
    /// <returns>Valid non duplicate non null member name.</returns>
    public string GetUniqueFieldName(string? name, bool isPublic = false)
    {
        name = MakeValidFieldName(name, isPublic);

        name = name.MakeUnique(this.FieldNamesUsed);

        this.FieldNamesUsed.Add(name);

        return name;
    }
}
