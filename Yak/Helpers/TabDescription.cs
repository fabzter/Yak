using System;

namespace Yak.Helpers
{
    /// <summary>
    /// Used to describe a movie tab
    /// </summary>
    public class TabDescription
    {
        public string ApiSort { get; set; }

        public string TabName { get; set; }

        public TabType Type { get; set; }

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
                if (!String.IsNullOrEmpty(movieName))
                {
                    TabName = movieName;
                }
                else
                {
                    TabName = "Playing";
                }

                ApiSort = String.Empty;
            }
            else if (tabType == TabType.Search)
            {
                TabName = "Search";
                ApiSort = String.Empty;
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
