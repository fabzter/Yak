using System;
using GalaSoft.MvvmLight.Messaging;

namespace Yak.Messaging
{
    /// <summary>
    /// MovieBufferedMessage
    /// </summary>
    public class MovieBufferedMessage : MessageBase
    {
        private readonly string _pathToFile;

        #region Constructor
        public MovieBufferedMessage(string pathToFile)
        {
            _pathToFile = pathToFile;
        }
        #endregion

        #region Properties

        #region Property -> PathToFile
        public string PathToFile
        {
            get
            {
                return _pathToFile;
            }
        }
        #endregion

        #endregion
    }
}
