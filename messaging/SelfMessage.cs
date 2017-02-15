namespace CodeCafeIRC.messaging
{
    public sealed class SelfMessage : Message
    {
        private string m_username;
        public SelfMessage(string username, string sender, string content)
            : base(sender, content)
        {
            m_username = username;
            Sender += ":";
            ResetFormatting();
        }

        public override void ResetFormatting()
        {
            TimeStampFormat = FormattingRule.TimeStampRule;

            SenderFormat = new FormattingRule();
            SenderFormat.Foreground = m_username ?? "LiveChat You";

            ContentFormat = new FormattingRule();
            ContentFormat.Foreground = "LiveChat Own Message";
        }
    }
}
