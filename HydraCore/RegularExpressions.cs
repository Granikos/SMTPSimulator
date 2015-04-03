using System.Text.RegularExpressions;

namespace HydraCore
{
    public static class RegularExpressions
    {
        public const string DomainNamePattern = @"(?i)([a-z](\-?[a-z0-9]+)*\.)+[a-z](\-?[a-z0-9]+)*(?-i)";
        public const string DomainPattern = @"(?:\[[^\]]+\]|" + DomainNamePattern + ")";
        public const string QuotedStringPattern = @"""(?:\\.|[^""\\]+)*""";
        public const string DotStringPattern = @"\w+(?:\.\w+)*";
        public const string AtDomainList = "@" + DomainPattern + "(,@" + DomainPattern + ")*";

        public const string MailboxPattern =
            "(?<LocalPart>" + DotStringPattern + "|" + QuotedStringPattern + ")@(?<Domain>" + DomainPattern + ")";

        public const string PathPattern = "<(?:(?<AtDomains>" + AtDomainList + "):)?" + MailboxPattern + ">";
        public static readonly Regex PathRegex = new Regex("^" + PathPattern + "$", RegexOptions.Compiled);
        public static readonly Regex DotStringRegex = new Regex("^" + DotStringPattern + "$", RegexOptions.Compiled);
    }
}