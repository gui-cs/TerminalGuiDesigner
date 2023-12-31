
//------------------------------------------------------------------------------

//  <auto-generated>
//      This code was generated by:
//        TerminalGuiDesigner v1.1.0.0
//      You can make changes to this file and they will not be overwritten when saving.
//  </auto-generated>
// -----------------------------------------------------------------------------
namespace TerminalGuiDesigner.UI.Windows {
    using System.Collections;
    using Terminal.Gui;
    using TerminalGuiDesigner.ToCode;

    public partial class ArrayEditor {

        /// <summary>
        /// True if the editing was aborted.
        /// </summary>
        public bool Cancelled { get; private set; } = true;

        private Type elementType;
        private readonly Property property;

        /// <summary>
        /// The new array 
        /// </summary>
        public IList Result { get; private set; }

        public ArrayEditor(Property property) {
            InitializeComponent();

            this.elementType = property.GetElementType();



            Type listType = typeof(List<>).MakeGenericType(property.GetElementType());
            Result = (IList)Activator.CreateInstance(listType);
         
            
            foreach(var e in (IList)property.GetValue())
            {
                Result.Add(e);
            }

            lvElements.SetSource(Result);
            btnOk.Clicked += BtnOk_Clicked;
            btnCancel.Clicked += BtnCancel_Clicked;
            btnAddElement.Clicked += BtnAddElement_Clicked;
            this.property = property;
        }

        private void BtnAddElement_Clicked(object sender, EventArgs e)
        {
            if(ValueFactory.GetNewValue(property.PropertyInfo.Name, this.property.Design, this.elementType,null, out var newValue, ValueFactory.AllowMultiLine(property)))
            {
                Result.Add(newValue);                
            }

            lvElements.SetSource(Result);
            lvElements.SetNeedsDisplay();
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
