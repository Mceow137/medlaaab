using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject5
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void IsUserBlocked_WhenBlocked_ReturnsTrue()
        {
            // Arrange
            var mockDbContext = new Mock<МедЛабDbContext>();
            var blocks = new List<Блокировки>
        {
            new Блокировки
            {
                ip_адрес = "192.168.1.1",
                время_начала = DateTime.Now.AddMinutes(-10),
                длительность_минуты = 30
            }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Блокировки>>();
            mockSet.As<IQueryable<Блокировки>>().Setup(m => m.Provider).Returns(blocks.Provider);
            mockSet.As<IQueryable<Блокировки>>().Setup(m => m.Expression).Returns(blocks.Expression);
            mockSet.As<IQueryable<Блокировки>>().Setup(m => m.ElementType).Returns(blocks.ElementType);
            mockSet.As<IQueryable<Блокировки>>().Setup(m => m.GetEnumerator()).Returns(blocks.GetEnumerator());

            mockDbContext.Setup(c => c.Блокировки).Returns(mockSet.Object);

            var blockingService = new BlockingService(mockDbContext.Object);

            // Act
            var result = blockingService.IsIpBlocked("192.168.1.1");

            // Assert
            Assert.IsTrue(result);
        }
    }
}
