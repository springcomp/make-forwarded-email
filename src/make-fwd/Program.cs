using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MimeKit;
using MimeKit.Encodings;
using MimeKit.Utils;

namespace make_fwd
{
    class Program
    {
        const string INDENT = "   ";

        static void Main(string[] args)
        {
            var input = args[0];
            var recipient = args[1];

            var message = ParseMimeMessage(input);
            var forwarded = ForwardTo(message, recipient);

            // redirect console output

            using (var memoryStream = new MemoryStream())
            {
                forwarded.WriteTo(memoryStream);
                Console.Out.Write(Encoding.ASCII.GetString(memoryStream.ToArray()));
            }
        }

        private static MimeMessage ForwardTo(MimeMessage message, string recipient)
        {
            var outgoing = new MimeMessage();
            outgoing.From.Add(new MailboxAddress("Masked Emails", "no-reply@masked-emails.me"));
            outgoing.To.Add(new MailboxAddress(recipient, recipient));
            outgoing.Subject = "Fwd: " + message.Subject;

            var attachments = GetMessageAttachments(message);
            var plain = MakeTextBodyPart(message, GetPlainTextForwardedHeader(message));
            var html = GetHtmlBodyPart(message);

            var messagePart = GetMessagePart(plain, html);
            var hasAttachments = attachments?.Count() > 0;

            MimeEntity bodyPart = null;

            if (hasAttachments)
            {
                // create the multipart/mixed container to hold the multipart/alternative
                // and the image attachment
                var multipart = new Multipart("mixed");
                multipart.Add(messagePart);
                foreach (var attachment in attachments)
                    multipart.Add(attachment);

                bodyPart = multipart;
            }
            else
            {
                bodyPart = messagePart;
            }

            outgoing.Body = bodyPart;

            return outgoing;
        }
        private static string GetPlainTextForwardedHeader(MimeMessage message)
        {
            const string fwd = @"

    ----- Forwarded Message -----
    From: %{FROM}%
    Date : %{DATE}%
    To: %{TO}%
    Subject : %{SUBJECT}%
    
";

            return fwd
                .Replace("%{FROM}%", FormatAddressList(message.From))
                .Replace("%{TO}%", FormatAddressList(message.To))
                .Replace("%{DATE}%", message.Date.ToString("R"))
                .Replace("%{SUBJECT}%", message.Subject)
                ;
        }

        private static string FormatAddressList(InternetAddressList to)
        {
            var addresses = new StringBuilder();
            foreach (var address in to)
            {
                addresses
                    .Append(address)
                    .Append(", ")
                    ;
            }

            return addresses
                .Remove(addresses.Length - 2, 2)
                .ToString()
                ;
        }

        private static MimeEntity MakeTextBodyPart(MimeMessage message, string header)
        {
            var text = new StringBuilder(header);

            using (var reader = new StringReader(message.TextBody))
            {
                var line = "";
                while ((line = reader.ReadLine()) != null)
                    text
                    .Append(INDENT)
                    .AppendLine(line)
                    ;
            }

            var textPart = new TextPart("plain")
            {
                ContentTransferEncoding = ContentEncoding.QuotedPrintable,
                Text = EncodeQuotedPrintableText(text.ToString()),
            };

            return textPart;
        }

        private static string EncodeQuotedPrintableText(string text)
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

        private static MimeEntity GetMessagePart(MimeEntity plain, MimeEntity html)
        {
            var hasPlain = plain != null;
            var hasHtml = html != null;

            if (hasPlain && hasHtml)
            {
                // Note: it is important that the text/html part is added second, because it is the
                // most expressive version and (probably) the most faithful to the sender's WYSIWYG 
                // editor.
                var alternative = new Multipart("alternative");
                alternative.Add(plain);
                alternative.Add(html);
                return alternative;
            }
            else
            {
                return html ?? plain;
            }
        }

        private static MimeMessage ParseMimeMessage(string input)
        {
            MimeMessage message = null;

            using (var stream = File.OpenRead(input))
                message = MimeMessage.Load(stream);
            return message;
        }

        private static MimeEntity GetTextBodyPart(MimeMessage message)
        {
            return message.BodyParts.FirstOrDefault(IsTextBodyPart);
        }
        private static MimeEntity GetHtmlBodyPart(MimeMessage message)
        {
            return message.BodyParts.FirstOrDefault(IsHtmlBodyPart);
        }
        private static IList<MimePart> GetMessageAttachments(MimeMessage message)
        {
            return new List<MimePart>();
        }
        private static bool IsTextBodyPart(MimeEntity arg)
        {
            return arg.ContentType.MimeType == "text/plain";
        }
        private static bool IsHtmlBodyPart(MimeEntity arg)
        {
            return arg.ContentType.MimeType == "text/html";
        }
    }
}
