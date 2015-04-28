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
        private readonly string _movieFilePath;
        private readonly MovieFullDetails _movie;

        #region Constructor
        public MovieBufferedMessage(MovieFullDetails movie, string movieFilePath)
        {
            _movieFilePath = movieFilePath;
            _movie = movie;
        }
        #endregion

        #region Properties

        #region Property -> movieFilePath
        public string MovieFilePath
        {
            get
            {
                return _movieFilePath;
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
