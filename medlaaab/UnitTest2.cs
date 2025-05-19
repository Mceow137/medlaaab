using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject2
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CreateOrder_WithValidData_CreatesOrderInDatabase()
        {
            // Arrange
            var mockDbContext = new Mock<МедЛабDbContext>();
            var orders = new List<Заказы>().AsQueryable();

            var mockSet = new Mock<DbSet<Заказы>>();
            mockSet.As<IQueryable<Заказы>>().Setup(m => m.Provider).Returns(orders.Provider);
            mockSet.As<IQueryable<Заказы>>().Setup(m => m.Expression).Returns(orders.Expression);
            mockSet.As<IQueryable<Заказы>>().Setup(m => m.ElementType).Returns(orders.ElementType);
            mockSet.As<IQueryable<Заказы>>().Setup(m => m.GetEnumerator()).Returns(orders.GetEnumerator());

            mockDbContext.Setup(c => c.Заказы).Returns(mockSet.Object);
            mockDbContext.Setup(c => c.SaveChanges()).Returns(1);

            var orderService = new OrderService(mockDbContext.Object);

            // Act
            var result = orderService.CreateOrder(1, 1, new List<int> { 1001, 1002 });

            // Assert
            Assert.IsTrue(result.Success);
            mockDbContext.Verify(m => m.SaveChanges(), Times.Once);
        }
    }
}
