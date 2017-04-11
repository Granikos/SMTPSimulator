using System.IO;
using log4net.Util;

namespace Granikos.SMTPSimulator.Core.Logging
{
    public class EndRowConverter : PatternConverter
    {
        protected override void Convert(TextWriter writer, object state)
        {
            var ctw = writer as CsvTextWriter;
            // write the ending quote for the last field
            if (ctw != null)
                ctw.WriteQuote();
            writer.WriteLine();
        }
    }
}