
//------------------------------------------------------------------------------

//  <auto-generated>
//      This code was generated by:
//        TerminalGuiDesigner v2.0.0.0
//      You can make changes to this file and they will not be overwritten when saving.
//  </auto-generated>
// -----------------------------------------------------------------------------

using System.Reflection;
using System.Text.Json;

namespace TerminalGuiDesigner.UI.Windows {
    using Terminal.Gui;
    
    
    public partial class KeyBindingsUI {
        private readonly KeyMap keyMap;
        private readonly PropertyInfo[] _props;

        public bool Save { get; set; } = false;

        public KeyBindingsUI(KeyMap keyMap) {
            this.keyMap = keyMap;
            InitializeComponent();

            _props = typeof(KeyMap).GetProperties()
                .Where(p=>p.PropertyType == typeof(string))
                .OrderBy(p=>p.Name)
                .ToArray();
            
            tableView.Table = new EnumerableTableSource<PropertyInfo>(_props,
                new Dictionary<string, Func<PropertyInfo, object>>()
                {
                    { "Function", p => p.Name },
                    { "Key", p => p.GetValue(this.keyMap) }
                });

            var keyStyle = tableView.Style.GetOrCreateColumnStyle(1);

            var badCellColor = CloneColorSchemeButMake(tableView.ColorScheme,Color.Red);

            keyStyle.ColorGetter = (k) =>
            {
                var val = _props[k.RowIndex].GetValue(this.keyMap);
                var matches = _props.Select(k => k.GetValue(this.keyMap)).Count(v => Equals(v,val));
                if (matches > 1)
                {
                    return badCellColor;
                }

                // Use normal color scheme
                return null;
            };

            tableView.CellActivated += (s, e) =>
            {
                var prop = _props[e.Row];
                var k = Modals.GetShortcut();
                prop.SetValue(this.keyMap,k.ToString());
                this.SetNeedsDraw();
            };
            btnReset.Accepting += (s, e) =>
            {
                var defaultMap = new KeyMap();
                foreach (var p in _props)
                {
                    var def = p.GetValue(defaultMap);
                    p.SetValue(this.keyMap, def);
                    this.SetNeedsDraw();
                }
            };

            btnSave.Accepting += (s, e) =>
            {
                Save = true;
                e.Cancel = true;
                Application.RequestStop();
            };
            btnCancel.Accepting += (s, e) =>
            {
                e.Cancel = true;
                Application.RequestStop();
            };
        }

        private ColorScheme CloneColorSchemeButMake(ColorScheme cs,Color color)
        {
            return new ColorScheme
            {
                Disabled = cs.Disabled,
                Focus = cs.Focus,
                HotFocus = cs.HotFocus,
                HotNormal = cs.HotNormal,
                Normal = new Attribute(color, cs.Normal.Background)
            };
        }
    }
}
