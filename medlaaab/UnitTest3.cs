using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.RegularExpressions;

namespace UnitTestProject3
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GenerateBarcode_ReturnsValidFormat()
        {
            // Arrange
            var barcodeService = new BarcodeService();
            var orderId = 123;
            var expectedPattern = @"^\d{3} \d{8} \d{6}$"; // 123 01112023 123456

            // Act
            var result = barcodeService.GenerateBarcode(orderId);

            // Assert
            Assert.IsTrue(Regex.IsMatch(result, expectedPattern));
        }
    }
}
