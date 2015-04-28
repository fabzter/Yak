using System;
using System.Windows;

namespace Yak.Events
{
    /// <summary>
    /// WindowSizeChangedEventArgs
    /// </summary>
    public class WindowSizeChangedEventArgs : EventArgs
    {
        private readonly WindowState _newWindowState;

        #region Constructor
        public WindowSizeChangedEventArgs(WindowState newWindowState)
        {
            _newWindowState = newWindowState;
        }
        #endregion

        #region Properties

        #region Property -> NewWindowState
        public WindowState NewWindowState
        {
            get
            {
                return _newWindowState;
            }
        }
        #endregion

        #endregion
    }
}
