using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MimeKit;

namespace make_fwd
{
    class Program
    {
        const string SENDER = "no-reply@masked-emails.me";

        static void Main(string[] args)
        {
            // TODO: command-line handling
            // TODO: file exists?
            // TODO: configurable sender no-reply@masked-emails.me

            // TODO: -o --output

            var input = args[0];
            var recipient = args[1];
            var sender = SENDER;

            var message = ParseMimeMessage(input);
            var forwarded = ForwardTo(message, sender, recipient);

            // redirect console output

            using (var memoryStream = new MemoryStream())
            {
                forwarded.WriteTo(memoryStream);
                Console.Out.Write(Encoding.ASCII.GetString(memoryStream.ToArray()));
            }
        }

        private static MimeMessage ForwardTo(MimeMessage message, string sender, string recipient)
        {
            var outgoing = new MimeMessage();
            outgoing.From.Add(new MailboxAddress("Masked Emails", sender));
            outgoing.To.Add(new MailboxAddress(recipient, recipient));
            outgoing.Subject = "Fwd: " + message.Subject;

            var attachments = GetMessageAttachments(message);
            var plain = message.MakeForwardedTextPlainPart();
            var html = message.MakeForwardedHtmlPart();

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

        private static IList<MimePart> GetMessageAttachments(MimeMessage message)
        {
            var attachments = new List<MimePart>();
            var iter = new MimeIterator(message);

            // collect our list of attachments and their parent multiparts
            while (iter.MoveNext())
            {
                var multipart = iter.Parent as Multipart;
                var part = iter.Current as MimePart;

                if (multipart != null && part != null && part.IsAttachment)
                    attachments.Add(part);
            }

            return attachments;
        }
    }
}
