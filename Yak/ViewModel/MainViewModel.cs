using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Ragnar;
using Yak.Helpers;
using Yak.Model.Api;
using Yak.Model.Cast;
using Yak.Model.Movie;
using Yak.Events;
using GalaSoft.MvvmLight.Command;
using TMDbLib.Objects.Movies;
using YoutubeExtractor;
using System.Collections.ObjectModel;
using System.Windows;
using Yak.Messaging;

namespace Yak.ViewModel
{
    /// <summary>
    /// MainViewModel
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Properties

        #region Property -> ApiService
        /// <summary>
        /// The service used to consume APIs
        /// </summary>
        private IService ApiService { get; set; }
        #endregion

        #region Property -> Movie
        /// <summary>
        /// The movie to play, retrieved from YTS API
        /// </summary>
        private MovieFullDetails _movie = new MovieFullDetails();
        public MovieFullDetails Movie
        {
            get { return _movie; }
            set { Set(() => Movie, ref _movie, value, true); }
        }
        #endregion

        #region Property -> MoviesViewModelTabs
        /// <summary>
        /// Tabs shown into the interface via TabControl
        /// </summary>
        private ObservableCollection<object> _moviesViewModelTabs;
        /// <summary>
        /// Tabs shown into the interface via TabControl
        /// </summary>
        public ObservableCollection<object> MoviesViewModelTabs
        {
            get { return _moviesViewModelTabs; }
            set { Set(() => MoviesViewModelTabs, ref _moviesViewModelTabs, value, true); }
        }
        #endregion

        #region Property -> SelectedTabViewModel
        /// <summary>
        /// The selected viewmodel tab via TabControl
        /// </summary>
        private object _selectedTabViewModel;
        /// <summary>
        /// The selected viewmodel tab via TabControl
        /// </summary>
        public object SelectedTabViewModel
        {
            get { return _selectedTabViewModel; }
            set { Set(() => SelectedTabViewModel, ref _selectedTabViewModel, value, true); }
        }
        #endregion

        #region Property -> CancellationLoadingToken
        /// <summary>
        /// Token to cancel movie loading
        /// </summary>
        private CancellationTokenSource CancellationLoadingToken { get; set; }
        #endregion

        #region Property -> CancellationDownloadingToken
        /// <summary>
        /// Token to cancel movie downloading
        /// </summary>
        private CancellationTokenSource CancellationDownloadingToken { get; set; }
        #endregion

        #region Property -> IsDownloadingMovie
        /// <summary>
        /// Specify if a movie is downloading
        /// </summary>
        private bool _isDownloadingMovie;
        /// <summary>
        /// Specify if a movie is downloading
        /// </summary>
        public bool IsDownloadingMovie
        {
            get { return _isDownloadingMovie; }
            set { Set(() => IsDownloadingMovie, ref _isDownloadingMovie, value, true); }
        }
        #endregion

        #region Property -> IsConnectionInError
        /// <summary>
        /// Specify if a connection error has occured
        /// </summary>
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

        #region Property -> SearchMessageToken
        /// <summary>
        /// Token for message subscription when searching movies
        /// </summary>
        private readonly Guid _searchMessageToken = Guid.NewGuid();
        #endregion

        #region Property -> SearchMoviesFilter
        /// <summary>
        /// The filter for searching movies
        /// </summary>
        private string _searchMoviesFilter;
        /// <summary>
        /// The filter for searching movies
        /// </summary>
        public string SearchMoviesFilter
        {
            get
            {
                return _searchMoviesFilter;

            }
            set
            {
                if (_searchMoviesFilter != value)
                {
                    string oldValue = _searchMoviesFilter;
                    _searchMoviesFilter = value;
                    Messenger.Default.Send(new PropertyChangedMessage<string>(oldValue, _searchMoviesFilter, "SearchMoviesFilter"), _searchMessageToken);

                    RemoveSearchTabIfEmptySearch();
                }
            }
        }

        #endregion

        /// <summary>
        /// Delete movie files when movie downloading has been cancelled
        /// </summary>
        private Action<bool> DeleteMovieFilesWhenCancelledDownloading;
        #endregion

