﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Yak.ViewModel;
using Yak.CustomPanels;
using Yak.Events;
using Yak.Helpers;

namespace Yak.UserControls
{
    /// <summary>
    /// Interaction logic for Movies.xaml
    /// </summary>
    public partial class Movies : UserControl
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the Movies class.
        /// </summary>
        public Movies()
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                var vm = DataContext as MoviesViewModel;
                if (vm != null)
                {
                    // Unsubscribe previous events if any
                    vm.MoviesLoaded -= OnMoviesLoaded;
                    vm.MoviesLoading -= OnMoviesLoading;

                    // Subscribe events
                    vm.MoviesLoaded += OnMoviesLoaded;
                    vm.MoviesLoading += OnMoviesLoading;

                    // At first load, we load the first page of movies
                    if (!vm.Movies.Any() && !vm.Tab.Type.Equals(TabDescription.TabType.Search))
                    {
                        await vm.LoadNextPage();
                    }
                }
            };
        }
        #endregion

        #region Methods

        #region Method -> OnMoviesLoading
        /// <summary>
        /// Fade in opacity of the UserControl, let the progress ring appear and collapse the NoMouvieFound label when loading movies
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnMoviesLoading(object sender, EventArgs e)
        {
            ProgressRing.IsActive = true;

            #region Fade in opacity

            DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            PowerEase opacityEasingFunction = new PowerEase();
            opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
            EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(1, KeyTime.FromPercent(0));
            EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(0.2, KeyTime.FromPercent(1.0),
                opacityEasingFunction);
            opacityAnimation.KeyFrames.Add(startOpacityEasing);
            opacityAnimation.KeyFrames.Add(endOpacityEasing);
            ItemsList.BeginAnimation(OpacityProperty, opacityAnimation);

            #endregion

            if (NoMouvieFound.Visibility != Visibility.Collapsed)
            {
                NoMouvieFound.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Method -> OnMoviesLoaded
        /// <summary>
        /// Fade out opacity of the window, let the progress ring disappear and manage visibility of NoMouvieFound label when movies has been loaded
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnMoviesLoaded(object sender, NumberOfLoadedMoviesEventArgs e)
        {
            // We exclude exceptions like TaskCancelled when getting back results
            if ((e.NumberOfMovies == 0 && !e.IsUnhandledException) || (e.NumberOfMovies != 0 && !e.IsUnhandledException))
            {
                ProgressRing.IsActive = false;

                #region Fade out opacity

                DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
                opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                PowerEase opacityEasingFunction = new PowerEase();
                opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
                EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(0.2, KeyTime.FromPercent(0));
                EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(1, KeyTime.FromPercent(1.0),
                    opacityEasingFunction);
                opacityAnimation.KeyFrames.Add(startOpacityEasing);
                opacityAnimation.KeyFrames.Add(endOpacityEasing);
                ItemsList.BeginAnimation(OpacityProperty, opacityAnimation);

                #endregion
            }

            var vm = DataContext as MoviesViewModel;
            if (vm != null)
            {
                // If we searched movies and there's no result, display the NoMovieFound label (we exclude exceptions like TaskCancelled when getting back results)
                if (!vm.Movies.Any() && e.NumberOfMovies == 0 && !e.IsUnhandledException)
                {
                    NoMouvieFound.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

        #region Method -> ScrollViewer_ScrollChanged
        /// <summary>
        /// Decide if we have to load previous or next page regarding to the scroll position
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">ScrollChangedEventArgs</param>
        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double totalHeight = e.VerticalOffset + e.ViewportHeight;

            if (totalHeight.Equals(e.ExtentHeight))
            {
                // We are at the bottom position of the window: we have to load the next movies
                var vm = DataContext as MoviesViewModel;

                // We check if no loading is occuring
                if (vm != null && !ProgressRing.IsActive)
                {
                    // We check if the searching form is empty or not
                    if (String.IsNullOrEmpty(vm.SearchMoviesFilter))
                    {
                        await vm.LoadNextPage();
                    }
                    else
                    {
                        await vm.LoadNextPage(vm.SearchMoviesFilter);
                    }
                }
            }
        }
        #endregion

        #region Method -> ElasticWrapPanel_Loaded
        /// <summary>
        /// Subscribe NumberOfColumnsChanged to the NumberOfColumnsChanged event of the ElasticWrapPanel
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">RoutedEventArgs</param>
        private void ElasticWrapPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var elasticWrapPanel = sender as ElasticWrapPanel;
            if (elasticWrapPanel != null)
            {
                elasticWrapPanel.NumberOfColumnsChanged += NumberOfColumnsChanged;
            }
        }
        #endregion

        #region Method -> NumberOfColumnsChanged
        /// <summary>
        /// When the column's number of the ElasticWrapPanel has changed, reset the MaxMoviesPerPage property to a value so that there's enough content to be able to scroll
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">NumberOfColumnChangedEventArgs</param>
        private void NumberOfColumnsChanged(object sender, NumberOfColumnChangedEventArgs e)
        {
            var vm = DataContext as MoviesViewModel;
            if (vm != null)
            {
                vm.MaxMoviesPerPage = e.NumberOfColumns * Constants.NumberOfRowsPerPage;
            }
        }
        #endregion

        #endregion
    }
}
