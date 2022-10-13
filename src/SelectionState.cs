using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Describes original <see cref="ColorScheme"/>
/// </summary>
internal class SelectionState : IDisposable
{
    public ColorScheme? OriginalScheme { get; set; }
	public Design Design{ get; }

	public SelectionState(Design design)
	{
        Design = design;
        OriginalScheme = Design.View.GetExplicitColorScheme();
        Design.View.DrawContentComplete += DrawContentComplete;
	}

	private void DrawContentComplete(Rect r)
	{ 
        if(Design.View.IsBorderlessContainerView())
        {
            DrawBorderlessViewFrame(r);
        }
    }

	private void DrawBorderlessViewFrame(Rect r)
    {
        var color = SelectionManager.Instance.Selected.Contains(Design) ?
            SelectionManager.Instance.SelectedScheme.Normal :
            Design.View.ColorScheme.Normal;

        Application.Driver.SetAttribute(color);

        var v = Design.View;

        for (int x = 0; x < r.Width ; x++)
            for (int y = 0; y < r.Height; y++)
            {
                if (y == 0 || y == r.Height - 1 || x == 0 || x == r.Width - 1)
                {
                    var rune = (y == r.Height - 1 && x == r.Width - 1) ? '╬' : '.';
                    v.AddRune(x,y,rune);
                }
            }
    }

    /// <summary>
    /// Clears draw callbacks and resets <see cref="OriginalScheme"/>
    /// </summary>
	public void Dispose()
	{
		Design.View.DrawContentComplete -= DrawContentComplete;
        Design.View.ColorScheme = OriginalScheme;
    }
}