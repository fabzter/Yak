namespace Yak.Helpers
{
    /// <summary>
    /// Used to describe a movie tab
    /// </summary>
    public class TabDescription
    {
        public string ApiSort { get; private set; }

        public string TabName { get; private set; }

        public TabType Type { get; private set; }

        public TabDescription(TabType tabType, string movieName = null)
        {
            Type = tabType;

            if (tabType == TabType.Popular)
            {
                TabName = "Popular";
                ApiSort = "like_count";
            }
            else if (tabType == TabType.Greatest)
            {
                TabName = "Greatest";
                ApiSort = "rating";
            }
            else if (tabType == TabType.Recent)
            {
                TabName = "Recent";
                ApiSort = "year";
            }
            else if (tabType == TabType.Playing)
            {
                if (!string.IsNullOrEmpty(movieName))
                {
                    TabName = movieName;
                }
                else
                {
                    TabName = "Playing";
                }

                ApiSort = string.Empty;
            }
            else if (tabType == TabType.Search)
            {
                TabName = "Search";
                ApiSort = string.Empty;
            }
        }

        public enum TabType
        {
            Popular = 0,
            Greatest = 1,
            Recent = 2,
            Playing = 3,
            Search = 4
        }
    }
}
