using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Yak.Converters
{
    /// <summary>
    /// Used to convert raw runtime movie to minutes
    /// </summary>
    public class RuntimeConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Convert to readable runtime
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string runtime = String.Empty;

            try
            {
                double? result = System.Convert.ToDouble(value);

                if (result >= 60)
                {
                    double hours = result.Value/60.0;
                    double minutes = result.Value%60.0;

                    if (minutes < 10)
                    {
                        runtime = Math.Floor(hours) + "h" + "0" + minutes;
                    }
                    else
                    {
                        runtime = Math.Floor(hours) + "h" + minutes;
                    }
                }
            }
            catch (Exception e)
            {
                return runtime;
            }


            return runtime;
        }

        /// <summary>
        /// ConvertBack
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
        #endregion
    }
}
