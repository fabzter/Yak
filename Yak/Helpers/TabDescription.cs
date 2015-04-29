using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yak.Helpers
{
    /// <summary>
    /// Used to describe a movie tab
    /// </summary>
    public class TabDescription
    {
        public string ApiSort { get; set; }

        public string TabName { get; set; }

        public TabDescription(TabType tabType)
        {
            if (tabType == TabType.Popular)
            {
                TabName = "popular";
                ApiSort = "like_count";
            }
            else if (tabType == TabType.BestRated)
            {
                TabName = "best rated";
                ApiSort = "rating";
            }
            else if (tabType == TabType.Recent)
            {
                TabName = "recent";
                ApiSort = "year";
            }
            else if (tabType == TabType.Playing)
            {
                TabName = "playing";
                ApiSort = String.Empty;
            }
            else if (tabType == TabType.Search)
            {
                TabName = "search";
                ApiSort = String.Empty;
            }
        }

        public enum TabType
        {
            Popular = 0,
            BestRated = 1,
            Recent = 2,
            Playing = 3,
            Search = 4
        }
    }
}
