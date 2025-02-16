
//------------------------------------------------------------------------------

//  <auto-generated>
//      This code was generated by:
//        TerminalGuiDesigner v1.1.0.0
//      You can make changes to this file and they will not be overwritten when saving.
//  </auto-generated>
// -----------------------------------------------------------------------------

using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace TerminalGuiDesigner.UI.Windows {
    using System.Collections;
    using Terminal.Gui;
    using TerminalGuiDesigner.ToCode;

    public partial class ArrayEditor : IValueGetterDialog {

        /// <summary>
        /// True if the editing was aborted.
        /// </summary>
        public bool Cancelled { get; private set; } = true;

        private readonly Design design;
        private Type elementType;

        /// <summary>
        /// The new array 
        /// </summary>
        [CanBeNull]
        public object Result => ResultAsList;
        public IList ResultAsList { get; private set; }


        /// <summary>
        /// Creates a new instance of the editor configured to build lists of <paramref name="elementType"/>
        /// and showing initial values held in <paramref name="oldValue"/> (if any).
        /// </summary>
        /// <param name="design"></param>
        /// <param name="elementType"></param>
        /// <param name="oldValue"></param>
        public ArrayEditor(Design design, Type elementType, IList oldValue) {
            InitializeComponent();
            this.design = design;
            this.elementType = elementType;
            
            Type listType = typeof(List<>).MakeGenericType(elementType);
            ResultAsList = (IList)Activator.CreateInstance(listType);
         
            foreach(var e in oldValue)
            {
                ResultAsList.Add(e);
            }

            lvElements.Source = ResultAsList.ToListDataSource();
            lvElements.KeyDown += LvElements_KeyDown;
            btnOk.Accepting += BtnOk_Clicked;
            btnCancel.Accepting += BtnCancel_Clicked;
            btnAddElement.Accepting += BtnAddElement_Clicked;
            btnDelete.Accepting += (s, e) => DeleteSelectedItem();
            btnMoveDown.Accepting += BtnMoveDown_Clicked;
            btnMoveUp.Accepting += BtnMoveUp_Clicked;
            btnEdit.Accepting += BtnEdit_Clicked;
        }


        private void BtnMoveUp_Clicked(object sender, EventArgs e)
        {
            // Moving up means reducing the index by 1
            var idx = lvElements.SelectedItem;

            if (idx >= 1 && idx < ResultAsList.Count)
            { 
                var toMove = ResultAsList[idx];
                var newIndex = idx - 1;
                ResultAsList.RemoveAt(idx);
                ResultAsList.Insert(newIndex, toMove);

                lvElements.Source = ResultAsList.ToListDataSource();
                lvElements.SelectedItem = newIndex;
                lvElements.SetNeedsDraw();
            }
        }

        private void BtnMoveDown_Clicked(object sender, EventArgs e)
        {
            // Moving up means increasing the index by 1
            var idx = lvElements.SelectedItem;

            if (idx >= 0 && idx < ResultAsList.Count-1)
            {
                var toMove = ResultAsList[idx];
                var newIndex = idx + 1;
                ResultAsList.RemoveAt(idx);
                ResultAsList.Insert(newIndex, toMove);

                lvElements.Source = ResultAsList.ToListDataSource();
                lvElements.SelectedItem = newIndex;
                lvElements.SetNeedsDraw();
            }
        }

        private void LvElements_KeyDown(object sender, Key e)
        {
            if(e == Key.DeleteChar)
            {
                DeleteSelectedItem();
                e.Handled = true;
            }
        }

        private void DeleteSelectedItem()
        {
            var idx = lvElements.SelectedItem;

            if (idx >= 0 && idx < ResultAsList.Count)
            {
                ResultAsList.RemoveAt(idx);

                lvElements.Source = ResultAsList.ToListDataSource();
                lvElements.SetNeedsDraw();
                lvElements.SelectedItem = 0;
            }
        }

        private void BtnAddElement_Clicked(object sender, EventArgs e)
        {
            if(ValueFactory.GetNewValue("Element Value", design, this.elementType,null, out var newValue,true))
            {
                ResultAsList.Add(newValue);                
            }

            lvElements.Source = ResultAsList.ToListDataSource();
            lvElements.SelectedItem = ResultAsList.Count - 1;
            lvElements.SetNeedsDraw();
        }
        private void BtnEdit_Clicked(object sender, EventArgs e)
        {
            var idx = lvElements.SelectedItem;

            if (idx >= 0 && idx < ResultAsList.Count)
            {
                var toEdit = ResultAsList[idx];

                if (ValueFactory.GetNewValue("Element Value", design, this.elementType, toEdit, out var newValue, true))
                {
                    // Replace old with new
                    ResultAsList.RemoveAt(idx);
                    ResultAsList.Insert(idx, newValue);
                }

                lvElements.Source = ResultAsList.ToListDataSource();
                lvElements.SelectedItem = idx;
                lvElements.SetNeedsDraw();
            }
        }

        private void BtnCancel_Clicked(object sender, EventArgs e)
        {
            Cancelled = true;
            Application.RequestStop();
        }

        private void BtnOk_Clicked(object sender, EventArgs e)
        {
            Cancelled = false;
            Application.RequestStop();
        }
    }
}
