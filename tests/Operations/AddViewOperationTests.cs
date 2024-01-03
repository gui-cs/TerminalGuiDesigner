using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace UnitTests.Operations;

[TestFixture]
[TestOf(typeof(AddViewOperation))]
internal class AddViewOperationTests : Tests
{
    private static Type[] SupportedViewTypes { get; } = ViewFactory.SupportedViewTypes // Add MenuBar last so order is preserved in Assert check.
                                                                   .OrderBy( t => t == typeof( MenuBar ) ? int.MaxValue : 0 )
                                                                   .Select(PickFirstTTypeForGenerics)
                                                                   .ToArray( );

    

    [Test( Description = "Tests AddViewOperation against all SupportedViewTypes" )]
    public void Do_AddsExpectedSubview( [ValueSource( nameof( SupportedViewTypes ) )] Type candidateType )
    {
        var d = Get10By10View( );

        var instance = ViewFactory.Create( candidateType );
        const string instanceFieldName = "blah";
        var op = new AddViewOperation( instance, d, instanceFieldName );
        Assert.That( op.Do, Throws.Nothing );

        IList<View> subviews = d.View.Subviews;

        Assert.That( subviews, Has.Count.EqualTo( 1 ) );

        var theOnlySubView = subviews[ 0 ];
        Assert.That( theOnlySubView, Is.SameAs( instance ) );
        Assert.That( ( (Design)theOnlySubView.Data ).FieldName, Is.EqualTo( instanceFieldName ) );
    }

    [Test]
    [TestCase( 1, "justOne" )]
    [TestCase( 10, "multiple" )]
    public void Do_SubviewNamesProperlyDeDuplicated( int numberOfViews, string baseName )
    {
        var d = Get10By10View( );

        for ( int operationNumber = 1; operationNumber <= numberOfViews; operationNumber++ )
        {
            // Doesn't matter which type we use, here, because this isn't a type test
            // Just needs to be a valid type, which is tested in Do_AddsExpectedSubview
            var instance = ViewFactory.Create<TextField>( );
            
            var op = new AddViewOperation( instance, d, baseName );
            Assert.That( op.Do, Throws.Nothing );

            IList<View> subviews = d.View.Subviews;

            Assert.That( subviews, Has.Count.EqualTo( operationNumber ) );

            var lastSubview = subviews.Last( );

            Assert.That( lastSubview, Is.SameAs( instance ) );

            string expectedFieldName = operationNumber == 1 ? baseName : $"{baseName}{operationNumber}";
            Assert.That( ( (Design)lastSubview.Data ).FieldName,
                         Is.EqualTo( expectedFieldName ) );
        }
    }

    [Test]
    public void TestAddView_RoundTrip( [ValueSource( nameof( SupportedViewTypes ) )] Type type )
    {
        using var windowIn = RoundTrip<Toplevel, Window>( ( d, v ) =>
        {
            var instance = ViewFactory.Create( type );
            var op = new AddViewOperation( instance, d, "blah" );
            op.Do( );
        }, out _ );

        IList<View> roundTripViews = windowIn.GetActualSubviews( );

        Assert.That( roundTripViews, Has.Count.EqualTo( 1 ) );
        Assert.That( roundTripViews[ 0 ], Is.InstanceOf( type ) );
    }

    [Test]
    public void UnDo_RemovesExpectedViews()
    {
        var d = Get10By10View();

        int stackSize = 0;

        Assume.That( d.View.Subviews, Is.Empty );
        
        foreach (var type in SupportedViewTypes)
        {
            stackSize++;
            var instance = ViewFactory.Create(type);
            var op = new AddViewOperation(instance, d, "blah");
            Assume.That( ( ) => OperationManager.Instance.Do( op ), Throws.Nothing );

            Assume.That( d.View.Subviews, Has.Count.EqualTo( stackSize ) );
        }

        for (int i = 1; i <= stackSize; i++)
        {
            Assert.That( OperationManager.Instance.Undo, Throws.Nothing );
            Assert.That( d.View.Subviews,
                         Has.Count.EqualTo( stackSize - i ),
                         "Expected the count of views to decrease once each time we Undo" );
        }

        Assert.That( d.View.Subviews, Is.Empty );
    }
}
