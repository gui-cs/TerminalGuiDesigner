namespace TerminalGuiDesigner
{
    public class PropertyDesign
    {

        public string Code {get;}
        public object Value {get;}

        public Func<string>[] CodeParameters { get; }
        
        public PropertyDesign(string code, object value, params Func<string>[] codeParameters)
        {
            Code = code;
            Value = value;

            // Using delegates means that rename operations will
            // not break us
            CodeParameters = codeParameters;
        }

        string GetCodeWithParameters()
        {
            return string.Format(Code,CodeParameters.Select(f=>f()).ToArray());
        }
        public override string ToString()
        {
            return GetCodeWithParameters();
        }
    }
}