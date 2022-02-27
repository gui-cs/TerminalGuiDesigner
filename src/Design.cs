
using System.CodeDom;
using System.Reflection;
using NLog;
using NStack;
using Terminal.Gui;

namespace TerminalGuiDesigner;
    
public class Design
{
    public string FieldName { get; }

    /// <summary>
    /// The view being designed.  Do not use <see cref="View.Add(Terminal.Gui.View)"/> on
    /// this instance.  Instead use <see cref="AddDesign(string, Terminal.Gui.View)"/> so that
    /// new child controls are preserved for design time changes
    /// </summary>
    public View View {get;}


    private Logger logger = LogManager.GetCurrentClassLogger();

    public Design(string fieldName, View view)
    {
        FieldName = fieldName;
        View = view;
    }

    public void AddDesign(string name, View subView)
    {
        View.Add(subView);
        subView.Data = CreateSubControlDesign(name, subView);
    }

    public void RemoveDesign(View view)
    {
        // TODO : make this an Activity that is undoable/tracked in a 
        // central activity stack.

        if(view.SuperView != null)
        {
            view.SuperView.Remove(view);
        }
    }

    public void CreateSubControlDesigns()
    {
        foreach (var subView in View.GetActualSubviews().ToArray())
        {
            logger.Info($"Found subView of Type '{subView.GetType()}'");

            if(subView.Data is string name)
            {
                subView.Data = CreateSubControlDesign(name, subView);
            }
        }
    }
    private Design CreateSubControlDesign(string name, View subView)
    {
        // HACK: if you don't pull the label out first it complains that you cant set Focusable to true
        // on the Label because its super is not focusable :(
        var super = subView.SuperView;
        if(super != null)
        {
            super.Remove(subView);
        }

        // all views can be focused so that they can be edited
        // or deleted etc
        subView.CanFocus = true;

        if (super != null)
        {
            super.Add(subView);
        }

        return new Design(name, subView);
    }


    /// <summary>
    /// Returns all designs in subviews of this control
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<Design> GetAllDesigns()
    {
        return GetAllDesigns(View);
    }


    /// <summary>
    /// Gets the designable properties of the hosted View
    /// </summary>
    public virtual IEnumerable<PropertyInfo> GetDesignableProperties()
    {
        yield return View.GetType().GetProperty(nameof(View.Text));
        yield return View.GetType().GetProperty(nameof(View.X));
        yield return View.GetType().GetProperty(nameof(View.Y));
    }


    /// <summary>
    /// Adds declaration and initialization statements to the .Designer.cs
    /// CodeDom class.
    /// </summary>
    public void ToCode(CodeTypeDeclaration addTo, CodeMemberMethod initMethod)
    {
        AddFieldToClass(addTo);
        AddConstructorCall(initMethod);

        foreach(var prop in GetDesignableProperties())
        {
            var val = prop.GetValue(View);
            AddPropertyAssignment(initMethod,prop.Name,val);
        }

        // Set View.Data to the name of the field so that we can 
        // determine later on which View instances come from which
        // Fields in the class
        AddPropertyAssignment(initMethod, nameof(View.Data), FieldName);

        AddAddToViewStatement(initMethod);
    }

    protected void AddConstructorCall(CodeMemberMethod initMethod, params CodeExpression[] parameters)
    {
        // Construct it
        var constructLhs = new CodeFieldReferenceExpression();
        constructLhs.FieldName = $"this.{FieldName}";
        var constructRhs = new CodeObjectCreateExpression(View.GetType(),parameters);
        var constructAssign = new CodeAssignStatement();
        constructAssign.Left = constructLhs;
        constructAssign.Right = constructRhs;
        initMethod.Statements.Add(constructAssign);        
    }

    /// <summary>
    /// Adds a statement to the InitializeComponent method like:
    /// <code>this.mylabel.Text = "hello"</code>
    /// </summary>
    /// <param name="initMethod">The InitializeComponent method</param>
    /// <param name="propertyName">The property to set e.g. Text</param>
    /// <param name="value">The value to assign to the property e.g. "hello"</param>
    protected void AddPropertyAssignment(CodeMemberMethod initMethod, string propertyName, object? value)
    {
        var setLhs = new CodeFieldReferenceExpression();
        setLhs.FieldName = $"this.{FieldName}.{propertyName}";
        CodeExpression setRhs = GetRhsForValue(value);

        var setTextAssign = new CodeAssignStatement();
        setTextAssign.Left = setLhs;
        setTextAssign.Right = setRhs;
        initMethod.Statements.Add(setTextAssign);
    }

    private CodeExpression GetRhsForValue(object? value)
    {
        if (value is ustring u)
        {
            return new CodePrimitiveExpression()
            {
                Value = u.ToString()
            };
        }
        if (value is Pos p)
        {
            // Value is a position e.g. X=2
            // Pos can be many different subclasses all of which are internal
            // lets deal with only PosAbsolute for now
            if (p.GetType().Name == "PosAbsolute")
            {
                var n = p.GetType().GetField("n", BindingFlags.NonPublic | BindingFlags.Instance);

                return new CodePrimitiveExpression()
                {
                    Value = n.GetValue(p)
                };            
            }
            else
                throw new NotImplementedException("Only absolute positions are supported at the moment");
        }
        else
        {
            return new CodePrimitiveExpression()
            {
                Value = value
            };
        }
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
        field.Type = new CodeTypeReference(View.GetType());

        addTo.Members.Add(field);
    }
    private IEnumerable<Design> GetAllDesigns(View view)
    {
        List<Design> toReturn = new List<Design>();

        foreach (var subView in view.GetActualSubviews().ToArray())
        {
            if (subView.Data is Design d)
            {
                toReturn.Add(d);
            }

            // even if this subview isn't designable there might be designable ones further down
            // e.g. a ContentView of a Window
            toReturn.AddRange(GetAllDesigns(subView));
        }

        return toReturn;
    }
}
