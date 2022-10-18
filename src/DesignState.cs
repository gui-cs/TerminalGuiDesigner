using Terminal.Gui;
using TerminalGuiDesigner.UI;

namespace TerminalGuiDesigner;

/// <summary>
/// Describes state based changes and custom callbacks on a <see cref="Design"/>
/// e.g. <see cref="OriginalScheme"/>
/// </summary>
public class DesignState
{
    public ColorScheme? OriginalScheme { get; set; }
	public Design Design{ get; }

	public DesignState(Design design)
	{
        Design = design;
        OriginalScheme = Design.View.GetExplicitColorScheme();
        Design.View.DrawContent += DrawContent;
        Design.View.Enter += Enter;
    }

    private void Enter(View.FocusEventArgs obj)
    {
        SelectionManager.Instance.SetSelection(Design);
    }

    private void DrawContent(Rect r)
	{ 
        if(Design.View.IsBorderlessContainerView() && Editor.ShowBorders)
        {
            DrawBorderlessViewFrame(r);
        }
    }

	private void DrawBorderlessViewFrame(Rect r)
    {
        bool isSelected = SelectionManager.Instance.Selected.Contains(Design);

        var color =  isSelected ?
            SelectionManager.Instance.SelectedScheme.Normal :
            Design.View.ColorScheme.Normal;

        Application.Driver.SetAttribute(color);

        var v = Design.View;

        for (int x = 0; x < r.Width ; x++)
            for (int y = 0; y < r.Height; y++)
            {
                if (y == 0 || y == r.Height - 1 || x == 0 || x == r.Width - 1)
                {
                    var rune = (y == r.Height - 1 && x == r.Width - 1 && isSelected) ? '╬' : '.';
                    v.AddRune(x,y,rune);
                }
            }
    }
}