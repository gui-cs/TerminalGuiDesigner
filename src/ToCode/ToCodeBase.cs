using System.CodeDom;
using Terminal.Gui;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// Abstract base class for classes that want to add content to
/// the .Designer.cs file during saving.
/// </summary>
public abstract class ToCodeBase
{
    /// <summary>
    /// Adds a call to <see cref="View.Add(View)"/>.
    /// </summary>
    /// <param name="args">State of the .Designer.cs file.</param>
    /// <param name="d">Wrapper for a <see cref="View"/>.</param>
    /// <param name="parentView">CodeDOM expression that describes the Parent of
    /// <paramref name="d"/> e.g. <see cref="CodeThisReferenceExpression"/>.</param>
    protected void AddAddToViewStatement(CodeDomArgs args, Design d, CodeExpression parentView)
    {
        this.AddMethodCall(
            args,
            parentView,
            nameof(View.Add),
            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), d.FieldName));
    }

    /// <summary>
    /// Adds a call to <paramref name="methodName"/> invoked on <paramref name="caller"/> passing
    /// the parameters <paramref name="methodArguments"/>.
    /// </summary>
    /// <param name="args">State of the .Designer.cs file.</param>
    /// <param name="caller">Code expression for the field/variable the method exists on
    /// e.g. <see cref="CodeThisReferenceExpression"/>.</param>
    /// <param name="methodName">Name of the method to invoke on <paramref name="caller"/>.</param>
    /// <param name="methodArguments">CodeDOM expressions to satisfy all parameters of <paramref name="methodName"/>.</param>
    protected void AddMethodCall(CodeDomArgs args, CodeExpression caller, string methodName, params CodeExpression[] methodArguments)
    {
        var call = new CodeMethodInvokeExpression();
        call.Method.TargetObject = caller;

        call.Method.MethodName = methodName;

        call.Parameters.AddRange(methodArguments);
        args.InitMethod.Statements.Add(call);
    }

    /// <summary>
    /// Adds a new field called <see cref="Design.FieldName"/> of the
    /// Type of <see cref="Design.View"/> to <see cref="CodeDomArgs.Class"/>.
    /// </summary>
    /// <param name="args">State of the .Designer.cs file.</param>
    /// <param name="d">Wrapper for a <see cref="View"/>.</param>
    protected void AddFieldToClass(CodeDomArgs args, Design d)
    {
        this.AddFieldToClass(args, d.View.GetType(), d.FieldName);
    }

    /// <summary>
    /// Adds a new field called <paramref name="fieldName"/> of the
    /// Type <paramref name="type"/> to <see cref="CodeDomArgs.Class"/>.
    /// </summary>
    /// <param name="args">State of the .Designer.cs file.</param>
    /// <param name="type">Type of field to add.</param>
    /// <param name="fieldName">Name for the field.</param>
    protected void AddFieldToClass(CodeDomArgs args, Type type, string fieldName)
    {
        // Create a private field for it
        var field = new CodeMemberField(type, fieldName);
        args.Class.Members.Add(field);
    }

    /// <summary>
    /// Adds a new local field to the <see cref="CodeDomArgs.InitMethod"/> of the
    /// .Designer.cs file being generated.
    /// </summary>
    /// <param name="args">State of the .Designer.cs file.</param>
    /// <param name="type">Type of variable to add.</param>
    /// <param name="name">Name for the variable.</param>
    protected void AddLocalFieldToMethod(CodeDomArgs args, Type type, string name)
    {
        var declare = new CodeVariableDeclarationStatement(type, name);
        args.InitMethod.Statements.Add(declare);
    }

    /// <summary>
    /// Adds a new constructor call to <see cref="CodeDomArgs.InitMethod"/> of the
    /// .Designer.cs file being generated.
    /// </summary>
    /// <param name="args">State of the .Designer.cs file.</param>
    /// <param name="fullySpecifiedFieldName">What the results of the constructor call should be assigned to.</param>
    /// <param name="typeToConstruct">The Type of object to construct.</param>
    /// <param name="parameters">Arguments to satisfy all constructor arguments of <paramref name="typeToConstruct"/>.</param>
    protected void AddConstructorCall(CodeDomArgs args, string fullySpecifiedFieldName, Type typeToConstruct, params CodeExpression[] parameters)
    {
        var constructAssign = this.GetConstructorCall(fullySpecifiedFieldName, typeToConstruct, parameters);
        args.InitMethod.Statements.Add(constructAssign);
    }

    /// <summary>
    /// Gets (but does not add) a CodeDOM object representing a constructor call.
    /// </summary>
    /// <param name="fullySpecifiedFieldName">What the results of the constructor call should be assigned to.</param>
    /// <param name="typeToConstruct">The Type of object to construct.</param>
    /// <param name="parameters">Arguments to satisfy all constructor arguments of <paramref name="typeToConstruct"/>.</param>
    /// <returns>CodeDOM assignment code representing constructor call.</returns>
    protected CodeAssignStatement GetConstructorCall(string fullySpecifiedFieldName, Type typeToConstruct, params CodeExpression[] parameters)
    {
        // Construct it
        var constructLhs = new CodeFieldReferenceExpression();
        constructLhs.FieldName = fullySpecifiedFieldName;
        var constructRhs = new CodeObjectCreateExpression(typeToConstruct, parameters);
        var constructAssign = new CodeAssignStatement();
        constructAssign.Left = constructLhs;
        constructAssign.Right = constructRhs;

        return constructAssign;
    }

    /// <summary>
    /// Adds a new property assignment to <see cref="CodeDomArgs.InitMethod"/> of the
    /// .Designer.cs file being generated e.g.
    /// <code>this.someField.Text = "Heya"</code>
    /// </summary>
    /// <param name="args">State of the .Designer.cs file.</param>
    /// <param name="lhs">Code for the left had operand of the assignment.</param>
    /// <param name="primativeValue">The new value that should be assigned.  Must be
    /// a primitive (i.e. not a complex class).</param>
    protected void AddPropertyAssignment(CodeDomArgs args, string lhs, object primativeValue)
    {
        this.AddPropertyAssignment(args, lhs, primativeValue.ToCodePrimitiveExpression());
    }

    /// <summary>
    /// Adds a new property assignment to <see cref="CodeDomArgs.InitMethod"/> of the
    /// .Designer.cs file being generated e.g.
    /// <code>this.someField.Text = "Heya"</code>
    /// </summary>
    /// <param name="args">State of the .Designer.cs file.</param>
    /// <param name="lhs">Code for the left had operand of the assignment.</param>
    /// <param name="rhs">Code for the right had operand of the assignment.</param>
    protected void AddPropertyAssignment(CodeDomArgs args, string lhs, CodeExpression rhs)
    {
        var setLhs = new CodeFieldReferenceExpression();
        setLhs.FieldName = lhs;

        var assignStatement = new CodeAssignStatement();
        assignStatement.Left = setLhs;
        assignStatement.Right = rhs;
        args.InitMethod.Statements.Add(assignStatement);
    }
}
