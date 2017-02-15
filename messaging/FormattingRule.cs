using System.Windows;

namespace CodeCafeIRC.messaging
{
    public class FormattingRule
    {
        public static readonly FormattingRule TimeStampRule = new FormattingRule { Foreground = "LiveChat Timestamp" };

        public double FontSize { get; set; }
        public FontStyle FontStyle { get; set; }
        public FontWeight FontWeight { get; set; }
        public string Foreground { get; set; }

        public FormattingRule()
        {
            FontSize = SystemFonts.MessageFontSize;
            FontStyle = SystemFonts.MessageFontStyle;
            FontWeight = SystemFonts.MessageFontWeight;
        }
    }
}
