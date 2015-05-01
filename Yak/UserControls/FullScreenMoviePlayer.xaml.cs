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
            PlayerUc.Dispose();
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
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
                Close();
            }
        }
    }
}
