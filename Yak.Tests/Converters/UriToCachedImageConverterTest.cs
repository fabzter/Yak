using System;
using System.Globalization;
using System.Windows.Media.Imaging;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Yak.Converters;


namespace Yak.Tests.Converters
{
    [TestFixture]
    [Timeout(500)]
    public class UriToCachedImageConverterTest
    {
        [Test]
        public void ConvertURi()
        {
           var converter = new UriToCachedImageConverter();
            var value = "http://www.google.com/";

            var result = converter.Convert(value, null, null, null);
            Assert.That(result,Is.TypeOf<BitmapImage>());

            var image = (BitmapImage)result;
            Assert.That(image.UriSource.ToString(),Is.EqualTo(value));
        }
    }
}
