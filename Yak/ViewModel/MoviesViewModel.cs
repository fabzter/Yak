using System;
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

        #region Property -> Tab
        /// <summary>
        /// Description of the tab
        /// </summary>
        public TabDescription Tab { get; set; }
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

        #region Property -> SearchMoviesFilter
        /// <summary>
        /// The filter for searching movies
        /// </summary>
        public string SearchMoviesFilter { get; set; }
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

            // Inform subscribers a connection error has occured
            Messenger.Default.Register<bool>(this, Constants.ConnectionErrorPropertyName, e => IsConnectionInError = e);

            // Set the CancellationToken for having the possibility to stop loading movies
            CancellationLoadingToken = new CancellationTokenSource();

            MaxMoviesPerPage = Constants.MaxMoviesPerPage;

            // Reload movies
            ReloadMovies = new RelayCommand(async () =>
            {
                await LoadNextPage();
            });
        }
        #endregion

        #endregion

        #region Methods

        #region Method -> SearchMovies
        /// <summary>
        /// Search movies
        /// </summary>
        /// <param name="searchFilter">The parameter of the search</param>
        public async Task SearchMovies(string searchFilter)
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
            // Set the CancellationToken for having the possibility to stop the loading task
            CancellationLoadingToken = new CancellationTokenSource();

            // We update the current pagination
            Pagination++;

            // Inform the subscribers we're actually loading movies
            OnMoviesLoading(new EventArgs());

            int moviesCount = 0;

            // The page to load is new, never met it before, so we load the new page via the service
            Tuple<IEnumerable<MovieShortDetails>, IEnumerable<Exception>> movies =
                await ApiService.GetMoviesAsync(Tab,
                    searchFilter,
                    MaxMoviesPerPage,
                    Pagination,
                    CancellationLoadingToken);

            if (String.IsNullOrEmpty(searchFilter))
            {
                moviesCount = movies.Item1 != null ? movies.Item1.Count() : 0;
            }
            else
            {
                /* 
                 * There's a search criteria : at the moment, there's no searching option into the interface (title, actor name...), so we 
                 * manually filter results on the title by default
                 */
                moviesCount = movies.Item1 != null
                    ? movies.Item1.Count(
                        movie => movie.Title.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                    : 0;
            }

            // Check if we met any exception in the GetMoviesInfosAsync method
            if (HandleExceptions(movies.Item2).Item1)
            {
                // Inform the subscribers we stopped loading this page and decrement pagination (we stopped loading this page's movies)
                OnMoviesLoaded(new NumberOfLoadedMoviesEventArgs(moviesCount, HandleExceptions(movies.Item2).Item2));
                Pagination--;
                return;
            }

            if (movies.Item1 != null)
            {
                // Now we download the cover image for each movie
                foreach (var movie in movies.Item1.Except(Movies, new MovieComparer()))
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

                        // Check if we met any exception in the GetMoviesInfosAsync method
                        if (HandleExceptions(movieCover.Item2).Item1)
                        {
                            // Inform the subscribers we stopped loading this page and decrement pagination (we stopped loading this page's movies)
                            OnMoviesLoaded(new NumberOfLoadedMoviesEventArgs(moviesCount, HandleExceptions(movieCover.Item2).Item2));
                            Pagination--;
                            return;
                        }

                        if (!CancellationLoadingToken.IsCancellationRequested)
                        {
                            // We have the movie only if no cancellation has been requested
                            movie.MediumCoverImageUri = movieCover.Item1;
                            Movies.Add(movie);
                        }
                    }
                }
            }

            // Inform the subscribers we loaded movies
            OnMoviesLoaded(new NumberOfLoadedMoviesEventArgs(moviesCount, false));
        }

        #endregion

        #region Method -> HandleExceptions
        /// <summary>
        /// Handle list of exceptions
        /// </summary>
        /// <param name="exceptions">List of exceptions</param>
        /// <returns>
        /// Returns a tuple which represents if exception(s) has been thrown, and if one of them is the result of an unhandled exception
        /// </returns>
        private Tuple<bool, bool> HandleExceptions(IEnumerable<Exception> exceptions)
        {
            bool isExceptionThrown = false;
            bool isUnhandledException = false;
            bool isConnexionInError = false;
            foreach (var e in exceptions)
            {
                isExceptionThrown = true;

                var webException = e as WebException;
                if (webException != null)
                {
                    if (webException.Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        // There's a connection error.
                        isConnexionInError = true;
                    }
                }
                else
                {
                    isUnhandledException = true;
                }
            }

            if (isConnexionInError)
            {
                Messenger.Default.Send<bool>(true, Constants.ConnectionErrorPropertyName);
            }

            return new Tuple<bool, bool>(isExceptionThrown, isUnhandledException);
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
                CancellationLoadingToken.Cancel(true);
            }
        }
        #endregion

        #endregion

        #region Events

        #region Event -> MoviesLoading
        /// <summary>
        /// MoviesLoading event
        /// </summary>
        public event EventHandler<EventArgs> MoviesLoading;
        /// <summary>
        /// Fire event when movies are loading
        /// </summary>
        ///<param name="e">Event data</param>
        protected virtual void OnMoviesLoading(EventArgs e)
        {
            EventHandler<EventArgs> handler = MoviesLoading;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> MoviesLoaded
        /// <summary>
        /// MoviesLoaded event
        /// </summary>
        public event EventHandler<NumberOfLoadedMoviesEventArgs> MoviesLoaded;
        /// <summary>
        /// Fire event when movies has finished loading
        /// </summary>
        ///<param name="e">Number of loaded movies</param>
        protected virtual void OnMoviesLoaded(NumberOfLoadedMoviesEventArgs e)
        {
            EventHandler<NumberOfLoadedMoviesEventArgs> handler = MoviesLoaded;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #endregion

        public override void Cleanup()
        {
            Messenger.Default.Unregister<Tuple<int, string>>(this);
            base.Cleanup();
        }
    }
}
