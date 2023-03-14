using Equinor.ProCoSys.Common.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Common.Tests.Misc
{
    [TestClass]
    public class EnumExtensionsTests
    {
        private enum TestEnums
        {
            [System.ComponentModel.Description("Some enum description")]
            EnumWithDescription,
            EnumWithoutDescription
        }

        [TestMethod]
        public void GetDescription_ShouldReturnDescription()
        {
            const string expected = "Some enum description";
            var actual = TestEnums.EnumWithDescription.GetDescription();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetDescription_WhenNoDescription_ShouldReturnEventType()
        {
            var expected = TestEnums.EnumWithoutDescription.ToString();
            var actual = TestEnums.EnumWithoutDescription.GetDescription();
            Assert.AreEqual(expected, actual);
        }
    }
}
