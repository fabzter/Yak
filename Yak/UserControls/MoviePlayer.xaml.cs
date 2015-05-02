using System;
using System.Globalization;
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

        #region Property -> MoviePlayerIsPlaying
        /// <summary>
        /// Indicates if a movie is playing
        /// </summary>
        private bool MoviePlayerIsPlaying;
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

        private FullScreenMoviePlayer fsFullScreenMoviePlayer;

        #endregion

        #region Constructor
        /// <summary>
        /// MoviePlayer
        /// </summary>
        public MoviePlayer()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            Unloaded += (s, e) =>
            {
                var vm = DataContext as MoviePlayerViewModel;
                if (vm != null)
                {
                    if (vm.IsInFullScreenMode)
                    {
                        Dispose();
                    }
                }
            };
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

                vm.StoppedDownloadingMovie += OnStoppedDownloadingMovie;

                if (!vm.IsInFullScreenMode)
                {
                    vm.ToggleFullScreenChanged += OnToggleFullScreen;
                }

                vm.BackToNormalScreenChanged += OnBackToNormalScreen;

                if (!String.IsNullOrEmpty(vm.MovieFilePath))
                {
                    PlayMovie(vm.CurrentMovieProgressValue, vm.MovieFilePath);
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
                if (Player != null && Player.Source != null)
                {
                    Player.Stop();
                    Player.Close();
                    Player.Source = null;
                    MoviePlayerIsPlaying = false;
                }

                MoviePlayerTimer.Tick -= MoviePlayerTimer_Tick;
                MoviePlayerTimer.Stop();
            });
        }
        #endregion

        #region Method -> PlayMovie
        /// <summary>
        /// Play the movie when buffered
        /// </summary>
        /// <param name="currentMoviePlayingProgressValue">Sender object</param>
        /// <param name="pathToFile">MovieBufferedEventArgs</param>
        private void PlayMovie(double currentMoviePlayingProgressValue, string pathToFile)
        {
            MoviePlayerTimer.Tick += MoviePlayerTimer_Tick;
            MoviePlayerTimer.Start();

            Player.Source = new Uri(pathToFile);
            Player.Position = TimeSpan.FromSeconds(currentMoviePlayingProgressValue);
            Player.Play();
            Player.StretchDirection = StretchDirection.Both;

            MoviePlayerIsPlaying = true;
        }
        #endregion

        #region Method -> MoviePlayerTimer_Tick
        /// <summary>
        /// Report the playing progress on the timeline
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        public void MoviePlayerTimer_Tick(object sender, EventArgs e)
        {
            if ((Player.Source != null) && (Player.NaturalDuration.HasTimeSpan) && (!UserIsDraggingMoviePlayerSlider))
            {
                MoviePlayerSliderProgress.Minimum = 0;
                MoviePlayerSliderProgress.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
                MoviePlayerSliderProgress.Value = Player.Position.TotalSeconds;
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
                e.CanExecute = (Player != null) && (Player.Source != null);
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
            Player.Play();
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
            Player.Pause();
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
            Player.Position = TimeSpan.FromSeconds(MoviePlayerSliderProgress.Value);
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
            Player.Volume += (e.Delta > 0) ? 0.1 : -0.1;
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
                if (fsFullScreenMoviePlayer == null)
                {
                    fsFullScreenMoviePlayer = new FullScreenMoviePlayer();
                    fsFullScreenMoviePlayer.Closed += (o, args) => fsFullScreenMoviePlayer = null;
                    fsFullScreenMoviePlayer.DataContext = DataContext;
                    Player.Pause();
                    fsFullScreenMoviePlayer.Launch();
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
                    Player.Position = TimeSpan.FromSeconds(moviePlayerViewModel.CurrentMovieProgressValue);
                    Player.Play();
                }
            }
        }
        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                Loaded -= OnLoaded;
                var vm = DataContext as MoviePlayerViewModel;
                if (vm != null)
                {
                    vm.StoppedDownloadingMovie -= OnStoppedDownloadingMovie;
                    vm.ToggleFullScreenChanged -= OnToggleFullScreen;
                    vm.BackToNormalScreenChanged -= OnBackToNormalScreen;
                }

                MoviePlayerTimer.Tick -= MoviePlayerTimer_Tick;
                MoviePlayerTimer.Stop();

                if (Player != null && Player.Source != null)
                {
                    Player.Stop();
                    Player.Close();
                    Player.Source = null;
                    MoviePlayerIsPlaying = false;
                }
                _disposed = true;
            }
        }
    }
}