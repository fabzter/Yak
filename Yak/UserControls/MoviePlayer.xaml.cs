using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using Yak.ViewModel;

namespace Yak.UserControls
{
    /// <summary>
    /// Interaction logic for MoviePlayer.xaml
    /// </summary>
    public partial class MoviePlayer : UserControl, IDisposable
    {
        #region Properties

        private bool _disposed;

        #region DependencyProperty -> MoviePlayerIsPlaying
        /// <summary>
        /// Identifies the <see cref="MoviePlayerIsPlaying"/> dependency property. 
        /// </summary>
        internal static readonly DependencyProperty MoviePlayerIsPlayingProperty = DependencyProperty.Register("MoviePlayerIsPlaying", typeof(bool), typeof(MoviePlayer), new PropertyMetadata(false, null));
        #endregion

        #region Property -> MoviePlayerIsPlaying
        /// <summary>
        /// MoviePlayerIsPlaying 
        /// </summary>
        public bool MoviePlayerIsPlaying
        {
            get
            {
                return (bool)GetValue(MoviePlayerIsPlayingProperty);
            }

            set
            {
                SetValue(MoviePlayerIsPlayingProperty, value);
            }
        }
        #endregion

        #region Property -> IsInFullScreen
        /// <summary>
        /// Indicates if this controls is hosted in a fullscreen context
        /// </summary>
        private bool IsInFullScreen { get; set; }
        #endregion

        #region Property -> UserIsDraggingMoviePlayerSlider
        /// <summary>
        /// Indicate if user is manipulating the timeline player
        /// </summary>
        private bool UserIsDraggingMoviePlayerSlider;
        #endregion

        #region Property -> MoviePlayerTimer
        /// <summary>
        /// Timer used for report time on the timeline
        /// </summary>
        private DispatcherTimer MoviePlayerTimer;
        #endregion

        #region Property -> fullScreenMoviePlayer
        /// <summary>
        /// The movie player used in FullScreen mode
        /// </summary>
        private FullScreenMoviePlayer fullScreenMoviePlayer;
        #endregion

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MoviePlayer class.
        /// </summary>
        public MoviePlayer()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            Unloaded += OnUnloaded;
        }
        #endregion

