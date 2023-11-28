using Equinor.ProCoSys.Common.Email;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Common.Tests.Email
{
    [TestClass]
    public class EmailValidatorTests
    {
        [TestMethod]
        public void ShouldBeValidEmailAddresses()
        {
            Assert.IsTrue(EmailValidator.IsValid("test@equinor.com"));
            Assert.IsTrue(EmailValidator.IsValid("test_user@equinor.com"));
            Assert.IsTrue(EmailValidator.IsValid("TEST@EQUINOR.COM"));
            Assert.IsTrue(EmailValidator.IsValid("test.email@equinor.com"));
            Assert.IsTrue(EmailValidator.IsValid("test@gmail.co.com"));
        }

        [TestMethod]
        public void ShouldBeInValidEmailAddresses()
        {
            Assert.IsFalse(EmailValidator.IsValid("test@equinor"));
            Assert.IsFalse(EmailValidator.IsValid("@equinor.com"));
            Assert.IsFalse(EmailValidator.IsValid("test@"));
            Assert.IsFalse(EmailValidator.IsValid("test@.com"));
            Assert.IsFalse(EmailValidator.IsValid("test"));
            Assert.IsFalse(EmailValidator.IsValid("test.com"));
            Assert.IsFalse(EmailValidator.IsValid("test@equinor,com"));
            Assert.IsFalse(EmailValidator.IsValid("test@.equinor.com"));
            Assert.IsFalse(EmailValidator.IsValid("test@gmail@equinor.com"));
        }
    }
}
