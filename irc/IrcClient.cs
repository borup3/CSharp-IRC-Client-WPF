using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using CodeCafeIRC.messaging;

namespace CodeCafeIRC.irc
{
    public class IrcClient : DispatcherObject, IDisposable
    {
        private readonly object m_lock = new object();
        private Thread _readerThread;
        private TcpClient _client;
        private readonly Encoding _encoding = Encoding.Default;
        private StreamWriter _writer;
        private StreamReader _reader;

        public readonly List<IrcChannel> PendingChannels = new List<IrcChannel>();
        public readonly List<IrcChannel> Channels = new List<IrcChannel>();
        public UserCredentials Credentials { get; private set; }
        public string Server { get; private set; }
        public int Port { get; private set; }
        public bool IsConnected { get; private set; }
        
        public delegate void Closing();
        public event Closing OnClosing;

        public IrcClient(string server, int port)
        {
            Server = server;
            Port = port;
        }

        public void Connect(UserCredentials credentials)
        {
            if (Server == null)
                throw new ArgumentNullException("Server");
            if (_client != null && _client.Connected)
                throw new Exception("client already connected");

            Credentials = credentials;

            if (!General.HasInternet())
                return;

            _client = new TcpClient();
            _client.Connect(Server, Port);

            if (!_client.Connected)
                return;

            NetworkStream stream = _client.GetStream();
            _writer = new StreamWriter(stream, _encoding);
            _reader = new StreamReader(stream, _encoding);

            _readerThread = new Thread(BeginRead);
            _readerThread.Start();
        }

        public void Join(string channel)
        {
            IrcChannel ircChannel = new IrcChannel(this, Credentials, channel);
            if (!IsConnected)
                lock(m_lock) PendingChannels.Add(ircChannel);
            else
            {
                Channels.Add(ircChannel);
                ircChannel.Join();
            }
        }

        public void Send(string command)
        {
            if (!General.HasInternet())
                return;

            if (_writer == null || !_client.Connected) return;
            _writer.WriteLine(command);
            _writer.Flush();
        }

        private void Pong(string server)
        {
            if (!General.HasInternet())
                return;

            if (_writer == null || !_client.Connected) return;
            _writer.WriteLine(IrcCommands.Pong(server));
            _writer.Flush();
        }

        private void BeginRead()
        {
            while (true)
            {
                Thread.Sleep(General.HasInternet() ? 10 : 5000);

                try
                {
                    if (!General.HasInternet())
                    {
                        // wait for internet to come back
                        continue;
                    }

                    if (!_client.Connected)
                    {
                        _client.Connect(Server, Port);
                        continue;
                    }

                    if (_writer == null || _reader == null)
                        return;

                    string response;
                    while((response = _reader.ReadLine()) != null)
                    {
                        IrcMessage message = new IrcMessage(response);

                        if (message.IsReply)
                        {
                            if (TryParseReply(message) || TrySkipReply(message))
                                continue;
                        }
                        else
                        {
                            if (message.IsError)
                            {
                                TryParseErrorReply(message);
                                continue;
                            }

                            if (TryParseCommand(message))
                                continue;
                        }

                        if (PendingChannels != null && PendingChannels.Any())
                        {
                            foreach (IrcChannel channel in PendingChannels.Where(c => !c.HasSentRegistration))
                            {
                                // Send registration
                                channel.SendConnectRequest();
                                // Now free password
                                channel.FreePassword();
                            }
                        }

                        if (message.Parameters == null || SendMessageToChannel(message))
                            continue;

                        Debug.WriteLine("Skipped message: " + message.ToString());
                    }
                }
                catch(Exception ex)
                {
                    // NOTE Because this code runs outside of the UI thread if an exception occurs here
                    //      and isn't squashed completely it will bring down the entire application
                    try
                    {
                        Error.LogFromThread(this, new ErrorEventArgs(ex));
                    }
                    catch { /* Explicitly squash if all else fails */ }
                }
            }
        }

        private bool TryParseReply(IrcMessage message)
        {
            if (!message.IsReply)
                return false;

            try
            {
                switch(message.REPLY)
                {
                    case IrcReplyCode.RPL_FORWARDINGTOCHANNEL:
                        string channelName = message.Parameters[1];
                        string newChannel = message.Parameters[2];
                        IrcChannel forwardedChannel = Channels.FirstOrDefault(c => c.ChannelName == channelName);
                        if (forwardedChannel != null)
                        {
                            forwardedChannel.ChannelName = newChannel;
                            forwardedChannel.AddMessage(new SystemMessage("Forwarded to channel " + newChannel + "."));
                            MainWindow.Instance.RenameTab(forwardedChannel, newChannel);
                        }
                        return true;

                    case IrcReplyCode.RPL_WELCOME:
                        IsConnected = true;
                        foreach(IrcChannel channel in PendingChannels.Where(c => c.HasSentRegistration).ToList())
                        {
                            channel.Join();
                            Channels.Add(channel);
                            PendingChannels.Remove(channel);
                        }
                        return true;

                    default:
                        float t;
                        if (message.Command.Length > 0 && !float.TryParse(message.Command, NumberStyles.Float, CultureInfo.InvariantCulture, out t))
                        {
                            // Dump
                            MainWindow.Instance.SendCurrent(new DumpMessage(message.Command + " " + message.Parameters.Where(p => Channels.All(ch => ch.Credentials.ChosenName != p)).Aggregate((c, n) => c + " " + n)));
                        }
                        return false;
                }
            }
            catch(Exception ex)
            {
                Error.LogFromThread(this, new ErrorEventArgs(ex));
            }

            return false;
        }
        
