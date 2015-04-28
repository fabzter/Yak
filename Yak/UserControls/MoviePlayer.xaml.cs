using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using Yak.Events;
using Yak.ViewModel;

namespace Yak.UserControls
{
    /// <summary>
    /// Logique d'interaction pour MoviePlayer.xaml
    /// </summary>
    public partial class MoviePlayer : UserControl
    {
        #region Properties

        #region Property -> MoviePlayerIsPlaying
        public bool MoviePlayerIsPlaying;
        #endregion

        #region Property -> UserIsDraggingMoviePlayerSlider
        public bool UserIsDraggingMoviePlayerSlider;
        #endregion

        #region Property -> MoviePlayerTimer
        public DispatcherTimer MoviePlayerTimer;
        #endregion

        #endregion

        public MoviePlayer()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var vm = DataContext as MoviePlayerViewModel;
                if (vm != null)
                {
                    MoviePlayerTimer = new DispatcherTimer()
                    {
                        Interval = TimeSpan.FromSeconds(1)
                    };

                    vm.WindowSizeChanged += OnWindowSizeChanged;
                    vm.StoppedDownloadingMovie += OnStoppedDownloadingMovie;

                    if (!String.IsNullOrEmpty(vm.MovieFilePath))
                    {
                        PlayMovie(vm.CurrentMovieProgressValue, vm.MovieFilePath);
                    }
                }
            };

            Unloaded += (s, e) =>
            {
                var vm = DataContext as MoviePlayerViewModel;
                if (vm != null)
                {
                    vm.WindowSizeChanged -= OnWindowSizeChanged;
                    vm.StoppedDownloadingMovie -= OnStoppedDownloadingMovie;
                }
            };
        }

        #region Method -> OnStoppedDownloadingMovie

        /// <summary>
        /// Close the player and go back to the movie page when the downloading of the movie has stopped
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

                #region Dispatcher Timer

                MoviePlayerTimer.Tick -= MoviePlayerTimer_Tick;
                MoviePlayerTimer.Stop();

                #endregion
            });
        }

        #endregion

        #region Method -> PlayMovie

        /// <summary>
        /// Play the movie when buffered
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">MovieBufferedEventArgs</param>
        private void PlayMovie(double currentMoviePlayingProgressValue,string pathToFile)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                #region Dispatcher Timer

                MoviePlayerTimer.Tick += MoviePlayerTimer_Tick;
                MoviePlayerTimer.Start();

                #endregion

                Player.Source = new Uri(pathToFile);
                Player.Position = TimeSpan.FromSeconds(currentMoviePlayingProgressValue);
                Player.Play();
                Player.StretchDirection = StretchDirection.Both;

                MoviePlayerIsPlaying = true;
            });
        }

        #endregion

        #region Method -> OnWindowSizeChanged

        /// <summary>
        /// Subscribes to events when window is loaded
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">RoutedEventArgs</param>
        private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.NewWindowState == WindowState.Maximized && Player != null && Player.Source != null)
            {
                //Player.Stretch = Stretch.Fill;
            }
            else if (e.NewWindowState != WindowState.Maximized && Player != null && Player.Source != null)
            {
                Player.Stretch = Stretch.Uniform;
            }
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
    }
}
