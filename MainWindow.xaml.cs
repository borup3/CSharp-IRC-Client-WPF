using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CodeCafeIRC.irc;
using CodeCafeIRC.messaging;
using CodeCafeIRC.themes;
using CodeCafeIRC.themes.Theme;
using CodeCafeIRC.ui;

//
//  Code Cafe IRC client, source available at https://github.com/borup3/CSharp-IRC-Client-WPF
//  Website http://thecode.cafe
//  Original code from Parakeet 2 http://parakeet-ide.org/
//
//  This code is released under the MIT license.
//  I do appreciate if you tell me what you're using the code for, purely out of interest (and a potential blogging topic on my website http://thecode.cafe if it's really cool).
//  If you make nice changes on a fork or personal project, everyone would benefit if you submit a pull request to bring those changes into this repository as well.
//

namespace CodeCafeIRC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static MainWindow Instance;

        public ObservableCollection<TabItem> TabItems { get; private set; }
        public List<IrcClient> Clients { get; private set; }
        private readonly ChatBox m_main = new ChatBox();

        public IrcChannel CurrentChannel { get; private set; }

        public string ShadowText { get { return "Send..."; } }

        public MainWindow()
        {
            TabItems = new ObservableCollection<TabItem>();
            InitializeComponent();
            ThemeManager.SetColorScheme(new OriginalColorScheme());
            Instance = this;
            
            Clients = new List<IrcClient>();
            m_main.AddMessage(new SystemMessage("Welcome to Code Cafe IRC, an open-source client. Source available at https://github.com/borup3/CSharp-IRC-Client-WPF."));
            m_main.AddMessage(new SystemMessage("/help to see available commands."));

            // Init tab control
            AddMainTab();
            TabControl.SelectionChanged += Tab_Selected;
        }

        /// <summary>
        /// This method is terrible.
        /// </summary>
        public void SendCurrent(Message message)
        {
            if (CurrentChannel != null) CurrentChannel.AddMessage(message);
            else m_main.AddMessage(message);
        }

        public void BroadcastMessage(IrcClient client, Message message)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => BroadcastMessage(client, message));
                return;
            }
            
            if (TabItems == null) return;
            foreach (ChatBox chatBox in TabItems.Select(t => t.Content as ChatBox).Where(c => c != null && c.Channel.Client == client))
                chatBox.AddMessage(message);
        }

        public void AddChannel(IrcChannel channel)
        {
            m_main.AddMessage(new StateMessage("Opened channel " + channel.ChannelName + "."));

            TabItem tabItem = new TabItem {DataContext = channel};
            tabItem.SetBinding(HeaderedContentControl.HeaderProperty, new Binding("ChannelName"));
            tabItem.Content = channel.Chat;
            tabItem.Tag = channel;
            tabItem.Unloaded += (sender, args) => channel.LeaveChannel();
            TabItems.Add(tabItem);
            TabControl.SelectedItem = tabItem;
        }

        public void RenameTab(IrcChannel channel, string name)
        {
            foreach (TabItem item in TabControl.Items)
            {
                if (!Equals(item.Tag, channel)) continue;
                item.Header = name;
                break;
            }
        }

        public void LeaveChannel(IrcChannel channel)
        {
            // this unloads the tab, which closes the channel
            m_main.AddMessage(new StateMessage("Left channel " + channel.ChannelName + "."));
            TabItem found = TabControl.Items.Cast<TabItem>().FirstOrDefault(item => Equals(item.Tag, channel));
            if(found != null) TabItems.Remove(found);
        }

        private void AddMainTab()
        {
            TabItem tabItem = new TabItem
            {
                DataContext = this,
                Header = "Main",
                Content = m_main
            };
            TabItems.Add(tabItem);
            TabControl.SelectedItem = tabItem;
        }

        private void Tab_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.SelectedItem == null)
            {
                CurrentChannel = null;
                return;
            }
            CurrentChannel = ((TabItem)TabControl.SelectedItem).Tag as IrcChannel;
        }

        private void Command_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = (TextBox)sender;
                General.ParseInput(textBox.Text);
                textBox.Text = string.Empty;
                e.Handled = true;
            }
        }

        private void Input_OnGotFocus(object sender, RoutedEventArgs e)
        {
            TextBox input = (TextBox)sender;
            if (input != null && input.Text == ShadowText)
                input.Text = string.Empty;
        }

        private void Input_OnLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox input = (TextBox)sender;
            if (input != null && string.IsNullOrEmpty(input.Text))
                input.Text = ShadowText;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            foreach (IrcClient client in Clients.ToList())
            {
                foreach (IrcChannel channel in client.Channels.ToList())
                {
                    if (!client.IsConnected) break;
                    channel.LeaveChannel();
                }
                client.Dispose();
            }
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MainWindow()
        {
            Dispose(false);
        }

        private void Dispose(bool alsoManaged)
        {
            if (!alsoManaged) return;
            foreach (IrcClient client in Clients.ToList())
                client.Dispose();
        }

        #endregion
    }
}
