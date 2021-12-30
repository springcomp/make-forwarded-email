using System.Text;
using MimeKit;

namespace make_fwd
{
    public static class MimeMessageExtensions
    {
        const string INDENT = "   ";

        public static string GetPlainTextForwardedHeader(this MimeMessage message)
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
    }
}
