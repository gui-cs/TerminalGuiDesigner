using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( OperationManager ) )]
[Category( "Core" )]
[Category( "UI" )]
internal class OperationManagerTests
{
    [Test]
    [TestOf( typeof( Label ) )]
    [TestOf( typeof( Property ) )]
    public void ChangingLabelProperty( [Values( "X" )] string propertyName )
    {
        var file = new FileInfo( "Test_ChangingLabelX.cs" );
        var viewToCode = new ViewToCode(Mock.Of<IApplication>());
        var designOut = viewToCode.GenerateNewView( file, "YourNamespace", typeof( Window ) );

        var op = new AddViewOperation(Mock.Of<IApplication>(), new Label() { Text = "Hello World" }, designOut, "myLabel" );
        op.Do( );

        Assume.That( designOut, Is.Not.Null.And.InstanceOf<Design>( ) );
        Assume.That( designOut.View, Is.Not.Null.And.InstanceOf<Window>( ) );

        // the Hello world label
        var lblDesign = designOut.GetAllDesigns( ).SingleOrDefault( d => d.View is Label );
        Assume.That( lblDesign, Is.Not.Null.And.InstanceOf<Design>( ) );
        Assume.That( lblDesign!.View, Is.Not.Null.And.InstanceOf<Label>( ) );

        var propertyBeingChanged = lblDesign.GetDesignableProperties( ).SingleOrDefault( p => p.PropertyInfo.Name.Equals( propertyName ) );
        Assume.That( propertyBeingChanged, Is.Not.Null.And.InstanceOf<Property>( ) );
        Assume.That( propertyBeingChanged!.PropertyInfo.Name, Is.EqualTo( propertyName ) );

        Assume.That( OperationManager.Instance.UndoStackSize, Is.Zero );
        Assume.That( OperationManager.Instance.RedoStackSize, Is.Zero );

        OperationManager.Instance.Do( new SetPropertyOperation(Mock.Of<IApplication>(), lblDesign, propertyBeingChanged, propertyBeingChanged.GetValue( ), Pos.Absolute( 10 ) ) );
        Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 1 ) );
        Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );

        OperationManager.Instance.Do( new SetPropertyOperation(Mock.Of<IApplication>(), lblDesign, propertyBeingChanged, propertyBeingChanged.GetValue( ), Pos.Percent( 50 ) ) );
        Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 2 ) );
        Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );

        OperationManager.Instance.Do( new SetPropertyOperation(Mock.Of<IApplication>(), lblDesign, propertyBeingChanged, propertyBeingChanged.GetValue( ), Pos.Absolute( 10 ) ) );
        Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 3 ) );
        Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );

        OperationManager.Instance.Do( new SetPropertyOperation(Mock.Of<IApplication>(), lblDesign, propertyBeingChanged, propertyBeingChanged.GetValue( ), Pos.Percent( 50 ) ) );
        Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 4 ) );
        Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );

        OperationManager.Instance.Undo( );
        Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 3 ) );
        Assert.That( OperationManager.Instance.RedoStackSize, Is.EqualTo( 1 ) );

        OperationManager.Instance.Undo( );
        Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 2 ) );
        Assert.That( OperationManager.Instance.RedoStackSize, Is.EqualTo( 2 ) );

        OperationManager.Instance.Redo( );
        Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 3 ) );
        Assert.That( OperationManager.Instance.RedoStackSize, Is.EqualTo( 1 ) );

        OperationManager.Instance.Redo( );
        Assert.That( OperationManager.Instance.UndoStackSize, Is.EqualTo( 4 ) );
        Assert.That( OperationManager.Instance.RedoStackSize, Is.Zero );

        Assert.That( lblDesign.View.X.ToString( ), Is.EqualTo( $"Percent({50})" ) );
    }
}
