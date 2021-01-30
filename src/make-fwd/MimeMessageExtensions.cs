using System;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using MimeKit;

namespace make_fwd
{
    public static class MimeMessageExtensions
    {
        public static MimeEntity MakeForwardedTextPlainPart(this MimeMessage message)
        {
            return message.MakeForwardedTextPlainPart(
                message.GetForwardedPlainTextContent()
                );
        }
        public static MimeEntity MakeForwardedTextPlainPart(this MimeMessage message, string rawText)
        {
            return rawText == null ? null : MakeTextPart("plain", rawText);
        }

        public static string GetForwardedPlainTextContent(this MimeMessage message)
        {
            return message.GetForwardedPlainTextContent(
                message.GetForwardedPlainTextHeader()
                );
        }
        public static string GetForwardedPlainTextContent(this MimeMessage message, string header)
        {
            var text = message.TextBody;
            if (text == null)
                return null;

            return GetForwardedPlainTextContent(text, header);
        }
        public static string GetForwardedPlainTextContent(string plainText, string header)
        {
            const string INDENT = "   ";

            var text = new StringBuilder(header);

            using (var reader = new StringReader(plainText))
            {
                var line = "";
                while ((line = reader.ReadLine()) != null)
                    text
                    .Append(INDENT)
                    .AppendLine(line)
                    ;
            }

            string rawText = _.LF(text.ToString());

            return rawText;
        }

        public static string GetForwardedPlainTextHeader(this MimeMessage message)
        {
            const string fwd = @"

    ----- Forwarded Message -----
    From: %{FROM}%
    Date : %{DATE}%
    To: %{TO}%
    Subject : %{SUBJECT}%
    
";

            return _.LF(fwd
                .Replace("%{FROM}%", FormatPlainTextAddressList(message.From))
                .Replace("%{TO}%", FormatPlainTextAddressList(message.To))
                .Replace("%{DATE}%", message.Date.ToString("R"))
                .Replace("%{SUBJECT}%", message.Subject)
            );
        }

        private static string FormatPlainTextAddressList(InternetAddressList addressList)
        {
            var addresses = new StringBuilder();
            foreach (var address in addressList)
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

        public static MimeEntity MakeForwardedHtmlPart(this MimeMessage message)
        {
            return message.MakeForwardedHtmlPart(
                    message.GetForwardedHtmlDocument()
                    );
        }
        public static MimeEntity MakeForwardedHtmlPart(this MimeMessage message, string html)
        {
            return html == null ? null : MakeTextPart("html", html);
        }

        public static string GetForwardedHtmlDocument(this MimeMessage message)
        {
            var html = message.HtmlBody;
            if (html == null)
                return null;

            const string divContainerStart = @"<div class=""forward-container""><br><br> -------- Forwarded Message --------";
            const string divContainerEnd = @"</div><!-- forward-container -->";
            const string tableStart = @"<table class=""moz-email-headers-table"" cellspacing=""0"" cellpadding=""0"" border=""0""><tbody><tr>";
            const string tableEnd = @"</tr></tbody></table><br><br>";

            const string tableNewRow = "</tr><tr>";

            const string header = @"<th valign=""BASELINE"" nowrap=""nowrap"" align=""RIGHT"">%{HEADER}%: </th>";
            const string headerPlaceholder = "%{HEADER}%";

            const string headerValue = @"<td>%{HEADER_VALUE}%</td>";
            const string headerValuePlaceholder = "%{HEADER_VALUE}%";

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var body = document.DocumentNode.SelectSingleNode("//body");

            var raw = new StringBuilder();
            raw
                .AppendLine(divContainerStart)
                .AppendLine(tableStart)

                .AppendLine(header.Replace(headerPlaceholder, "From"))
                .Append(FormatHtmlAddressList(message.From))
                .AppendLine(tableNewRow)

                .AppendLine(header.Replace(headerPlaceholder, "To"))
                .Append(FormatHtmlAddressList(message.To))
                .AppendLine(tableNewRow)

                .AppendLine(header.Replace(headerPlaceholder, "Date"))
                .Append(headerValue.Replace(headerValuePlaceholder, message.Date.ToString("R")))
                .AppendLine(tableNewRow)

                .AppendLine(header.Replace(headerPlaceholder, "Subject"))
                .AppendLine(headerValue.Replace(headerValuePlaceholder, message.Subject))

                .AppendLine(tableEnd)
                .AppendLine(body.InnerHtml)
                .Append(divContainerEnd)
                ;

            System.Diagnostics.Debug.WriteLine(raw);

            var innerHtml = raw.ToString();
            var newBody = HtmlNode.CreateNode(innerHtml);
            body.ParentNode.ReplaceChild(newBody, body);

            var outerHtml = document.DocumentNode.OuterHtml;

            return _.LF(outerHtml);
        }

        private static string FormatHtmlAddressList(InternetAddressList adressList)
        {
            const string emailAddress = @"%{NAME}% <a class=""txt-link-rfc2396E"" href=""%{EMAIL}%"">&lt;%{EMAIL}%&gt;</a>";
            const string namePlaceholder = "%{NAME}%";
            const string emailPlaceholder = "%{EMAIL}%";

            var addresses = new StringBuilder();
            foreach (var address in adressList.OfType<MailboxAddress>())
            {
                var fragment = emailAddress
                    .Replace(namePlaceholder, String.IsNullOrEmpty(address.Name) ? address.Address : address.Name)
                    .Replace(emailPlaceholder, address.Address)
                    ;

                addresses
                    .Append(fragment)
                    .Append(", ")
                    ;
            }

            var list = addresses
                .Remove(addresses.Length - 2, 2)
                .ToString()
                ;

            return $"<td>{list}</td>";
        }

        private static MimeEntity MakeTextPart(string subType, string rawText)
        {
            return new TextPart(subType)
            {
                ContentTransferEncoding = ContentEncoding.QuotedPrintable,
                Text = rawText,
            };
        }

        public static MimeEntity GetTextBodyPart(this MimeMessage message)
        {
            return message.BodyParts.FirstOrDefault(IsTextBodyPart);
        }
        private static bool IsTextBodyPart(MimeEntity arg)
        {
            return arg.ContentType.MimeType == "text/plain";
        }
        public static MimeEntity GetHtmlBodyPart(this MimeMessage message)
        {
            return message.BodyParts.FirstOrDefault(IsHtmlBodyPart);
        }
        private static bool IsHtmlBodyPart(MimeEntity arg)
        {
            return arg.ContentType.MimeType == "text/html";
        }
    }
    public static class _
    {
        public static string LF(string text)
            { return text.Replace("\r\n", "\n"); }
    }
}
