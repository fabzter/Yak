using System;
using System.Windows;

namespace Yak.Events
{
    /// <summary>
    /// MovieLoadingState
    /// </summary>
    public class MovieLoadingState
    {
        private bool _isLoading;
        private int _numberOfLoadedMovies;
        private bool _isExceptionThrown;

        private bool _isNoMovieFound;
        private bool _isProgressRingActive;
        private bool _isFadeRequired;

        #region Constructor
        public MovieLoadingState(bool isLoading, int numberOfLoadedMovies, bool isExceptionThrown)
        {
            IsLoading = isLoading;
            NumberOfLoadedMovies = numberOfLoadedMovies;
            IsExceptionThrown = isExceptionThrown;

            if (IsLoading)
            {
                IsProgressRingActive = true;
                IsFadeRequired = true;

                if (IsNoMovieFound)
                {
                    IsNoMovieFound = false;
                }
            }
            else
            {
                if ((NumberOfLoadedMovies == 0 && !IsExceptionThrown) || (NumberOfLoadedMovies != 0 && !IsExceptionThrown))
                {
                    IsProgressRingActive = false;
                    IsFadeRequired = false;
                }

                if (NumberOfLoadedMovies == 0 && !IsExceptionThrown) // NE PAS OUBLIER DANS LE MULTIBINDING LA CONDITION : !vm.Movies.Any()
                {
                    IsNoMovieFound = true;
                }
            }
        }
        #endregion

        #region Properties

        #region Property -> IsLoading
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                if (value != _isLoading)
                {
                    _isLoading = value;
                }
            }
        }
        #endregion

        #region Property -> NumberOfLoadedMovies
        public int NumberOfLoadedMovies
        {
            get
            {
                return _numberOfLoadedMovies;
            }
            set
            {
                if (value != _numberOfLoadedMovies)
                {
                    _numberOfLoadedMovies = value;
                }
            }
        }
        #endregion

        #region Property -> IsExceptionThrown
        public bool IsExceptionThrown
        {
            get
            {
                return _isExceptionThrown;
            }
            set
            {
                if (value != _isExceptionThrown)
                {
                    _isExceptionThrown = value;
                }
            }
        }
        #endregion

        #region Property -> IsProgressRingActive
        public bool IsProgressRingActive
        {
            get
            {
                return _isProgressRingActive;
            }
            set
            {
                if (value != _isProgressRingActive)
                {
                    _isProgressRingActive = value;
                }
            }
        }
        #endregion

        #region Property -> IsFadeRequired
        public bool IsFadeRequired
        {
            get
            {
                return _isFadeRequired;
            }
            set
            {
                if (value != _isFadeRequired)
                {
                    _isFadeRequired = value;
                }
            }
        }
        #endregion

        #region Property -> IsNoMovieFound
        public bool IsNoMovieFound
        {
            get
            {
                return _isNoMovieFound;
            }
            set
            {
                if (value != _isNoMovieFound)
                {
                    _isNoMovieFound = value;
                }
            }
        }
        #endregion

        #endregion
    }
}
