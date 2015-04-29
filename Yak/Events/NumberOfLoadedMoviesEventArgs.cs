using System;

namespace Yak.Events
{
    /// <summary>
    /// ConnectionErrorEventArgs
    /// </summary>
    public class NumberOfLoadedMoviesEventArgs : EventArgs
    {
        private readonly int _numberOfMovies;
        private readonly bool _isUnhandledException;

        #region Constructor
        public NumberOfLoadedMoviesEventArgs(int numberOfMovies, bool isUnhandledException)
        {
            _numberOfMovies = numberOfMovies;
            _isUnhandledException = isUnhandledException;
        }
        #endregion

        #region Properties

        #region Property -> NumberOfMovies
        public int NumberOfMovies
        {
            get
            {
                return _numberOfMovies;
            }
        }
        #endregion

        #region Property -> isUnhandledException
        public bool IsUnhandledException
        {
            get
            {
                return _isUnhandledException;
            }
        }
        #endregion

        #endregion
    }
}
