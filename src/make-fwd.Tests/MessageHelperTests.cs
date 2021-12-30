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
            var actual = message.GetPlainTextForwardedHeader();

            // assert expectations

            Assert.Equal(expected, actual);
        }
    }
}
