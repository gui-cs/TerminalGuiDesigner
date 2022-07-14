using System.Collections.ObjectModel;
using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner;

namespace TerminalGuiDesigner
{
    /// <summary>
    /// A user defined <see cref="ColorScheme"/> and its name as defined
    /// by the user.  The <see cref="Name"/> will be used as Field name
    /// in the class code generated so must not contain illegal characters/spaces
    /// </summary>
    public class NamedColorScheme
    {
        public string Name { get; set; }
        public ColorScheme Scheme { get; set; }
        public NamedColorScheme(string name, ColorScheme scheme)
        {
            Name = name;
            Scheme = scheme;
        }
        public override string ToString()
        {
            return Name;
        }
    }

    public class ColorSchemeManager
    {
        
        List<NamedColorScheme> _colorSchemes = new();

        /// <summary>
        /// All known color schemes defined by name
        /// </summary>
        public ReadOnlyCollection<NamedColorScheme> Schemes => _colorSchemes.ToList().AsReadOnly();

        public static ColorSchemeManager Instance = new();

        private ColorSchemeManager()
        {

        }
        public void Clear()
        {
            _colorSchemes = new();
        }

        public void Remove(NamedColorScheme toDelete)
        {
            _colorSchemes.Remove(toDelete);
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

                if (val != null && !_colorSchemes.Any(s=>s.Name.Equals(f.Name)))
                    _colorSchemes.Add(new NamedColorScheme(f.Name, val));
            }
        }

        public string? GetNameForColorScheme(ColorScheme s)
        {
            var match = _colorSchemes.Where(kvp => s.AreEqual(kvp.Scheme)).ToArray();

            if (match.Length > 0)
                return match[0].Name;

            // no match
            return null;
        }

        public void AddOrUpdateScheme(string name, ColorScheme scheme)
        {
            var match = _colorSchemes.FirstOrDefault(c=>c.Name.Equals(name));

            if(match!=null)
            {
                match.Scheme = scheme;
            }
            else
            {
                _colorSchemes.Add(new NamedColorScheme(name,scheme));
            }
        }

        public void RenameScheme(string oldName, string newName)
        {
            var match = _colorSchemes.FirstOrDefault(c=>c.Name.Equals(oldName));

            if(match!=null)
            {
                match.Name = newName;
            }
        }
    }
}