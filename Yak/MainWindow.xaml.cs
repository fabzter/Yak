using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Yak.Events;
using Yak.ViewModel;
using GalaSoft.MvvmLight.Threading;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;

namespace Yak
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        #region Properties

        #region Property -> MoviePlayerIsPlaying
        private bool MoviePlayerIsPlaying;
        #endregion

        #region Property -> UserIsDraggingMoviePlayerSlider
        private bool UserIsDraggingMoviePlayerSlider;
        #endregion

        #region Property -> MoviePlayerTimer
        private DispatcherTimer MoviePlayerTimer;
        #endregion

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            // Action when window is about to close
            Closing += (s, e) =>
            {
                // Unsubscribe events
                Loaded -= MainWindow_Loaded;

                // Stop playing and downloading a movie if any
                var vm = DataContext as MainViewModel;
                if (vm != null)
                {
                    if (vm.IsDownloadingMovie)
                    {
                        MoviePlayer.Stop();
                        MoviePlayer.Close();
                        MoviePlayer.Source = null;
                        vm.StopDownloadingMovie();
                    }

                    // Unsubscribe events
                    vm.ConnectionError -= OnConnectionInError;
                    vm.DownloadingMovie -= OnDownloadingMovie;
                    vm.StoppedDownloadingMovie -= OnStoppedDownloadingMovie;
                    vm.LoadedMovie -= OnLoadedMovie;
                    vm.BufferedMovie -= OnBufferedMovie;
                    vm.LoadingMovieProgress -= OnLoadingMovieProgress;
                }

                ViewModelLocator.Cleanup();
            };
        }
        #endregion

        #region Methods

        #region Method -> MainWindow_Loaded
        /// <summary>
        /// Subscribes to events when window is loaded
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">RoutedEventArgs</param>
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm != null)
            {
                vm.ConnectionError += OnConnectionInError;
                vm.DownloadingMovie += OnDownloadingMovie;
                vm.StoppedDownloadingMovie += OnStoppedDownloadingMovie;
                vm.LoadedMovie += OnLoadedMovie;
                vm.LoadingMovie += OnLoadingMovie;
                vm.BufferedMovie += OnBufferedMovie;
                vm.LoadingMovieProgress += OnLoadingMovieProgress;
                vm.LoadedTrailer += OnLoadedTrailer;
            }

            MoviePlayerTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            MouseDoubleClick += (a, b) =>
            {
                if (WindowState == WindowState.Maximized && MoviePlayer != null && MoviePlayer.Source != null)
                {
                    MoviePlayer.Stretch = Stretch.Fill;
                }
                else if (WindowState != WindowState.Maximized && MoviePlayer != null && MoviePlayer.Source != null)
                {
                    MoviePlayer.Stretch = Stretch.Uniform;
                }
            };
        }

        #endregion

        #region Method -> OnLoadedTrailer
        /// <summary>
        /// Play the trailer when loaded
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">TrailerLoadedEventArgs</param>
        private void OnLoadedTrailer(object sender, TrailerLoadedEventArgs e)
        {
            if (!e.InError)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    #region Fade in content opacity
                    DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
                    opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                    PowerEase opacityEasingFunction = new PowerEase();
                    opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
                    EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(0));
                    EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(0.0, KeyTime.FromPercent(1.0),
                        opacityEasingFunction);
                    opacityAnimation.KeyFrames.Add(startOpacityEasing);
                    opacityAnimation.KeyFrames.Add(endOpacityEasing);

                    MovieContainer.BeginAnimation(OpacityProperty, opacityAnimation);
                    #endregion
                });

                TrailerPlayer.Source = new Uri(e.TrailerUrl);
                TrailerPlayer.Play();
            }
        }
        #endregion

        #region Method -> OnLoadingMovieProgress
        /// <summary>
        /// Report progress when a movie is loading and set to visible the progressbar, the cancel button and the LoadingText label
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">MovieLoadingProgressEventArgs</param>
        private void OnLoadingMovieProgress(object sender, MovieLoadingProgressEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (ProgressBar.Visibility == Visibility.Collapsed)
                {
                    ProgressBar.Visibility = Visibility.Visible;
                }

                if (StopLoadingMovieButton.Visibility == Visibility.Collapsed)
                {
                    StopLoadingMovieButton.Visibility = Visibility.Visible;
                }

                if (LoadingText.Visibility == Visibility.Collapsed)
                {
                    LoadingText.Visibility = Visibility.Visible;
                }

                ProgressBar.Value = e.Progress;

                // The percentage here is related to the buffering progress
                double percentage = e.Progress/Helpers.Constants.MinimumBufferingBeforeMoviePlaying*100.0;

                if (percentage >= 100)
                {
                    percentage = 100;
                }

                if (e.DownloadRate >= 1000)
                {
                    LoadingText.Text = "Buffering : " + Math.Round(percentage, 0) + "%" + " ( " + e.DownloadRate / 1000 + " MB/s)";
                }
                else
                {
                    LoadingText.Text = "Buffering : " + Math.Round(percentage, 0) + "%" + " ( " + e.DownloadRate + " kB/s)";
                }
            });
        }
        #endregion

        #region Method -> OnDownloadingMovie
        /// <summary>
        /// Fade in the movie page's opacity when a movie is loading
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnDownloadingMovie(object sender, EventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                #region Fade in opacity
                DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
                opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                PowerEase opacityEasingFunction = new PowerEase();
                opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
                EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(0));
                EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(0.0, KeyTime.FromPercent(1.0),
                    opacityEasingFunction);
                opacityAnimation.KeyFrames.Add(startOpacityEasing);
                opacityAnimation.KeyFrames.Add(endOpacityEasing);

                Content.BeginAnimation(OpacityProperty, opacityAnimation);
                #endregion
            });
        }
        #endregion

        #region Method -> OnLoadingMovie
        /// <summary>
        /// Open the movie flyout when a movie is selected from the main interface, show progress and hide content
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        void OnLoadingMovie(object sender, EventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                MoviePage.IsOpen = true;

                MovieProgressBar.Visibility = Visibility.Visible;
                MovieContainer.Visibility = Visibility.Collapsed;

                MovieContainer.Opacity = 0.0;
            });
        }
        #endregion

        #region Method -> OnLoadedMovie
        /// <summary>
        /// Show content and hide progress when a movie is loaded
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        void OnLoadedMovie(object sender, EventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                MovieProgressBar.Visibility = Visibility.Collapsed;
                MovieContainer.Visibility = Visibility.Visible;

                #region Fade in opacity

                DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
                opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                PowerEase opacityEasingFunction = new PowerEase();
                opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
                EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(0.0, KeyTime.FromPercent(0));
                EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0),
                    opacityEasingFunction);
                opacityAnimation.KeyFrames.Add(startOpacityEasing);
                opacityAnimation.KeyFrames.Add(endOpacityEasing);

                MovieContainer.BeginAnimation(OpacityProperty, opacityAnimation);

                #endregion
            });
        }
        #endregion

        #region Method -> OnBufferedMovie
        /// <summary>
        /// Play the movie when buffered
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">MovieBufferedEventArgs</param>
        private void OnBufferedMovie(object sender, MovieBufferedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                #region Dispatcher Timer

                MoviePlayerTimer.Tick += MoviePlayerTimer_Tick;
                MoviePlayerTimer.Start();

                #endregion

                // Open the player and play the movie
                MoviePlayerFlyout.IsOpen = true;
                MoviePlayer.Source = new Uri(e.PathToFile);
                MoviePlayer.Play();
                MoviePlayer.StretchDirection = StretchDirection.Both;

                MoviePlayerIsPlaying = true;

                ProgressBar.Visibility = Visibility.Collapsed;
                StopLoadingMovieButton.Visibility = Visibility.Collapsed;
                LoadingText.Visibility = Visibility.Collapsed;
            });
        }
        #endregion

        #region Method -> OnConnectionInError
        /// <summary>
        /// Open the popup when a connection error has occured
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">MovieBufferedEventArgs</param>
        private async void OnConnectionInError(object sender, EventArgs e)
        {
            // Set and open a MetroDialog to inform that a connection error occured
            MetroDialogSettings settings = new MetroDialogSettings();
            settings.ColorScheme = MetroDialogColorScheme.Theme;
            MessageDialogResult result = await
                this.ShowMessageAsync("Internet connection error.",
                    "You seem to have an internet connection error. Please retry.",
                    MessageDialogStyle.Affirmative, settings);

            if (MoviesUc.ProgressRing.IsActive)
            {
                MoviesUc.ProgressRing.IsActive = false;
            }

            // Catch the response's user (when clicked OK)
            if (result == MessageDialogResult.Affirmative)
            {
                // Close the movie page
                if (MoviePage.IsOpen)
                {
                    MoviePage.IsOpen = false;
                    // Hide the movies list (the connection is in error, so no movie manipulation is available)
                    MoviesUc.Opacity = 0;
                }
            }
        }
        #endregion

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
                if (MoviePlayer != null && MoviePlayer.Source != null)
                {
                    MoviePlayer.Stop();
                    MoviePlayer.Close();
                    MoviePlayer.Source = null;
                    MoviePlayerIsPlaying = false;
                }

                #region Dispatcher Timer
                MoviePlayerTimer.Tick -= MoviePlayerTimer_Tick;
                MoviePlayerTimer.Stop();
                #endregion

                ProgressBar.Visibility = Visibility.Collapsed;
                StopLoadingMovieButton.Visibility = Visibility.Collapsed;
                LoadingText.Visibility = Visibility.Collapsed;
                ProgressBar.Value = 0.0;

                #region Fade out opacity

                DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
                opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                PowerEase opacityEasingFunction = new PowerEase();
                opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
                EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(0.0, KeyTime.FromPercent(0));
                EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0),
                    opacityEasingFunction);
                opacityAnimation.KeyFrames.Add(startOpacityEasing);
                opacityAnimation.KeyFrames.Add(endOpacityEasing);

                Content.BeginAnimation(OpacityProperty, opacityAnimation);

                #endregion
            });
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
            if ((MoviePlayer.Source != null) && (MoviePlayer.NaturalDuration.HasTimeSpan) && (!UserIsDraggingMoviePlayerSlider))
            {
                MoviePlayerSliderProgress.Minimum = 0;
                MoviePlayerSliderProgress.Maximum = MoviePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                MoviePlayerSliderProgress.Value = MoviePlayer.Position.TotalSeconds;
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
            e.CanExecute = (MoviePlayer != null) && (MoviePlayer.Source != null);
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
        #endregion

        #region Method -> MoviePlayerPlay_Executed
        /// <summary>
        /// Play the current movie
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">ExecutedRoutedEventArgs</param>
        private void MoviePlayerPlay_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoviePlayer.Play();
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
        #endregion

        #region Method -> MoviePlayerPause_Executed
        /// <summary>
        /// Pause the movie
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">CanExecuteRoutedEventArgs</param>
        private void MoviePlayerPause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoviePlayer.Pause();
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
            MoviePlayer.Position = TimeSpan.FromSeconds(MoviePlayerSliderProgress.Value);
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
            var vm = DataContext as MainViewModel;
            if (vm != null)
            {
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
            MoviePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }
        #endregion

        #endregion
    }
}