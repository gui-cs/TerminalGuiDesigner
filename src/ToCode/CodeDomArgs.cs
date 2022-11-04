using System.CodeDom;
using System.Text.RegularExpressions;

namespace TerminalGuiDesigner.ToCode;

public class CodeDomArgs
{
    /// <summary>
    /// The root class that is being designed e.g. MyView as it is declared
    /// in the .Designer.cs file.  Use this property to add new fields for
    /// each subview and subcomponent needed by the class
    /// </summary>
    public CodeTypeDeclaration Class;

    /// <summary>
    /// The InitializeComponent() method of the .Designer.cs file
    /// </summary>
    public CodeMemberMethod InitMethod;

    /// <summary>
    /// The members that have already been outputted.  Prevents
    /// any possibility of duplicate adding to the .Designer.cs
    /// </summary>
    public HashSet<Design> OutputAlready = new();

    /// <summary>
    /// Record of all declared fields / local variable names. Prevents
    /// any duplicate field names being generated.
    /// </summary>
    public HashSet<string> FieldNamesUsed = new();

    public CodeDomArgs(CodeTypeDeclaration rootClass, CodeMemberMethod initMethod)
    {
        this.Class = rootClass;
        this.InitMethod = initMethod;
    }

    public static string MakeValidFieldName(string? name)
    {
        name = string.IsNullOrWhiteSpace(name) ? "empty" : name;
        name = Regex.Replace(name, "\\W", "");

        // remove leading digits
        name = Regex.Replace(name, "^\\d+", "");

        return name;
    }

    /// <summary>
    /// Returns a unique field name based on the passed value.
    /// Removes non word characters and applies a numerical
    /// suffix if name collides with an existing field
    /// </summary>
    public string GetUniqueFieldName(string? name)
    {
        name = MakeValidFieldName(name);

        if (!FieldNamesUsed.Contains(name))
        {
            FieldNamesUsed.Add(name);
            return name;
        }

        // name is already used, add a number
        int number = 2;
        while (FieldNamesUsed.Contains(name + number))
        {
            // menu2 is taken, try menu3 etc
            number++;
        }

        // found a unique one
        FieldNamesUsed.Add(name + number);
        return name + number;
    }
}
