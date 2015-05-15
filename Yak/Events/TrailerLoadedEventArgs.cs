using System;

namespace Yak.Events
{
    /// <summary>
    /// TrailerLoadedEventArgs
    /// </summary>
    public class TrailerLoadedEventArgs : EventArgs
    {
        #region Constructor
        public TrailerLoadedEventArgs(string trailerUrl, bool inErrorOrCancelled)
        {
            TrailerUrl = trailerUrl;
            InErrorOrCancelled = inErrorOrCancelled;
        }
        #endregion

        #region Properties

        #region Property -> TrailerUrl
        public string TrailerUrl { get; private set; }
        #endregion

        #region Property -> InErrorOrCancelled
        public bool InErrorOrCancelled { get; private set; }
        #endregion

        #endregion
    }
}
