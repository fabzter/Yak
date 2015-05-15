using System;

namespace Yak.Events
{
    /// <summary>
    /// ConnectionErrorEventArgs
    /// </summary>
    public class NumberOfLoadedMoviesEventArgs : EventArgs
    {
        #region Constructor
        public NumberOfLoadedMoviesEventArgs(int numberOfMovies, bool isUnhandledException)
        {
            NumberOfMovies = numberOfMovies;
            IsUnhandledException = isUnhandledException;
        }
        #endregion

        #region Properties

        #region Property -> NumberOfMovies
        public int NumberOfMovies { get; private set; }
        #endregion

        #region Property -> isUnhandledException
        public bool IsUnhandledException { get; private set; }
        #endregion

        #endregion
    }
}
