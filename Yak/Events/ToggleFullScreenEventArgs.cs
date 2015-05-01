using System;

namespace Yak.Events
{
    /// <summary>
    /// ToggleFullScreenEventArgs
    /// </summary>
    public class ToggleFullScreenEventArgs : EventArgs
    {
        private readonly bool _fullScreenRequested;

        #region Constructor
        public ToggleFullScreenEventArgs(bool fullScreenRequested)
        {
            _fullScreenRequested = fullScreenRequested;
        }
        #endregion

        #region Properties

        #region Property -> FullScreenRequested
        public bool FullScreenRequested
        {
            get
            {
                return _fullScreenRequested;
            }
        }
        #endregion

        #endregion
    }
}
