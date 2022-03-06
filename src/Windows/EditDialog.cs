using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner.Windows;

internal class EditDialog : Window
{
    private List<PropertyInListView> collection;
    private ListView list;

    public Design Design { get; }


    public EditDialog(Design design)
    {
        Design = design;
        collection = Design.GetDesignableProperties().Select(p => new PropertyInListView(p, design)).ToList();
        
        // Add the (Name) entry as editable too
        collection.Insert(0, new NamePropertyInListView(design));

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

        this.Add(list);
        this.Add(btnSet);
        this.Add(btnClose);
    }

    private void SetProperty(bool setNull)
    {
        if (list.SelectedItem != -1)
        {
            try
            {
                var p = collection[list.SelectedItem];
                
                if(p is NamePropertyInListView nameProperty)
                {
                    if (setNull)
                    {
                        MessageBox.ErrorQuery("Not Possible", "You cannot set a View (Name) property to null", "Ok");
                        return;
                    }
                    else
                    {
                        if(Modals.GetString("Name","New Name",Design.FieldName,out string? newName))
                        {
                            if(!string.IsNullOrWhiteSpace(newName))
                            {
                                OperationManager.Instance.Do(new RenameViewOperation(Design, Design.FieldName, newName));
                                p.UpdateValue();
                            }
                        }

                        // value was updated or user cancelled typing a new name
                        return;
                    }
                }

                // TODO make this work with PropertyDesign correctly
                // i.e. return the PropertyDesign not the value
                var oldValue = p.PropertyInfo.GetValue(Design.View);

                    
                if(setNull)
                {
                    // user wants to set this property to null/default
                    OperationManager.Instance.Do(
                        new SetPropertyOperation(Design,p.PropertyInfo,oldValue,null)
                    );
                }
                else
                {
                    // user wants to give us a new value for this property
                    if(GetNewValue(p, out object? newValue))
                    {
                        OperationManager.Instance.Do(
                            new SetPropertyOperation(Design,p.PropertyInfo,oldValue,newValue)
                        );
                    }
                    else
                    {
                        // user cancelled editing the value
                        return;
                    }
                }
                    

                p.UpdateValue();

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

    private bool GetNewValue(PropertyInListView propertyInListView, out object? newValue)
    {
        var property = propertyInListView.PropertyInfo;
        var oldValue = Design.GetDesignablePropertyValue(propertyInListView.PropertyInfo);

        // user is editing a Pos
        if (property.PropertyType == typeof(Pos))
        {        
            var designer = new PosDesigner();

            if(designer.GetPosDesign(Design,property,out PropertyDesign posDesign))
            {
                newValue = posDesign;
                return true;
            }
        }

        // user is editing a Dim
        if (property.PropertyType == typeof(Dim))
        {
            var designer = new DimDesigner();

            if (designer.GetDimDesign(property, out PropertyDesign dimDesign))
            {
                newValue = dimDesign;
                return true;
            }
        }

        if (Modals.GetString(property.Name,"New Value",oldValue?.ToString() ?? string.Empty, out string result))
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

    /// <summary>
    /// The <see cref="Design.FieldName"/> which will be refered to as "(Name)".  This
    /// is what the currently edited View will be called as a member in its parent class
    /// when written out to .Designer.cs.  For example "label1"
    /// </summary>
    private class NamePropertyInListView : PropertyInListView
    {
        public NamePropertyInListView(Design d):base(null,d)
        {
            DisplayMember = "(Name):" + Design.FieldName;
        }

        public override void UpdateValue()
        {
            DisplayMember = "(Name):" + Design.FieldName;
        }
    }

    /// <summary>
    /// A list view entry with the value of the field and 
    /// </summary>
    private class PropertyInListView
    {
        public PropertyInfo PropertyInfo;
        public string DisplayMember;

        public Design Design;

        public PropertyInListView(PropertyInfo p, Design design)
        {
            PropertyInfo = p;
            Design = design;
            UpdateValue();
        }

        public override string ToString()
        {
            return DisplayMember;
        }

        /// <summary>
        /// Updates the <see cref="DisplayMember"/> to indicate the new value
        /// </summary>
        /// <param name="newValue"></param>
        public virtual void UpdateValue()
        {
            var val = Design.GetDesignablePropertyValue(PropertyInfo) ?? string.Empty;

            // If it is a password property
            if (PropertyInfo.Name.Contains("Password", StringComparison.InvariantCultureIgnoreCase))
            {
                // With a non null value
                if (!string.IsNullOrWhiteSpace(val.ToString()))
                {
                    val = "****";
                }
            }

            DisplayMember = PropertyInfo.Name + ":" + val;
        }
    }
}
