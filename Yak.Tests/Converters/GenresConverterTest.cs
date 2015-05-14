using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Yak.Converters;

namespace Yak.Tests.Converters
{
    [TestFixture]
    public class GenresConverterTest
    {
        [Test]
        public void GenresConvertTest()
        {
            var converter = new GenresConverter();
            var value = new Fixture().Create<List<string>>();
            var converted = converter.Convert(value, null, null, CultureInfo.CurrentUICulture);

            Assert.That(converted, Is.Not.Null);

            foreach (var item in value)
            {
                Assert.That(converted,Is.StringContaining(item));
            }
        }
    }
}
