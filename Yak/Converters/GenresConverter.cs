﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Yak.Converters
{
    /// <summary>
    /// Format string genres to add "/" character between each genre
    /// </summary>
    public class GenresConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Used to add "/" character at the end of each genre
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Formated string</returns>
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var result = string.Empty;

            var genres = value as List<string>;
            if (genres == null)
            {
                return result;
            }

            var index = 0;
            foreach (var genre in genres)
            {
                index++;

                result += genre;
                // Add the slash at the end of each genre.
                if (index != genres.Count())
                {
                    result += ", ";
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
