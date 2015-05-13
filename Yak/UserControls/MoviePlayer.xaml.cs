using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using NuGet;
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

        #region DependencyProperty -> MoviePlayerIsPlayingProperty
        /// <summary>
        /// Identifies the <see cref="MoviePlayerIsPlaying"/> dependency property. 
        /// </summary>
        internal static readonly DependencyProperty MoviePlayerIsPlayingProperty = DependencyProperty.Register("MoviePlayerIsPlaying", typeof(bool), typeof(MoviePlayer), new PropertyMetadata(false, null));
        #endregion

        #region Property -> MoviePlayerIsPlaying
        /// <summary>
        /// Indicates if a movie is playing 
        /// </summary>
        private bool MoviePlayerIsPlaying
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

        #region DependencyProperty -> VolumeProperty
        /// <summary>
        /// Identifies the <see cref="MoviePlayerIsPlaying"/> dependency property. 
        /// </summary>
        internal static readonly DependencyProperty VolumeProperty = DependencyProperty.Register("Volume", typeof(int), typeof(MoviePlayer), new PropertyMetadata(100, new PropertyChangedCallback(OnVolumeChanged)));
        #endregion

        #region Property -> Volume
        /// <summary>
        /// Get or set the movie volume 
        /// </summary>
        public int Volume
        {
            get
            {
                return (int)GetValue(VolumeProperty);
            }

            set
            {
                SetValue(VolumeProperty, value);
            }
        }
        #endregion

        #region Property -> ActivityMouse
        /// <summary>
        /// Used to update the activity mouse and mouse position.
        /// Used by <see cref="OnActivity"/> and <see cref="OnInactivity"/> to update PlayerStatusBar visibility.
        /// </summary>
        private DispatcherTimer _activityTimer;
        private Point _inactiveMousePosition = new Point(0, 0);
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
                // start the timer used to report time on MoviePlayerSliderProgress
                MoviePlayerTimer = new DispatcherTimer{ Interval = TimeSpan.FromSeconds(1) };
                MoviePlayerTimer.Tick += MoviePlayerTimer_Tick;
                MoviePlayerTimer.Start();

                // start the activity timer used to manage visibility of the PlayerStatusBar
                InputManager.Current.PreProcessInput += OnActivity;
                _activityTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                _activityTimer.Tick += OnInactivity;

                vm.StoppedDownloadingMovie += OnStoppedDownloadingMovie;

                if (!vm.IsInFullScreenMode)
                {
                    vm.ToggleFullScreenChanged += OnToggleFullScreen;
                }

                vm.BackToNormalScreenChanged += OnBackToNormalScreen;

                Window.GetWindow(this).Closing += (s1, e1) => Dispose();

                // Set the Vlc lib directory used by MediaPlayer
                Player.MediaPlayer.VlcLibDirectory = Helpers.Constants.GetVlcLibDirectory();

                if (vm.MovieUri != null)
                {
                    Player.MediaPlayer.SetMedia(vm.MovieUri);
                    PlayMovie(vm.CurrentMovieProgress);
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
                else if (MoviePlayerIsPlaying && Player != null && Player.MediaPlayer != null)
                {
                    Player.MediaPlayer.Pause();
                }
            }
        }
        #endregion

        #region Method -> OnVolumeChanged
        /// <summary>
        /// When media's volume changed, update volume for all MediaPlayer instances (normal screen and fullscreen)
        /// </summary>
        /// <param name="e">e</param>
        /// <param name="obj">obj</param>
        private static void OnVolumeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MoviePlayer moviePlayer = obj as MoviePlayer;
            if (moviePlayer != null)
            {
                int newVolume = (int)e.NewValue;
                moviePlayer.ChangeMediaVolume(newVolume);
                MoviePlayerViewModel vm = moviePlayer.DataContext as MoviePlayerViewModel;
                if (vm != null)
                {
                    vm.MediaVolume = newVolume;
                }
            }
        }
        #endregion

        #region Method -> ChangeMediaVolume
        /// <summary>
        /// Change the media's volume
        /// </summary>
        /// <param name="newValue">New volume value</param>
        private void ChangeMediaVolume(int newValue)
        {
            Player.MediaPlayer.Audio.Volume = newValue;
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
                if (Player != null && Player.MediaPlayer != null && MoviePlayerIsPlaying)
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
        private void PlayMovie(double currentMoviePlayingProgressValue)
        {
            Player.MediaPlayer.Play();
            Player.MediaPlayer.Time =
                Convert.ToInt64(TimeSpan.FromSeconds(currentMoviePlayingProgressValue).TotalMilliseconds);

            MoviePlayerIsPlaying = true;
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
                MoviePlayerSliderProgress.Maximum = TimeSpan.FromMilliseconds(Player.MediaPlayer.Length).TotalSeconds;
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
                vm.CurrentMovieProgress = MoviePlayerSliderProgress.Value;
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
            if ((Volume <= 190 && e.Delta > 0) || (Volume >= 10 && e.Delta < 0))
            {
                Volume += (e.Delta > 0) ? 10 : -10;
            }
        }
        #endregion

        #region Method -> OnToggleFullScreen
        /// <summary>
        /// When got fullscreen, pause current movie player instance, open a new one in fullscreen then play movie inside
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
                    fullScreenMoviePlayer.PlayerUc.Volume = moviePlayerViewModel.MediaVolume;
                    IsInFullScreen = true;
                }
            }
        }
        #endregion

        #region Method -> OnBackToNormalScreen
        /// <summary>
        /// When back to normal screen size has been requested, update volume and play movie
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnBackToNormalScreen(object sender, EventArgs e)
        {
            var moviePlayerViewModel = DataContext as MoviePlayerViewModel;
            if (moviePlayerViewModel != null)
            {
                if (Player != null && IsInFullScreen)
                {
                    Volume = moviePlayerViewModel.MediaVolume;
                    PlayMovie(moviePlayerViewModel.CurrentMovieProgress);
                    IsInFullScreen = false;
                }
            }
        }
        #endregion

        #region Method -> OnInactivity
        /// <summary>
        /// Hide the PlayerStatusBar on mouse inactivity 
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        void OnInactivity(object sender, EventArgs e)
        {
            // remember mouse position
            _inactiveMousePosition = Mouse.GetPosition(MoviePlayerSliderProgress);

            if (PlayerStatusBar.Opacity.Equals(1.0))
            {
                // set UI on inactivity
                #region Fade in PlayerStatusBar opacity

                DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
                opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                PowerEase opacityEasingFunction = new PowerEase();
                opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
                EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(0));
                EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(0.0, KeyTime.FromPercent(1.0),
                    opacityEasingFunction);
                opacityAnimation.KeyFrames.Add(startOpacityEasing);
                opacityAnimation.KeyFrames.Add(endOpacityEasing);

                PlayerStatusBar.BeginAnimation(OpacityProperty, opacityAnimation);

                Task.Delay(500).ContinueWith(_ =>
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        PlayerStatusBar.Visibility = Visibility.Collapsed;
                    });
                });
                #endregion
            }
        }
        #endregion

        #region Method -> OnActivity
        /// <summary>
        /// Show the PlayerStatusBar on mouse activity 
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        void OnActivity(object sender, PreProcessInputEventArgs e)
        {
            InputEventArgs inputEventArgs = e.StagingItem.Input;

            if (inputEventArgs is MouseEventArgs || inputEventArgs is KeyboardEventArgs)
            {
                if (e.StagingItem.Input is MouseEventArgs)
                {
                    MouseEventArgs mouseEventArgs = (MouseEventArgs)e.StagingItem.Input;

                    // no button is pressed and the position is still the same as the application became inactive
                    if (mouseEventArgs.LeftButton == MouseButtonState.Released &&
                        mouseEventArgs.RightButton == MouseButtonState.Released &&
                        mouseEventArgs.MiddleButton == MouseButtonState.Released &&
                        mouseEventArgs.XButton1 == MouseButtonState.Released &&
                        mouseEventArgs.XButton2 == MouseButtonState.Released &&
                        _inactiveMousePosition == mouseEventArgs.GetPosition(MoviePlayerSliderProgress))
                        return;
                }

                if (PlayerStatusBar.Opacity.Equals(0.0))
                {
                    // set UI on activity
                    #region Fade out PlayerStatusBar opacity

                    DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
                    opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                    PowerEase opacityEasingFunction = new PowerEase();
                    opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
                    EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(0.0, KeyTime.FromPercent(0));
                    EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0),
                        opacityEasingFunction);
                    opacityAnimation.KeyFrames.Add(startOpacityEasing);
                    opacityAnimation.KeyFrames.Add(endOpacityEasing);

                    PlayerStatusBar.BeginAnimation(OpacityProperty, opacityAnimation);

                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        PlayerStatusBar.Visibility = Visibility.Visible;
                    });

                    #endregion
                }

                _activityTimer.Stop();
                _activityTimer.Start();
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

                MoviePlayerTimer.Tick -= MoviePlayerTimer_Tick;
                MoviePlayerTimer.Stop();

                if (Player != null && Player.MediaPlayer != null)
                {
                    Player.MediaPlayer.Stop();
                    Player.MediaPlayer.Dispose();
                    MoviePlayerIsPlaying = false;
                }

                var vm = DataContext as MoviePlayerViewModel;
                if (vm != null)
                {
                    if (vm.DeleteMovieFilesAction != null)
                    {
                        vm.DeleteMovieFilesAction(true);
                    }
                    vm.StoppedDownloadingMovie -= OnStoppedDownloadingMovie;
                    vm.ToggleFullScreenChanged -= OnToggleFullScreen;
                    vm.BackToNormalScreenChanged -= OnBackToNormalScreen;
                }

                DataContext = null;
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