        private bool TrySkipReply(IrcMessage message)
        {
            if (message == null)
                return false;

            if (!message.IsReply)
                return false;

            switch (message.REPLY)
            {
                case IrcReplyCode.RPL_YOURHOST:
                case IrcReplyCode.RPL_CREATED:
                case IrcReplyCode.RPL_MYINFO:
                case IrcReplyCode.RPL_MOTDSTART:
                case IrcReplyCode.RPL_MOTD:
                case IrcReplyCode.RPL_ENDOFMOTD:
                case IrcReplyCode.RPL_ENDOFBANLIST:
                case IrcReplyCode.RPL_ENDOFEXCEPTLIST:
                case IrcReplyCode.RPL_ENDOFINFO:
                case IrcReplyCode.RPL_ENDOFINVITELIST:
                case IrcReplyCode.RPL_ENDOFLINKS:
                case IrcReplyCode.RPL_ENDOFNAMES:
                case IrcReplyCode.RPL_ENDOFSTATS:
                case IrcReplyCode.RPL_ENDOFUSERS:
                case IrcReplyCode.RPL_ENDOFWHOIS:
                case IrcReplyCode.RPL_ENDOFWHOWAS:
                case IrcReplyCode.RPL_ENDOFWHO:
                case IrcReplyCode.RPL_BOUNCE:
                case IrcReplyCode.RPL_LUSERCHANNELS:
                case IrcReplyCode.RPL_LUSERCLIENT:
                case IrcReplyCode.RPL_LUSERME:
                case IrcReplyCode.RPL_LUSEROP:
                case IrcReplyCode.RPL_LUSERUNKNOWN:
                case IrcReplyCode.RPL_WELCOME:
                    return true;
            }

            return false;
        }

        private bool SendMessageToChannel(IrcMessage message)
        {
            try
            {
                bool found = false;
                foreach(IrcChannel channel in Channels)
                {
                    if (message.Command == "PRIVMSG" || channel.UserNames.Any(message.Prefix.StartsWith) || message.Parameters.Any(p => p == channel.ChannelName))
                    {
                        channel.OnMessageReceived(message);
                        found = true;
                    }
                }
                return found;
            }
            catch
            {
                return false;
            }
        }

        private bool TryParseCommand(IrcMessage message)
        {
            try
            {
                if (message.IsReply || message.IsError)
                    return false;

                switch(message.Command)
                {
                    case "PING":
                        Pong(message.Parameters[0]);
                        return true;
                    case "ERROR":
                        if(OnClosing != null) OnClosing.Invoke();
                        Dispose();
                        return true;
                        
                    case "MODE":
                    case "JOIN":
                    case "QUIT":
                    case "PART":
                    case "PRIVMSG":
                        return false; // skip, but pass through to the right chat

                    default:
                        float t;
                        if (message.Command.Length > 0 && !float.TryParse(message.Command, NumberStyles.Float, CultureInfo.InvariantCulture, out t))
                        {
                            // Dump
                            MainWindow.Instance.SendCurrent(new DumpMessage(message.Command + " " + message.Parameters.Where(p => Channels.All(ch => ch.Credentials.ChosenName != p)).Aggregate((c, n) => c + " " + n)));
                        }
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool TryParseErrorReply(IrcMessage message)
        {
            try
            {
                if (!message.IsError)
                    return false;

                switch(message.ERR_REPLY)
                {
                    case IrcErrorReplyCode.ERR_NOMOTD:
                        return true;

                    case IrcErrorReplyCode.ERR_NOCHANMODES:
                    {
                        string channelName = message.Parameters[1];
                        IrcChannel channel = Channels.FirstOrDefault(c => c.ChannelName == channelName);
                        if(channel != null) channel.AddMessage(new SystemMessage("You must be identified to join " + channelName + "!"));
                        return true;
                    }

                    case IrcErrorReplyCode.ERR_NICKCOLLISION:
                        MainWindow.Instance.SendCurrent(new SystemMessage("Nickname " + message.Parameters[0] + " is registered."));
                        return true;

                    case IrcErrorReplyCode.ERR_NICKNAMEINUSE:
                    {
                        IrcChannel channel = PendingChannels.FirstOrDefault(c => c.Credentials.ChosenName == message.Parameters[0]);
                        MainWindow.Instance.SendCurrent(new SystemMessage("Nickname " + message.Parameters[0] + " is already in use in this channel."));
                        // Change nickname
                        int index = channel.UserNames.IndexOf(channel.Credentials.ChosenName);
                        if(index > -1) channel.UserNames.RemoveAt(index);
                        channel.Credentials.ChosenName = message.Parameters[0] + "1";
                        channel.UserNames.Insert(Math.Max(0, index), channel.Credentials.ChosenName);
                        Send(IrcCommands.Nickname(channel.Credentials.ChosenName));
                        return true;
                    }

                    case IrcErrorReplyCode.ERR_CHANNELISFULL:
                        MainWindow.Instance.SendCurrent(new SystemMessage("The channel you attempted to join is full."));
                        return true;

                    default:
                        float t;
                        if (message.Command.Length > 0 && !float.TryParse(message.Command, NumberStyles.Float, CultureInfo.InvariantCulture, out t))
                        {
                            // Dump
                            MainWindow.Instance.SendCurrent(new DumpMessage(message.Command + " " + message.Parameters.Where(p => Channels.All(ch => ch.Credentials.ChosenName != p)).Aggregate((c, n) => c + " " + n)));
                        }
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~IrcClient()
        {
            Dispose(false);
        }
        
        protected virtual void Dispose(bool alsoManaged)
        {
            if (alsoManaged)
            {
                try
                {
                    _readerThread.Abort();
                    if (_client != null && _client.Connected)
                        _client.Close();
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
