using Terminal.Gui.TextValidateProviders;

namespace UnitTests;

[TestFixture]
[Category( "Code Generation" )]
[TestOf( typeof( ViewToCode ) )]
[TestOf( typeof( CodeToView ) )]
internal class TextValidateFieldTests : Tests
{
    [Test]
    public void RoundTrip_PreservesProvider( )
    {
        ViewToCode viewToCode = new( );

        FileInfo file = new( $"{nameof( RoundTrip_PreservesProvider )}.cs" );
        Design designOut = viewToCode.GenerateNewView( file, "YourNamespace", typeof( Window ) );

        using TextValidateField tvfOut = ViewFactory.Create<TextValidateField>( );

        // Expected default from the ViewFactory:
        Assume.That( tvfOut.Provider, Is.Not.Null.And.InstanceOf<TextRegexProvider>( ) );
        Assume.That( ( (TextRegexProvider)tvfOut.Provider ).Pattern, Is.EqualTo( ".*" ) );

        AddViewOperation addTvfOp = new( tvfOut, designOut, "myfield" );
        Assume.That( addTvfOp.IsImpossible, Is.False );
        bool addTvfOpSucceeded = false;
        Assume.That( ( ) => addTvfOpSucceeded = OperationManager.Instance.Do( addTvfOp ), Throws.Nothing );
        Assume.That( addTvfOpSucceeded );

        Assert.That( ( ) => viewToCode.GenerateDesignerCs( designOut, typeof( Window ) ), Throws.Nothing );

        CodeToView codeToView = new( designOut.SourceCode );
        Design designBackIn = codeToView.CreateInstance( );

        using TextValidateField tvfIn = designBackIn.View.GetActualSubviews( ).OfType<TextValidateField>( ).Single( );
        Assert.That( tvfIn, Is.Not.Null.And.InstanceOf<TextValidateField>( ) );
        Assert.That( tvfIn, Is.Not.SameAs( tvfOut ) );
        Assert.That( tvfIn.Provider, Is.Not.Null.And.InstanceOf<TextRegexProvider>( ) );
        Assert.That( ( (TextRegexProvider)tvfIn.Provider ).Pattern, Is.EqualTo( ".*" ) );
    }
}