        #region Property -> StreamingQualityMap
        /// <summary>
        /// Map for defining youtube video quality
        /// </summary>
        private static readonly IReadOnlyDictionary<YoutubeStreamingQuality, IEnumerable<int>> StreamingQualityMap =
    new Dictionary<YoutubeStreamingQuality, IEnumerable<int>>
            {
                { YoutubeStreamingQuality.High, new HashSet<int> { 1080, 720 } },
                { YoutubeStreamingQuality.Medium, new HashSet<int> { 480 } },
                { YoutubeStreamingQuality.Low, new HashSet<int> { 360, 240 } }
            };
        #endregion

        #endregion

        #region Commands

        #region Command -> StopDownloadingMovieCommand
        /// <summary>
        /// StopDownloadingMovieCommand
        /// </summary>
        public RelayCommand StopDownloadingMovieCommand
        {
            get; 
            private set;
        }
        #endregion

        #region Command -> DownloadMovieCommand
        /// <summary>
        /// DownloadMovieCommand
        /// </summary>
        public RelayCommand DownloadMovieCommand
        {
            get;
            private set;
        }
        #endregion

        #region Command -> LoadMovieCommand
        /// <summary>
        /// LoadMovieCommand
        /// </summary>
        public RelayCommand<MovieShortDetails> LoadMovieCommand
        {
            get;
            private set;
        }
        #endregion

        #region Command -> GetTrailerCommand
        /// <summary>
        /// GetTrailerCommand
        /// </summary>
        public RelayCommand GetTrailerCommand
        {
            get;
            private set;
        }
        #endregion

        #region Command -> MainWindowClosingCommand
        /// <summary>
        /// MainWindowClosingCommand
        /// </summary>
        public RelayCommand MainWindowClosingCommand
        {
            get;
            private set;
        }
        #endregion

        #region Command -> SearchMovieCommand
        /// <summary>
        /// SearchMovieCommand
        /// </summary>
        public RelayCommand SearchMovieCommand
        {
            get;
            private set;
        }
        #endregion

        #endregion

        #region Constructors

        #region Constructor -> MainViewModel
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
            : this(new Service())
        {
        }
        #endregion

