using System;
using GalaSoft.MvvmLight.Messaging;
using Yak.Model.Movie;
using Yak.ViewModel;

namespace Yak.Messaging
{
    /// <summary>
    /// MovieBufferedMessage
    /// </summary>
    public class MovieBufferedMessage : MessageBase
    {
        private readonly Uri _movieUri;
        private readonly MovieFullDetails _movie;

        #region Constructor
        public MovieBufferedMessage(MovieFullDetails movie, Uri movieUri)
        {
            _movieUri = movieUri;
            _movie = movie;
        }
        #endregion

        #region Properties

        #region Property -> movieUri
        public Uri MovieUri
        {
            get
            {
                return _movieUri;
            }
        }
        #endregion

        #region Property -> Movie
        public MovieFullDetails Movie
        {
            get
            {
                return _movie;
            }
        }
        #endregion

        #endregion
    }
}
