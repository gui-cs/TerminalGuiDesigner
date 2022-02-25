using System.CodeDom;
using Terminal.Gui;

namespace TerminalGuiDesigner;

public  class DesignLabel : Design<Label>
{

    public DesignLabel(string fieldName, Label label) : base(fieldName,label)
    {
    }

    public void ToCode(CodeTypeDeclaration addTo, CodeMemberMethod initMethod)
    {
        AddFieldToClass(addTo);
        AddConstructorCall(initMethod);

        AddPropertyAssignment(initMethod,nameof(Label.Text),View.Text);

        AddAddToViewStatement(initMethod);
    }

    
}