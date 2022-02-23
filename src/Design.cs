
using System.CodeDom;

namespace TerminalGuiDesigner;
    
public abstract class Design<T>
{
    public string FieldName { get; }
    public T View {get;}

    public Design(string fieldName, T view)
    {
        FieldName = fieldName;
        View = view;
    }
    protected void AddConstructorCall(CodeMemberMethod initMethod, params CodeExpression[] parameters)
    {
        // Construct it
        var constructLhs = new CodeFieldReferenceExpression();
        constructLhs.FieldName = $"this.{FieldName}";
        var constructRhs = new CodeObjectCreateExpression(typeof(T),parameters);
        var constructAssign = new CodeAssignStatement();
        constructAssign.Left = constructLhs;
        constructAssign.Right = constructRhs;
        initMethod.Statements.Add(constructAssign);        
    }
    protected void AddPropertyAssignment(CodeMemberMethod initMethod, string propertyName, string value)
    {
        // TODO: Hydrate it
        var setTextLhs = new CodeFieldReferenceExpression();
        setTextLhs.FieldName = "this.myLabel.Text";
        var setTextRhs = new CodePrimitiveExpression();
        setTextRhs.Value = "Hello World";           

        var setTextAssign = new CodeAssignStatement();
        setTextAssign.Left = setTextLhs;
        setTextAssign.Right = setTextRhs;
        initMethod.Statements.Add(setTextAssign);
    }

    protected void AddAddToViewStatement(CodeMemberMethod initMethod)
    {
        // Add it to the view 
        var callAdd = new CodeMethodInvokeExpression();
        callAdd.Method.TargetObject = new CodeThisReferenceExpression();
        callAdd.Method.MethodName = "Add";
        callAdd.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),FieldName));
        initMethod.Statements.Add(callAdd);
    }

    protected void AddFieldToClass(CodeTypeDeclaration addTo)
    {
        // Create a private field for it
        var field = new CodeMemberField();
        field.Name = FieldName;
        field.Type = new CodeTypeReference(typeof(T));

        addTo.Members.Add(field);
    }
}