        #region Constructor -> MainViewModel
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// <param name="apiService">The service which will be used</param>
        private MainViewModel(IService apiService)
        {
            ApiService = apiService;

            #region Messaging
            // Inform subscribers a connection error has occured
            Messenger.Default.Register<bool>(this, Constants.ConnectionErrorPropertyName, arg => OnConnectionError(new ConnectionErrorEventArgs(arg)));

            // Create and open movie tab of the buffered movie
            Messenger.Default.Register<MovieBufferedMessage>(this, e =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Create a tab with the movie name as the title
                    MoviesViewModelTabs.Add(new MoviePlayerViewModel(e.Movie, e.MovieUri)
                    {
                        Tab = new TabDescription(TabDescription.TabType.Playing, e.Movie.Title)
                    });

                    // Select this tab in the tab control
                    SelectedTabViewModel = MoviesViewModelTabs.Last();

                    // Inform subscribers that a movie has been buffered
                    OnBufferedMovie(new MovieBufferedEventArgs(e.MovieUri));
                });
            });

            // Inform subscribers that a movie has stopped downloading
            Messenger.Default.Register<StopDownloadingMovieMessage>(
                this, 
                message => 
                    {
                        DeleteMovieFilesWhenCancelledDownloading(true);
                        OnStoppedDownloadingMovie(new EventArgs());
                    }
                );

            // Stop downloading a movie if any
            Messenger.Default.Register<MainWindowClosingMessage>(this, e =>
            {
                if (IsDownloadingMovie)
                {
                    StopDownloadingMovie();
                }
            });
            #endregion

            // Set the CancellationToken for having the possibility to stop a loading movie infos
            CancellationLoadingToken = new CancellationTokenSource();

            // Set the CancellationToken for having the possibility to stop downloading a movie
            CancellationDownloadingToken = new CancellationTokenSource();

            #region Commands
            // Search movies with the current SearchMoviesFilter as criteria
            SearchMovieCommand = new RelayCommand(async () =>
            {
                await SearchMovies(SearchMoviesFilter);
            });

            // Stop downloading a movie if any
            StopDownloadingMovieCommand = new RelayCommand(() =>
            {
                StopDownloadingMovie();
            });

            // Download the current movie
            DownloadMovieCommand = new RelayCommand(async () =>
            {
                if (Movie != null && !IsDownloadingMovie)
                {
                    CancellationDownloadingToken = new CancellationTokenSource();
                    await DownloadMovie(Movie);
                }
            });

            // The app is about to close
            MainWindowClosingCommand = new RelayCommand(() =>
            {
                Messenger.Default.Send<MainWindowClosingMessage>(new MainWindowClosingMessage());
            });

            // Load requested movie
            LoadMovieCommand = new RelayCommand<MovieShortDetails>(async movie =>
            {
                await LoadMovie(movie.Id, movie.ImdbCode);
            });

            // Load requested movie trailer
            GetTrailerCommand = new RelayCommand(async () =>
            {
                await GetTrailer(Movie.ImdbCode);
            });
            #endregion

            // Creates the different tabs in the tab control
            MoviesViewModelTabs = new ObservableCollection<object>
            {
                new MoviesViewModel
                { 
                    Tab = new TabDescription(TabDescription.TabType.Popular),
                },
                new MoviesViewModel
                { 
                    Tab = new TabDescription(TabDescription.TabType.Greatest),
                },
                new MoviesViewModel
                { 
                    Tab = new TabDescription(TabDescription.TabType.Recent),
                }
            };

            // Select the Popular tab at the beginning
            SelectedTabViewModel = MoviesViewModelTabs.FirstOrDefault();
        }
        #endregion

        #endregion

        #region Methods

        #region Method -> SearchMovies
        /// <summary>
        /// Search for movie with a criteria
        /// </summary>
        /// <param name="criteria">The criteria used for search</param>
        private async Task SearchMovies(string criteria)
        {
            foreach (object tab in MoviesViewModelTabs)
            {
                // Looking for a Search tab. If any, search movies with the criteria, and select this tab to be shown in the UI
                var moviesViewModel = tab as MoviesViewModel;
                if (moviesViewModel != null && moviesViewModel.Tab.TabName.Equals("Search"))
                {
                    moviesViewModel.SearchMoviesFilter = criteria;

                    await moviesViewModel.SearchMovies(criteria);
                    
                    if (SelectedTabViewModel != moviesViewModel)
                    {
                        SelectedTabViewModel = moviesViewModel;
                    }

                    return;
                }
            }

            // There is no current search tab, we have to create it
            MoviesViewModelTabs.Add(new MoviesViewModel()
            {
                Tab = new TabDescription(TabDescription.TabType.Search),
                SearchMoviesFilter = criteria
            });

            // Select in the UI the search tab we've just created
            SelectedTabViewModel = MoviesViewModelTabs.Last();

            // Search movies with criteria
            var searchMovieTab = SelectedTabViewModel as MoviesViewModel;
            if (searchMovieTab != null)
            {
                await searchMovieTab.SearchMovies(criteria);
            }
        }
        #endregion

        #region Method -> RemoveSearchTabIfEmptySearch
        /// <summary>
        /// Remove the Search tab if search filter is empty
        /// </summary>
        private void RemoveSearchTabIfEmptySearch()
        {
            if (String.IsNullOrEmpty(_searchMoviesFilter))
            {
                // The search filter is empty. We have to find the search tab if any
                MoviesViewModel searchTabToRemove = new MoviesViewModel();
                foreach (object tab in MoviesViewModelTabs)
                {
                    var moviesViewModel = tab as MoviesViewModel;
                    if (moviesViewModel != null && moviesViewModel.Tab.TabName.Equals("Search"))
                    {
                        // We've found it
                        searchTabToRemove = moviesViewModel;
                    }
                }

                // The search tab is currently selected in the UI, we have to pick a different selected tab prior deleting
                if (searchTabToRemove == SelectedTabViewModel)
                {
                    SelectedTabViewModel = MoviesViewModelTabs.FirstOrDefault();
                }

                // Remove the search tab
                if (searchTabToRemove.Tab != null && !String.IsNullOrEmpty(searchTabToRemove.Tab.TabName) && searchTabToRemove.Tab.TabName.Equals("Search"))
                {
                    MoviesViewModelTabs.Remove(searchTabToRemove);
                }
            }
        }
        #endregion

        #region Method -> LoadMovie
        /// <summary>
        /// Get the requested movie
        /// </summary>
        /// <param name="movieId">The movie ID</param>
        /// <param name="imdbCode">The IMDb code</param>
        private async Task LoadMovie(int movieId, string imdbCode)
        {
            Movie = new MovieFullDetails();

            // Inform subscribers we are loading the requested movie
            OnLoadingMovie(new EventArgs());

            // Get the requested movie using the service
            Tuple<MovieFullDetails, IEnumerable<Exception>> movie =
                await ApiService.GetMovieAsync(movieId,
                    CancellationLoadingToken).ConfigureAwait(false);

            // Check if we met any exception in the GetMoviesInfosAsync method
            if (HandleExceptions(movie.Item2))
            {
                // Inform we loaded the requested movie
                OnLoadedMovie(new EventArgs());
                return;
            }

            // Our loaded movie is here
            Movie = movie.Item1;

            // Inform we loaded the requested movie
            OnLoadedMovie(new EventArgs());

            // Download the movie poster
            Tuple<string, IEnumerable<Exception>> moviePosterAsyncResults =
                await ApiService.DownloadMoviePosterAsync(Movie.ImdbCode,
                    Movie.Images.LargeCoverImage,
                    CancellationLoadingToken).ConfigureAwait(false);

            // Set the path to the poster image if no exception occured in the DownloadMoviePosterAsync method
            if (!HandleExceptions(moviePosterAsyncResults.Item2))
                Movie.PosterImage = moviePosterAsyncResults.Item1;

            // For each director, we download its image
            foreach (Director director in Movie.Directors)
            {
                Tuple<string, IEnumerable<Exception>> directorsImagesAsyncResults =
                    await ApiService.DownloadDirectorImageAsync(director.Name.Trim(),
                        director.SmallImage,
                        CancellationLoadingToken).ConfigureAwait(false);

                // Set the path to the director image if no exception occured in the DownloadDirectorImageAsync method
                if (!HandleExceptions(directorsImagesAsyncResults.Item2))
                    director.SmallImagePath = directorsImagesAsyncResults.Item1;
            }

            // For each actor, we download its image
            foreach (Actor actor in Movie.Actors)
            {
                Tuple<string, IEnumerable<Exception>> actorsImagesAsyncResults =
                    await ApiService.DownloadActorImageAsync(actor.Name.Trim(),
                        actor.SmallImage,
                        CancellationLoadingToken).ConfigureAwait(false);

                // Set the path to the actor image if no exception occured in the DownloadActorImageAsync method
                if (!HandleExceptions(actorsImagesAsyncResults.Item2))
                    actor.SmallImagePath = actorsImagesAsyncResults.Item1;
            }

            Tuple<string, IEnumerable<Exception>> movieBackgroundImageResults =
                await ApiService.DownloadMovieBackgroundImageAsync(imdbCode, CancellationLoadingToken).ConfigureAwait(false);

            // Set the path to the poster image if no exception occured in the DownloadMoviePosterAsync method
            if (!HandleExceptions(movieBackgroundImageResults.Item2))
                Movie.BackgroundImage = movieBackgroundImageResults.Item1;
        }
        #endregion

        #region Method -> GetTrailer
        /// <summary>
        /// Get trailer of a movie
        /// </summary>
        /// <param name="imdbId">The IMDb Id of the movie</param>
        private async Task GetTrailer(string imdbId)
        {
            // Retrieve trailer from API
            Tuple<Trailers, Exception> trailer =
                ApiService.GetMovieTrailer(imdbId);

            if (trailer.Item2 != null)
            {
                // Inform we have loaded trailer with error
                OnLoadedTrailer(new TrailerLoadedEventArgs(String.Empty, true));
                return;
            }

            // No error has been encounter, we can create our VideoInfo
            VideoInfo video = null;

            try
            {
                // Retrieve Youtube Infos
                video = await GetVideoInfoForStreaming(Constants.YoutubePath + trailer.Item1.Youtube.Select(x => x.Source).FirstOrDefault(), YoutubeStreamingQuality.High);

                if (video != null && video.RequiresDecryption)
                {
                    // Decrypt encoded Youtube video link 
                    await Task.Run(() => DownloadUrlResolver.DecryptDownloadUrl(video));
                }
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex is VideoNotAvailableException || ex is YoutubeParseException)
                {
                    // An error as occured. Inform we have loaded trailer with error
                    OnLoadedTrailer(new TrailerLoadedEventArgs(String.Empty, true));
                    return;
                }
            }

            if (video == null)
            {
                // There is no VideoInfo. Inform we have loaded trailer with error
                OnLoadedTrailer(new TrailerLoadedEventArgs(String.Empty, true));
                return;
            }

            // Inform we have loaded trailer successfully
            OnLoadedTrailer(new TrailerLoadedEventArgs(video.DownloadUrl, false));
        }

        #endregion

        #region Method -> GetVideoInfoForStreaming
        /// <summary>
        /// Get VideoInfo of a youtube video
        /// </summary>
        /// <param name="youtubeLink">The youtube link of a movie</param>
        /// <param name="qualitySetting">The youtube quality settings</param>
        private static async Task<VideoInfo> GetVideoInfoForStreaming(string youtubeLink, YoutubeStreamingQuality qualitySetting)
        {
            // Get video infos of the requested video
            IEnumerable<VideoInfo> videoInfos = await Task.Run(() => DownloadUrlResolver.GetDownloadUrls(youtubeLink, false));

            // We only want video matching criterias : only mp4 and no adaptive
            IEnumerable<VideoInfo> filtered = videoInfos
                .Where(info => info.VideoType == VideoType.Mp4 && !info.Is3D && info.AdaptiveType == AdaptiveType.None);

            return GetVideoByStreamingQuality(filtered, qualitySetting);
        }
        #endregion

        #region Method -> GetVideoByStreamingQuality
        /// <summary>
        /// Get youtube video depending of choosen quality settings
        /// </summary>
        /// <param name="videos">List of VideoInfo</param>
        /// <param name="quality">The youtube quality settings</param>
        private static VideoInfo GetVideoByStreamingQuality(IEnumerable<VideoInfo> videos, YoutubeStreamingQuality quality)
        {
            videos = videos.ToList(); // Prevent multiple enumeration

            if (quality == YoutubeStreamingQuality.High)
            {
                // Choose high quality Youtube video
                return videos.OrderByDescending(x => x.Resolution)
                    .FirstOrDefault();
            }

            // Pick the video with the requested quality settings
            IEnumerable<int> preferredResolutions = StreamingQualityMap[quality];

            IEnumerable<VideoInfo> preferredVideos = videos
                .Where(info => preferredResolutions.Contains(info.Resolution))
                .OrderByDescending(info => info.Resolution);

            VideoInfo video = preferredVideos.FirstOrDefault();

            if (video == null)
            {
                // We search for an other video quality if none has been found
                return GetVideoByStreamingQuality(videos, (YoutubeStreamingQuality)(((int)quality) - 1));
            }

            return video;
        }
        #endregion

        #region Method -> HandleExceptions
        /// <summary>
        /// Handle list of exceptions
        /// </summary>
        /// <param name="exceptions">List of exceptions</param>
        /// <returns name="isExceptionThrown">Returns true if an exception has been thrown, else false</returns>
        private static bool HandleExceptions(IEnumerable<Exception> exceptions)
        {
            bool isExceptionThrown = false;
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
            }

            if (isConnexionInError)
            {
                // Inform we have a connection error
                Messenger.Default.Send<bool>(true, Constants.ConnectionErrorPropertyName);
            }

            return isExceptionThrown;
        }
        #endregion

        #region Method -> DownloadMovie
        /// <summary>
        /// Download a movie
        /// </summary>
        /// <param name="movie">The movie to download</param>
        private async Task DownloadMovie(MovieFullDetails movie)
        {
            using (Session session = new Session())
            {
                IsDownloadingMovie = true;

                // Inform subscribers we're actually loading a movie
                OnDownloadingMovie(new EventArgs());

                // Listening to a port which is randomly between 6881 and 6889
                session.ListenOn(6881, 6889);

                var addParams = new AddTorrentParams
                {
                    // Where do we save the video file
                    SavePath = Constants.MovieDownloads,
                    // At this time, no quality selection is available in the interface, so we take the lowest
                    Url = movie.Torrents.Aggregate((i1, i2) => (i1.SizeBytes < i2.SizeBytes ? i1 : i2)).Url
                };

                TorrentHandle handle = session.AddTorrent(addParams);
                // We have to download sequentially, so that we're able to play the movie without waiting
                handle.SequentialDownload = true;
                bool alreadyBuffered = false;
                while (IsDownloadingMovie)
                {
                    TorrentStatus status = handle.QueryStatus();
                    double progress = status.Progress * 100.0;

                    // We have to flush cache regularly (otherwise memory cache get full very quickly)
                    handle.FlushCache();

                    if (handle.NeedSaveResumeData())
                    {
                        handle.SaveResumeData();
                    }

                    // Inform subscribers of our progress
                    OnLoadingMovieProgress(new MovieLoadingProgressEventArgs(progress, status.DownloadRate / 1024));

                    // We consider 2% of progress is enough to start playing
                    if (progress >= Constants.MinimumBufferingBeforeMoviePlaying && !alreadyBuffered)
                    {
                        // Get movie file
                        foreach (
                            string filePath in 
                                Directory.GetFiles(status.SavePath + handle.TorrentFile.Name, "*" + Constants.VideoFileExtension)
                            )
                        {
                            // Inform subscribers we have finished buffering the movie
                            Messenger.Default.Send<MovieBufferedMessage>(new MovieBufferedMessage(Movie,
                                new Uri(filePath)));
                            alreadyBuffered = true;
                        }
                    }

                    // Wait for a second before update torrent status
                    await Task.Delay(1000, CancellationDownloadingToken.Token).ContinueWith(_ =>
                    {
                        // Check if movie downloading has been cancelled
                        if (CancellationDownloadingToken.IsCancellationRequested && session != null)
                        {
                            IsDownloadingMovie = false;

                            // Send a StopDownloadingMovieMessage to every subscribers and ask if we have to delete movie file
                            var message = new StopDownloadingMovieMessage(

                                // Feedback
                                deleteMovieFiles =>
                                {
                                    if (handle.TorrentFile != null)
                                    {
                                        string torrentFile = handle.TorrentFile.Name;
                                        session.RemoveTorrent(handle, false);

                                        if (deleteMovieFiles && !String.IsNullOrEmpty(torrentFile) && Directory.Exists(status.SavePath + torrentFile))
                                        {
                                            try
                                            {
                                                // Delete movie file
                                                Directory.Delete(status.SavePath + torrentFile, true);
                                            }
                                            catch (Exception e)
                                            {
                                                // TODO
                                            }
                                        }
                                    }
                                });
                            Messenger.Default.Send<StopDownloadingMovieMessage>(message);
                        }
                    }).ConfigureAwait(false);
                }
            }            
        }
        #endregion

        #region Method -> StopDownloadingMovie
        /// <summary>
        /// Stop downloading a movie
        /// </summary>
        public void StopDownloadingMovie()
        {
            if (CancellationDownloadingToken != null)
            {
                CancellationDownloadingToken.Cancel(true);
            }
        }
        #endregion  
      
        #endregion

        #region Events

        #region Event -> OnConnectionError
        /// <summary>
        /// ConnectionError event
        /// </summary>
        public event EventHandler<ConnectionErrorEventArgs> ConnectionError;
        /// <summary>
        /// Fire on connection error
        /// </summary>
        ///<param name="e">Event data</param>
        protected virtual void OnConnectionError(ConnectionErrorEventArgs e)
        {
            EventHandler<ConnectionErrorEventArgs> handler = ConnectionError;
            if (handler != null)
            {
                if (e != null && e.IsInError)
                {
                    IsConnectionInError = true;
                }
                else
                {
                    IsConnectionInError = false;
                }
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnLoadingMovieProgress
        /// <summary>
        /// LoadingMovieProgress event
        /// </summary>
        public event EventHandler<MovieLoadingProgressEventArgs> LoadingMovieProgress;
        /// <summary>
        /// Reports movie loading progress
        /// </summary>
        ///<param name="e">Event data</param>
        protected virtual void OnLoadingMovieProgress(MovieLoadingProgressEventArgs e)
        {
            EventHandler<MovieLoadingProgressEventArgs> handler = LoadingMovieProgress;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnDownloadingMovie
        /// <summary>
        /// DownloadingMovie event
        /// </summary>
        public event EventHandler<EventArgs> DownloadingMovie;
        /// <summary>
        /// Fire on movie downloading
        /// </summary>
        ///<param name="e">Event data</param>
        protected virtual void OnDownloadingMovie(EventArgs e)
        {
            EventHandler<EventArgs> handler = DownloadingMovie;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnLoadingMovie
        /// <summary>
        /// LoadingMovie event
        /// </summary>
        public event EventHandler<EventArgs> LoadingMovie;
        /// <summary>
        /// Fire on movie loading
        /// </summary>
        ///<param name="e">Event data</param>
        protected virtual void OnLoadingMovie(EventArgs e)
        {
            EventHandler<EventArgs> handler = LoadingMovie;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnLoadedMovie
        /// <summary>
        /// LoadedMovie event
        /// </summary>
        public event EventHandler<EventArgs> LoadedMovie;
        /// <summary>
        /// Fire on movie loaded
        /// </summary>
        ///<param name="e">Event data</param>
        protected virtual void OnLoadedMovie(EventArgs e)
        {
            EventHandler<EventArgs> handler = LoadedMovie;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnStoppedDownloadingMovie
        /// <summary>
        /// StoppedDownloadingMovie event
        /// </summary>
        public event EventHandler<EventArgs> StoppedDownloadingMovie;
        /// <summary>
        /// Fire on movie stopped downloading
        /// </summary>
        ///<param name="e">Event data</param>
        protected virtual void OnStoppedDownloadingMovie(EventArgs e)
        {
            EventHandler<EventArgs> handler = StoppedDownloadingMovie;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnBufferedMovie
        /// <summary>
        /// BufferedMovie event
        /// </summary>
        public event EventHandler<MovieBufferedEventArgs> BufferedMovie;
        /// <summary>
        /// Fire on movie finished buffering
        /// </summary>
        ///<param name="e">Event data</param>
        protected virtual void OnBufferedMovie(MovieBufferedEventArgs e)
        {
            EventHandler<MovieBufferedEventArgs> handler = BufferedMovie;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnLoadedTrailer
        /// <summary>
        /// LoadedTrailer event
        /// </summary>
        public event EventHandler<TrailerLoadedEventArgs> LoadedTrailer;
        /// <summary>
        /// Fire when movie trailer has finished loading
        /// </summary>
        ///<param name="e">Event data</param>
        protected virtual void OnLoadedTrailer(TrailerLoadedEventArgs e)
        {
            EventHandler<TrailerLoadedEventArgs> handler = LoadedTrailer;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #endregion

        public override void Cleanup()
        {
            Messenger.Default.Unregister<PropertyChangedMessage<string>>(this);
            Messenger.Default.Unregister<bool>(this);
            Messenger.Default.Unregister<MovieBufferedMessage>(this);
            Messenger.Default.Unregister<StopDownloadingMovieMessage>(this);
            Messenger.Default.Unregister<MainWindowClosingMessage>(this);
            base.Cleanup();
        }
    }
}