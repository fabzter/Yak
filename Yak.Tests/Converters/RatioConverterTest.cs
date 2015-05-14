using System.Globalization;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Yak.Converters;

namespace Yak.Tests.Converters
{
    [TestFixture]
    public class RatioConverterTest
    {
        [Test]
        public void RatioConvertTest()
        {
            var fixture = new Fixture();
            var converter = new RatioConverter();
            var value = fixture.Create<double>();
            var parameter = fixture.Create<double>();

            var result = converter.Convert(value, null, parameter, CultureInfo.CurrentCulture);

            Assert.That(result, Is.EqualTo(value * parameter));
            Assert.That(result, Is.TypeOf<double>());
        }
    }
}
