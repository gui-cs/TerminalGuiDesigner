using System.Text;


namespace UnitTests
{
    internal class SliderTests : Tests
    {
        private static IEnumerable<TestCaseData> Orientation_Cases
        {
            get
            {
                return new TestCaseData[]
                {
                    new TestCaseData( Orientation.Horizontal ),
                    new TestCaseData( Orientation.Vertical ),
                };
            }
        }
        [Test]
        public void TestRoundTrip_Slider_PreserveStringOptions()
        {
            var sliderIn = RoundTrip<Dialog, Slider<string>>((d, v) =>
            {
                v.Options.Add(new SliderOption<string> { Legend = "l1", LegendAbbr = new Rune('1'), Data = "Fun1" });
                v.Options.Add(new SliderOption<string> { Legend = "l2", LegendAbbr = new Rune('2'), Data = "Fun2" });

                Assert.That(v.Options.Count, Is.EqualTo(2));
            }, out _);

            Assert.That(sliderIn.Options.Count, Is.EqualTo(2));

            // TODO: Will pass tests when https://github.com/gui-cs/Terminal.Gui/issues/3100 is merged and nuget package drawn down.  And relevant constructor called in our CodeDOM
            Assert.That(sliderIn.Options[0].Legend, Is.EqualTo("l1"));
            Assert.That(sliderIn.Options[0].LegendAbbr, Is.EqualTo(new Rune('1')));
            Assert.That(sliderIn.Options[0].Data, Is.EqualTo("Fun1"));

            Assert.That(sliderIn.Options[1].Legend, Is.EqualTo("l2"));
            Assert.That(sliderIn.Options[1].LegendAbbr, Is.EqualTo(new Rune('2')));
            Assert.That(sliderIn.Options[1].Data, Is.EqualTo("Fun2"));
        }

        [Test]
        [TestCaseSource(nameof(Orientation_Cases))]
        public void TestRoundTrip_Slider_PreserveOrientation(Orientation o)
        {
            var sliderIn = RoundTrip<Dialog, Slider<string>>((d, v) =>
            {
                d.GetDesignableProperty("Orientation")?.SetValue(o);
                Assert.That(v.Orientation, Is.EqualTo(o));
            }, out _);

            Assert.That(sliderIn.Orientation, Is.EqualTo(o));
        }
    }
}
