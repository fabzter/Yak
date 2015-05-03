using System;

namespace Yak.Events
{
    /// <summary>
    /// MovieBufferedEventArgs
    /// </summary>
    public class MovieBufferedEventArgs : EventArgs
    {
        private readonly Uri _movieUri;

        #region Constructor
        public MovieBufferedEventArgs(Uri movieUri)
        {
            _movieUri = movieUri;
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

        #endregion
    }
}
