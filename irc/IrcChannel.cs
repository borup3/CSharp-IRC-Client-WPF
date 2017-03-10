using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows;
using CodeCafeIRC.messaging;
using CodeCafeIRC.themes;
using CodeCafeIRC.ui;

namespace CodeCafeIRC.irc
{
    public class IrcChannel : DependencyObject
    {
        public static readonly DependencyProperty ChannelNameProperty = DependencyProperty.Register(
            "ChannelName", typeof(string), typeof(IrcChannel), new PropertyMetadata(default(string)));

        public string ChannelName
        {
            get
            {
                if (!CheckAccess())
                    return Dispatcher.Invoke(() => ChannelName);

                return (string) GetValue(ChannelNameProperty);
            }
            set
            {
                if (!CheckAccess())
                {
                    Dispatcher.Invoke(() => { ChannelName = value; });
                    return;
                }

                SetValue(ChannelNameProperty, value);
            }
        }

        private bool _allowNotification = true;
        private Timer _cooldownTimer;
        private ChatBox _chat;
        private UserCredentials _credentials;
        private IrcClientState _state;

        public ObservableCollection<string> UserNames { get; private set; }
        public bool HasSentRegistration { get; private set; }
        public ChatBox Chat
        {
            get { return _chat; }
            private set { _chat = value; }
        }
        public UserCredentials Credentials
        {
            get { return _credentials; }
            private set
            {
                _credentials = value;
                _chat.AddMessage(new SystemMessage("Your nickname is " + value.ChosenName + "."));
            }
        }
        public IrcClientState State
        {
            get { return _state; }
            set
            {
                if (_state == value)
                    return;
                
                switch (value)
                {
                    case IrcClientState.PENDING_SERVER_CONNECTION:
                        _chat.AddMessage(new SystemMessage("Connecting to server..."));
                        break;
                    case IrcClientState.CONNECTED_TO_SERVER:
                        _chat.AddMessage(new SystemMessage("You are connected to server."));
                        break;
                    case IrcClientState.PENDING_CHANNEL_CONNECTION:
                        _chat.AddMessage(new SystemMessage("Connecting to " + ChannelName + "..."));
                        break;
                    case IrcClientState.CONNECTED_TO_CHANNEL:
                        _chat.AddMessage(new SystemMessage("You are now in " + ChannelName + "."));
                        break;
                }
                _state = value;
            }
        }
        public readonly IrcClient Client;

        private IrcChannel()
        {
            _cooldownTimer = new Timer(5000);
            _cooldownTimer.Elapsed += (sender, args) => { _allowNotification = true; };

            UserNames = new ObservableCollection<string>();
            _chat = new ChatBox();
            _chat.SetChannel(this);
            HasSentRegistration = false;
        }
        
        public IrcChannel(IrcClient client, UserCredentials credentials, string channel)
            : this()
        {
            if (client == null) 
                throw new ArgumentNullException("client");

            ChannelName = channel;
            Credentials = credentials;
            Client = client;
            General.AddChannel(this);
        }

        public void SendConnectRequest()
        {
            State = IrcClientState.PENDING_SERVER_CONNECTION;
            Client.Send(IrcCommands.Pass(Credentials.Password));
            Client.Send(IrcCommands.Nickname(Credentials.ChosenName));
            Client.Send(IrcCommands.UserInfo(Credentials.ChosenName, Credentials.RealName));
            HasSentRegistration = true;
        }

        public void FreePassword()
        {
            Credentials.Password = "";
        }

        public void Join()
        {
            Client.Send(IrcCommands.Join(ChannelName));
            State = IrcClientState.PENDING_CHANNEL_CONNECTION;
        }
        
        public void LeaveChannel()
        {
            Client.Send(IrcCommands.Leave(ChannelName));
            Client.Channels.Remove(this);
        }

        public void AddMessage(Message message)
        {
            _chat.AddMessage(message);
        }

        public void SendMessage(string input)
        {
            if (Client == null)
                return;
            if (string.IsNullOrEmpty(input))
                return;

            if (State != IrcClientState.CONNECTED_TO_CHANNEL)
            {
                _chat.AddMessage(new ServerMessage("Not in a channel."));
                return;
            }

            _chat.AddMessage(new SelfMessage(Credentials.ChosenName, _credentials.ChosenName, input));
            Client.Send(IrcCommands.Say(ChannelName, input));
        }
        
