using System.Collections;
using System.ComponentModel.Design;
using System.Text;
using Terminal.Gui;
using Terminal.Gui.TextValidateProviders;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner.UI.Windows;

/// <summary>
/// Dialog that shows all <see cref="Property"/> on a <see cref="design"/> including
/// current values.  Allows editing those values by launching other dialogs.
/// </summary>
public class EditDialog : Window
{
    private readonly ListView list;
    private readonly Design design;
    private readonly List<Property> collection = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EditDialog"/> class.
    /// </summary>
    /// <param name="design">The <see cref="Design"/> on which you want to set properties.</param>
    public EditDialog(Design design)
    {
        this.design = design;
        this.collection.Clear( );
        this.collection.AddRange( this.design.GetDesignableProperties( )
                                      .OrderByDescending( p => p is NameProperty )
                                      .ThenBy( p => p.ToString( ) ) );

        // Don't let them rename the root view that would go badly
        // See `const string RootDesignName`
        if (design.IsRoot)
        {
            this.collection.RemoveAll( p => p is NameProperty );
        }

        this.list = new ListView(this.collection)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(2),
            Height = Dim.Fill(2),
        };
        this.list.KeyDown += this.List_KeyPress;

        var btnSet = new Button("Set")
        {
            X = 0,
            Y = Pos.Bottom(this.list),
            IsDefault = true,
        };

        btnSet.Clicked += (s, e) =>
        {
            this.SetProperty(false);
        };

        var btnClose = new Button("Close")
        {
            X = Pos.Right(btnSet),
            Y = Pos.Bottom(this.list),
        };
        btnClose.Clicked += (s, e) => Application.RequestStop();

