using System;

namespace Yak.Events
{
    /// <summary>
    /// MovieLoadingProgressEventArgs
    /// </summary>
    public class MovieLoadingProgressEventArgs : EventArgs
    {
        #region Constructor
        public MovieLoadingProgressEventArgs(double progress, int downloadRate)
        {
            Progress = progress;
            DownloadRate = downloadRate;
        }
        #endregion

        #region Properties

        #region Property -> Progress
        public double Progress { get; private set; }
        #endregion

        #region Property -> DownloadRate
        public int DownloadRate { get; private set; }
        #endregion

        #endregion
    } 
}
