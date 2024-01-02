namespace UnitTests
{
    internal class TreeViewTests : Tests
    {
        [Test]
        public void TestRoundTrip_TreeView_FileSystemInfo()
        {
            Assert.That(
                TTypes.GetSupportedTTypesForGenericViewOfType(typeof(TreeView<>)),
                Contains.Item(typeof(FileSystemInfo)));

            var sliderIn = RoundTrip<Dialog, TreeView<FileSystemInfo>>((d, v) =>
            {
                var objectsProperty = d.GetDesignableProperty("Objects");
                Assert.That(objectsProperty,Is.InstanceOf<TreeObjectsProperty<FileSystemInfo>>());

                // When TreeView is empty it is invisible, so should show border
                Assert.That(v.IsBorderlessContainerView, Is.True);

                objectsProperty.SetValue(
                    new List<FileSystemInfo>(
                        new FileSystemInfo[] {
                            new DirectoryInfo("/"),
                            new FileInfo("/blah.txt")
                        }));

                Assert.That(v.Objects.Count, Is.EqualTo(2));
                Assert.That(v.TreeBuilder, Is.Not.Null);

                // Once tree view is no longer empty it can loose its 'invisible' border
                Assert.That(v.IsBorderlessContainerView, Is.False);

            }, out var viewAfter);


            Assert.That(viewAfter.Objects.Count, Is.EqualTo(2));

            Assert.That(viewAfter.Objects.Count, Is.EqualTo(2));
            Assert.That(viewAfter.Objects.Count, Is.EqualTo(2));

            Assert.That(viewAfter.Objects.ElementAt(0).ToString(), Is.EqualTo(new DirectoryInfo("/").ToString()));
            Assert.That(viewAfter.Objects.ElementAt(1).ToString(), Is.EqualTo(new FileInfo("/blah.txt").ToString()));
            Assert.That(viewAfter.TreeBuilder, Is.Not.Null);

            Assert.That(viewAfter.IsBorderlessContainerView, Is.False);
        }

        [Test]
        public void TestRoundTrip_TreeView_Object()
        {
            Assert.That(
                TTypes.GetSupportedTTypesForGenericViewOfType(typeof(TreeView<>)),
                Contains.Item(typeof(object)));

            var sliderIn = RoundTrip<Dialog, TreeView<object>>((d, v) =>
            {
                var objectsProperty = d.GetDesignableProperty("Objects");
                Assert.That(objectsProperty, Is.InstanceOf<TreeObjectsProperty<object>>());
                Assert.That(v.TreeBuilder, Is.Null);

                Assert.That(v.IsBorderlessContainerView, Is.True);

            }, out var viewAfter);

            Assert.That(viewAfter.TreeBuilder, Is.Null);
            Assert.That(viewAfter.IsBorderlessContainerView, Is.True);
        }
    }
}
