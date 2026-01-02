using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.Core.Transactions;

namespace SujaySarma.Data.Core.Tests.Transactions
{
    [TestClass]
    [TestCategory("Functional")]
    public class BatchCollectionTests
    {
        [TestMethod(DisplayName = "BatchCollection: Bounds check: cannot set zero-sized batch")]
        public void Constructor_WithZero_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BatchCollection(0u));
        }

        [TestMethod(DisplayName = "BatchCollection: Bounds check: create batch successfully")]
        public void Constructor_WithValidBatchSize_CreatesInstance()
        {
            var batch = new BatchCollection(10u);
            Assert.IsNotNull(batch);
            Assert.AreEqual(0, batch.Count);
        }

        [TestMethod(DisplayName = "BatchCollection: Adding entity: NULLs not allowed")]
        public void Add_Null_ThrowsArgumentNullException()
        {
            var batch = new BatchCollection(1u);
            Assert.Throws<ArgumentNullException>(() => batch.Add(null!));
        }

        [TestMethod(DisplayName = "BatchCollection: Adding entity: Valid entities increase Count correctly")]
        public void Add_ValidEntity_IncreasesCount()
        {
            var batch = new BatchCollection(5u);
            batch.Add("one");
            Assert.AreEqual(1, batch.Count);
            
            batch.Add("two").Add(3);
            Assert.AreEqual(3, batch.Count);
        }

        [TestMethod(DisplayName = "BatchCollection: Adding entity: Adding 100s of items, Count is correct")]
        public void Add_HundredsOfItems_TracksCountCorrectly()
        {
            var batch = new BatchCollection(50u);
            
            for (int i = 0; i < 500; i++)
            {
                batch.Add(i);
            }
            
            Assert.AreEqual(500, batch.Count);
        }

        [TestMethod(DisplayName = "BatchCollection: Batching: Returns all when collection is smaller than batch size")]
        public void GetNext_WithLessThanBatchSize_ReturnsAllItems()
        {
            var batch = new BatchCollection(10u);
            batch.Add(1).Add(2).Add(3);

            var items = batch.GetNext().Cast<object>().ToArray();
            
            CollectionAssert.AreEqual(new object[] { 1, 2, 3 }, items);
            Assert.AreEqual(0, batch.Count);
        }

        [TestMethod(DisplayName = "BatchCollection: Batching: Returns all items when collection is exactly the batch size")]
        public void GetNext_WithExactlyBatchSize_ReturnsAllItems()
        {
            var batch = new BatchCollection(3u);
            batch.Add(1).Add(2).Add(3);

            var items = batch.GetNext().Cast<object>().ToArray();
            
            CollectionAssert.AreEqual(new object[] { 1, 2, 3 }, items);
            Assert.AreEqual(0, batch.Count);
        }

        [TestMethod(DisplayName = "BatchCollection: Batching: Returns correct number of items when collection > batch size")]
        public void GetNext_WithMoreThanBatchSize_ReturnsOnlyBatchSizeItems()
        {
            var batch = new BatchCollection(3u);
            batch.Add(1).Add(2).Add(3).Add(4).Add(5);

            var first = batch.GetNext().Cast<object>().ToArray();
            
            CollectionAssert.AreEqual(new object[] { 1, 2, 3 }, first);
            Assert.AreEqual(2, batch.Count);

            var second = batch.GetNext().Cast<object>().ToArray();
            CollectionAssert.AreEqual(new object[] { 4, 5 }, second);
            Assert.AreEqual(0, batch.Count);
        }

        [TestMethod(DisplayName = "BatchCollection: Batching (300): Returns correct number of items when collection > batch size")]
        public void GetNext_HundredsOfItems_ProcessesInBatches()
        {
            const uint batchSize = 25u;
            const int totalItems = 300;
            var batch = new BatchCollection(batchSize);
            
            // Add 300 items
            for (int i = 0; i < totalItems; i++)
            {
                batch.Add(i);
            }
            
            Assert.AreEqual(totalItems, batch.Count);
            
            int expectedValue = 0;
            int batchCount = 0;
            
            // Process all batches
            while (batch.Count > 0)
            {
                var items = batch.GetNext().Cast<int>().ToArray();
                batchCount++;
                
                // Verify batch size (should be batchSize except possibly the last batch)
                if (batch.Count > 0)
                {
                    Assert.HasCount((int)batchSize, items);
                }
                else
                {
                    Assert.IsLessThanOrEqualTo(items.Length, (int)batchSize);
                }
                
                // Verify items are in correct order
                foreach (var item in items)
                {
                    Assert.AreEqual(expectedValue, item);
                    expectedValue++;
                }
            }
            
            Assert.AreEqual(totalItems / (int)batchSize, batchCount);
            Assert.AreEqual(totalItems, expectedValue);
        }

        [TestMethod(DisplayName = "BatchCollection: Batching (1000): Returns correct number of items when collection > batch size")]
        public void GetNext_LargeBatchWith1000Items_ProcessesCorrectly()
        {
            const uint batchSize = 100u;
            const int totalItems = 1000;
            var batch = new BatchCollection(batchSize);
            
            // Add 1000 items with string values
            for (int i = 0; i < totalItems; i++)
            {
                batch.Add($"Item_{i:D4}");
            }
            
            Assert.AreEqual(totalItems, batch.Count);
            
            int processedCount = 0;
            int batchNumber = 0;
            
            while (batch.Count > 0)
            {
                var items = batch.GetNext().Cast<string>().ToArray();
                batchNumber++;
                
                foreach (var item in items)
                {
                    Assert.AreEqual($"Item_{processedCount:D4}", item);
                    processedCount++;
                }
            }
            
            Assert.AreEqual(10, batchNumber); // 1000 / 100 = 10 batches
            Assert.AreEqual(totalItems, processedCount);
            Assert.AreEqual(0, batch.Count);
        }

        [TestMethod(DisplayName = "BatchCollection: Batching (500): Returns correct number of items when collection > batch size")]
        public void GetNext_500ItemsWithOddBatchSize_ProcessesCorrectly()
        {
            const uint batchSize = 47u; // Odd number that doesn't divide evenly
            const int totalItems = 500;
            var batch = new BatchCollection(batchSize);
            
            for (int i = 0; i < totalItems; i++)
            {
                batch.Add(i);
            }
            
            int totalProcessed = 0;
            
            while (batch.Count > 0)
            {
                var items = batch.GetNext().Cast<int>().ToArray();
                totalProcessed += items.Length;
            }
            
            Assert.AreEqual(totalItems, totalProcessed);
            Assert.AreEqual(0, batch.Count);
        }

        [TestMethod(DisplayName = "BatchCollection: Batching: Returns nothing on empty collection")]
        public void GetNext_EmptyCollection_ReturnsNoItems()
        {
            var batch = new BatchCollection(10u);
            
            var items = batch.GetNext().Cast<object>().ToArray();
            
            Assert.IsEmpty(items);
        }

        [TestMethod(DisplayName = "BatchCollection: Batching (250): Returns correct number of items when collection > batch size")]
        public void Clear_WithHundredsOfItems_EmptiesCollection()
        {
            var batch = new BatchCollection(50u);
            
            for (int i = 0; i < 250; i++)
            {
                batch.Add(i);
            }
            
            Assert.AreEqual(250, batch.Count);

            batch.Clear();
            
            Assert.AreEqual(0, batch.Count);
            
            var items = batch.GetNext().Cast<object>().ToArray();
            Assert.IsEmpty(items);
        }

        [TestMethod(DisplayName = "BatchCollection: Clear(): Returns self-instance for method chaining")]
        public void Clear_ReturnsInstanceForChaining()
        {
            var batch = new BatchCollection(10u);
            batch.Add(1).Add(2).Add(3);
            
            var result = batch.Clear();
            
            Assert.AreSame(batch, result);
            Assert.AreEqual(0, batch.Count);
        }

        [TestMethod(DisplayName = "BatchCollection: Add(): Returns self-instance for method chaining")]
        public void Add_ReturnsInstanceForChaining()
        {
            var batch = new BatchCollection(10u);
            
            var result = batch.Add(1);
            
            Assert.AreSame(batch, result);
        }

        [TestMethod(DisplayName = "BatchCollection: GetNext(): With mixed types (object collection) preserves order")]
        public void GetNext_MixedTypes_PreservesOrder()
        {
            var batch = new BatchCollection(5u);
            
            batch.Add(1)
                 .Add("two")
                 .Add(3.0)
                 .Add(true)
                 .Add(DateTime.Now);

            var items = batch.GetNext().Cast<object>().ToArray();
            
            Assert.HasCount(5, items);
            Assert.AreEqual(1, items[0]);
            Assert.AreEqual("two", items[1]);
            Assert.AreEqual(3.0, items[2]);
            Assert.IsTrue((bool?)items[3]);
            Assert.IsInstanceOfType(items[4], typeof(DateTime));
        }
    }
}