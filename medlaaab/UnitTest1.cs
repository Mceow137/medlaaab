using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnitTestProject1
{
    [TestClass]
    public class BarcodeServiceTests
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

    internal class BarcodeService
    {
        public BarcodeService()
        {
        }

        internal string GenerateBarcode(int orderId)
        {
            throw new NotImplementedException();
        }
    }
}
