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
        internal static bool GetNewValue(string propertyName, Design design, Type type, object? oldValue, out object? newValue, bool allowMultiLine)
        {
            newValue = null;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SliderOption<>))
            {
                var designer = new SliderOptionEditor(type.GetGenericArguments()[0], oldValue);
                Application.Run(designer);

                if (!designer.Cancelled)
                {
                    newValue = designer.Result;
                    return true;
                }
                else
                {
                    // user canceled designing the Option
                    return false;
                }
            }
            else
            if (type== typeof(ColorScheme))
            {
                return GetNewColorSchemeValue(design, out newValue);
            }
            else
            if (type == typeof(Attribute) ||
                type == typeof(Attribute?))
            {
                // if its an Attribute or nullableAttribute
                var picker = new ColorPicker((Attribute?)oldValue);
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
            if (type== typeof(ITextValidateProvider))
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
            if (type== typeof(Pos))
            {
                // user is editing a Pos
                var designer = new PosEditor(design, (Pos)oldValue?? throw new Exception("Pos property was unexpectedly null"));

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
            if (type== typeof(Size))
            {
                // user is editing a Size
                var oldSize = (Size)(oldValue ?? throw new Exception($"Property {propertyName} is of Type Size but it's current value is null"));
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
            if (type== typeof(PointF))
            {
                // user is editing a PointF
                var oldPointF = (PointF)(oldValue ?? throw new Exception($"Property {propertyName} is of Type PointF but it's current value is null"));
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
            if (type== typeof(Dim))
            {
                // user is editing a Dim
                var designer = new DimEditor(design, (Dim)oldValue);
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
            if (type== typeof(bool))
            {
                int answer = ChoicesDialog.Query(propertyName, $"New value for {type}", "Yes", "No");

                newValue = answer == 0 ? true : false;
                return answer != -1;
            }
            else
            if (
                type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
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
                    var designer = new ArrayEditor(design,type.GetElementTypeEx(), (IList)oldValue);
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
            if (type== typeof(IListDataSource))
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
            if (type== typeof(int)
                || type== typeof(int?)
                || type== typeof(uint)
                || type== typeof(uint?))
            {
                // deals with null, int and uint
                var v = oldValue == null ? null : (int?)Convert.ToInt32(oldValue);

                if (Modals.GetInt(propertyName, "New Int Value", v, out var resultInt))
                {
                    // change back to uint/int/null
                    newValue = resultInt == null ? null : Convert.ChangeType(resultInt, type);
                    return true;
                }
            }
            else
            if (type== typeof(float)
                || type== typeof(float?))
            {
                if (Modals.GetFloat(propertyName, "New Float Value", (float?)oldValue, out var resultInt))
                {
                    newValue = resultInt;
                    return true;
                }
            }
            else
            if (type== typeof(char?)
                || type== typeof(char))
            {
                if (Modals.GetChar(propertyName, "New Single Character", oldValue is null ? null : (char?)oldValue.ToPrimitive() ?? null, out var resultChar))
                {
                    newValue = resultChar;
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

                Application.Run(fd);
                if (fd.Canceled || string.IsNullOrWhiteSpace(fd.Path))
                {
                    return false;
                }
                else
                {
                    newValue = IsPathDirectory(fd.Path) ?
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
        internal static bool GetNewValue(Design design, Property property, object? oldValue, out object? newValue)
        {
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

                // User cancelled dialog
                newValue = null;
                return false;
            }
            else
            {
                return GetNewValue(property.PropertyInfo.Name, design, property.PropertyInfo.PropertyType,oldValue, out newValue, ValueFactory.AllowMultiLine(property));
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

        /// <summary>
        /// Returns true if path is to a directory.
        /// Thanks: https://stackoverflow.com/a/19596821/4824531
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        static bool IsPathDirectory(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            path = path.Trim();

            if (Directory.Exists(path))
                return true;

            if (File.Exists(path))
                return false;

            // neither file nor directory exists. guess intention

            // if has trailing slash then it's a directory
            if (new[] { Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar}.Any(x => path.EndsWith(x)))
                return true; // ends with slash

            // if has extension then its a file; directory otherwise
            return string.IsNullOrWhiteSpace(Path.GetExtension(path));
        }
    }
}
