using NUnit.Framework;
using TerminalGuiDesigner;
using Terminal.Gui;
using System.Linq;

namespace tests;

public class DesignTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void GetAllDesigns_FindSubviewDesign()
    {
        var v1 = new View();
        var v2 = new View();
        var v3 = new View();

        v1.Add(v2);
        v2.Add(v3);

        v3.Data = new Design("yarg",v3);

        var d = new Design("root",v1);

        Assert.AreEqual("yarg",d.GetAllDesigns().Single().FieldName);
    }
}