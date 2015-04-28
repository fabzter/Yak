using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Yak.Events;
using Yak.Messaging;
using Yak.Model.Movie;

namespace Yak.ViewModel
{
    public class MoviePlayerViewModel : ViewModelBase
    {
        #region Property -> TabName
        /// <summary>
        /// Name of the tab to be displayed in the interface
        /// </summary>
        public string TabName { get; set; }
        #endregion

        #region Property -> SearchMoviesFilter
        /// <summary>
        /// Useless search filter (defined here to avoid binding error in MainWindow.xaml with TabControl)
        /// </summary>
        public string SearchMoviesFilter { get; set; }
        #endregion

        #region Property -> Movie
        /// <summary>
        /// The movie to be played
        /// </summary>
        public MovieFullDetails Movie { get; set; }
        #endregion

        #region Property -> MovieFilePath
        /// <summary>
        /// File path to the movie to be played
        /// </summary>
        public string MovieFilePath { get; set; }
        #endregion

        #region Property -> CurrentMovieProgressValue
        /// <summary>
        /// The current progress playing value
        /// </summary>
        public double CurrentMovieProgressValue { get; set; }
        #endregion

        #region Constructor -> MoviePlayerViewModel
        /// <summary>
        /// Initializes a new instance of the MoviePlayerViewModel class.
        /// </summary>
        public MoviePlayerViewModel()   
            : this(null, null)
        {

        }
        #endregion

        #region Constructor -> MoviePlayerViewModel
        /// <summary>
        /// Initializes a new instance of the MoviePlayerViewModel class.
        /// </summary>
        public MoviePlayerViewModel(MovieFullDetails movie, string movieFilePath)
        {
            Movie = movie;
            MovieFilePath = movieFilePath;
            Messenger.Default.Register<WindowSizeChangedMessage>(this, e => OnWindowSizeChanged(new WindowSizeChangedEventArgs(e.NewWindowState)));
            Messenger.Default.Register<StopDownloadingMovieMessage>(this, e => OnStoppedDownloadingMovie(new EventArgs()));
        }
        #endregion

        #region Events

        #region Event -> OnStoppedDownloadingMovie
        /// <summary>
        /// StoppedDownloadingMovie event
        /// </summary>
        public event EventHandler<EventArgs> StoppedDownloadingMovie;
        /// <summary>
        /// When movie is stopped downloading
        /// </summary>
        ///<param name="e">EventArgs parameter</param>
        protected virtual void OnStoppedDownloadingMovie(EventArgs e)
        {
            EventHandler<EventArgs> handler = StoppedDownloadingMovie;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnWindowSizeChanged
        /// <summary>
        /// WindowSizeChanged event
        /// </summary>
        public event EventHandler<WindowSizeChangedEventArgs> WindowSizeChanged;
        /// <summary>
        /// When windows size has changed
        /// </summary>
        ///<param name="e">EventArgs parameter</param>
        protected virtual void OnWindowSizeChanged(WindowSizeChangedEventArgs e)
        {
            EventHandler<WindowSizeChangedEventArgs> handler = WindowSizeChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #endregion
    }
}
