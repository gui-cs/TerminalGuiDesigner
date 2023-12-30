namespace UnitTests;

[TestFixture]
[TestOf( typeof( Design ) )]
[Category( "Core" )]
[Category( "UI" )]
internal class DesignTests
{
    [Test]
    [Category( "Terminal.Gui Compatibility" )]
    [TestCaseSource( nameof( CanDesignExpectedProperties_Cases ) )]
    [TestOf( typeof( Property ) )]
    public void CanDesignExpectedProperty<T>( T viewToTest, string propertyName, Type propertyType )
        where T : View
    {
        var file = new FileInfo( $"FakeFileFor_CanDesignExpectedProperties_{typeof( T ).Name}.cs" );
        var viewToCode = new ViewToCode( );
        var designOut = viewToCode.GenerateNewView( file, "YourNamespace", typeof( Window ) );

        var op = new AddViewOperation( viewToTest, designOut, "myLabel" );
        op.Do( );

        Assume.That( designOut, Is.Not.Null.And.InstanceOf<Design>( ) );
        Assume.That( designOut.View, Is.Not.Null.And.InstanceOf<Window>( ) );

        // the Hello world label
        var viewDesigner = designOut.GetAllDesigns( ).SingleOrDefault( d => d.View is T );
        Assert.That( viewDesigner, Is.Not.Null.And.InstanceOf<Design>( ) );
        Assert.That( viewDesigner!.View, Is.Not.Null.And.InstanceOf<T>( ) );

        var propertyBeingChanged = viewDesigner.GetDesignableProperties( ).SingleOrDefault( p => p.PropertyInfo.Name.Equals( propertyName ) );
        Assert.That( propertyBeingChanged, Is.Not.Null.And.InstanceOf<Property>( ) );
        Assert.That( propertyBeingChanged!.PropertyInfo.Name, Is.EqualTo( propertyName ) );
        Assert.That( propertyBeingChanged.PropertyInfo.PropertyType, Is.EqualTo( propertyType ) );
    }

    private static IEnumerable<TestCaseData> CanDesignExpectedProperties_Cases( )
    {
        foreach ( TestCaseData d in LabelTests.ExpectedDesignableProperties_Cases )
        {
            yield return d;
        }
        
        yield return new( ViewFactory.Create<TextField>( null, 1, "Hello World" ), "X", typeof( Pos ) );
        yield return new( ViewFactory.Create<TextField>( null, 1, "Hello World" ), "Y", typeof( Pos ) );
    }

}