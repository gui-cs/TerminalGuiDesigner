using NStack;
using System.CodeDom;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.ToCode;

public abstract class ToCodeBase
{
    protected void AddAddToViewStatement(Design d, CodeDomArgs args)
    {
        // Add it to the view 
        var callAdd = new CodeMethodInvokeExpression();
        callAdd.Method.TargetObject = new CodeThisReferenceExpression();
        callAdd.Method.MethodName = "Add";
        callAdd.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), d.FieldName));
        args.InitMethod.Statements.Add(callAdd);
    }

    protected CodeMemberField AddFieldToClass(Design d, CodeDomArgs args)
    {
        return AddFieldToClass(args, d.FieldName, d.View.GetType());
    }
    protected CodeMemberField AddFieldToClass(CodeDomArgs args, string fieldName, Type type)
    {
        // Create a private field for it
        var field = new CodeMemberField();
        field.Name = fieldName;
        field.Type = new CodeTypeReference(type);

        args.Class.Members.Add(field);

        return field;
    }

    protected void AddConstructorCall(Design d, CodeDomArgs args, params CodeExpression[] parameters)
    {
        // Construct it
        var constructLhs = new CodeFieldReferenceExpression();
        constructLhs.FieldName = $"this.{d.FieldName}";
        var constructRhs = new CodeObjectCreateExpression(d.View.GetType(), parameters);
        var constructAssign = new CodeAssignStatement();
        constructAssign.Left = constructLhs;
        constructAssign.Right = constructRhs;
        args.InitMethod.Statements.Add(constructAssign);
    }

    /// <summary>
    /// Adds a statement to the InitializeComponent method like:
    /// <code>this.mylabel.Text = "hello"</code>
    /// </summary>
    /// <param name="d">The designed view to add</param>
    /// <param name="initMethod">The InitializeComponent method</param>
    /// <param name="propertyName">The property to set e.g. Text</param>
    /// <param name="value">The value to assign to the property e.g. "hello"</param>
    protected void AddPropertyAssignment(Design d, CodeDomArgs args, string propertyName, object? value)
    {
        var setLhs = new CodeFieldReferenceExpression();
        setLhs.FieldName = $"this.{d.FieldName}.{propertyName}";
        CodeExpression setRhs = GetRhsForValue(value);

        var setTextAssign = new CodeAssignStatement();
        setTextAssign.Left = setLhs;
        setTextAssign.Right = setRhs;
        args.InitMethod.Statements.Add(setTextAssign);
    }

    private CodeExpression GetRhsForValue(object? value)
    {
        if (value is PropertyDesign pd)
        {
            return new CodeSnippetExpression(pd.GetCodeWithParameters());
        }

        return new CodePrimitiveExpression(value.ToPrimitive());
    }

}
