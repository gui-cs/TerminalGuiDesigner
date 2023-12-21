using System.Reflection;
using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>Helper methods to simplify reflection operations.</summary>
public static class ReflectionHelpers
{
    /// <summary>
    ///   Gets a non-public field value from a non-null <typeparamref name="TIn" /> inheriting from <see cref="View" /> as a
    ///   <typeparamref name="TOut" />.
    /// </summary>
    /// <typeparam name="TOut">The type of the field. Must pass a <see langword="notnull" /> constraint.</typeparam>
    /// <typeparam name="TIn">
    ///   The type of the <see cref="View" /> to get the field on.
    /// </typeparam>
    /// <param name="item">The <see cref="View" /> to reflect on.</param>
    /// <param name="fieldName">The name of the field to get via reflection.</param>
    /// <returns>
    ///   A non-null <typeparamref name="TOut" /> from the reflected private field of <paramref name="item" />.
    /// </returns>
    /// <exception cref="MissingFieldException">
    ///   If the requested <paramref name="fieldName" /> does not exist on type <typeparamref name="TIn" />.
    /// </exception>
    /// <exception cref="FieldAccessException">
    ///   If the requested <paramref name="fieldName" /> on type <typeparamref name="TIn" /> is not of type <typeparamref name="TOut" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   If the value of the requested <paramref name="fieldName" /> on <paramref name="item" /> was null.
    /// </exception>
    internal static TOut GetNonNullNonPublicFieldValue<TOut, TIn>( this TIn? item, string fieldName )
        where TIn : View
        where TOut : notnull
    {
        ArgumentNullException.ThrowIfNull( item, nameof( item ) );
        ArgumentException.ThrowIfNullOrEmpty( fieldName, nameof( fieldName ) );

        FieldInfo selectedField = typeof( TIn ).GetField( fieldName, BindingFlags.NonPublic | BindingFlags.Instance )
                                  ?? throw new MissingFieldException( $"Expected non-public instance field {fieldName} was not present on {typeof( TIn ).Name}" );

        if ( selectedField.FieldType != typeof( TOut ) )
        {
            throw new FieldAccessException( $"Field {fieldName} on {typeof( TIn ).Name} is not of expected type {typeof( TOut ).Name}" );
        }

        return (TOut)( selectedField.GetValue( item )
                       ?? throw new InvalidOperationException( $"Non-public instance field {fieldName} was unexpectedly null on {typeof( TIn ).Name}" ) );
    }

    public static View GetDefaultViewInstance( Type t )
    {
        if ( !t.IsAssignableTo( typeof(View) ) )
        {
            throw new ArgumentOutOfRangeException( nameof( t ), $"{t.Name} must be assignable to the View type" );
        }

        var instance = Activator.CreateInstance( t ) as View ?? throw new Exception( $"CreateInstance returned null for Type '{t.Name}'" );
        instance.SetActualText( "Heya" );

        instance.Width = Math.Max( instance.Bounds.Width, 4 );
        instance.Height = Math.Max( instance.Bounds.Height, 1 );

        return instance;
    }
}
