
using System.CodeDom;
using System.Reflection;
using NStack;
using Terminal.Gui;

namespace TerminalGuiDesigner;
    
public class Design<T> where T : View
{
    public string FieldName { get; }
    public T View {get;}

    public Design(string fieldName, T view)
    {
        FieldName = fieldName;
        View = view;
    }

    /// <summary>
    /// Gets the designable properties of the hosted View
    /// </summary>
    public virtual IEnumerable<PropertyInfo> GetProperties()
    {
        yield return View.GetType().GetProperty(nameof(View.Text));
    }


    /// <summary>
    /// Adds declaration and initialization statements to the .Designer.cs
    /// CodeDom class.
    /// </summary>
    public void ToCode(CodeTypeDeclaration addTo, CodeMemberMethod initMethod)
    {
        AddFieldToClass(addTo);
        AddConstructorCall(initMethod);

        foreach(var prop in GetProperties())
        {
            var val = prop.GetValue(View);

            AddPropertyAssignment(initMethod,prop.Name,val);
        }
        

        AddAddToViewStatement(initMethod);
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
    protected void AddPropertyAssignment(CodeMemberMethod initMethod, string propertyName, object? value)
    {
        var setTextLhs = new CodeFieldReferenceExpression();
        setTextLhs.FieldName = "this.myLabel.Text";
        var setTextRhs = new CodePrimitiveExpression();

        if(value is ustring u)
        {
            setTextRhs.Value = u.ToString();
        }
        else
        {
            setTextRhs.Value = value;
        }

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
