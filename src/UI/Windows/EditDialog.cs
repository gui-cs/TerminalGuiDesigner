using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner.UI.Windows;

public class EditDialog : Window
{
    private List<Property> collection;
    private ListView list;

    public Design Design { get; }


    public EditDialog(Design design)
    {
        Design = design;
        collection = Design.GetDesignableProperties().ToList();

        list = new ListView(collection)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(2),
            Height = Dim.Fill(2)
        };
        list.KeyPress += List_KeyPress;

        var btnSet = new Button("Set")
        {
            X = 0,
            Y = Pos.Bottom(list),
            IsDefault = true
        };

        btnSet.Clicked += () =>
        {
            SetProperty(false);
        };

        var btnClose = new Button("Close")
        {
            X = Pos.Right(btnSet),
            Y = Pos.Bottom(list)
        };
        btnClose.Clicked += () => Application.RequestStop();

        Add(list);
        Add(btnSet);
        Add(btnClose);
    }

    private void SetProperty(bool setNull)
    {
        if (list.SelectedItem != -1)
        {
            try
            {
                var p = collection[list.SelectedItem];

                // TODO make this work with PropertyDesign correctly
                // i.e. return the PropertyDesign not the value
                var oldValue = p.GetValue();


                if (setNull)
                {
                    // user wants to set this property to null/default
                    OperationManager.Instance.Do(
                        new SetPropertyOperation(Design, p, oldValue, null)
                    );
                }
                else
                {
                    // user wants to give us a new value for this property
                    if (GetNewValue(p, out object? newValue))
                    {
                        OperationManager.Instance.Do(
                            new SetPropertyOperation(Design, p, oldValue, newValue)
                        );
                    }
                    else
                    {
                        // user cancelled editing the value
                        return;
                    }
                }

                var oldSelected = list.SelectedItem;
                list.SetSource(collection = collection.ToList());
                list.SelectedItem = oldSelected;
                list.EnsureSelectedItemVisible();

            }
            catch (Exception e)
            {
                ExceptionViewer.ShowException("Failed to set Property", e);
            }
        }
    }

    private bool GetNewValue(Property property, out object? newValue)
    {
        var oldValue = property.GetValue();

        // user is editing a Pos
        if (property.PropertyInfo.PropertyType == typeof(Pos))
        {
            var designer = new PosDesigner();

            if (designer.GetPosDesign(Design, property, out SnippetProperty posDesign))
            {
                newValue = posDesign;
                return true;
            }
        }

        // user is editing a Dim
        if (property.PropertyInfo.PropertyType == typeof(Dim))
        {
            var designer = new DimDesigner();

            if (designer.GetDimDesign(property, out SnippetProperty dimDesign))
            {
                newValue = dimDesign;
                return true;
            }
        }

        if (property.PropertyInfo.PropertyType == typeof(bool))
        {
            int answer = MessageBox.Query(property.PropertyInfo.Name, $"New value for {property.PropertyInfo.PropertyType}", "Yes", "No");

            newValue = answer ==0 ? true : false;
            return answer != -1;
        }

        if (Modals.GetString((string)property.PropertyInfo.Name, "New Value", oldValue?.ToString() ?? string.Empty, out string result))
        {
            newValue = result;
            return true;
        }

        newValue = null;
        return false;
    }



    private void List_KeyPress(KeyEventEventArgs obj)
    {
        if (obj.KeyEvent.Key == Key.DeleteChar)
        {
            int rly = MessageBox.Query("Clear", "Clear Property Value?", "Yes", "Cancel");
            if (rly == 0)
            {
                obj.Handled = true;
                SetProperty(true);
            }
        }
    }
}
