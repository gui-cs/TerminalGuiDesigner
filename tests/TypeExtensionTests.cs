
using System.Collections.ObjectModel;

namespace UnitTests;

[TestFixture]
public class TypeExtensionTests
{
    [Test]
    public void IReadOnlyListOfString_IsDetected()
    {
        var type = typeof(IReadOnlyList<string>);

        Assert.That(
            type.IsGenericType(typeof(IReadOnlyList<>)),
            Is.True);
    }

    [Test]
    public void ListOfString_Implements_IReadOnlyList()
    {
        var type = typeof(List<string>);

        Assert.That(
            type.IsGenericType(typeof(IReadOnlyList<>)),
            Is.True);
    }

    [Test]
    public void ArrayOfString_Implements_IReadOnlyList()
    {
        var type = typeof(string[]);

        Assert.That(
            type.IsGenericType(typeof(IReadOnlyList<>)),
            Is.True);
    }

    [Test]
    public void CompilerGeneratedReadOnlyArray_IsDetected()
    {
        IReadOnlyList<int> list = new[] { 1, 2, 3 };
        var type = list.GetType(); // <>z__ReadOnlyArray`1

        Assert.That(
            type.IsGenericType(typeof(IReadOnlyList<>)),
            Is.True);
    }

    [Test]
    public void ReadOnlyCollection_IsDetected()
    {
        var type = typeof(ReadOnlyCollection<int>);

        Assert.That(
            type.IsGenericType(typeof(IReadOnlyList<>)),
            Is.True);
    }

    [Test]
    public void NonGenericType_DoesNotMatch()
    {
        var type = typeof(string);

        Assert.That(
            type.IsGenericType(typeof(IReadOnlyList<>)),
            Is.False);
    }

    [Test]
    public void UnrelatedGenericType_DoesNotMatch()
    {
        var type = typeof(Dictionary<int, string>);

        Assert.That(
            type.IsGenericType(typeof(IReadOnlyList<>)),
            Is.False);
    }

    [Test]
    public void OpenGeneric_IReadOnlyListDefinition_MatchesItself()
    {
        var type = typeof(IReadOnlyList<>);

        Assert.That(
            type.IsGenericType(typeof(IReadOnlyList<>)),
            Is.True);
    }

    [Test]
    public void Array_ReturnsElementType()
    {
        var type = typeof(string[]);

        Assert.That(
            type.GetElementTypeEx(),
            Is.EqualTo(typeof(string)));
    }

    [Test]
    public void GenericIList_ReturnsElementType()
    {
        var type = typeof(List<int>);

        Assert.That(
            type.GetElementTypeEx(),
            Is.EqualTo(typeof(int)));
    }

    [Test]
    public void IReadOnlyList_ReturnsElementType()
    {
        var type = typeof(IReadOnlyList<double>);

        Assert.That(
            type.GetElementTypeEx(),
            Is.EqualTo(typeof(double)));
    }

    [Test]
    public void IEnumerable_ReturnsElementType()
    {
        var type = typeof(IEnumerable<Guid>);

        Assert.That(
            type.GetElementTypeEx(),
            Is.EqualTo(typeof(Guid)));
    }

    [Test]
    public void CompilerGeneratedReadOnlyArray_ReturnsElementType()
    {
        IReadOnlyList<int> list = new[] { 1, 2, 3 };
        var type = list.GetType(); // <>z__ReadOnlyArray`1

        Assert.That(
            type.GetElementTypeEx(),
            Is.EqualTo(typeof(int)));
    }

    [Test]
    public void MultipleGenericArguments_ThrowsInvalidOperation()
    {
        var type = typeof(Dictionary<int, string>); // 2 generic args

        Assert.That(
            () => type.GetElementTypeEx(),
            Throws.InvalidOperationException
                  .With.Message.Contain("Expected exactly one generic argument"));
    }


    private class TestType
    {
        public int Foo { get; set; }
    }

    [Test]
    public void ExistingProperty_IsReturned()
    {
        var prop = typeof(TestType).GetPropertyOrThrow("Foo");

        Assert.That(prop, Is.Not.Null);
        Assert.That(prop.Name, Is.EqualTo("Foo"));
    }

    [Test]
    public void MissingProperty_Throws()
    {
        Assert.That(
            () => typeof(TestType).GetPropertyOrThrow("Bar"),
            Throws.Exception.With.Message.Contains("Could not find expected property"));
    }

}