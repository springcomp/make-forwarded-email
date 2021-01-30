using System;
using System.Text;
using MimeKit.Encodings;

namespace make_fwd
{
    public sealed class FormatHelper
    { 
        public static string EncodeQuotedPrintableText(string text)
        {
            var encoder = new QuotedPrintableEncoder();
            var buffer = Encoding.UTF8.GetBytes(text);
            var multiplier = 3.0d;

            while (true)
            {
                try
                {
                    var length = (int)Math.Floor(buffer.Length * multiplier);
                    var output = new byte[length];
                    var count = encoder.Encode(buffer, 0, buffer.Length, output);

                    return Encoding.ASCII.GetString(output, 0, count);
                }
                catch (ArgumentException)
                {
                    multiplier *= 1.44;
                }
            }
        }
    }
}
