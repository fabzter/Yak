using System;
using System.Windows;
using System.Windows.Data;

namespace Yak.Converters
{
    /// <summary>
    /// Used to convert and inverse boolean to its inverse
    /// </summary>
    public class BoolToInverseBoolConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Convert bool to inverse
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
        #endregion
    }
}