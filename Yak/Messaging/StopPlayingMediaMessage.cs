using System;
using GalaSoft.MvvmLight.Messaging;
using Yak.Helpers;

namespace Yak.Messaging
{
    /// <summary>
    /// StopPlayingMediaMessage
    /// </summary>
    public class StopPlayingMediaMessage : MessageBase
    {
        #region Property -> DeleteMovieFile
        /// <summary>
        /// Delete movie file
        /// </summary>
        public Action<bool> DeleteMovieFile
        {
            get;
            private set;
        }
        #endregion

        #region Property -> MediaType
        /// <summary>
        /// Media type
        /// </summary>
        public Constants.MediaType MediaType
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// StopPlayingMediaMessage
        /// </summary>
        /// <param name="mediaType">Type of the media to stop playing</param>
        /// <param name="deleteMovieFile">Action used to delete movie file</param>
        public StopPlayingMediaMessage(Constants.MediaType mediaType, Action<bool> deleteMovieFile)
        {
            MediaType = mediaType;
            DeleteMovieFile = deleteMovieFile;
        }
        #endregion
    }
}
