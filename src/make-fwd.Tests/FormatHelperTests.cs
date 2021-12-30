using System;
using Xunit;

namespace make_fwd.Tests
{
    public class FormatHelperTests
    {
        [Fact]
        public void FormatHelper_QuotedPrintablePlainText()
        {
            const string plainText = @"
Ce message est envoyé depuis Outlook à destination de maskedbox.space.
Ceci est pour tester la chaîne de redirection.

Provenance : Courrier<https://go.microsoft.com/fwlink/?LinkId=3D550986> pour Windows 10";

            const string expected = "\nCe message est envoy=C3=A9 depuis Outlook =C3=A0 destination de masked=\nbox.space.\nCeci est pour tester la cha=C3=AEne de redirection.\n\nProvenance : Courrier<https://go.microsoft.com/fwlink/?LinkId=3D3D5509=\n86> pour Windows 10";

            var actual = FormatHelper.EncodeQuotedPrintableText(plainText);
            System.Diagnostics.Debug.WriteLine(actual);
            System.Diagnostics.Debug.WriteLine(expected);

            Assert.Equal(expected, actual);
        }
    }
}
