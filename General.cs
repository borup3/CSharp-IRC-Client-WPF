using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CodeCafeIRC.irc;
using CodeCafeIRC.messaging;

namespace CodeCafeIRC
{
    public static class General
    {
        private static IDictionary<string, string> _setCommands = new Dictionary<string, string>();
        private static IDictionary<string, string> _availableOptions = new Dictionary<string, string>() { { "autojoin", "" } };
        private static IDictionary<string, string> _overriddenOptions = new Dictionary<string, string>();

        static General()
        {
            LoadCommands();
            LoadOptions();
        }

        public static bool TryGetOption(string _option, out string _value)
        {
            return _overriddenOptions.TryGetValue(_option, out _value);
        }

        private static void LoadOptions()
        {
            try
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Code Cafe IRC");
                if (!Directory.Exists(dir)) return;
                string file = Path.Combine(dir, "options.txt");
                if (!File.Exists(file)) return;
                using (StreamReader sr = new StreamReader(file))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] split = line.Split(new[] { ":::" }, StringSplitOptions.None);
                        _overriddenOptions[split[0]] = Encoding.UTF8.GetString(Convert.FromBase64String(split[1]));
                    }
                }
            }
            catch (Exception e)
            {
                MainWindow.Instance.SendCurrent(new ErrorMessage("Failed to load options: " + e.Message));
            }
        }

        private static void SaveOptions()
        {
            try
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Code Cafe IRC");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                using (StreamWriter sw = new StreamWriter(Path.Combine(dir, "options.txt")))
                {
                    foreach (var pair in _overriddenOptions)
                    {
                        sw.WriteLine(pair.Key + ":::" + Convert.ToBase64String(Encoding.UTF8.GetBytes(pair.Value)));
                    }
                }
            }
            catch (Exception e)
            {
                MainWindow.Instance.SendCurrent(new ErrorMessage("Failed to save commands: " + e.Message));
            }
        }

        private static void LoadCommands()
        {
            try
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Code Cafe IRC");
                if (!Directory.Exists(dir)) return;
                string file = Path.Combine(dir, "cmds.priv");
                if (!File.Exists(file)) return;
                using (StreamReader sr = new StreamReader(file))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] split = line.Split(new [] {":::"}, StringSplitOptions.None);
                        string command = Encoding.UTF8.GetString(Convert.FromBase64String(split[0]));
                        string val = Encoding.UTF8.GetString(Convert.FromBase64String(split[1]));
                        _setCommands[command] = val;
                    }
                }
            }
            catch (Exception e)
            {
                MainWindow.Instance.SendCurrent(new ErrorMessage("Failed to load commands: " + e.Message));
            }
        }

        private static void SaveCommands()
        {
            try
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Code Cafe IRC");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                using (StreamWriter sw = new StreamWriter(Path.Combine(dir, "cmds.priv")))
                {
                    foreach (var pair in _setCommands)
                    {
                        sw.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(pair.Key)) + ":::" + Convert.ToBase64String(Encoding.UTF8.GetBytes(pair.Value)));
                    }
                }
            }
            catch (Exception e)
            {
                MainWindow.Instance.SendCurrent(new ErrorMessage("Failed to save commands: " + e.Message));
            }
        }

        public static void AddChannel(IrcChannel channel)
        {
            MainWindow.Instance.AddChannel(channel);
        }

        public static void ParseInput(string command)
        {
            if (string.IsNullOrEmpty(command)) return;

            command = command.Trim(' ');
            if (command[0] == '/')
            {
                string[] split = command.Split(' ');
                switch (split[0])
                {
                    case "/set":
                        DoSet(command);
                        break;

                    case "/option":
                        DoOption(command);
                        break;

                    case "/join":
                        DoJoin(split);
                        break;

                    case "/leave":
                        MainWindow.Instance.SendCurrent(new SelfMessage(null, "You", command));
                        DoLeave(split);
                        break;

                    case "/help":
                        MainWindow.Instance.SendCurrent(new SelfMessage(null, "You", command));
                        DoHelp(split);
                        break;

                    case "/users":
                        DoUsers(split);
                        break;

                    case "/nick":
                        MainWindow.Instance.SendCurrent(new SelfMessage(null, "You", command));
                        DoNick(split);
                        break;

                    case "/pm":
                        DoPM(split);
                        break;

                    default:
                        if (TryCustomCommand(split[0])) break;
                        MainWindow.Instance.SendCurrent(new SelfMessage(null, "You", command));
                        MainWindow.Instance.SendCurrent(new ErrorMessage("command not recognized."));
                        break;
                }
            }
            else
            {
                // Writing in channel
                if (MainWindow.Instance.CurrentChannel != null)
                    MainWindow.Instance.CurrentChannel.SendMessage(command);
                else
                {
                    MainWindow.Instance.SendCurrent(new SelfMessage(null, "You", command));
                    MainWindow.Instance.SendCurrent(new ErrorMessage("not a valid command."));
                }
            }
        }

        private static bool TryCustomCommand(string _command)
        {
            if (!_command.StartsWith("/")) return false;
            _command = _command.Substring(1);
            string cmd;
            if (!_setCommands.TryGetValue(_command, out cmd)) return false;
            ParseInput(cmd);
            return true;
        }

        private static void DoSet(string split)
        {
            split = split.Trim();
            split = split.Remove(0, "/set ".Length);
            string command = split.Substring(0, split.IndexOf(' '));
            split = split.Remove(0, command.Length + 1);
            string text = split.Trim('"');

            if (split.Length < 3)
            {
                Error.Write("/set <command> \"string\"");
                return;
            }

            if (_setCommands.ContainsKey(command))
            {
                // Overwriting
                MainWindow.Instance.SendCurrent(new SystemMessage(string.Format("Overwriting existing command '{0}'", command)));
            }

            _setCommands[command] = text;
            SaveCommands();
        }

        private static void DoOption(string split)
        {
            split = split.Trim();
            split = split.Remove(0, "/option ".Length);
            string option = split.Substring(0, split.IndexOf(' '));
            split = split.Remove(0, option.Length + 1);
            string value = split.Trim('"');

            if (split.Length < 3)
            {
                Error.Write("/option <option> \"string\"");
                return;
            }

            if (_overriddenOptions.ContainsKey(option))
            {
                // Overwriting
                MainWindow.Instance.SendCurrent(new SystemMessage(string.Format("Overwriting existing option '{0}'", option)));
            }

            _overriddenOptions[option] = value;
            SaveOptions();
        }

        private static void DoPM(string[] split)
        {
            if (split.Length < 3)
            {
                Error.Write("/pm <username> <message>");
                return;
            }

            if (MainWindow.Instance.CurrentChannel == null)
            {
                MainWindow.Instance.SendCurrent(new SystemMessage("Not in a channel."));
                return;
            }

            string user = split[1];
            string message = split.Skip(2).Aggregate((c,n) => c + " " + n);
            MainWindow.Instance.SendCurrent(new PrivateMessage("You", user, message));
            MainWindow.Instance.CurrentChannel.Client.Send(IrcCommands.PrivateMessage(user, message));
        }

        private static void DoNick(string[] split)
        {
            if (MainWindow.Instance.CurrentChannel == null)
                MainWindow.Instance.SendCurrent(new SystemMessage("Not in a channel."));
            else
            {
                MainWindow.Instance.CurrentChannel.Credentials.ChosenName = split[1];
                MainWindow.Instance.CurrentChannel.Client.Send(IrcCommands.Nickname(split[1]));
                MainWindow.Instance.SendCurrent(new StateMessage("Changed nickname to " + split[1] + "."));
            }
        }

        public static void ShowUserList(IrcChannel channel)
        {
            if (channel.UserNames.Count == 0) return;
            MainWindow.Instance.SendCurrent(new DumpMessage("Users: " + channel.UserNames.Aggregate((c, n) => c + ", " + n)));
        }

        private static void DoUsers(string[] split)
        {
            MainWindow.Instance.SendCurrent(new SelfMessage(null, "You", split[0]));
            if (MainWindow.Instance.CurrentChannel == null)
                MainWindow.Instance.SendCurrent(new SystemMessage("Not in a channel."));
            else
            {
                if (MainWindow.Instance.CurrentChannel.UserNames.Count == 0)
                    MainWindow.Instance.SendCurrent(new SystemMessage("Not in a channel."));
                else ShowUserList(MainWindow.Instance.CurrentChannel);
            }
        }

        private static void DoHelp(string[] args)
        {
            MainWindow.Instance.SendCurrent(new SystemMessage("> /join <server> <channel> <username> [<password>]"));
            MainWindow.Instance.SendCurrent(new SystemMessage("> /leave"));
            MainWindow.Instance.SendCurrent(new SystemMessage("> /users"));
            MainWindow.Instance.SendCurrent(new SystemMessage("> /nick <nickname>"));
            MainWindow.Instance.SendCurrent(new SystemMessage("> /pm <username> <message>"));
            MainWindow.Instance.SendCurrent(new SystemMessage("> /set <command> \"string\""));
            MainWindow.Instance.SendCurrent(new SystemMessage("> /option <option> \"string\""));

            MainWindow.Instance.SendCurrent(new SystemMessage("Options:"));
            foreach (var option in _availableOptions)
            {
                string str = _overriddenOptions.ContainsKey(option.Key) ? option.Key + " = " + _overriddenOptions[option.Key] : option.Key;
                MainWindow.Instance.SendCurrent(new SystemMessage("> " + str));
            }

            MainWindow.Instance.SendCurrent(new SystemMessage("Custom commands:"));
            foreach (var cmd in _setCommands)
                MainWindow.Instance.SendCurrent(new SystemMessage("> /" + cmd.Key));
        }

        private static void DoLeave(string[] args)
        {
            if (MainWindow.Instance.CurrentChannel != null)
            {
                MainWindow.Instance.LeaveChannel(MainWindow.Instance.CurrentChannel);
            }
        }

        private static void DoJoin(string[] args)
        {
            if (args.Length != 4 && args.Length != 5)
            {
                Error.Write("/join <server> <channel> <username> [<password>]");
                return;
            }

            MainWindow.Instance.SendCurrent(new SelfMessage(null, "You", args[0] + " " + args[1] + " " + args[2] + " " + args[3] + (args.Length == 5 ? " ***" : "")));

            string server = args[1];
            string channel = args[2];
            string username = args[3];
            string password = args.Length == 5 ? args[4] : "";
            UserCredentials credentials = new UserCredentials(username, password);

            IrcClient client = MainWindow.Instance.Clients.FirstOrDefault(c => c.Channels != null && c.Server == server && c.Channels.Any(chn => chn.Credentials.ChosenName == username));
            if (client == null)
            {
                client = new IrcClient(server, 6667);
                client.OnClosing += () =>
                {
                    MainWindow.Instance.Clients.Remove(client);
                };
                client.Connect(credentials);
                client.Join(channel);
                MainWindow.Instance.Clients.Add(client);
            }
            else
            {
                if (client.Channels.Any(c => c.ChannelName == channel))
                    MainWindow.Instance.SendCurrent(new StateMessage("You are already in " + channel + "."));
                else client.Join(channel);
            }
        }

        public static bool HasInternet()
        {
            return true; // I don't really care about this... just a tiny app anyway
        }
    }
}
