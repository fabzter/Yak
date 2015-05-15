using System;

namespace Yak.Events
{
    /// <summary>
    /// NumberOfColumnChangedEventArgs
    /// </summary>
    public class NumberOfColumnChangedEventArgs : EventArgs
    {
        #region Constructor
        public NumberOfColumnChangedEventArgs(int numberOfColumns)
        {
            NumberOfColumns = numberOfColumns;
        }
        #endregion

        #region Properties

        #region Property -> NumberOfColumns
        public int NumberOfColumns { get; private set; }
        #endregion

        #endregion
    }
}
