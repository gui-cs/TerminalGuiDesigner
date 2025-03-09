using System.Collections;
using System.Text;
using Terminal.Gui.TextValidateProviders;
using Terminal.Gui;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner.UI.Windows;
using ColorPicker = TerminalGuiDesigner.UI.Windows.ColorPicker;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner.UI
{
    static class ValueFactory
    {
        public static readonly Type[] ConvertChangeTypeSupports = new Type[]
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(bool),
            typeof(char),
            typeof(string),
            typeof(DateTime),

            typeof(byte?),
            typeof(sbyte?),
            typeof(short?),
            typeof(ushort?),
            typeof(int?),
            typeof(uint?),
            typeof(long?),
            typeof(ulong?),
            typeof(float?),
            typeof(double?),
            typeof(decimal?),
            typeof(bool?),
            typeof(char?),
            typeof(DateTime?)
        };

        internal static bool GetNewValue(string propertyName, Design design, Type type, object? oldValue, out object? newValue, bool allowMultiLine)
        {
            newValue = null;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SliderOption<>))
            {
                return RunEditor(new SliderOptionEditor(type.GetGenericArguments()[0], oldValue), out newValue);
            }
            if (type == typeof(ColorScheme))
            {
                return GetNewColorSchemeValue(design, out newValue);
            }
            if (type == typeof(Attribute) || type == typeof(Attribute?))
            {
                return RunEditor(new ColorPicker((Attribute?)oldValue), out newValue);
            }
            if (type == typeof(Pos))
            {
                return RunEditor(new PosEditor(design, (Pos)oldValue ?? throw new Exception("Pos property was unexpectedly null")), out newValue);
            }
            if (type == typeof(Size))
            {
                return RunEditor(new SizeEditor((Size)(oldValue ?? throw new Exception($"Property {propertyName} is of Type Size but its current value is null"))), out newValue);
            }
            if (type == typeof(Point))
            {
                var oldPoint = (Point)(oldValue ?? throw new Exception($"Property {propertyName} is of Type Point but its current value is null"));
                var result = RunEditor(new PointEditor(oldPoint.X, oldPoint.Y), out newValue);

                if (newValue != null)
                {
                    newValue = new Point((int)((PointF)newValue).X, (int)((PointF)newValue).Y);
                }
                return result;

            }
            if (type == typeof(PointF))
            {
                var oldPointF = (PointF)(oldValue ?? throw new Exception($"Property {propertyName} is of Type PointF but its current value is null"));
                return RunEditor(new PointEditor(oldPointF.X, oldPointF.Y), out newValue);
            }
            if (type == typeof(Dim))
            {
                return RunEditor(new DimEditor(design, (Dim)oldValue), out newValue);
            }
            if (type == typeof(bool))
            {
                int answer = ChoicesDialog.Query(propertyName, $"New value for {type}", "Yes", "No");
                newValue = answer == 0 ? true : false;
                return answer != -1;
            }

            if (
                type.IsGenericType(typeof(IEnumerable<>)) ||
                type.IsAssignableTo(typeof(IList))
                )
            {
                var elementType = type.GetElementTypeEx()
                    ?? throw new Exception($"Property {propertyName} was array but had no element type"); ;

                if (elementType.IsValueType || elementType == typeof(string))
                {
                    if (Modals.GetArray(
                        propertyName,
                        "New Array Value",
                        type.GetElementType() ?? throw new Exception("Property was an Array but GetElementType returned null"),
                        (Array?)oldValue,
                        out Array? resultArray))
                    {
                        newValue = resultArray;
                        return true;
                    }
                }
                else
                {
                    var designer = new ArrayEditor(design, type.GetElementTypeEx(), (IList)oldValue);
                    Application.Run(designer);

                    if (!designer.Cancelled)
                    {
                        newValue = designer.ResultAsList;
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
            if (type == typeof(IListDataSource))
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
                    propertyName,
                    "New List Value",
                    oldValueAsArrayOfStrings,
                    out Array? resultArray))
                {
                    newValue = resultArray;
                    return true;
                }
            }
            else
            if (type.IsEnum)
            {
                if (Modals.GetEnum(propertyName, "New Enum Value", type, (Enum?)oldValue, out var resultEnum))
                {
                    newValue = resultEnum;
                    return true;
                }
            }
            else
            if (ConvertChangeTypeSupports.Contains(type))
            {
                var oldValueConverted = oldValue == null ? null : Convert.ChangeType(oldValue, type);
                if (Modals.GetString(propertyName, $"New {type.Name} Value", oldValueConverted?.ToString(), out var result, allowMultiLine))
                {
                    newValue = string.IsNullOrWhiteSpace(result) ? null : Convert.ChangeType(result, type);
                    return true;
                }
            }
            else
            if (type== typeof(Rune)
                || type== typeof(Rune?))
            {
                if (Modals.GetChar(propertyName, "New Single Character", oldValue is null ? null : (char?)oldValue.ToPrimitive() ?? null, out var resultChar))
                {
                    newValue = resultChar == null ? null : new Rune(resultChar.Value);
                    return true;
                }
            }
            else
            if (type == typeof(FileSystemInfo))
            {
                var fd = new FileDialog();
                fd.AllowsMultipleSelection = false;

                int answer = ChoicesDialog.Query(propertyName, $"Directory or File?", "Directory", "File", "Cancel");

                if (answer < 0 || answer >= 2)
                {
                    return false;
                }
                bool pickDir = answer == 0;

                fd.OpenMode = pickDir ? OpenMode.Directory : OpenMode.File;

                Application.Run(fd);
                if (fd.Canceled || string.IsNullOrWhiteSpace(fd.Path))
                {
                    return false;
                }
                else
                {
                    newValue = pickDir ?
                        new DirectoryInfo(fd.Path) : new FileInfo(fd.Path);
                    return true;
                }
            }
            else
            if (Modals.GetString(propertyName, "New String Value", oldValue?.ToString(), out var result, allowMultiLine))
            {
                newValue = result;
                return true;
            }

            newValue = null;
            return false;
        }

        private static bool RunEditor<T>(T editor, out object? result) where T : Dialog, IValueGetterDialog
        {
            Application.Run(editor);
            if (editor.Cancelled)
            {
                result = null;
                return false;
            }

            result = editor.Result;
            return true;
        }

        internal static bool GetNewValue(Design design, Property property, object? oldValue, out object? newValue)
        {
            if (property is InstanceOfProperty inst)
            {
                if (Modals.Get<Type>(
                    property.PropertyInfo.Name,
                    "New Value",
                    typeof(Label).Assembly.GetTypes().Where(inst.MustBeDerivedFrom.IsAssignableFrom).ToArray(),
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

                // User cancelled dialog
                newValue = null;
                return false;
            }
            else
            {
                return GetNewValue(property.PropertyInfo.Name, design, property.PropertyInfo.PropertyType, oldValue, out newValue, ValueFactory.AllowMultiLine(property));
            }

        }
        public static bool AllowMultiLine(Property property)
        {
            // for the text editor control let them put multiple lines in
            if (property.PropertyInfo.Name.Equals("Text") && property.Design.View is TextView tv && tv.Multiline)
            {
                return true;
            }

            return false;
        }

        private static bool GetNewColorSchemeValue(Design design, out object? newValue)
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

            if (Modals.Get("Color Scheme", "Ok", offer.ToArray(), design.View.ColorScheme, out var selected,false))
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
    }
}
