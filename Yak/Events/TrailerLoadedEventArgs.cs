using System;

namespace Yak.Events
{
    /// <summary>
    /// TrailerLoadedEventArgs
    /// </summary>
    public class TrailerLoadedEventArgs : EventArgs
    {
        private readonly string _trailerUrl;
        private readonly bool _inErrorOrCancelled;

        #region Constructor
        public TrailerLoadedEventArgs(string trailerUrl, bool inErrorOrCancelled)
        {
            _trailerUrl = trailerUrl;
            _inErrorOrCancelled = inErrorOrCancelled;
        }
        #endregion

        #region Properties

        #region Property -> TrailerUrl
        public string TrailerUrl
        {
            get
            {
                return _trailerUrl;
            }
        }
        #endregion

        #region Property -> InErrorOrCancelled
        public bool InErrorOrCancelled
        {
            get
            {
                return _inErrorOrCancelled;
            }
        }
        #endregion

        #endregion
    }
}
