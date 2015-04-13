namespace HydraCore
{
    public enum SMTPStatusCode
    {
        SyntaxError = 500,
        ParamError = 501,
        NotImplemented = 502,
        BadSequence = 503,
        ParamNotImplemented = 504,

        SystemStatus = 211,
        Help = 214,
        Ready = 220,
        Closing = 221,
        NotAvailiable = 421,

        Okay = 250,
        WillForward = 251,
        CannotVerify = 252,
        MailboxBusy = 450,
        MailboxUnavailiableError = 550,
        ProcessingError = 451,
        InsufficientStorage = 452,
        ExceededStorage = 552,
        MailboxNameNotAllowed = 553,
        StartMailInput = 354,
        TransactionFailed = 554,

        AuthContinue = 334,
        AuthTooWeak = 534,
        AuthEncryptionRequired = 538,
        AuthRequired = 530,
        AuthFailed = 535,
        AuthSuccess = 235,

        TLSNotAvailiable = 454
    }
}