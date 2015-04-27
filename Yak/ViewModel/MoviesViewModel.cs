﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Yak.Model.Api;
using Yak.Model.Movie;
using GalaSoft.MvvmLight.Command;
using Yak.Events;
using Yak.Comparers;
using Yak.Helpers;

namespace Yak.ViewModel
{
    /// <summary>
    /// ViewModel which takes care of movies' list (searching, retrieving from API and pagination)
    /// </summary>
    public class MoviesViewModel : ViewModelBase
    {
        #region Properties

        #region Property -> ApiService
        /// <summary>
        /// Service used to consume the API
        /// </summary>
        private IService ApiService { get; set; }
        #endregion

        #region Property -> Movies
        /// <summary>
        /// Movies loaded from the service and shown in the interface
        /// </summary>
        private ObservableCollection<MovieShortDetails> _movies = new ObservableCollection<MovieShortDetails>();
        public ObservableCollection<MovieShortDetails> Movies
        {
            get { return _movies; }
            set { Set(() => Movies, ref _movies, value, true); }
        }
        #endregion

        #region Property -> Pagination
        /// <summary>
        /// Current page number of loaded movies
        /// </summary>
        private int Pagination { get; set; }
        #endregion

        #region Property -> CancellationLoadMoviesInfosToken
        /// <summary>
        /// Token to cancel movie loading
        /// </summary>
        private CancellationTokenSource CancellationLoadingToken { get; set; }
        #endregion

        #region Property -> MaxMoviesPerPage
        /// <summary>
        /// Maximum movies number to load per page request
        /// </summary>
        public int MaxMoviesPerPage { private get; set; }
        #endregion

        #region Property -> SearchMessageToken
        /// <summary>
        /// Token for message subscription when searching movies
        /// </summary>
        private Guid SearchMessageToken = Guid.NewGuid();
        #endregion

        #region Property -> SearchMoviesFilter
        /// <summary>
        /// The filter for searching movies
        /// </summary>
        private string _searchMoviesFilter = String.Empty;
        public string SearchMoviesFilter
        {
            get { return _searchMoviesFilter; }
            set
            {
                if (value != _searchMoviesFilter)
                {
                    string oldValue = _searchMoviesFilter;
                    _searchMoviesFilter = value;
                    Messenger.Default.Send(new PropertyChangedMessage<string>(oldValue, value, "SearchMoviesFilter"), SearchMessageToken);
                }
            }
        }
        #endregion

        #region Property -> TabName
        /// <summary>
        /// Name of the tab
        /// </summary>
        public string TabName { get; set; }
        #endregion

        #region Property -> IsConnectionInError
        private bool _isConnectionInError;
        /// <summary>
        /// Specify if a connection error has occured
        /// </summary>
        public bool IsConnectionInError
        {
            get { return _isConnectionInError; }
            set { Set(() => IsConnectionInError, ref _isConnectionInError, value, true); }
        }

        #endregion

        #region Property -> MovieLoadingState
        private MovieLoadingState _movieLoadingState;
        /// <summary>
        /// Specify he state of loading
        /// </summary>
        public MovieLoadingState MovieLoadingState
        {
            get { return _movieLoadingState; }
            set { Set(() => MovieLoadingState, ref _movieLoadingState, value, true); }
        }

        #endregion

        #region Property -> IsNoMovieFound
        private bool _isNoMovieFound;
        /// <summary>
        /// Specify if no movie was found
        /// </summary>
        public bool IsNoMovieFound
        {
            get { return _isNoMovieFound; }
            set { Set(() => IsNoMovieFound, ref _isNoMovieFound, value, true); }
        }

        #endregion

        #endregion

        #region Commands

        #region Command -> ReloadMovies
        /// <summary>
        /// Reload movies 
        /// </summary>
        public RelayCommand ReloadMovies
        {
            get;
            private set;
        }
        #endregion

        #endregion

        #region Constructors

        #region Constructor -> MoviesViewModel
        /// <summary>
        /// Initializes a new instance of the MoviesViewModel class.
        /// </summary>
        public MoviesViewModel()
            : this(new Service())
        {
        }
        #endregion

        #region Constructor -> MoviesViewModel
        /// <summary>
        /// Initializes a new instance of the MoviesViewModel class.
        /// </summary>
        /// <param name="apiService">apiService</param>
        private MoviesViewModel(IService apiService)
        {
            ApiService = apiService;

            // Set the CancellationToken for having the possibility to stop a task
            CancellationLoadingToken = new CancellationTokenSource();

            MaxMoviesPerPage = Constants.MaxMoviesPerPage;

            ReloadMovies = new RelayCommand(async () =>
            {
                Messenger.Default.Send<bool>(false, Constants.ConnectionErrorPropertyName);
                await LoadNextPage();
            });

            Messenger.Default.Register<PropertyChangedMessage<string>>(
                this, SearchMessageToken, async e => await SearchMovies(e.NewValue)
            );

            Messenger.Default.Register<bool>(this, Constants.ConnectionErrorPropertyName, e => IsConnectionInError = e);
        }
        #endregion