        /// <summary>
        /// Callback occurs when IRC client receives a message for this channel.
        /// </summary>
        /// <param name="message"></param>
        public void OnMessageReceived(IrcMessage message)
        {
            if (message == null || message.Parameters == null || _chat == null || message.IsError)
                return;

            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => OnMessageReceived(message));
                return;
            }
            
            if (message.IsReply)
                // Server reply message
                TryParseReply(message);
            else
                TryParseCommand(message);
        }

        private void TryParseReply(IrcMessage message)
        {
            string msg = message.Parameters.Last().Trim();
            if (string.IsNullOrEmpty(msg))
                return;
            
            switch (message.REPLY)
            {
                case IrcReplyCode.RPL_HELP:
                    _chat.AddMessage(new ServerMessage(msg));
                    break;

                case IrcReplyCode.RPL_TOPIC:
                    _chat.AddMessage(new ServerMessage(msg));
                    break;

                case IrcReplyCode.RPL_NAMREPLY:
                    UserNames.Clear();
                    foreach (string s in msg.Split(' '))
                    {
                        ThemeManager.AddUserColor(s);
                        UserNames.Add(s);
                    }
                    if(UserNames.Count < 15) General.ShowUserList(this);
                    break;
            }
        }

        private void TryParseCommand(IrcMessage message)
        {
            try
            {
                switch (message.Command)
                {
                    // case "NOTICE":
                    //     _chat.AddMessage(new ServerMessage("Your nickname (" + message.Parameters[0] + ") is not registered."));
                    //     break;

                    case "JOIN":
                        string joinee = message.Prefix.Substring(0, message.Prefix.IndexOf("!"));
                        if (joinee != Credentials.ChosenName)
                        {
                            // inject a color for this username
                            ThemeManager.AddUserColor(joinee);
                            if (!UserNames.Contains(joinee)) UserNames.Add(joinee);
                            _chat.AddMessage(new StateMessage(joinee + " has joined!"));
                        }
                        else
                            State = IrcClientState.CONNECTED_TO_CHANNEL;
                        break;

                    case "PART":
                        string partee = message.Prefix.Substring(0, message.Prefix.IndexOf("!"));
                        UserNames.Remove(partee); // Remove user
                        _chat.AddMessage(new StateMessage(partee + " has left the chat."));
                        break;

                    case "QUIT":
                        string quitter = message.Prefix.Substring(0, message.Prefix.IndexOf("!"));
                        UserNames.Remove(quitter); // Remove user
                        _chat.AddMessage(new StateMessage(quitter + " has quit (" + message.Parameters.Last() + ")"));
                        break;

                    case "NICK":
                        UserNames.Remove(Credentials.ChosenName); // Remove old
                        Credentials.ChosenName = message.Parameters[0];
                        UserNames.Add(Credentials.ChosenName); // Add new
                        ThemeManager.AddUserColor(message.Parameters[0]);
                        _chat.AddMessage(new ServerMessage("Your nickname is now " + Credentials.ChosenName + "."));
                        break;

                    case "PRIVMSG":
                        string sender = message.Prefix.Substring(0, message.Prefix.IndexOf("!"));
                        string target = message.Parameters[0];
                        string content = message.Parameters[1];
                        if (target == ChannelName)
                        {
                            // Public channel message
                            UserMessage userMessage = new UserMessage(sender, sender, content);
                            if (content.Contains(_credentials.ChosenName))
                            {
                                // User mentioned you
                                userMessage.ContentFormat.Foreground = "LiveChat Mentioned You";

                                if (_allowNotification)
                                {
                                    // Show notification
                                    //NotificationCenter.ShowNotification("Live Chat", sender + ": \"" + content + "\"", LiveChat.Interface.LiveChat.OpenLiveChat);
                                    _allowNotification = false;
                                    _cooldownTimer.Start();
                                }
                            }
                            _chat.AddMessage(userMessage);
                        }
                        else
                            // Private message
                            _chat.AddMessage(new PrivateMessage(sender, "You", content));
                        break;
                }
            }
            catch (Exception e)
            {
                _chat.AddMessage(new ErrorMessage("Oops! " + e.Message));
            }
        }
    }
}
