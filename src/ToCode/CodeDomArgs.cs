using System.CodeDom;

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

    public CodeDomArgs(CodeTypeDeclaration rootClass, CodeMemberMethod initMethod)
    {
        this.Class = rootClass;
        this.InitMethod = initMethod;
    }
}
