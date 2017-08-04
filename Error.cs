using System.IO;
using CodeCafeIRC.irc;
using CodeCafeIRC.messaging;

namespace CodeCafeIRC
{
    public static class Error
    {
        public static void Write(string error)
        {
            MainWindow.Instance.Main.AddMessage(new ErrorMessage(error));
        }

        public static void LogFromThread(IrcClient ircClient, ErrorEventArgs errorEventArgs)
        {
            MainWindow.Instance.Main.AddMessage(new ErrorMessage(errorEventArgs.GetException().Message));
        }
    }
}
