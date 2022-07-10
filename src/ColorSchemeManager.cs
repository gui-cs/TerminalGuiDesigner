using System.Collections.ObjectModel;
using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner;

namespace TerminalGuiDesigner
{
    public class ColorSchemeManager
    {
        Dictionary<string, ColorScheme> _colorSchemes = new();

        /// <summary>
        /// All known color schemes defined by name
        /// </summary>
        public ReadOnlyCollection<KeyValuePair<string, ColorScheme>> Schemes => _colorSchemes.ToList().AsReadOnly();

        public static ColorSchemeManager Instance = new();

        private ColorSchemeManager()
        {

        }
        public void Clear()
        {
            _colorSchemes = new();
        }

        public void FindDeclaredColorSchemes(Design viewBeingEdited)
        {
            if (!viewBeingEdited.IsRoot)
                throw new ArgumentException("Expected to only be passed the root view");

            var view = viewBeingEdited.View;

            // find all fields in class
            var schemes = view.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(t => t.FieldType == typeof(ColorScheme));

            foreach (var f in schemes)
            {
                var val = f.GetValue(view) as ColorScheme;

                if (val != null && !_colorSchemes.ContainsKey(f.Name))
                    _colorSchemes.Add(f.Name, val);
            }
        }

        public string? GetNameForColorScheme(ColorScheme s)
        {
            var match = _colorSchemes.Where(kvp => AreEqual(s, kvp.Value)).ToArray();

            if (match.Length > 0)
                return match[0].Key;

            // no match
            return null;
        }

        private bool AreEqual(ColorScheme a, ColorScheme b)
        {
            return
                a.Normal.Value == b.Normal.Value &&
                a.HotNormal.Value == b.HotNormal.Value &&
                a.Focus.Value == b.Focus.Value &&
                a.HotFocus.Value == b.HotFocus.Value &&
                a.Disabled.Value == b.Disabled.Value;
        }

        public void AddOrUpdateScheme(string name, ColorScheme scheme)
        {
            _colorSchemes.AddOrUpdate(name,scheme);
        }
    }
}