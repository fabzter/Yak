using System;
using System.Windows;
using Yak.ViewModel;

namespace Yak.UserControls
{
    /// <summary>
    /// Interaction logic for FullScreenMediaPlayer.xaml
    /// </summary>
    public partial class FullScreenMediaPlayer : IDisposable
    {
        private bool _disposed;

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the FullScreenMediaPlayer class.
        /// </summary>
        public FullScreenMediaPlayer()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var mediaPlayerViewModel = DataContext as MediaPlayerViewModel;
                if (mediaPlayerViewModel != null)
                {
                    mediaPlayerViewModel.BackToNormalScreenChanged += OnBackToNormalScreenChanged;
                }

                Window.GetWindow(this).Closing += (s1, e1) => Dispose();
            };

            Unloaded += (s, e) =>
            {
                var mediaPlayerViewModel = DataContext as MediaPlayerViewModel;
                if (mediaPlayerViewModel != null)
                {
                    mediaPlayerViewModel.BackToNormalScreenChanged -= OnBackToNormalScreenChanged;
                }
            };

        }
        #endregion

        #region Methods

        #region Method -> Onloaded
        /// <summary>
        /// Close and dispose this player when back to normal screen
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="toggleFullScreenEventArgs">EventArgs</param>
        private void OnBackToNormalScreenChanged(object sender, EventArgs toggleFullScreenEventArgs)
        {
            Dispose();
        }
        #endregion

        #region Method -> Launch
        /// <summary>
        /// Open the FullScreen media player
        /// </summary>
        public void Launch()
        {
            Owner = Application.Current.MainWindow;
            UseNoneWindowStyle = true;
            WindowState = WindowState.Maximized;
            Show();
        }
        #endregion

        #endregion

        #region Dispose
        /// <summary>
        /// Dispose all resources and close the UserControl
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                PlayerUc.Dispose();

                DataContext = null;

                _disposed = true;

                if (disposing)
                {
                    GC.SuppressFinalize(this);
                    Close();
                }
            }
        }
        #endregion
    }
}
