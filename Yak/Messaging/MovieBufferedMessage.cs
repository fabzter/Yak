using System;
using GalaSoft.MvvmLight.Messaging;
using Yak.Model.Movie;

namespace Yak.Messaging
{
    /// <summary>
    /// MovieBufferedMessage
    /// </summary>
    public class MovieBufferedMessage : MessageBase
    {
        #region Constructor
        public MovieBufferedMessage(MovieFullDetails movie, Uri movieUri)
        {
            MovieUri = movieUri;
            Movie = movie;
        }
        #endregion

        #region Properties

        #region Property -> movieUri
        public Uri MovieUri { get; private set; }
        #endregion

        #region Property -> Movie
        public MovieFullDetails Movie { get; private set; }
        #endregion

        #endregion
    }
}
