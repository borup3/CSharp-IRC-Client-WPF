using System.Windows;

namespace CodeCafeIRC.messaging
{
    public sealed class DebugMessage : Message
    {
        public DebugMessage(string message)
            : base(message)
        {
            Sender = "[Debug]";
            ResetFormatting();
        }

        public override void ResetFormatting()
        {
            TimeStampFormat = FormattingRule.TimeStampRule;

            SenderFormat = new FormattingRule();
            SenderFormat.FontWeight = FontWeights.Normal;

            ContentFormat = new FormattingRule();
            ContentFormat.FontWeight = FontWeights.Normal;
        }
    }
}
