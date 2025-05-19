using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject4
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void SearchPatients_FuzzySearch_ReturnsMatchingResults()
        {
            // Arrange
            var patients = new List<Пациенты>
        {
            new Пациенты { фамилия = "Иванов", имя = "Иван" },
            new Пациенты { фамилия = "Петров", имя = "Петр" },
            new Пациенты { фамилия = "Сидоров", имя = "Сидор" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Пациенты>>();
            mockSet.As<IQueryable<Пациенты>>().Setup(m => m.Provider).Returns(patients.Provider);
            mockSet.As<IQueryable<Пациенты>>().Setup(m => m.Expression).Returns(patients.Expression);
            mockSet.As<IQueryable<Пациенты>>().Setup(m => m.ElementType).Returns(patients.ElementType);
            mockSet.As<IQueryable<Пациенты>>().Setup(m => m.GetEnumerator()).Returns(patients.GetEnumerator());

            var mockDbContext = new Mock<МедЛабDbContext>();
            mockDbContext.Setup(c => c.Пациенты).Returns(mockSet.Object);

            var patientService = new PatientService(mockDbContext.Object);

            // Act
            var result = patientService.SearchPatients("Иванов", 2); // Макс. расстояние Левенштейна = 2

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Иванов", result.First().фамилия);
        }
    }
}
