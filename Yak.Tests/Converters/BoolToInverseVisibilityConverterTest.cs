using System.Globalization;
using System.Windows;
using NUnit.Framework;
using Yak.Converters;

namespace Yak.Tests.Converters
{
    [TestFixture]
    public class BoolToInverseVisibilityConverterTest
    {
        [Test]
        public void BoolToInverseConvertTest()
        {
            var converter = new BoolToInverseVisibilityConverter();
            Assert.That(converter.Convert(true, typeof(Visibility), null, CultureInfo.CurrentUICulture).Equals(Visibility.Visible));
            Assert.That(converter.Convert(false, typeof(Visibility), null, CultureInfo.CurrentUICulture).Equals(Visibility.Collapsed));
        }
    }
}