        #endregion

        #region Methods

        #region Method -> SearchMovies
        /// <summary>
        /// Search movies
        /// </summary>
        /// <param name="searchFilter">The parameter of the search</param>
        private async Task SearchMovies(string searchFilter)
        {
            // We stop any loading before searching 
            StopLoadingMovies();

            // We start from scratch : clean everything to not interfer with the results
            Movies.Clear();
            Pagination = 0;

            if (!String.IsNullOrEmpty(searchFilter))
            {
                // Retrieve page with the search filter
                await LoadNextPage(searchFilter);
            }
            else
            {
                // No filter: load the first page without criteria
                await LoadNextPage();
            }
        }
        #endregion

        #region Method -> LoadNextPage

        /// <summary>
        /// Load next page with an optional search parameter
        /// </summary>
        /// <param name="searchFilter">An optional search parameter which is specified to the API</param>
        public async Task LoadNextPage(string searchFilter = null)
        {
            // Set the CancellationToken for having the possibility to stop a task
            CancellationLoadingToken = new CancellationTokenSource();

            // We update the current pagination
            Pagination++;
            MovieLoadingState = new MovieLoadingState(true, 0, false);

            int moviesCount = 0;

            // The page to load is new, never met it before, so we load the new page via the service
            Tuple<IEnumerable<MovieShortDetails>, IEnumerable<Exception>> results =
                await ApiService.GetMoviesAsync(TabName,
                    searchFilter,
                    MaxMoviesPerPage,
                    Pagination,
                    CancellationLoadingToken);

            if (String.IsNullOrEmpty(searchFilter))
            {
                moviesCount = results.Item1 != null ? results.Item1.Count() : 0;
            }
            else
            {
                moviesCount = results.Item1 != null
                    ? results.Item1.Count(
                        movie => movie.Title.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                    : 0;
            }

            // Check if we met any exception in the GetMoviesInfosAsync method
            if (HandleExceptions(results.Item2))
            {
                // Inform the subscribers we loaded movies
                MovieLoadingState = new MovieLoadingState(false, moviesCount, true);
                return;
            }

            if (results.Item1 != null)
            {
                // Now we download the cover image for each movie
                foreach (var movie in results.Item1.Except(Movies, new MovieComparer()))
                {
                    // The API filters on titles, actor's name and director's name. Here we just want to filter on title movie.
                    if (String.IsNullOrEmpty(searchFilter) ||
                        (!String.IsNullOrEmpty(searchFilter) &&
                         movie.Title.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        // Download the cover image of the movie
                        Tuple<string, IEnumerable<Exception>> movieCover =
                            await ApiService.DownloadMovieCoverAsync(movie.ImdbCode,
                                movie.MediumCoverImage,
                                CancellationLoadingToken);

                        // Check if we met any exception
                        if (HandleExceptions(movieCover.Item2))
                        {
                            // Inform the subscribers we loaded movies
                            MovieLoadingState = new MovieLoadingState(false, moviesCount, true);
                            return;
                        }

                        movie.MediumCoverImageUri = movieCover.Item1;

                        Movies.Add(movie);
                    }
                }
            }

            // Inform the subscribers we loaded movies
            MovieLoadingState = new MovieLoadingState(false, moviesCount, false);
        }

        #endregion

        #region Method -> HandleExceptions
        /// <summary>
        /// Handle list of exceptions
        /// </summary>
        /// <param name="exceptions">List of exceptions</param>
        private bool HandleExceptions(IEnumerable<Exception> exceptions)
        {
            foreach (var e in exceptions)
            {
                var taskCancelledException = e as TaskCanceledException;
                if (taskCancelledException != null)
                {
                    // Something as cancelled the loading. We go back.
                    Pagination--;
                    return true;
                }

                var webException = e as WebException;
                if (webException != null)
                {
                    if (webException.Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        // There's a connection error.
                        Messenger.Default.Send<bool>(true, Helpers.Constants.ConnectionErrorPropertyName);
                        Pagination--;
                        return true;
                    }
                }

                // Another exception has occured. Go back.
                Pagination--;
                return true;
            }
            return false;
        }
        #endregion

        #region Method -> StopLoadingMovies
        /// <summary>
        /// Cancel the loading of movies 
        /// </summary>
        private void StopLoadingMovies()
        {
            if (CancellationLoadingToken != null)
            {
                CancellationLoadingToken.Cancel(false);
            }
        }
        #endregion

        #endregion

        public override void Cleanup()
        {
            Messenger.Default.Unregister<Tuple<int, string>>(this);
            Messenger.Default.Unregister<PropertyChangedMessage<string>>(this);
            base.Cleanup();
        }
    }
}
