using System.IO;
using System.Text;

namespace Granikos.Hydra.Core.Logging
{
    public class CsvTextWriter : TextWriter
    {
        private readonly TextWriter _textWriter;

        public CsvTextWriter(TextWriter textWriter)
        {
            _textWriter = textWriter;
        }

        public override Encoding Encoding
        {
            get { return _textWriter.Encoding; }
        }

        public override void Write(char value)
        {
            _textWriter.Write(value);
            // double all quotes
            if (value == '"')
                _textWriter.Write(value);
        }

        public void WriteQuote()
        {
            // write a literal (unescaped) quote
            _textWriter.Write('"');
        }
    }
}