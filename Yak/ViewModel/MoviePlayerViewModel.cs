using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Yak.Helpers;
using Yak.Messaging;
using Yak.Model.Movie;
using GalaSoft.MvvmLight.Command;

namespace Yak.ViewModel
{
    /// <summary>
    /// MoviePlayerViewModel
    /// </summary>
    public class MoviePlayerViewModel : ViewModelBase
    {
        #region Property -> Tab
        /// <summary>
        /// Name of the tab to be displayed into the interface
        /// </summary>
        public TabDescription Tab { get; set; }
        #endregion

        #region Property -> SearchMoviesFilter
        /// <summary>
        /// Useless search filter (defined here to avoid binding error in MainWindow.xaml with TabControl)
        /// </summary>
        public string SearchMoviesFilter { get; set; }
        #endregion

        #region Property -> Movie
        /// <summary>
        /// The movie to be played
        /// </summary>
        public MovieFullDetails Movie { get; set; }
        #endregion

        #region Property -> MovieUri
        /// <summary>
        /// Uri to file path of the movie to be played
        /// </summary>
        public Uri MovieUri { get; set; }
        #endregion

        #region Property -> CurrentMovieProgressValue
        /// <summary>
        /// The current progress playing value
        /// </summary>
        public double CurrentMovieProgressValue { get; set; }
        #endregion

        #region Property -> MediaVolume
        /// <summary>
        /// The current volume of the media set in the player
        /// </summary>
        public int MediaVolume { get; set; }
        #endregion

        #region Property -> IsInFullScreenMode
        /// <summary>
        /// Indicates if we are in fullscreen mode
        /// </summary>
        private bool _isInFullScreenMode;

        public bool IsInFullScreenMode
        {
            get { return _isInFullScreenMode; }
            set { Set(() => IsInFullScreenMode, ref _isInFullScreenMode, value, true); }
        }

        #endregion

        #region Property -> DeleteMovieFilesAction
        /// <summary>
        /// Delete movie files
        /// </summary>
        public Action<bool> DeleteMovieFilesAction;
        #endregion

        #region Commands

        #region Command -> ToggleFullScreenCommand
        /// <summary>
        /// ToggleFullScreenCommand
        /// </summary>
        public RelayCommand ToggleFullScreenCommand
        {
            get;
            private set;
        }
        #endregion

        #region Command -> BackToNormalScreenComand
        /// <summary>
        /// ToggleFullScreenCommand
        /// </summary>
        public RelayCommand BackToNormalScreenComand
        {
            get;
            private set;
        }
        #endregion

        #endregion

        #region Constructor -> MoviePlayerViewModel
        /// <summary>
        /// Initializes a new instance of the MoviePlayerViewModel class.
        /// </summary>
        public MoviePlayerViewModel()   
            : this(null, null)
        {

        }
        #endregion

        #region Constructor -> MoviePlayerViewModel
        /// <summary>
        /// Initializes a new instance of the MoviePlayerViewModel class.
        /// </summary>
        public MoviePlayerViewModel(MovieFullDetails movie, Uri movieUri)
        {
            Messenger.Default.Register<StopDownloadingMovieMessage>(
                this, 
                message => 
                    {
                        DeleteMovieFilesAction = message.DeleteMovieFilesAction;
                        OnStoppedDownloadingMovie(new EventArgs());
                    });

            Movie = movie;
            MovieUri = movieUri;

            ToggleFullScreenCommand = new RelayCommand(() =>
            {
                OnToggleFullScreen(new EventArgs());
            });

            BackToNormalScreenComand = new RelayCommand(() =>
            {
                OnBackToNormalScreen(new EventArgs());
            });
        }
        #endregion

        #region Events

        #region Event -> OnStoppedDownloadingMovie
        /// <summary>
        /// StoppedDownloadingMovie event
        /// </summary>
        public event EventHandler<EventArgs> StoppedDownloadingMovie;
        /// <summary>
        /// Fire event when movie has stopped downloading
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

        #region Event -> OnToggleFullScreen
        /// <summary>
        /// ToggleFullScreenChanged event
        /// </summary>
        public event EventHandler<EventArgs> ToggleFullScreenChanged;
        /// <summary>
        /// Fire event when fullscreen mode has been requested
        /// </summary>
        ///<param name="e">Event data</param>
        protected virtual void OnToggleFullScreen(EventArgs e)
        {
            EventHandler<EventArgs> handler = ToggleFullScreenChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnBackToNormalScreen
        /// <summary>
        /// BackToNormalScreenChanged event
        /// </summary>
        public event EventHandler<EventArgs> BackToNormalScreenChanged;
        /// <summary>
        /// Fire event when back to normal screen has been requested
        /// </summary>
        ///<param name="e">Event data</param>
        protected virtual void OnBackToNormalScreen(EventArgs e)
        {
            EventHandler<EventArgs> handler = BackToNormalScreenChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #endregion
        public override void Cleanup()
        {
            Messenger.Default.Unregister<StopDownloadingMovieMessage>(this);
            base.Cleanup();
        }
    }
}
