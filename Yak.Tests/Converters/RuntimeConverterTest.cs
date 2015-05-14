using System;
using System.Globalization;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Yak.Converters;


namespace Yak.Tests.Converters
{
    [TestFixture]
    [Timeout(100)]
    public class RuntimeConverterTest
    {
        [Test]
        public void RuntimeTwoHourTenTest()
        {
            var converter = new RuntimeConverter();
            var value = 130;


            var result = converter.Convert(value, null, null, CultureInfo.CurrentCulture);

            Assert.That(result,Is.EqualTo("2h10"));
        }

        [Test]
        public void RuntimeMaxIntTest()
        {
            var converter = new RuntimeConverter();
            var value = Int32.MaxValue;


            var result = converter.Convert(value, null, null, CultureInfo.CurrentCulture);

            Assert.That(result, Is.EqualTo("35791394h07"));
        }
    }
}
