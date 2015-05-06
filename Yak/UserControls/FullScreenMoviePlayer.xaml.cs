using System;
using System.Windows;
using Yak.ViewModel;

namespace Yak.UserControls
{
    public partial class FullScreenMoviePlayer : IDisposable
    {
        private bool _disposed;

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

        private void OnBackToNormalScreenChanged(object sender, EventArgs toggleFullScreenEventArgs)
        {
            Dispose();
        }

        public void Launch()
        {
            Owner = Application.Current.MainWindow;
            UseNoneWindowStyle = true;
            WindowState = WindowState.Maximized;
            Show();
        }

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
    }
}
