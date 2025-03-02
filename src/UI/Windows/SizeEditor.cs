
//------------------------------------------------------------------------------

//  <auto-generated>
//      This code was generated by:
//        TerminalGuiDesigner v1.0.18.0
//      You can make changes to this file and they will not be overwritten when saving.
//  </auto-generated>
// -----------------------------------------------------------------------------
namespace TerminalGuiDesigner.UI.Windows;

using Terminal.Gui;

/// <summary>
/// Popup editor for the <see cref="Size"/> class.
/// </summary>
public partial class SizeEditor : IValueGetterDialog
{

    /// <summary>
    /// The users edited <see cref="Size"/> 
    /// </summary>
    public object? Result { get; private set; }

    /// <summary>
    /// True if user cancelled the dialog instead of hitting "Ok".
    /// </summary>
    public bool Cancelled { get; private set; }

    /// <summary>
    /// Creates a new instance of the <see cref="SizeEditor"/> class.
    /// </summary>
    /// <param name="s"></param>
    public SizeEditor(Size s)
    {
        InitializeComponent();
        Result = s;

        tfWidth.Text = s.Width.ToString();
        tfHeight.Text = s.Height.ToString();

        btnOk.Accepting += (s, e) =>
        {
            e.Cancel = true;
            try
            {
                Result = new Size(int.Parse(tfWidth.Text.ToString()), int.Parse(tfHeight.Text.ToString()));
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Bad Value", ex.Message);
                return;
            }

            Cancelled = false;
            RequestStop();
        };

        btnCancel.Accepting += (s, e) =>
        {
            e.Cancel = true;
            Cancelled = true;
            RequestStop();
        };
    }
}
