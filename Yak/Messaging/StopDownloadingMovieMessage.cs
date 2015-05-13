using System;
using GalaSoft.MvvmLight.Messaging;

namespace Yak.Messaging
{
    /// <summary>
    /// StopDownloadingMovieMessage
    /// </summary>
    public class StopDownloadingMovieMessage : MessageBase
    {
        #region Property -> DeleteMovieFileWhenCancelledDownload
        /// <summary>
        /// Delete movie files
        /// </summary>
        public Action<bool> DeleteMovieFileWhenCancelledDownload
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// StopDownloadingMovieMessage
        /// </summary>
        /// <param name="deleteMovieFileWhenCancelledDownload">Action used to delete movies</param>
        public StopDownloadingMovieMessage(Action<bool> deleteMovieFileWhenCancelledDownload)
        {
            DeleteMovieFileWhenCancelledDownload = deleteMovieFileWhenCancelledDownload;
        }
        #endregion
    }
}
