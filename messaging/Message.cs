using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;

namespace CodeCafeIRC.messaging
{
    public abstract class Message : IFormattedMessage, INotifyPropertyChanged
    {
        private const string HYPERLINK_PATTERN = @"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?";

        private FormattingRule _timeStampFormat;
        private FormattingRule _senderFormat;
        private FormattingRule _contentFormat;

        private DateTime _timeStamp;
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set
            {
                if (value.Equals(_timeStamp)) return;
                _timeStamp = value;
                OnPropertyChanged();
                OnPropertyChanged("TimeStampString");
            }
        }

        public string TimeStampString {
            get { return string.Format("{0}", TimeStamp.ToString("HH:mm:ss", CultureInfo.InvariantCulture)); }
        }

        private string _sender;

        public string Sender
        {
            get { return _sender; }
            set
            {
                if (value == _sender) return;
                _sender = value;
                OnPropertyChanged();
            }
        }

        private string _content;

        public string Content
        {
            get { return _content; }
            set
            {
                if (value == _content) return;
                _content = value;
                OnPropertyChanged();
            }
        }

        public FormattingRule TimeStampFormat
        {
            get { return _timeStampFormat; }
            set
            {
                if (value.Equals(_timeStampFormat)) return;
                _timeStampFormat = value;
                OnPropertyChanged();
            }
        }

        public FormattingRule SenderFormat
        {
            get { return _senderFormat; }
            set
            {
                if (value.Equals(_senderFormat)) return;
                _senderFormat = value;
                OnPropertyChanged();
            }
        }

        public FormattingRule ContentFormat
        {
            get { return _contentFormat; }
            set
            {
                if (value.Equals(_contentFormat)) return;
                _contentFormat = value;
                OnPropertyChanged();
            }
        }

        private Message()
        {
            Sender = String.Empty;
            TimeStamp = DateTime.Now;
            ContentFormat = new FormattingRule();
            TimeStampFormat = FormattingRule.TimeStampRule;
            SenderFormat = new FormattingRule();
        }

        protected Message(string content)
            : this()
        {
            Content = content;
        }

        protected Message(string sender, string content)
            : this()
        {
            Sender = sender;
            Content = content;
        }

        public abstract void ResetFormatting();

        public Paragraph ToParagraph()
        {
            Paragraph p = new Paragraph();
            p.TextIndent = 0;
            p.Margin = new Thickness(0);

            if (!String.IsNullOrEmpty(TimeStampString))
            {
                Run timeStamp = new Run(TimeStampString);
                timeStamp.FontSize = TimeStampFormat.FontSize;
                timeStamp.FontWeight = TimeStampFormat.FontWeight;
                timeStamp.FontStyle = TimeStampFormat.FontStyle;
                if(!string.IsNullOrEmpty(TimeStampFormat.Foreground))
                    timeStamp.SetResourceReference(TextElement.ForegroundProperty, TimeStampFormat.Foreground);
                p.Inlines.Add(timeStamp);
            }

            if (!String.IsNullOrEmpty(Sender))
            {
                if(p.Inlines.Any())
                    p.Inlines.Add(" ");

                Run sender = new Run(Sender);
                sender.FontSize = SenderFormat.FontSize;
                sender.FontWeight = SenderFormat.FontWeight;
                sender.FontStyle = SenderFormat.FontStyle;
                if (!string.IsNullOrEmpty(SenderFormat.Foreground))
                    sender.SetResourceReference(TextElement.ForegroundProperty, SenderFormat.Foreground);
                p.Inlines.Add(sender);
            }
            
            if (!String.IsNullOrEmpty(Content))
            {
                if (p.Inlines.Any())
                    p.Inlines.Add(" ");

                ParseContent(p, Content);
            }

            return p;
        }

        private void ParseContent(Paragraph paragraph, string content)
        {
            if (content == null) 
                throw new ArgumentNullException("content");

            if (!ContainsHyperlink(content))
                paragraph.Inlines.Add(NewContentRun(content));
            else
            {
                do
                {
                    Match match = Regex.Match(content, HYPERLINK_PATTERN);
                    string preLink = content.Substring(0, match.Index);
                    if (!string.IsNullOrEmpty(preLink))
                        paragraph.Inlines.Add(NewContentRun(preLink));

                    string uri = match.Value;
                    if (!uri.StartsWith("http://") && !uri.StartsWith("https://"))
                        uri = "http://" + uri;

                    Hyperlink link = new Hyperlink(NewContentRun(match.Value))
                    {
                        Cursor = Cursors.Hand,
                        NavigateUri = new Uri(uri)
                    };
                    link.RequestNavigate += LinkOnRequestNavigate;
                    link.MouseDown += (sender, args) => link.DoClick();
                    link.SetResourceReference(TextElement.ForegroundProperty, "LiveChat Hyperlink");
                    paragraph.Inlines.Add(link);

                    content = content.Remove(0, match.Index + match.Length);
                }
                while (ContainsHyperlink(content));

                if (!string.IsNullOrEmpty(content))
                    paragraph.Inlines.Add(NewContentRun(content));
            }
        }

        private void LinkOnRequestNavigate(object sender, RequestNavigateEventArgs requestNavigateEventArgs)
        {
            Process.Start(requestNavigateEventArgs.Uri.AbsoluteUri);
            requestNavigateEventArgs.Handled = true;
        }

        private static bool ContainsHyperlink(string s)
        {
            return Regex.Match(s, HYPERLINK_PATTERN).Success;
        }

        private Run NewContentRun(string s)
        {
            if (s == null) throw new ArgumentNullException("s");
            Run r = new Run(s);
            r.FontSize = ContentFormat.FontSize;
            r.FontWeight = ContentFormat.FontWeight;
            r.FontStyle = ContentFormat.FontStyle;
            if (!string.IsNullOrEmpty(ContentFormat.Foreground))
                r.SetResourceReference(TextElement.ForegroundProperty, ContentFormat.Foreground);
            return r;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
