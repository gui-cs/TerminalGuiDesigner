using System;
using System.IO;
using TerminalGuiDesigner.UI;
using YamlDotNet.Serialization;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( KeyMap ) )]
[Category( "Core" )]
[Category( "UI" )]
[Parallelizable( ParallelScope.All )]
internal class KeyMapTests : Tests
{
    [Test]
    public void TestSerializingKeyMap()
    {
        var keys = Path.Combine(TestContext.CurrentContext.TestDirectory, "Keys.yaml");
        FileAssert.Exists(keys);

        var km = new KeyMap();

        var serializer = new Serializer();
        var expected = serializer.Serialize(km);

        ClassicAssert.AreEqual(
            expected.Replace("\r\n", "\n").Trim(),
            File.ReadAllText(keys).Replace("\r\n", "\n").Trim(),
            $"The default yaml file ('Keys.yaml') that ships with TerminalGuiDesigner should match the default values in KeyMap. Set it to:{Environment.NewLine}{expected}");
    }
}