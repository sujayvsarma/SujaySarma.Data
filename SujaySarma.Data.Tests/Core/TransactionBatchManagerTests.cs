using System.Collections.Generic;

using SujaySarma.Data.Core;

namespace SujaySarma.Data.Tests.Core
{
    [TestClass()]
    public class TransactionBatchManagerTests
    {
        [TestMethod()]
        public void GetNextTest()
        {
            while (_batchManager.ItemsLeft > 0)
            {
                List<int> batch = _batchManager.GetNext();
                Assert.IsTrue(batch.Count <= batchSize);
            }
        }

        
        public TransactionBatchManagerTests()
        {
            _batchManager = new TransactionBatchManager<int>(batchSize);
            for (int i = 0; i < 65535; i++)
            {
                _batchManager.Add(i);
            }
        }

        private readonly int batchSize = 100;
        private readonly TransactionBatchManager<int> _batchManager;
    }
}