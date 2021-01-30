using MimeKit;
using System.IO;
using System.Text;
using Xunit;

namespace make_fwd.Tests
{
    public class MessageHelperTests
    {
        [Fact]
        public void MessageHelperTests_GetPlainTextForwardedHeader()
        {
            const string eml = @"Subject: Hello, world!
Date: Mon, 22 Jan 2021 17:45:33 GMT
From: no-reply@masked-emails.me
To: postmaster@masked-emails.me

Hello, world!

This is a simple text paragraph that is
used to test rendering text output.

--
See ya!";

            const string expected = @"

    ----- Forwarded Message -----
    From: no-reply@masked-emails.me
    Date : Fri, 22 Jan 2021 17:45:33 GMT
    To: postmaster@masked-emails.me
    Subject : Hello, world!
    
";

            // system under test

            var message = MimeMessage.Load(new MemoryStream(Encoding.ASCII.GetBytes(eml)));
            var actual = message.GetForwardedPlainTextHeader();

            // assert expectations

            Assert.Equal(_.LF(expected), actual);
        }

        [Fact]
        public void MessageHelper_GetForwardedHtmlDocument()
        {
            const string eml = @"Subject: Hello, world!
Date: Mon, 22 Jan 2021 17:45:33 GMT
From: no-reply@masked-emails.me
To: postmaster@masked-emails.me
Content-Type: text/html; charset=""utf-8""

<html>
<head></head>
<body>
<p>
This is the body of the original message.</p>
</body>
";


            const string expected = @"<html>
<head></head>
<div class=""forward-container""><br><br> -------- Forwarded Message --------
<table class=""moz-email-headers-table"" cellspacing=""0"" cellpadding=""0"" border=""0""><tbody><tr>
<th valign=""BASELINE"" nowrap=""nowrap"" align=""RIGHT"">From: </th>
<td>no-reply@masked-emails.me <a class=""txt-link-rfc2396E"" href=""no-reply@masked-emails.me"">&lt;no-reply@masked-emails.me&gt;</a></td></tr><tr>
<th valign=""BASELINE"" nowrap=""nowrap"" align=""RIGHT"">To: </th>
<td>postmaster@masked-emails.me <a class=""txt-link-rfc2396E"" href=""postmaster@masked-emails.me"">&lt;postmaster@masked-emails.me&gt;</a></td></tr><tr>
<th valign=""BASELINE"" nowrap=""nowrap"" align=""RIGHT"">Date: </th>
<td>Fri, 22 Jan 2021 17:45:33 GMT</td></tr><tr>
<th valign=""BASELINE"" nowrap=""nowrap"" align=""RIGHT"">Subject: </th>
<td>Hello, world!</td>
</tr></tbody></table><br><br>

<p>
This is the body of the original message.</p>

</div>
</html>";

            // system under test

            var message = MimeMessage.Load(new MemoryStream(Encoding.ASCII.GetBytes(eml)));
            var actual = message.GetForwardedHtmlDocument();

            // assert expectations

            Assert.Equal(_.LF(expected), actual);
        }
    }
}
