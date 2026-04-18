using System.Collections;
using System.Text;
using Terminal.Gui.ViewBase;


namespace UnitTests
{
    internal class SliderTests : Tests
    {
        private static IEnumerable<TestCaseData> LinearRange_SupportedTTypes =>
            TTypes.GetSupportedTTypesForGenericViewOfType(typeof(LinearRange<>))
                  .Distinct()
                  .Select(t => new TestCaseData(t).SetName($"Create_LinearRange_HasOptions<{t.Name}>"));

        [Test]
        [TestCaseSource(nameof(LinearRange_SupportedTTypes))]
        public void Create_LinearRange_AllSupportedTTypes_HaveAtLeastTwoOptions(Type tType)
        {
            var linearRangeType = typeof(LinearRange<>).MakeGenericType(tType);

            var method = typeof(ViewFactory).GetMethods()
                .Single(m => m.Name == "Create" && m.IsGenericMethodDefinition);
            var concreteMethod = method.MakeGenericMethod(linearRangeType);
            var view = (View)concreteMethod.Invoke(null, new object?[] { null, null, null })!;

            var optionsProp = linearRangeType.GetProperty("Options");
            var options = optionsProp!.GetValue(view) as IList;

            Assert.That(options, Is.Not.Null, $"Options property was null for LinearRange<{tType.Name}>");
            Assert.That(options!.Count, Is.GreaterThanOrEqualTo(2), $"LinearRange<{tType.Name}> had fewer than 2 options after ViewFactory.Create");
        }

        [Test]
        public void TestRoundTrip_LinearRange_Bool_PreservesDefaultOptions()
        {
            var sliderIn = RoundTrip<Dialog, LinearRange<bool>>((d, v) =>
            {
                Assert.That(v.Options.Count, Is.EqualTo(2));
            }, out var viewOut);

            Assert.That(sliderIn.Options.Count, Is.EqualTo(viewOut.Options.Count));
            for (int i = 0; i < viewOut.Options.Count; i++)
            {
                Assert.That(sliderIn.Options[i].Legend, Is.EqualTo(viewOut.Options[i].Legend));
                Assert.That(sliderIn.Options[i].LegendAbbr, Is.EqualTo(viewOut.Options[i].LegendAbbr));
                Assert.That(sliderIn.Options[i].Data, Is.EqualTo(viewOut.Options[i].Data));
            }
        }

        [Test]
        public void TestRoundTrip_LinearRange_Int_PreservesDefaultOptions()
        {
            var sliderIn = RoundTrip<Dialog, LinearRange<int>>((d, v) =>
            {
                Assert.That(v.Options.Count, Is.GreaterThanOrEqualTo(2));
            }, out var viewOut);

            Assert.That(sliderIn.Options.Count, Is.EqualTo(viewOut.Options.Count));
            for (int i = 0; i < viewOut.Options.Count; i++)
            {
                Assert.That(sliderIn.Options[i].Legend, Is.EqualTo(viewOut.Options[i].Legend));
                Assert.That(sliderIn.Options[i].LegendAbbr, Is.EqualTo(viewOut.Options[i].LegendAbbr));
                Assert.That(sliderIn.Options[i].Data, Is.EqualTo(viewOut.Options[i].Data));
            }
        }

        [Test]
        public void TestRoundTrip_LinearRange_Double_PreservesDefaultOptions()
        {
            var sliderIn = RoundTrip<Dialog, LinearRange<double>>((d, v) =>
            {
                Assert.That(v.Options.Count, Is.GreaterThanOrEqualTo(2));
            }, out var viewOut);

            Assert.That(sliderIn.Options.Count, Is.EqualTo(viewOut.Options.Count));
            for (int i = 0; i < viewOut.Options.Count; i++)
            {
                Assert.That(sliderIn.Options[i].Legend, Is.EqualTo(viewOut.Options[i].Legend));
                Assert.That(sliderIn.Options[i].LegendAbbr, Is.EqualTo(viewOut.Options[i].LegendAbbr));
                Assert.That(sliderIn.Options[i].Data, Is.EqualTo(viewOut.Options[i].Data));
            }
        }

        [Test]
        public void TestRoundTrip_LinearRange_String_PreservesDefaultOptions()
        {
            var sliderIn = RoundTrip<Dialog, LinearRange<string>>((d, v) =>
            {
                Assert.That(v.Options.Count, Is.EqualTo(3));
            }, out var viewOut);

            Assert.That(sliderIn.Options.Count, Is.EqualTo(viewOut.Options.Count));
            for (int i = 0; i < viewOut.Options.Count; i++)
            {
                Assert.That(sliderIn.Options[i].Legend, Is.EqualTo(viewOut.Options[i].Legend));
                Assert.That(sliderIn.Options[i].LegendAbbr, Is.EqualTo(viewOut.Options[i].LegendAbbr));
                Assert.That(sliderIn.Options[i].Data, Is.EqualTo(viewOut.Options[i].Data));
            }
        }


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
            // ViewFactory.Create gives 3 default options; adjust adds 2 more = 5 total
            var sliderIn = RoundTrip<Dialog, LinearRange<string>>((d, v) =>
            {
                v.Options.Add(new LinearRangeOption<string>("l1", new Rune('1'), "Fun1"));
                v.Options.Add(new LinearRangeOption<string> { Legend = "l2", LegendAbbr = new Rune('2'), Data = "Fun2" });

                Assert.That(v.Options.Count, Is.EqualTo(5));
            }, out _);

            Assert.That(sliderIn.Options.Count, Is.EqualTo(5));

            Assert.That(sliderIn.Options[0].Legend, Is.EqualTo("Option 1"));
            Assert.That(sliderIn.Options[0].LegendAbbr, Is.EqualTo(new Rune('1')));
            Assert.That(sliderIn.Options[0].Data, Is.EqualTo("Option 1"));

            Assert.That(sliderIn.Options[1].Legend, Is.EqualTo("Option 2"));
            Assert.That(sliderIn.Options[1].LegendAbbr, Is.EqualTo(new Rune('2')));
            Assert.That(sliderIn.Options[1].Data, Is.EqualTo("Option 2"));

            Assert.That(sliderIn.Options[2].Legend, Is.EqualTo("Option 3"));
            Assert.That(sliderIn.Options[2].LegendAbbr, Is.EqualTo(new Rune('3')));
            Assert.That(sliderIn.Options[2].Data, Is.EqualTo("Option 3"));

            Assert.That(sliderIn.Options[3].Legend, Is.EqualTo("l1"));
            Assert.That(sliderIn.Options[3].LegendAbbr, Is.EqualTo(new Rune('1')));
            Assert.That(sliderIn.Options[3].Data, Is.EqualTo("Fun1"));

            Assert.That(sliderIn.Options[4].Legend, Is.EqualTo("l2"));
            Assert.That(sliderIn.Options[4].LegendAbbr, Is.EqualTo(new Rune('2')));
            Assert.That(sliderIn.Options[4].Data, Is.EqualTo("Fun2"));
        }

        [Test]
        [TestCaseSource(nameof(Orientation_Cases))]
        public void TestRoundTrip_Slider_PreserveOrientation(Orientation o)
        {
            var sliderIn = RoundTrip<Dialog, LinearRange<string>>((d, v) =>
            {
                d.GetDesignableProperty("Orientation")?.SetValue(o);
                Assert.That(v.Orientation, Is.EqualTo(o));
            }, out _);

            Assert.That(sliderIn.Orientation, Is.EqualTo(o));
        }
    }
}
