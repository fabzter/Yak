﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Yak.Converters
{
    /// <summary>
    /// Convert from rating string ("0" to "10") to an double (0.0 to 5.0)
    /// </summary>
    public class StringRatingToIntRatingConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Convert rating string ("0" to "10") to a double (0.0 to 5.0)
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Formated rating double</returns>
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            double result = 0.0;
            string rating = value as string;

            if (rating != null)
            {
                try
                {
                    result = System.Convert.ToDouble(rating, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    return result;
                }

                if (!double.Equals(result, 0.0))
                {
                    result = result / 2.0;
                    result = Math.Round(result);
                }
            }

            return result;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
