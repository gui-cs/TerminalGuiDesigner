using NStack;
using System.CodeDom;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.ToCode;

public abstract class ToCodeBase
{
    protected void AddAddToViewStatement(CodeDomArgs args, Design d)
    {
        // Add it to the view 
        var callAdd = new CodeMethodInvokeExpression();
        callAdd.Method.TargetObject = new CodeThisReferenceExpression();
        callAdd.Method.MethodName = "Add";
        callAdd.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), d.FieldName));
        args.InitMethod.Statements.Add(callAdd);
    }

    protected CodeMemberField AddFieldToClass(CodeDomArgs args, Design d)
    {
        return AddFieldToClass(args, d.View.GetType(), d.FieldName);
    }
    protected CodeMemberField AddFieldToClass(CodeDomArgs args, Type type, string fieldName)
    {
        // Create a private field for it
        var field = new CodeMemberField(type,fieldName);
        args.Class.Members.Add(field);

        return field;
    }

    protected void AddLocalFieldToMethod(CodeDomArgs args, Type type, string name)
    {
        var declare = new CodeVariableDeclarationStatement(type, name);
        args.InitMethod.Statements.Add(declare);
    }

    protected void AddConstructorCall(CodeDomArgs args, Design d, params CodeExpression[] parameters)
    {
        AddConstructorCall($"this.{d.FieldName}", d.View.GetType(), args, parameters);
    }
    protected void AddConstructorCall(string fullySpecifiedFieldName,Type typeToConstruct, CodeDomArgs args, params CodeExpression[] parameters)
    {
        // Construct it
        var constructLhs = new CodeFieldReferenceExpression();
        constructLhs.FieldName = fullySpecifiedFieldName;
        var constructRhs = new CodeObjectCreateExpression(typeToConstruct, parameters);
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
    protected void AddPropertyAssignment(CodeDomArgs args,Design d, string propertyName, object? value)
    {
        AddPropertyAssignment(args, $"this.{d.FieldName}", propertyName, value);
    }


    /// <summary>
    /// Adds a line "this.someField.Text = "Heya"
    /// </summary>
    /// <param name="args"></param>
    /// <param name="fullySpecifiedFieldName">The field or local variable upon which you want to set the <paramref name="propertyName"/></param>
    /// <param name="propertyName">Field or property to change</param>
    /// <param name="value">The new value that should be assigned.  Must be a primitive.</param>
    protected void AddPropertyAssignment(CodeDomArgs args,string fullySpecifiedFieldName, string propertyName, object? value)
    {
        var setLhs = new CodeFieldReferenceExpression();
        setLhs.FieldName = $"{fullySpecifiedFieldName}.{propertyName}";
        CodeExpression setRhs = GetRhsForValue(value);

        var assignStatement = new CodeAssignStatement();
        assignStatement.Left = setLhs;
        assignStatement.Right = setRhs;
        args.InitMethod.Statements.Add(assignStatement);
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