        #region Method -> Onloaded
        /// <summary>
        /// Do action when loaded
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnLoaded(object sender, EventArgs e)
        {
            var vm = DataContext as MoviePlayerViewModel;
            if (vm != null)
            {
                MoviePlayerTimer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromSeconds(1)
                };

                MoviePlayerTimer.Tick += MoviePlayerTimer_Tick;
                MoviePlayerTimer.Start();

                vm.StoppedDownloadingMovie += OnStoppedDownloadingMovie;

                if (!vm.IsInFullScreenMode)
                {
                    vm.ToggleFullScreenChanged += OnToggleFullScreen;
                }

                vm.BackToNormalScreenChanged += OnBackToNormalScreen;

                var currentAssembly = Assembly.GetEntryAssembly();
                var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
                if (currentDirectory == null)
                    return;
                Player.MediaPlayer.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"lib\"));

                if (vm.MovieUri != null)
                {
                    PlayMovie(vm.CurrentMovieProgressValue, vm.MovieUri);
                }
            }
        }
        #endregion

        #region Method -> OnUnloaded
        /// <summary>
        /// Do action when unloaded
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnUnloaded(object sender, EventArgs e)
        {
            var vm = DataContext as MoviePlayerViewModel;
            if (vm != null)
            {
                if (vm.IsInFullScreenMode)
                {
                    Dispose();
                }
            }
        }
        #endregion

        #region Method -> OnStoppedDownloadingMovie
        /// <summary>
        /// When downloading movie has finished, stop player and reset timer
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnStoppedDownloadingMovie(object sender, EventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (Player != null && Player.MediaPlayer != null)
                {
                    Player.MediaPlayer.Stop();
                    MoviePlayerIsPlaying = false;
                }
            });
        }
        #endregion

        #region Method -> PlayMovie

        /// <summary>
        /// Play the movie when buffered
        /// </summary>
        /// <param name="currentMoviePlayingProgressValue">Sender object</param>
        /// <param name="movieUri">ovie Uri to be played</param>
        private void PlayMovie(double currentMoviePlayingProgressValue, Uri movieUri)
        {
            FileInfo movieFile = new FileInfo(Uri.UnescapeDataString(movieUri.AbsolutePath));
            Player.MediaPlayer.Play(movieFile);
            Player.MediaPlayer.Time =
                Convert.ToInt64(TimeSpan.FromSeconds(currentMoviePlayingProgressValue).TotalMilliseconds);
            MoviePlayerIsPlaying = true;
            Player.MediaPlayer.Audio.Volume = 100;
        }

        #endregion

        #region Method -> MoviePlayerTimer_Tick
        /// <summary>
        /// Report the playing progress on the timeline
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void MoviePlayerTimer_Tick(object sender, EventArgs e)
        {
            if ((Player != null && Player.MediaPlayer != null) && (!UserIsDraggingMoviePlayerSlider))
            {
                MoviePlayerSliderProgress.Minimum = 0;
                MoviePlayerSliderProgress.Maximum = Player.MediaPlayer.Length;
                MoviePlayerSliderProgress.Value = TimeSpan.FromMilliseconds(Player.MediaPlayer.Time).TotalSeconds;
            }
        }
        #endregion

        #region Method -> MoviePlayerPlay_CanExecute
        /// <summary>
        /// Each time the CanExecute play command change, update the visibility of Play/Pause buttons in the player
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">CanExecuteRoutedEventArgs</param>
        private void MoviePlayerPlay_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (MoviePlayerStatusBarItemPlay != null && MoviePlayerStatusBarItemPause != null)
            {
                e.CanExecute = (Player != null) && (Player.MediaPlayer != null);
                if (MoviePlayerIsPlaying)
                {
                    MoviePlayerStatusBarItemPlay.Visibility = Visibility.Collapsed;
                    MoviePlayerStatusBarItemPause.Visibility = Visibility.Visible;
                }
                else
                {
                    MoviePlayerStatusBarItemPlay.Visibility = Visibility.Visible;
                    MoviePlayerStatusBarItemPause.Visibility = Visibility.Collapsed;
                }
            }
        }

        #endregion

        #region Method -> MoviePlayerPlay_Executed
        /// <summary>
        /// Play the current movie
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">ExecutedRoutedEventArgs</param>
        private void MoviePlayerPlay_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Player.MediaPlayer.Play();
            MoviePlayerIsPlaying = true;

            MoviePlayerStatusBarItemPlay.Visibility = Visibility.Collapsed;
            MoviePlayerStatusBarItemPause.Visibility = Visibility.Visible;
        }
        #endregion

        #region Method -> MoviePlayerPause_CanExecute
        /// <summary>
        /// Each time the CanExecute play command change, update the visibility of Play/Pause buttons in the movie player
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">CanExecuteRoutedEventArgs</param>
        private void MoviePlayerPause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (MoviePlayerStatusBarItemPlay != null && MoviePlayerStatusBarItemPause != null)
            {
                e.CanExecute = MoviePlayerIsPlaying;
                if (MoviePlayerIsPlaying)
                {
                    MoviePlayerStatusBarItemPlay.Visibility = Visibility.Collapsed;
                    MoviePlayerStatusBarItemPause.Visibility = Visibility.Visible;
                }
                else
                {
                    MoviePlayerStatusBarItemPlay.Visibility = Visibility.Visible;
                    MoviePlayerStatusBarItemPause.Visibility = Visibility.Collapsed;
                }
            }
        }
        #endregion

        #region Method -> MoviePlayerPause_Executed
        /// <summary>
        /// Pause the movie
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">CanExecuteRoutedEventArgs</param>
        private void MoviePlayerPause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Player.MediaPlayer.Pause();
            MoviePlayerIsPlaying = false;

            MoviePlayerStatusBarItemPlay.Visibility = Visibility.Visible;
            MoviePlayerStatusBarItemPause.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Method -> MovieSliderProgress_DragStarted
        /// <summary>
        /// Report when dragging is used on movie player
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">DragStartedEventArgs</param>
        private void MovieSliderProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            UserIsDraggingMoviePlayerSlider = true;
        }
        #endregion

        #region Method -> MovieSliderProgress_DragCompleted
        /// <summary>
        /// Report when user has finished dragging the movie player progress
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">DragCompletedEventArgs</param>
        private void MovieSliderProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            UserIsDraggingMoviePlayerSlider = false;
            Player.MediaPlayer.Time = Convert.ToInt64(TimeSpan.FromSeconds(MoviePlayerSliderProgress.Value).TotalMilliseconds);
        }
        #endregion

        #region Method -> MovieSliderProgress_ValueChanged
        /// <summary>
        /// Report runtime when movie player progress changed
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">RoutedPropertyChangedEventArgs</param>
        private void MovieSliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var vm = DataContext as MoviePlayerViewModel;
            if (vm != null)
            {
                vm.CurrentMovieProgressValue = MoviePlayerSliderProgress.Value;
                MoviePlayerTextProgressStatus.Text = TimeSpan.FromSeconds(MoviePlayerSliderProgress.Value).ToString(@"hh\:mm\:ss", CultureInfo.CurrentCulture) + " / " + TimeSpan.FromSeconds(vm.Movie.Runtime * 60).ToString(@"hh\:mm\:ss", CultureInfo.CurrentCulture);
            }
        }
        #endregion

        #region Method -> MouseWheelMoviePlayer
        /// <summary>
        /// When user uses the mousewheel, update the volume
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">MouseWheelEventArgs</param>
        private void MouseWheelMoviePlayer(object sender, MouseWheelEventArgs e)
        {
            Player.MediaPlayer.Audio.Volume += (e.Delta > 0) ? 10 : -10;
        }
        #endregion

        #region Method -> OnToggleFullScreen
        /// <summary>
        /// When fullscreen value has changed
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnToggleFullScreen(object sender, EventArgs e)
        {
            var moviePlayerViewModel = DataContext as MoviePlayerViewModel;
            if (moviePlayerViewModel != null && !IsInFullScreen)
            {
                if (fullScreenMoviePlayer == null)
                {
                    fullScreenMoviePlayer = new FullScreenMoviePlayer();
                    fullScreenMoviePlayer.Closed += (o, args) => fullScreenMoviePlayer = null;
                    fullScreenMoviePlayer.DataContext = DataContext;
                    Player.MediaPlayer.Pause();
                    fullScreenMoviePlayer.Launch();
                    IsInFullScreen = true;
                }
            }
        }
        #endregion

        #region Method -> OnBackToNormalScreen
        /// <summary>
        /// When back to normal screen size has been requested
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnBackToNormalScreen(object sender, EventArgs e)
        {
            var moviePlayerViewModel = DataContext as MoviePlayerViewModel;
            if (moviePlayerViewModel != null)
            {
                IsInFullScreen = false;

                if (Player != null)
                {
                    Player.MediaPlayer.Time = Convert.ToInt64(TimeSpan.FromSeconds(moviePlayerViewModel.CurrentMovieProgressValue).TotalMilliseconds);
                    Player.MediaPlayer.Play();
                }
            }
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Free resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                Loaded -= OnLoaded;
                Unloaded -= OnUnloaded;
                var vm = DataContext as MoviePlayerViewModel;
                if (vm != null)
                {
                    vm.StoppedDownloadingMovie -= OnStoppedDownloadingMovie;
                    vm.ToggleFullScreenChanged -= OnToggleFullScreen;
                    vm.BackToNormalScreenChanged -= OnBackToNormalScreen;
                }

                MoviePlayerTimer.Tick -= MoviePlayerTimer_Tick;
                MoviePlayerTimer.Stop();

                if (Player != null && Player.MediaPlayer != null)
                {
                    Player.MediaPlayer.Stop();
                    Player.MediaPlayer.Dispose();
                    MoviePlayerIsPlaying = false;
                }
                _disposed = true;

                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
            }
        }
        #endregion
    }
}