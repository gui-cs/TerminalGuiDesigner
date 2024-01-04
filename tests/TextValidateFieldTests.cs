using Terminal.Gui.TextValidateProviders;

namespace UnitTests;

[TestFixture]
[Category( "Code Generation" )]
[TestOf( typeof( ViewToCode ) )]
[TestOf( typeof( CodeToView ) )]
internal class TextValidateFieldTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveProvider()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_PreserveProvider.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Window));

        var tvfOut = ViewFactory.Create<TextValidateField>();

        ClassicAssert.IsNotNull(tvfOut.Provider);

        OperationManager.Instance.Do(new AddViewOperation(tvfOut, designOut, "myfield"));

        viewToCode.GenerateDesignerCs(designOut, typeof(Window));

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        var tvfIn = designBackIn.View.GetActualSubviews().OfType<TextValidateField>().Single();

        ClassicAssert.IsNotNull(tvfIn.Provider);
    }
}
