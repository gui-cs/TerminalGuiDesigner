# TerminalGuiDesigner - Adding new Properties

## Making a new Property designable

To add desiging for a new View property update the `IEnumerable<Property> LoadDesignableProperties()` method.

For example:

```csharp
if(View is ScrollView)
{
	yield return CreateProperty(nameof(ScrollView.ContentSize));
}
```

Adding a new 'round trip' test to confirm that the property is serialized/loaded correctly:

```csharp
[Test]
public void TestRoundTrip_PreserveContentSize()
{
    var scrollViewIn = RoundTrip<ScrollView>((s) =>
            s.ContentSize = new Size(10, 5)
            );

    Assert.AreEqual(10, scrollViewIn.ContentSize.Width);
    Assert.AreEqual(5, scrollViewIn.ContentSize.Height);
}
```

## New Type

### To Code

If you need to add support for a new `Type` of Property (e.g. `ContentSize` property is of Type `Size`) then you must do the following:

- Update Property `virtual CodeExpression GetRhs()`:

```csharp
if(val is Size s)
{
    return new CodeSnippetExpression(s.ToCode());
}
```

Provide an implementation of `ToCode` as an [Extension Method](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods) on the new Type

```csharp
public static class SizeExtensions
{
    public static string ToCode(this Size s)
    {
        return $"new Size({s.Width},{s.Height})";
    }
}
```

This should make the 'round trip' test (see above) pass.  But you will still need to add designer UI support for editing the Type.

### Designer UIs

Create a new `Dialog` for the new `Type` in `TerminalGuiDesigner.UI.Windows`.  Use TerminalGuiDesigner to create the form.
For example see `SizeEditor.Designer.cs` / `SizeEditor.cs`

Update `EditDialog.cs` method `GetNewValue` to call the new editor window.
