namespace CodeCafeIRC.messaging
{
    interface IFormattedMessage
    {
        FormattingRule TimeStampFormat { get; set; }
        FormattingRule SenderFormat { get; set; }
        FormattingRule ContentFormat { get; set; }
    }
}
