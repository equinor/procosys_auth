using Equinor.ProCoSys.Common.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Common.Tests.Misc;

[TestClass]
public class StringExtensionsTests
{
    [TestMethod]
    public void IsEmpty_IsTrue_OnEmptyString()
    {
        // Act
        var result = "".IsEmpty();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsEmpty_IsTrue_OnNullString()
    {
        // Arrange
        string s = null;

        // Act
        // ReSharper disable once ExpressionIsAlwaysNull
        var result = s.IsEmpty();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsEmpty_IsFalse_OnString()
    {
        // Act
        var result = "a".IsEmpty();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ToPascalCase_ShouldTransform_ToPascalCase()
    {
        // Act
        var result = "camelCased".ToPascalCase();

        // Assert
        Assert.AreEqual("CamelCased", result);
    }
}
