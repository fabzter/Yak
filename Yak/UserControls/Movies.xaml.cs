using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Yak.ViewModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;
using Yak.CustomPanels;
using Yak.Events;
using System.Windows.Controls.Primitives;
using MahApps.Metro.Controls;
using Microsoft.Practices.ServiceLocation;

namespace Yak.UserControls
{
    /// <summary>
    /// Interaction logic for Movies.xaml
    /// </summary>
    public partial class Movies : UserControl
    {
        #region Constructor
        public Movies()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                await OnMoviesUcLoaded(s, e);
            };
        }
        #endregion

        #region Methods

        #region Method -> OnMoviesUcLoaded
        /// <summary>
        /// Subscribe to events and load first page
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">RoutedEventArgs</param>
        private async Task OnMoviesUcLoaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MoviesViewModel;
            if (vm != null)
            {
                if (!vm.Movies.Any())
                {
                    // At first load, we load the first page of movies
                    await vm.LoadNextPage();
                }
            }
        }
        #endregion

        #region Method -> ScrollViewer_ScrollChanged
        /// <summary>
        /// Decide if we have to load previous or next page regarding the scroll position
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
                vm.MaxMoviesPerPage = e.NumberOfColumns * Helpers.Constants.NumberOfRowsPerPage;
            }
        }
        #endregion

        #endregion
    }
}
