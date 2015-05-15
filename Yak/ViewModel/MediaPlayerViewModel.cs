using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Yak.Helpers;
using Yak.Messaging;
using GalaSoft.MvvmLight.Command;

namespace Yak.ViewModel
{
    /// <summary>
    /// MediaPlayerViewModel
    /// </summary>
    public class MediaPlayerViewModel : ViewModelBase
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

        #region Property -> MediaUri

        /// <summary>
        /// Uri to file path of the media to be played
        /// </summary>
        public Uri MediaUri { get; private set; }

        #endregion

        #region Property -> MediaPosition

        /// <summary>
        /// Current position related to media's time progress
        /// </summary>
        public double MediaPosition { get; set; }

        #endregion

        #region Property -> MediaVolume

        /// <summary>
        /// The current volume of the media set in the player
        /// </summary>
        public int MediaVolume { get; set; }

        #endregion

        #region Property -> IsInFullScreenMode

        /// <summary>
        /// Indicates if player is in fullscreen mode
        /// </summary>
        private bool _isInFullScreenMode;

        public bool IsInFullScreenMode
        {
            get { return _isInFullScreenMode; }
            set { Set(() => IsInFullScreenMode, ref _isInFullScreenMode, value, true); }
        }

        #endregion

        #region Property -> MediaType

        /// <summary>
        /// Indicates the media's type
        /// </summary>
        private Constants.MediaType MediaType { get; }

        #endregion

        #region Property -> DeleteMovieFile

        /// <summary>
        /// Delete movie files when movie has stopped playing
        /// </summary>
        public Action<bool> DeleteMovieFile;

        #endregion

        #region Commands

        #region Command -> ToggleFullScreenCommand

        /// <summary>
        /// ToggleFullScreenCommand
        /// </summary>
        public RelayCommand ToggleFullScreenCommand { get; private set; }

        #endregion

        #region Command -> BackToNormalScreenComand

        /// <summary>
        /// ToggleFullScreenCommand
        /// </summary>
        public RelayCommand BackToNormalScreenComand { get; private set; }

        #endregion

        #region Command -> StopPlayingMediaCommand

        /// <summary>
        /// StopPlayingMediaCommand
        /// </summary>
        public RelayCommand StopPlayingMediaCommand { get; private set; }

        #endregion

        #endregion

        #region Constructor -> MediaPlayerViewModel

        /// <summary>
        /// Initializes a new instance of the MediaPlayerViewModel class.
        /// </summary>
        public MediaPlayerViewModel(Constants.MediaType mediaType, Uri mediaUri)
        {
            Messenger.Default.Register<StopPlayingMediaMessage>(
                this,
                message =>
                {
                    DeleteMovieFile = message.DeleteMovieFile;
                    OnStoppedDownloadingMovie(new EventArgs());
                });
            MediaType = mediaType;
            MediaUri = mediaUri;
            MediaVolume = 100;

            ToggleFullScreenCommand = new RelayCommand(() =>
            {
                OnToggleFullScreen(new EventArgs());
            });

            BackToNormalScreenComand = new RelayCommand(() =>
            {
                OnBackToNormalScreen(new EventArgs());
            });

            StopPlayingMediaCommand = new RelayCommand(() =>
            {
                if (MediaType == Constants.MediaType.Trailer)
                {
                    Messenger.Default.Send(new StopPlayingMediaMessage(Constants.MediaType.Trailer, null));
                }
                else if (MediaType == Constants.MediaType.Movie)
                {
                    Messenger.Default.Send(new StopPlayingMediaMessage(Constants.MediaType.Movie, null));
                }
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
            var handler = StoppedDownloadingMovie;
            handler?.Invoke(this, e);
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
            var handler = ToggleFullScreenChanged;
            handler?.Invoke(this, e);
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
            var handler = BackToNormalScreenChanged;
            handler?.Invoke(this, e);
        }

        #endregion

        #endregion

        public override void Cleanup()
        {
            Messenger.Default.Unregister<StopPlayingMediaMessage>(this);
            base.Cleanup();
        }
    }
}
