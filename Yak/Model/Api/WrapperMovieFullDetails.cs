using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using Yak.Model.Movie;

namespace Yak.Model.Api
{
    public class WrapperMovieFullDetails : ObservableObject
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_message")]
        public string StatusMessage { get; set; }

        [JsonProperty("data")]
        public MovieFullDetails Movie { get; set; }
    }
}
