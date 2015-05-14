using System;
using System.Globalization;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Yak.Converters;


namespace Yak.Tests.Converters
{
    [TestFixture]
    [Timeout(200)]
    public class StringRatingToIntRatingConverterTest
    {
        [Test]
        public void ConvertInvalidString()
        {
           var converter = new StringRatingToIntRatingConverter();
           var value = new Fixture().Create<string>();

            var result = converter.Convert(value, null, null, null);

            Assert.That(result,Is.EqualTo(0));
        }

        [Test]
        public void ConvertValidString()
        {
            var converter = new StringRatingToIntRatingConverter();
            var value = "100";

            var result = converter.Convert(value, null, null, null);

            Assert.That(result, Is.EqualTo((double)50));
            Assert.That(result, Is.TypeOf<double>());
        }
    }
}
