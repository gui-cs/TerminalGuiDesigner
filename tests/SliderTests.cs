using System.Text;


namespace UnitTests
{
    internal class SliderTests : Tests
    {

        [Test]
        public void TestRoundTrip_Slider_PreserveStringOptions()
        {
            var sliderIn = RoundTrip<Dialog, Slider<string>>((d, v) =>
            {
                v.Options.Add(new SliderOption<string> { Legend = "l1", LegendAbbr = new Rune('1'), Data = "Fun1" });
                v.Options.Add(new SliderOption<string> { Legend = "l2", LegendAbbr = new Rune('2'), Data = "Fun2" });

                ClassicAssert.AreEqual(2, v.Options.Count);
            }, out _);

            ClassicAssert.AreEqual(2, sliderIn.Options.Count);
            
            ClassicAssert.AreEqual("l1", sliderIn.Options[0].Legend);
            ClassicAssert.AreEqual(new Rune('1'), sliderIn.Options[0].LegendAbbr);
            ClassicAssert.AreEqual("Fun1", sliderIn.Options[0].Data);

            ClassicAssert.AreEqual("l2", sliderIn.Options[1].Legend);
            ClassicAssert.AreEqual(new Rune('2'), sliderIn.Options[1].LegendAbbr);
            ClassicAssert.AreEqual("Fun2", sliderIn.Options[1].Data);
        }
    }
}