        this.Add(this.list);
        this.Add(btnSet);
        this.Add(btnClose);
    }

    /// <inheritdoc/>
    public override bool OnKeyDown(Key Key)
    {
        if (Key == Key.Enter && this.list.HasFocus)
        {
            this.SetProperty(false);
            return true;
        }

        return base.OnKeyDown(Key);
    }

    internal static bool SetPropertyToNewValue(Design design, Property p, object? oldValue)
    {
        // user wants to give us a new value for this property
        if (GetNewValue(design, p, p.GetValue(), out object? newValue))
        {
            OperationManager.Instance.Do(
                new SetPropertyOperation(design, p, oldValue, newValue));

            return true;
        }

        return false;
    }

    internal static bool GetNewValue(Design design, Property property, object? oldValue, out object? newValue)
    {
        if (property.PropertyInfo.PropertyType == typeof(ColorScheme))
        {
            return GetNewColorSchemeValue(design, property, out newValue);
        }
        else
        if (property.PropertyInfo.PropertyType == typeof(Attribute) ||
            property.PropertyInfo.PropertyType == typeof(Attribute?))
        {
            // if its an Attribute or nullableAttribute
            var picker = new ColorPicker((Attribute?)property.GetValue());
            Application.Run(picker);

            if (!picker.Cancelled)
            {
                newValue = picker.Result;
                return true;
            }
            else
            {
                // user cancelled designing the Color
                newValue = null;
                return false;
            }
        }
        else
        if (property.PropertyInfo.PropertyType == typeof(ITextValidateProvider))
        {
            string? oldPattern = oldValue is TextRegexProvider r ? (string?)r.Pattern.ToPrimitive() : null;
            if (Modals.GetString("New Validation Pattern", "Regex Pattern", oldPattern, out var newPattern))
            {
                newValue = string.IsNullOrWhiteSpace(newPattern) ? null : new TextRegexProvider(newPattern);
                return true;
            }

            // user cancelled entering a pattern
            newValue = null;
            return false;
        }
        else
        if (property.PropertyInfo.PropertyType == typeof(Pos))
        {
            // user is editing a Pos
            var designer = new PosEditor(design, property);

            Application.Run(designer);

            if (!designer.Cancelled)
            {
                newValue = designer.Result;
                return true;
            }
            else
            {
                // user cancelled designing the Pos
                newValue = null;
                return false;
            }
        }
        else
        if (property.PropertyInfo.PropertyType == typeof(Size))
        {
            // user is editing a Size
            var oldSize = (Size)(oldValue ?? throw new Exception($"Property {property.PropertyInfo.Name} is of Type Size but it's current value is null"));
            var designer = new SizeEditor(oldSize);

            Application.Run(designer);

            if (!designer.Cancelled)
            {
                newValue = designer.Result;
                return true;
            }
            else
            {
                // user cancelled designing the Pos
                newValue = null;
                return false;
            }
        }
        else
        if (property.PropertyInfo.PropertyType == typeof(PointF))
        {
            // user is editing a PointF
            var oldPointF = (PointF)(oldValue ?? throw new Exception($"Property {property.PropertyInfo.Name} is of Type PointF but it's current value is null"));
            var designer = new PointEditor(oldPointF.X, oldPointF.Y);

            Application.Run(designer);

            if (!designer.Cancelled)
            {
                newValue = new PointF(designer.ResultX, designer.ResultY);
                return true;
            }
            else
            {
                // user cancelled designing the Pos
                newValue = null;
                return false;
            }
        }
        else
        if (property.PropertyInfo.PropertyType == typeof(Dim))
        {
            // user is editing a Dim
            var designer = new DimEditor(design, property);
            Application.Run(designer);

            if (!designer.Cancelled)
            {
                newValue = designer.Result;
                return true;
            }
            else
            {
                // user cancelled designing the Dim
                newValue = null;
                return false;
            }
        }
        else
        if (property is InstanceOfProperty inst)
        {
            if (Modals.Get<Type>(
                property.PropertyInfo.Name,
                "New Value",
                typeof(Label).Assembly.GetTypes().Where(inst.MustBeDerrivedFrom.IsAssignableFrom).ToArray(),
                inst.GetValue()?.GetType(),
                out Type? typeChosen))
            {
                if (typeChosen == null)
                {
                    newValue = null;
                    return false;
                }

                newValue = Activator.CreateInstance(typeChosen);
                return true;
            }
        }
        else
        if (property.PropertyInfo.PropertyType == typeof(bool))
        {
            int answer = ChoicesDialog.Query(property.PropertyInfo.Name, $"New value for {property.PropertyInfo.PropertyType}", "Yes", "No");

            newValue = answer == 0 ? true : false;
            return answer != -1;
        }
        else
        if (
            // TODO: I just changed this from IsArray to IList assignable, need to worry about conversions a bit more
            property.PropertyInfo.PropertyType.IsAssignableTo(typeof(IList))
            )
        {
            var elementType = property.GetElementType()
                ?? throw new Exception($"Property {property.GetHumanReadableName()} was array but had no element type"); ;

            if (elementType.IsValueType || elementType == typeof(string))
            {
                if (Modals.GetArray(
                    property.PropertyInfo.Name,
                    "New Array Value",
                    property.PropertyInfo.PropertyType.GetElementType() ?? throw new Exception("Property was an Array but GetElementType returned null"),
                    (Array?)oldValue,
                    out Array? resultArray))
                {
                    newValue = resultArray;
                    return true;
                }
            }
            else
            {
                var designer = new ArrayEditor(property);
                Application.Run(designer);

                if (!designer.Cancelled)
                {
                    newValue = designer.Result;
                    return true;
                }
                else
                {
                    // user cancelled designing the Dim
                    newValue = null;
                    return false;
                }
            }

        }
        else
        if (property.PropertyInfo.PropertyType == typeof(IListDataSource))
        {
            // TODO : Make this work with non strings e.g.
            // if user types a bunch of numbers in or dates
            var oldValueAsArrayOfStrings = oldValue == null ?
                    Array.Empty<string>() :
                    ((IListDataSource)oldValue).ToList()
                                               .Cast<object>()
                                               .Select(o => o?.ToString())
                                               .ToArray();

            if (Modals.TryGetArray<string>(
                property.PropertyInfo.Name,
                "New List Value",
                oldValueAsArrayOfStrings,
                out Array? resultArray))
            {
                newValue = resultArray;
                return true;
            }
        }
        else
        if (property.PropertyInfo.PropertyType.IsEnum)
        {
            if (Modals.GetEnum(property.PropertyInfo.Name, "New Enum Value", property.PropertyInfo.PropertyType, (Enum?)property.GetValue(), out var resultEnum))
            {
                newValue = resultEnum;
                return true;
            }
        }
        else
        if (property.PropertyInfo.PropertyType == typeof(int)
            || property.PropertyInfo.PropertyType == typeof(int?)
            || property.PropertyInfo.PropertyType == typeof(uint)
            || property.PropertyInfo.PropertyType == typeof(uint?))
        {
            // deals with null, int and uint
            var v = oldValue == null ? null : (int?)Convert.ToInt32(oldValue);

            if (Modals.GetInt(property.PropertyInfo.Name, "New Int Value", v, out var resultInt))
            {
                // change back to uint/int/null
                newValue = resultInt == null ? null : Convert.ChangeType(resultInt, property.PropertyInfo.PropertyType);
                return true;
            }
        }
        else
        if (property.PropertyInfo.PropertyType == typeof(float)
            || property.PropertyInfo.PropertyType == typeof(float?))
        {
            if (Modals.GetFloat(property.PropertyInfo.Name, "New Float Value", (float?)oldValue, out var resultInt))
            {
                newValue = resultInt;
                return true;
            }
        }
        else
        if (property.PropertyInfo.PropertyType == typeof(char?)
            || property.PropertyInfo.PropertyType == typeof(char))
        {
            if (Modals.GetChar(property.PropertyInfo.Name, "New Single Character", oldValue is null ? null : (char?)oldValue.ToPrimitive() ?? null, out var resultChar))
            {
                newValue = resultChar;
                return true;
            }
        }
        else
        if (property.PropertyInfo.PropertyType == typeof(Rune)
            || property.PropertyInfo.PropertyType == typeof(Rune?))
        {
            if (Modals.GetChar(property.PropertyInfo.Name, "New Single Character", oldValue is null ? null : (char?)oldValue.ToPrimitive() ?? null, out var resultChar))
            {
                newValue = resultChar == null ? null : new Rune(resultChar.Value);
                return true;
            }
        }
        else
        if (Modals.GetString(property.PropertyInfo.Name, "New String Value", oldValue?.ToString(), out var result, AllowMultiLine(property)))
        {
            newValue = result;
            return true;
        }

        newValue = null;
        return false;
    }

    private static bool GetNewColorSchemeValue(Design design, Property property, out object? newValue)
    {
        const string custom = "Edit Color Schemes...";
        List<object> offer = new();

        var defaults = new DefaultColorSchemes();
        var schemes = ColorSchemeManager.Instance.Schemes.ToList();

        offer.AddRange(schemes);

        foreach (var d in defaults.GetDefaultSchemes())
        {
            // user is already explicitly using this default and may even have modified it
            if (offer.OfType<NamedColorScheme>().Any(s => s.Name.Equals(d.Name)))
            {
                continue;
            }

            offer.Add(d);
        }

        // add the option to jump to custom colors
        offer.Add(custom);

        if (Modals.Get("Color Scheme", "Ok", offer.ToArray(), design.View.ColorScheme, out var selected))
        {
            // if user clicked "Custom..."
            if (selected is string s && string.Equals(s, custom))
            {
                // show the custom colors dialog
                var colorSchemesUI = new ColorSchemesUI(design);
                Application.Run(colorSchemesUI);
                newValue = null;
                return false;
            }

            if (selected is NamedColorScheme ns)
            {
                newValue = ns.Scheme;

                // if it was a default one, tell ColorSchemeManager we are now using it
                if (!schemes.Contains(ns))
                {
                    ColorSchemeManager.Instance.AddOrUpdateScheme(ns.Name, ns.Scheme, design.GetRootDesign());
                }

                return true;
            }

            newValue = null;
            return false;
        }
        else
        {
            // user cancelled selecting scheme
            newValue = null;
            return false;
        }
    }

    private static bool AllowMultiLine(Property property)
    {
        // for the text editor control let them put multiple lines in
        if (property.PropertyInfo.Name.Equals("Text") && property.Design.View is TextView tv && tv.Multiline)
        {
            return true;
        }

        return false;
    }

    private void SetProperty(bool setNull)
    {
        if (this.list.SelectedItem != -1)
        {
            try
            {
                var p = this.collection[this.list.SelectedItem];
                var oldValue = p.GetValue();

                if (setNull)
                {
                    // user wants to set this property to null/default
                    OperationManager.Instance.Do(
                        new SetPropertyOperation(this.design, p, oldValue, null));
                }
                else
                {
                    if (!SetPropertyToNewValue(this.design, p, oldValue))
                    {
                        // user cancelled editing the value
                        return;
                    }
                }

                var oldSelected = this.list.SelectedItem;
                this.list.SetSource(this.collection);
                this.list.SelectedItem = oldSelected;
                this.list.EnsureSelectedItemVisible();
            }
            catch (Exception e)
            {
                ExceptionViewer.ShowException("Failed to set Property", e);
            }
        }
    }

    private void List_KeyPress(object? sender, Key obj)
    {
        // TODO: Should really be using the _keyMap here
        if (obj == Key.DeleteChar)
        {
            int rly = ChoicesDialog.Query("Clear", "Clear Property Value?", "Yes", "Cancel");
            obj.Handled = true;

            if (rly == 0)
            {
                this.SetProperty(true);
            }
        }
    }
}
