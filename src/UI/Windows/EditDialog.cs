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

        this.list = new ListView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(2),
            Height = Dim.Fill(2),
        };
        this.list.Source = this.collection.ToListDataSource();
        this.list.KeyDown += this.List_KeyPress;

        var btnSet = new Button()
        {
            Text = "Set",
            X = 0,
            Y = Pos.Bottom(this.list),
            IsDefault = true,
        };

        btnSet.Accept += (s, e) =>
        {
            this.SetProperty(false);
        };

        var btnClose = new Button()
        {
            Text = "Close",
            X = Pos.Right(btnSet),
            Y = Pos.Bottom(this.list),
        };
        btnClose.Accept += (s, e) => Application.RequestStop();

        this.list.KeyDown += (s, e) =>
        {

            if (e == Key.Enter && this.list.HasFocus)
            {
                this.SetProperty(false);
                e.Handled = true;
            }
        };

        this.Add(this.list);
        this.Add(btnSet);
        this.Add(btnClose);
    }


    internal static bool SetPropertyToNewValue(Design design, Property p, object? oldValue)
    {
        // user wants to give us a new value for this property
        if (ValueFactory.GetNewValue(design, p, p.GetValue(), out object? newValue))
        {
            OperationManager.Instance.Do(
                new SetPropertyOperation(design, p, oldValue, newValue));

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
                this.list.Source = this.collection.ToListDataSource();
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
