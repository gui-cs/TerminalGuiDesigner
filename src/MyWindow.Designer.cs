using Terminal.Gui;

namespace TerminalGuiDesigner;


public partial class MyWindow 
{
    private void InitializeComponent()
    {
        var lbl = new Label("Hello World");
        Add(lbl);
    }
}
