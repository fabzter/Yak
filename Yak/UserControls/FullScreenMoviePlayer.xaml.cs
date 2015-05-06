using System;
using System.Windows;
using System.Windows.Controls;
using Yak.ViewModel;

namespace Yak.UserControls
{
    /// <summary>
    /// Interaction logic for FullScreenMoviePlayer.xaml
    /// </summary>
    public partial class FullScreenMoviePlayer : IDisposable
    {
        private bool _disposed;

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the FullScreenMoviePlayer class.
        /// </summary>
        public FullScreenMoviePlayer()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var moviePlayerViewModel = DataContext as MoviePlayerViewModel;
                if (moviePlayerViewModel != null)
                {
                    moviePlayerViewModel.BackToNormalScreenChanged += OnBackToNormalScreenChanged;
                }
            };

            Unloaded += (s, e) =>
            {
                var moviePlayerViewModel = DataContext as MoviePlayerViewModel;
                if (moviePlayerViewModel != null)
                {
                    moviePlayerViewModel.BackToNormalScreenChanged -= OnBackToNormalScreenChanged;
                }
            };

        }
        #endregion

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
        /// Open the FullScreen movie player
        /// </summary>
        public void Launch()
        {
            Owner = Application.Current.MainWindow;
            UseNoneWindowStyle = true;
            WindowState = WindowState.Maximized;
            Show();
        }
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
