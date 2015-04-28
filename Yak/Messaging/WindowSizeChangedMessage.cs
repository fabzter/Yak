using System.Windows;
using GalaSoft.MvvmLight.Messaging;

namespace Yak
{
    public class WindowSizeChangedMessage : MessageBase
    {
        public WindowState NewWindowState { get; private set; }

        public WindowSizeChangedMessage(WindowState newWindowState)
        {
            NewWindowState = newWindowState;
        }
    }
}
