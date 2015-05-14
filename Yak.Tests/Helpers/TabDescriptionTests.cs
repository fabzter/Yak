using NUnit.Framework;
using Ploeh.AutoFixture;
using Yak.Helpers;

namespace Yak.Tests.Helpers
{
    [TestFixture]
    public class TabDescriptionTests
    {
        [Test]
        public void GreatestTabDescriptionTest()
        {
            var tab = new TabDescription(TabDescription.TabType.Greatest);

            Assert.That(tab,Is.Not.Null);
            Assert.That(tab.ApiSort,Is.EqualTo("rating"));
            Assert.That(tab.TabName, Is.EqualTo("Greatest"));
        }

        [Test]
        public void PopularTabDescriptionTest()
        {
            var tab = new TabDescription(TabDescription.TabType.Popular);

            Assert.That(tab, Is.Not.Null);
            Assert.That(tab.ApiSort, Is.EqualTo("like_count"));
            Assert.That(tab.TabName, Is.EqualTo("Popular"));
        }

        [Test]
        public void RecentTabDescriptionTest()
        {
            var tab = new TabDescription(TabDescription.TabType.Recent);

            Assert.That(tab, Is.Not.Null);
            Assert.That(tab.ApiSort, Is.EqualTo("year"));
            Assert.That(tab.TabName, Is.EqualTo("Recent"));
        }

        [Test]
        public void SearchTabDescriptionTest()
        {
            var tab = new TabDescription(TabDescription.TabType.Search);

            Assert.That(tab, Is.Not.Null);
            Assert.That(tab.ApiSort, Is.EqualTo(string.Empty));
            Assert.That(tab.TabName, Is.EqualTo("Search"));
        }


        [Test]
        public void PlayingNoMovieTabDescriptionTest()
        {
            var tab = new TabDescription(TabDescription.TabType.Playing);

            Assert.That(tab, Is.Not.Null);
            Assert.That(tab.ApiSort, Is.EqualTo(string.Empty));
            Assert.That(tab.TabName, Is.EqualTo("Playing"));
        }

        [Test]
        public void PlayingWithMovieTabDescriptionTest()
        {
            var movie = new Fixture().Create<string>();
            var tab = new TabDescription(TabDescription.TabType.Playing, movie);

            Assert.That(tab, Is.Not.Null);
            Assert.That(tab.ApiSort, Is.EqualTo(string.Empty));
            Assert.That(tab.TabName, Is.EqualTo(movie));
        }
    }
}
