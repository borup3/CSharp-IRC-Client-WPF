using System;

namespace CodeCafeIRC.irc
{
    static class IrcCommands
    {
        public delegate void OutgoingEventHandler(OutgoingEventArgs args);
        public static event OutgoingEventHandler OutgoingCommand = delegate { };

        public static string Pass(string password)
        {
            string outgoing = "PASS " + password;
            OutgoingCommand(new OutgoingEventArgs(outgoing));
            return outgoing + "\n";
        }
        
        public static string Nickname(string nickname)
        {
            string outgoing = "NICK " + nickname;
            OutgoingCommand(new OutgoingEventArgs(outgoing));
            return outgoing + "\n";
        }

        public static string UserInfo(string username, string realname, IrcUserFlags mode = IrcUserFlags.NORMAL)
        {
            string outgoing = "USER " + username + " " + (byte)mode + " * :\"" + realname + "\"";
            OutgoingCommand(new OutgoingEventArgs(outgoing));
            return outgoing + "\n";
        }

        public static string Join(string channel)
        {
            string outgoing = "JOIN " + channel;
            OutgoingCommand(new OutgoingEventArgs(outgoing));
            return outgoing + "\n";
        }

        public static string Leave(string channel)
        {
            string outgoing = "PART " + channel;
            OutgoingCommand(new OutgoingEventArgs(outgoing));
            return outgoing + "\n";
        }

        public static string Say(string channel, string message)
        {
            string outgoing = "PRIVMSG " + channel + " :" + message;
            OutgoingCommand(new OutgoingEventArgs(outgoing));
            return outgoing + "\n";
        }

        public static string PrivateMessage(string user, string message)
        {
            string outgoing = "PRIVMSG " + user + " :" + message;
            OutgoingCommand(new OutgoingEventArgs(outgoing));
            return outgoing + "\n";
        }

        public static string LeaveAllJoinedChannels()
        {
            string outgoing = "JOIN 0";
            OutgoingCommand(new OutgoingEventArgs(outgoing));
            return outgoing + "\n";
        }

        public static string Quit(string message)
        {
            string outgoing = "QUIT :" + message;
            OutgoingCommand(new OutgoingEventArgs(outgoing));
            return outgoing + "\n";
        }

        public static string Ping(string server)
        {
            string outgoing = "PING " + server;
            OutgoingCommand(new OutgoingEventArgs(outgoing));
            return outgoing + "\n";
        }

        public static string Pong(string server)
        {
            string outgoing = "PONG " + server;
            OutgoingCommand(new OutgoingEventArgs(outgoing));
            return outgoing + "\n";
        }
    }

    public class OutgoingEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public OutgoingEventArgs(string message)
        {
            Message = message;
        }
    }
}
