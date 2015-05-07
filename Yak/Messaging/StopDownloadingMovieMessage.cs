using System;
using GalaSoft.MvvmLight.Messaging;

namespace Yak.Messaging
{
    /// <summary>
    /// StopDownloadingMovieMessage
    /// </summary>
    public class StopDownloadingMovieMessage : MessageBase
    {
        #region Property -> DeleteMovieFilesAction
        /// <summary>
        /// Delete movie files
        /// </summary>
        public Action<bool> DeleteMovieFilesAction
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// StopDownloadingMovieMessage
        /// </summary>
        /// <param name="deleteMovieFilesAction">Action used to delete movies</param>
        public StopDownloadingMovieMessage(Action<bool> deleteMovieFilesAction)
        {
            DeleteMovieFilesAction = deleteMovieFilesAction;
        }
        #endregion
    }
}
