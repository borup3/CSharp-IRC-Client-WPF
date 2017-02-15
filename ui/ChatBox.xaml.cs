using System;
using System.Windows.Controls;
using CodeCafeIRC.irc;
using CodeCafeIRC.messaging;

namespace CodeCafeIRC.ui
{
    /// <summary>
    /// Interaction logic for ChatBox.xaml
    /// </summary>
    public partial class ChatBox : UserControl
    {
        public IrcChannel Channel { get; private set; }

        public ChatBox()
        {
            InitializeComponent();
        }

        public void SetChannel(IrcChannel channel)
        {
            Channel = channel;
        }

        public void AddMessage(Message message)
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke((Action)(() => AddMessage(message)));
                return;
            }

            Document.Blocks.Add(message.ToParagraph());
            TextBox.ScrollToEnd();
        }

        public void Clear()
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke((Action)(Clear));
                return;
            }
            
            Document.Blocks.Clear();
        }
    }
}
