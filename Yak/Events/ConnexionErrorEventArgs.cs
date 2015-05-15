using System;

namespace Yak.Events
{
    /// <summary>
    /// ConnectionErrorEventArgs
    /// </summary>
    public class ConnectionErrorEventArgs : EventArgs
    {

        #region Constructor
        public ConnectionErrorEventArgs(bool isInError)
        {
            IsInError = isInError;
        }
        #endregion

        #region Properties

        #region Property -> IsInError
        public bool IsInError { get; private set; }
        #endregion

        #endregion
    }
}
