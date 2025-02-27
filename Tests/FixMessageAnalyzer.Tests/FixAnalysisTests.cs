using FixMessageAnalyzer.Data;
using FixMessageAnalyzer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FixMessageAnalyzer.Tests.Services
{
    [TestClass]
    public class FixAnalyticsServiceTests
    {
        [TestMethod]
        public async Task GetSymbolDistributionAsync_ReturnsCorrectDistribution()
        {
            // Arrange
            var symbolCounts = new List<SymbolCount>
            {
                new SymbolCount { Symbol = "AAPL", MessageCount = 5 },
                new SymbolCount { Symbol = "GOOG", MessageCount = 3 },
                new SymbolCount { Symbol = null, MessageCount = 2 } // Should map to "Unknown"
            };

            // Create mock DbSet
            var mockSymbolSet = CreateMockDbSet(symbolCounts);

            // Create a mock DbContext
            var mockContext = new Mock<FixDbContext>();

            // Setup the FromSqlRaw method to return our mock DbSet
            mockContext
                .Setup(c => c.Set<SymbolCount>().FromSqlRaw(It.IsAny<string>()))
                .Returns(mockSymbolSet.Object);

            // Create service with mock context
            var service = new FixAnalyticsService(mockContext.Object);

            // Act
            var result = await service.GetSymbolDistributionAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count); // Expecting 3 entries (AAPL, GOOG, Unknown)
            Assert.AreEqual(5, result["AAPL"]); // Verify AAPL count
            Assert.AreEqual(3, result["GOOG"]); // Verify GOOG count
            Assert.AreEqual(2, result["Unknown"]); // Verify null/unknown count
        }

        [TestMethod]
        public async Task GetSymbolDistributionAsync_EmptyResult_ReturnsEmptyDictionary()
        {
            // Arrange
            var emptyList = new List<SymbolCount>();
            var mockEmptySet = CreateMockDbSet(emptyList);

            var mockContext = new Mock<FixDbContext>();
            mockContext
                .Setup(c => c.Set<SymbolCount>().FromSqlRaw(It.IsAny<string>()))
                .Returns(mockEmptySet.Object);

            var service = new FixAnalyticsService(mockContext.Object);

            // Act
            var result = await service.GetSymbolDistributionAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count); // Expecting an empty dictionary
        }

        // Helper method to create mock DbSet
        private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> entities) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            var queryable = entities.AsQueryable();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            // Setup async enumeration
            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(entities.GetEnumerator()));

            // Setup ToListAsync
            mockSet.Setup(m => m.ToListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(entities);

            return mockSet;
        }
    }

    // AsyncEnumerator implementation for testing async LINQ methods
    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }
}