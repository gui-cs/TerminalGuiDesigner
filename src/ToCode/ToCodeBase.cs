using System.CodeDom;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

public abstract class ToCodeBase
{
    protected void AddAddToViewStatement(CodeDomArgs args, Design d, CodeExpression parentView)
    {
        // Add it to the view 
        var callAdd = new CodeMethodInvokeExpression();
        callAdd.Method.TargetObject = parentView;

        callAdd.Method.MethodName = nameof(View.Add);
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
    /// Adds a line "this.someField.Text = "Heya"
    /// </summary>
    /// <param name="args"></param>
    /// <param name="fullySpecifiedFieldName">The code that should go on the left of the equals</param>
    /// <param name="primativeValue">The new value that should be assigned.  Must be a primitive.</param>
    protected void AddPropertyAssignment(CodeDomArgs args, string lhs, object primativeValue)
    {
        AddPropertyAssignment(args, lhs, new CodePrimitiveExpression(primativeValue.ToPrimitive()));
    }
    /// <summary>
    /// Adds a line "this.someField.Text = "Heya"
    /// </summary>
    /// <param name="args"></param>
    /// <param name="fullySpecifiedFieldName">The field or local variable upon which you want to set the <paramref name="propertyName"/></param>
    /// <param name="propertyName">Field or property to change</param>
    protected void AddPropertyAssignment(CodeDomArgs args,string lhs, CodeExpression rhs)
    {
        var setLhs = new CodeFieldReferenceExpression();
        setLhs.FieldName = lhs;

        var assignStatement = new CodeAssignStatement();
        assignStatement.Left = setLhs;
        assignStatement.Right = rhs;
        args.InitMethod.Statements.Add(assignStatement);
    }


}
